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
    /// An Behaviour to process an incoming stepper control list. 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_StepperList
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_WaldosEnabledState
    /// 
    /// NOTE: this class expects the mainObject to implement IBehaviour_ProcessStepperControlList
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_ProcessStepperControlList : Behaviour_Base
    {
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_ProcessStepperControlList(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_StepperList) == false) throw new Exception("The behaviour stack does not implement IBehaviour_StepperList");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_StepperList) == false) return;
            if ((globalDataStore is IBehaviour_WaldosEnabledState) == false) return;
            if (globalDataStore.MainObject == null) return;
            if ((globalDataStore.MainObject is IBehaviour_ProcessStepperControlList) == false) return;

            // get the stepper control list from the global settings
            List<SCData_Stepper> stControlList = (globalDataStore as IBehaviour_StepperList).StepperList;
            // null out the stepper list so we do not process it twice
            // this should be ok since the only thing that populates it is an earlier behaviour poll worker
            (globalDataStore as IBehaviour_StepperList).StepperList = null;

            // a null stControlList on the process command below means Stop All Waldos, however the global data store stControlList
            // can be null without meaning that so we do not process if it is null
            if(stControlList == null) return;

            // but we should check to see if we have stop all waldos command 
            if ((globalDataStore as IBehaviour_WaldosEnabledState).WaldosEnabledState == false)
            {
                // deliberately null this out so we stop all waldo
                stControlList = null;
                // fall through
            }

            // send the stepper control list off for processing
            (globalDataStore.MainObject as IBehaviour_ProcessStepperControlList).ProcessStepperControlList(stControlList);
        }

    }
}
