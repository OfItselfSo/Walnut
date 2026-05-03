using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WalnutCommon;


/// +------------------------------------------------------------------------------------------------------------------------------+
/// ¦                                                   TERMS OF USE: MIT License                                                  ¦
/// +------------------------------------------------------------------------------------------------------------------------------¦
/// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
/// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
/// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
/// ¦is furnished to do so, subject to the following conditions:                                                                   ¦
/// ¦                                                                                                                              ¦
/// ¦The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.¦
/// ¦                                                                                                                              ¦
/// ¦THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          ¦
/// ¦WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         ¦
/// ¦COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   ¦
/// ¦ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         ¦
/// +------------------------------------------------------------------------------------------------------------------------------+

namespace WalnutBehaviours
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A base class for Behaviour Statemachine Data. Statemachine Data classes are
    /// specialized Data classes which contain the global data for the Behaviour stack.
    /// This is ultimately what the system uses to turn on motors etc
    /// 
    /// </summary>
    [SerializableAttribute]
    public abstract class Behaviour_StateMachine : Behaviour_Base, IBehaviour_WaldosEnabledState
    {
        // this is our lock object. All functions that access data in here must 
        // lock and unlock this object
        protected Object lockObject = new Object();

        // this is the list of data. Lower on the list means more fundamental
        protected LinkedList<Behaviour_Base> behaviourList = new LinkedList<Behaviour_Base>();

        // a global setting to inhibit any Behaviour worker thread from running, they can be turned off individually as well
        private bool workerThreadsOKToRun = false;

        public const int DEFAULT_POLL_DELAY_MS = 100;
        public const int MIN_POLL_DELAY_MS = 10;

        // the time between poll of the behaviours
        private int pollDelayMS = DEFAULT_POLL_DELAY_MS;

        // the global waldos enabled state. 
        private bool waldosEnabledState = false;

        // this object is what we use to acquire the data we need. On the WALNUT_SERVER it is 
        // frmMain() on the WALNUT_CLIENT is is MainClass()
        [NonSerialized]
        private object mainObject = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// Note:  we are protected here. The derived class must provide the 
        /// operating location information. Since we are the global class we feed
        /// null into the call to the base class constructor and set it manually 
        /// in our own constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        protected Behaviour_StateMachine(BehaviourLocationEnum operatingLocationIn) : base(operatingLocationIn, null)
        {
            globalDataStore = this;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The thread worker for this behaviour. We poll each behaviour in turn in 
        /// here. It is up to the behaviour what it does with this call
        /// 
        /// </summary>
        protected override void ThreadWorker()
        {

            while (this.WorkerThreadIsRunnable == true)
            {
                // loop through the list and call each behaviours PollWorker function
                foreach (Behaviour_Base behaviour in BehaviourList)
                {
                    // re-check our globaldata to see if any worker thread is ok to run
                    if (GlobalDataStore == null) return;
                    if (GlobalDataStore.WorkerThreadsOKToRun == false) return;
                    // should never be null really
                    if (behaviour != null)
                    {
                        // call the behaviours specific poll worker
                        behaviour.PollWorker();
                    }
                }

                // send the global data to the client, but only if we are isDirty (have changed)
                // this minimises the traffic sent to the other side - no point in sending unchanged
                // data since the globalDataStore there already has it.

                // having said that, on the server side, the detected source point jumps about so much only about 
                // half of the possible opportunities to skip the send are actually possible

                // note that the variables that are not of interest to the other side do not not 
                // update isDirty because that would cause that stack to be updated unnecessarily

                if ((globalDataStore.MainObject != null)
                    && ((globalDataStore.MainObject is IBehaviour_TransmitGlobalStack) == true)
                    && ((globalDataStore is IBehaviour_IsDirtyOnServer) == true)
                    && ((globalDataStore is IBehaviour_IsDirtyOnClient) == true))
                {
                    // only send data to the client if the data has changed and we are on the server
                    if (((globalDataStore as IBehaviour_IsDirtyOnServer).IsDirtyOnServer == true) && (CurrentLocation == BehaviourLocationEnum.WALNUT_SERVER))
                    {
                        (globalDataStore.MainObject as IBehaviour_TransmitGlobalStack).TransmitGlobalStackData();
                        // reset this now
                        (globalDataStore as IBehaviour_IsDirtyOnServer).IsDirtyOnServer = false;
                        //Console.WriteLine("111 isDirtyOnServer now false");
                    }
                    // only send data to the server if the data has changed and we are on the client
                    if (((globalDataStore as IBehaviour_IsDirtyOnClient).IsDirtyOnClient == true) && (CurrentLocation == BehaviourLocationEnum.WALNUT_CLIENT))
                    {
                        (globalDataStore.MainObject as IBehaviour_TransmitGlobalStack).TransmitGlobalStackData();
                        // reset this now
                        (globalDataStore as IBehaviour_IsDirtyOnClient).IsDirtyOnClient = false;
                        //Console.WriteLine("222 isDirtyOnClient now false");
                    }
                }

                // sleep now
                Thread.Sleep(PollDelayMS);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// get and set the Waldos enabled state
        /// </summary>
        public bool WaldosEnabledState { get => waldosEnabledState; set => waldosEnabledState = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global data store as a derived class name. Will return null!
        /// 
        /// </summary>
        public Behaviour_StateMachine GlobalDataStore
        {
            get
            {                
                // we are the global data store
                return this;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the delay we take after polling. We never let it go below the 
        /// specified minimum poll delay
        /// 
        /// </summary>
        public int PollDelayMS
        {
            get
            {
                return pollDelayMS;
            }
            set
            {
                pollDelayMS = value;
                if (pollDelayMS < MIN_POLL_DELAY_MS) pollDelayMS = MIN_POLL_DELAY_MS;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the Behaviours as a list. Normally this 
        /// class is built on instantiation and external agents are expected to 
        /// add,insert and delete from it as appropriate.
        /// 
        /// Note: we never get/set null if the proper list is not available we just
        /// make up a new empty one
        /// 
        /// </summary>
        public LinkedList<Behaviour_Base> BehaviourList
        {
            get
            {
                if (behaviourList == null) behaviourList = new LinkedList<Behaviour_Base>();
                return behaviourList;
            }
            set
            {
                behaviourList = value;
                if (behaviourList == null) behaviourList = new LinkedList<Behaviour_Base>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Run set the current location on each behaviour in the stack
        /// Should be called before startup and before thread starts so the 
        /// behaviours can make decisions on it
        /// 
        /// Note: the current location you set cannot be BehaviourLocationEnum.WALNUT_BOTH
        /// 
        /// </summary>
        /// <param name="currentLocationIn">our curent location</param>
        public void SetCurrentLocation(BehaviourLocationEnum currentLocationIn)
        {
            if (currentLocationIn == BehaviourLocationEnum.WALNUT_BOTH) throw new Exception("The current location cannot be BehaviourLocationEnum.WALNUT_BOTH");

            // loop through the list and set the location on each
            foreach (Behaviour_Base behaviour in BehaviourList)
            {
                // should never be null really
                if (behaviour == null) continue;
                behaviour.CurrentLocation = currentLocationIn;
            }
            // also set this on ourself
            this.CurrentLocation = currentLocationIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Run the startup actions on each Behaviour, this should be called
        /// before all the worker threads start. We operate
        /// down the stack from top to bottom.
        /// 
        /// Note: the current location is assumed to have been set
        /// 
        /// </summary>
        /// <param name="currentLocationIn">our curent location</param>
        public void RunStartupActions()
        {
            // because this object is a statemachine we can allow this behaviours thread to work
            this.WorkerThreadIsRunnable = true;

            // loop through the list and call each behaviours StartUp function
            // we do not have to worry about starting behaviours on the wrong 
            // platform. The StartWorkerThread() call takes care of that.
            foreach (Behaviour_Base behaviour in BehaviourList)
            {
                // should never be null really
                if (behaviour == null) continue;
                // just call it
                behaviour.Startup();
            }

            // also run our own startup actions.
            this.Startup();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts all local behaviour Threads. Ignores the remote ones. We operate
        /// down the stack from top to bottom.
        /// 
        /// Note: the current location is assumed to have been set
        /// 
        /// </summary>
        public void StartBehaviourThreads()
        {

            // loop through the list and call each behaviours StartWorkerThread function
            // we do not have to worry about starting behaviour Actors on the wrong 
            // platform. The StartWorkerThread() call takes care of that.
            foreach (Behaviour_Base behaviour in BehaviourList)
            {
                // should never be null really
                if (behaviour == null) continue;
                // just create one
                behaviour.StartWorkerThread();
            }
            // also start our own worker thread. This does the polling 
            this.StartWorkerThread();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Stops and closes all behaviours
        /// 
        /// </summary>
        public void StopAllBehaviours()
        {
            // turn off all behaviour worker threads globally
            this.WorkerThreadsOKToRun = false;
            // turn off this worker thread specifically
            this.WorkerThreadIsRunnable = false;

            // loop through the list and call each behaviours Shutdown function
            foreach (Behaviour_Base behaviour in BehaviourList)
            {
                // should never be null really
                if (behaviour == null) continue;
                // shut it down
                behaviour.Shutdown();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Derived classes are required to implement a shallow clone which 
        /// does not reproduce the BehaviourList
        /// 
        /// </summary>
        public abstract Behaviour_StateMachine ShallowClone();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Derived classes are required to implement a shallow copy which 
        /// updates the server variables if we are on the server and the client 
        /// variables if we are on the client
        /// 
        /// NOTE: derived classes should call this base version to achieve a proper copy
        /// 
        /// </summary>
        /// <param name="shallowClonedBehaviourStack">a shallow cloned behaviour stack</param>
        /// <param name="currentLocation">the current location</param>
        public virtual void CopyServerClientData(Behaviour_StateMachine shallowClonedBehaviourStack, BehaviourLocationEnum currentLocation)
        {
            if(shallowClonedBehaviourStack == null) return;
            // we know which variables need to be updated on which platform
            if (currentLocation == BehaviourLocationEnum.WALNUT_CLIENT)
            {
                // we are on the client, give it these variables
                WaldosEnabledState = shallowClonedBehaviourStack.WaldosEnabledState;
            }
            else if (currentLocation == BehaviourLocationEnum.WALNUT_SERVER)
            {
                // we are on the server, give it these variables
            }
            else { }

        }
        public bool WorkerThreadsOKToRun { get => workerThreadsOKToRun; set => workerThreadsOKToRun = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// gets/sets object is what we use to acquire the data from the system. On the WALNUT_SERVER it is 
        /// frmMain() on the WALNUT_CLIENT is is MainClass(). We expect it to implement various interfaces
        /// 
        /// </summary>
       public object MainObject
        {
            get
            {
                lock (lockObject)
                {
                    return mainObject;
                }
            }
            set
            {
                lock (lockObject)
                {
                    mainObject = value;
                }
            }
        }

    }
}
