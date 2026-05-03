using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using BBBCSIO;
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
    /// An Behaviour to cause an IO to follow an alpha channel variable 
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_PinOutputControl
    /// NOTE: we expect the global data store to implement IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_IO_OutputState_OnAlpha : Behaviour_Base
    {
        // if we see this bit in the alpha value we trigger high
        private byte triggerValueForIOHigh = 0;
        // the gpio that gets triggered when the alpha channel indicates
        private GpioEnum gpio = GpioEnum.GPIO_NONE;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_IO_OutputState_OnAlpha(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_PinOutputControl) == false) throw new Exception("The behaviour stack does not implement IBehaviour_PinOutputControl");
            if ((globalDataIn is IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay) == false) throw new Exception("The behaviour stack does not implement IBehaviour_IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay");
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
            if ((globalDataStore is IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay) == false) return;
            if ((globalDataStore is IBehaviour_PinOutputControl) == false) return;

            // Do we have a trigger?
            byte currentAlpha = (globalDataStore as IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay).SourcePointLowestAlphaValueFoundOnMask_Overlay;
            //if(currentAlpha == 128)
            //{
            //    int foo = 1;
            //}
            if (currentAlpha == TriggerValueForIOHigh)
            {
                // we have a trigger to turn the IO on
                (globalDataStore as IBehaviour_PinOutputControl).SetPinOutputStateByGPIO(Gpio, true);
                return;
            }
            else 
            {
                // we have a trigger to turn the IO off
                (globalDataStore as IBehaviour_PinOutputControl).SetPinOutputStateByGPIO(Gpio, false);
                return;
            }
        }

        public byte TriggerValueForIOHigh { get => triggerValueForIOHigh; set => triggerValueForIOHigh = value; }
        public GpioEnum Gpio { get => gpio; set => gpio = value; }
    }
}
