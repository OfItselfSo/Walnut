using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;


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
    /// A base class for Behaviour Data. 
    /// 
    /// </summary>
    [SerializableAttribute]
    public abstract class Behaviour_Base
    {
        // where we operate
        private BehaviourLocationEnum operatingLocation = BehaviourLocationEnum.None;

        // the platform we are now on
        [NonSerialized]
        private BehaviourLocationEnum currentLocation = BehaviourLocationEnum.None;

        // the worker thread (if used)
        [NonSerialized]
        private Thread workingThread = null;
        [NonSerialized]
        private bool workerThreadIsRunnable = false;

        // if true we are a one shot event and running the Startup() call is all
        // that is required. After that the object can be removed from the polling 
        // list to minimize the amount of work that has to be done and to avoid
        // transmitting it to the client.

        // NOTE that you do not _have_ to remove these after startup. Everything
        // will still work. It is just more efficient to do so.
        private bool removableAfterStartup = false;

        // the datastore specific to all behaviours
        protected Behaviour_StateMachine globalDataStore = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// Note we are protected here. The derived class must provide the 
        /// operating location information.
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        protected Behaviour_Base(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn)
        {
            operatingLocation = operatingLocationIn;
            globalDataStore = globalDataIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to start the behaviour. This should only be called 
        /// once after contruction and never called again
        /// 
        /// </summary>
        public virtual void Startup()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to stop the behaviour cleanly. Override in the 
        /// derived class but always call base.Shutdown() in it.
        /// 
        /// </summary>
        public virtual void Shutdown()
        {
            WorkerThreadIsRunnable = false;
            StopWorkerThread();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Start the worker thread
        /// 
        /// </summary>
        public void StartWorkerThread()
        {
            // can we start? We never start if this behaviour is not designed for 
            // the current location
            if (LocationMatchesOperationLocation() == false) return;
            // never start if we are not supposed to have a running thread
            if(WorkerThreadIsRunnable != true) return;

            if (workingThread == null)
            {
                workingThread = new Thread(new ThreadStart(ThreadWorker));
                workingThread.Start();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Stop the worker thread
        /// 
        /// Override this if you need to use it.
        /// 
        /// </summary>
        public void StopWorkerThread()
        {
            // can never be restarted in this implementation
            WorkerThreadIsRunnable = false;

            if (workingThread != null)
            {
                try
                {
                    workingThread.Abort();
                }
                catch { }
            }
            workingThread = null;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The thread worker for this behaviour. 
        /// 
        /// Override this if you need to use it.
        /// 
        /// </summary>
        protected virtual void ThreadWorker()
        {
            // Some work here, so that we do not spin wildly if called inappropriately
            Thread.Sleep(1000);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// Override this if you need to use it.
        /// 
        /// </summary>
        public virtual void PollWorker()
        {
            if (LocationMatchesOperationLocation() == false) return;
            // override as needed
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Quick utility to see if our current location matches the operation location
        /// 
        /// </summary>
        public bool LocationMatchesOperationLocation()
        {
            // never match on this
            if (CurrentLocation == BehaviourLocationEnum.None) return false;
            // exact match is ok
            if (CurrentLocation == OperatingLocation) return true;
            // operating location is BOTH but not NONE is ok
            if (OperatingLocation == BehaviourLocationEnum.WALNUT_BOTH) return true;
            // don't match
            return false;
        }

        public bool WorkerThreadIsRunnable { get => workerThreadIsRunnable; set => workerThreadIsRunnable = value; }
        // there is no set accessor, this is done in the constructor
        public BehaviourLocationEnum OperatingLocation { get => operatingLocation; }
        public BehaviourLocationEnum CurrentLocation { get => currentLocation; set => currentLocation = value; }
        public bool RemovableAfterStartup { get => removableAfterStartup; set => removableAfterStartup = value; }
    }
}
