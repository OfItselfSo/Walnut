using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// An Behaviour to automatically turn on recording to a file when we start
    /// and turn it off when we stop. Saves on work and automates things
    /// 
    /// NOTE: this class expects the mainObject to implement IBehaviour_RecordingOnOff
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_RecordingOnOff : Behaviour_Base
    {
        // we have to explicitly turn this on
        private bool wantRecordingOnOffAction = false;
        // we record the previous shot descriptor and replace it when we shut down
        private string previousShotDescriptor = "";
        // the current shot descriptor we use, should never be null
        private string workingShotDescriptor = "";


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_RecordingOnOff(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
        }

        public bool WantRecordingOnOffAction { get => wantRecordingOnOffAction; set => wantRecordingOnOffAction = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the working shot descriptor - will never get/set null
        /// 
        /// </summary>
        public string WorkingShotDescriptor
        {
            get
            {
                if (workingShotDescriptor == null) workingShotDescriptor = "";
                if (workingShotDescriptor.Length == 0) workingShotDescriptor = "";
                return workingShotDescriptor;
            }
            set
            {
                workingShotDescriptor = value;
                if (workingShotDescriptor == null) workingShotDescriptor = "";
                if (workingShotDescriptor.Length == 0) workingShotDescriptor = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to start the behaviour. This should only be called 
        /// once after contruction and never called again
        /// 
        /// </summary>
        public override void Startup()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if (globalDataStore.MainObject == null) return;
            if ((globalDataStore.MainObject is IBehaviour_RecordingOnOff) == false) return;

            // do we want to enable/disable the recording
            if (WantRecordingOnOffAction == true)
            {
                // yes, we do, record this
                previousShotDescriptor = (globalDataStore.MainObject as IBehaviour_RecordingOnOff).ShotDescriptor;
                // set it with our own shot descriptor text
                (globalDataStore.MainObject as IBehaviour_RecordingOnOff).ShotDescriptor = WorkingShotDescriptor;
                // turn on the recording
                (globalDataStore.MainObject as IBehaviour_RecordingOnOff).SetScreenRecordingState(true);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to stop the behaviour cleanly. Override in the 
        /// derived class but always call base.ShutdownBehaviour() in it.
        /// 
        /// </summary>
        public override void Shutdown()
        {
            if (LocationMatchesOperationLocation() == false) return;

            // do we want to enable/disable the recording
            if (WantRecordingOnOffAction == true)
            {
                // yes, we do, replace the old shot descriptor, we acquired this at startup
                (globalDataStore.MainObject as IBehaviour_RecordingOnOff).ShotDescriptor = previousShotDescriptor;
                // turn off the recording
                (globalDataStore.MainObject as IBehaviour_RecordingOnOff).SetScreenRecordingState(false);
            }
            // always call the base class
            base.Shutdown();
        }

    }
}
