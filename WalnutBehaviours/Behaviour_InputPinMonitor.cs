using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using BBBCSIO;

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
    /// An Behaviour to encapsulate monitor the state of input pins in the 
    /// global data and take action 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_GPIO_InputStates
    /// 
    /// Note: we expect the main object to implment IBehaviour_UpdateScreenWithPinStates
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_InputPinMonitor : Behaviour_Base
    {

        private GpioEnum pin1GPIO = GpioEnum.GPIO_NONE;
        private GpioEnum pin2GPIO = GpioEnum.GPIO_NONE;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_InputPinMonitor(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_GPIO_InputStates) == false) throw new Exception("The behaviour stack does not implement IBehaviour_GPIO_InputStates");
        }

        public GpioEnum Pin1GPIO { get => pin1GPIO; set => pin1GPIO = value; }
        public GpioEnum Pin2GPIO { get => pin2GPIO; set => pin2GPIO = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_GPIO_InputStates) == false) return;
            if (globalDataStore.MainObject == null) return;
            if ((globalDataStore.MainObject is IBehaviour_UpdateScreenWithPinStates) == false) return;

            // do we need to act?
            if ((globalDataStore as IBehaviour_GPIO_InputStates).GPIOPinsAreDirty == false) return;

            // yes we do
            bool pin1State = (globalDataStore as IBehaviour_GPIO_InputStates).GetPinStatusByGPIO(Pin1GPIO);
            bool pin2State = (globalDataStore as IBehaviour_GPIO_InputStates).GetPinStatusByGPIO(Pin2GPIO);

            // set the states now
            (globalDataStore.MainObject as IBehaviour_UpdateScreenWithPinStates).UpdateScreenWithPinStates(pin1State, pin2State); 
            // reset this now
            (globalDataStore as IBehaviour_GPIO_InputStates).GPIOPinsAreDirty = false;

        }

    }
}
