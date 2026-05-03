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
    /// An Behaviour to monitor the state of the input pins in the 
    /// global data which represent the endstops and take action 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_GPIO_InputStates
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_ProbeEndstopTrigger : Behaviour_Base
    {

        private GpioEnum leftEndstopGPIO = GpioEnum.GPIO_NONE;
        private GpioEnum rightEndstopGPIO = GpioEnum.GPIO_NONE;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_ProbeEndstopTrigger(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_GPIO_InputStates) == false) throw new Exception("The behaviour stack does not implement IBehaviour_GPIO_InputStates");
            if ((globalDataIn is IBehaviour_ProbeRotationControl) == false) throw new Exception("The behaviour stack does not implement IBehaviour_ProbeRotationControl");
        }

        public GpioEnum LeftEndstopGPIO { get => leftEndstopGPIO; set => leftEndstopGPIO = value; }
        public GpioEnum RightEndstopGPIO { get => rightEndstopGPIO; set => rightEndstopGPIO = value; }

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
            if ((globalDataStore is IBehaviour_ProbeRotationControl) == false) return;

            // NOTE: this is a trigger, all we do is inhibit the rotation, something else must re-enable it

            // get the pins
            bool pinLeftState = (globalDataStore as IBehaviour_GPIO_InputStates).GetPinStatusByGPIO(LeftEndstopGPIO);
            bool pinRightState = (globalDataStore as IBehaviour_GPIO_InputStates).GetPinStatusByGPIO(RightEndstopGPIO);

            // is the left endstop triggered?
            if (pinLeftState==true)
            {
                if((globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Left == false) Console.WriteLine("ProbeRotationInhibit_Left now changed to true");
                // yes, inhibit rotation
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Left = true;
            }

            // is the right endstop triggered?
            if (pinRightState == true)
            {
                if ((globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Right == false) Console.WriteLine("ProbeRotationInhibit_Right now changed to true");
                // yes, inhibit rotation
                (globalDataStore as IBehaviour_ProbeRotationControl).ProbeRotationInhibit_Right = true;
            }

        }
    }
}
