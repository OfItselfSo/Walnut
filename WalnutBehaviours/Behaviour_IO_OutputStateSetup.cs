using BBBCSIO;
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
    /// An Behaviour to setup our GPIO outputs
    /// 
    /// NOTE: we expect the global data store to implement IBehaviour_PinOutputControl
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_IO_OutputStateSetup : Behaviour_Base
    {
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_IO_OutputStateSetup(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
            // we must have this in the global data store
            if ((globalDataIn is IBehaviour_PinOutputControl) == false) throw new Exception("The behaviour stack does not implement IBehaviour_PinOutputControl");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Add a GPIO configuration container to the global data store
        /// 
        /// </summary>
        public void AddOutputGPIO(GpioEnum gpioIn)
        {
            // do we already have this one? we do not allow duplicates
            if ((globalDataStore as IBehaviour_PinOutputControl).HasGPIO(gpioIn) == true) return;
            // we do not have it, so add it
            (globalDataStore as IBehaviour_PinOutputControl).PinStateList_Output.Add(new SCData_PinOutputConfig(gpioIn, false));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. 
        /// 
        /// </summary>
        public override void PollWorker()
        {
            // do nothing
        }

    }
}
