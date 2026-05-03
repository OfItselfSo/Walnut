using BBBCSIO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// An Behaviour to encapsulate GPIO Inputs. 
    /// 
    /// </summary>
    [SerializableAttribute]
    public class Behaviour_GPIO_Inputs : Behaviour_Base
    {
        // the list of gpios we use for this class
        private List<SCData_PinInputConfig> inputGPIOList = new List<SCData_PinInputConfig>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        /// <param name="globalDataIn">the global data class</param>
        public Behaviour_GPIO_Inputs(BehaviourLocationEnum operatingLocationIn, Behaviour_StateMachine globalDataIn) : base(operatingLocationIn, globalDataIn)
        {
        }

        public List<SCData_PinInputConfig> InputGPIOList { get => inputGPIOList; set => inputGPIOList = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to start the behaviour. This should only be called 
        /// once after contruction and never called again
        /// 
        /// </summary>
        public override void Startup()
        {
            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_GPIO_InputStates) == false) return;

            // we do this on both the server and client
            foreach (SCData_PinInputConfig pinCfg in inputGPIOList)
            {
                // sanity check
                if (pinCfg.Gpio == GpioEnum.GPIO_NONE) continue;
                // create an input state object to hold any pin status changes we might find
                (globalDataStore as IBehaviour_GPIO_InputStates).GPIOInputStatusList.Add(new SCData_PinInputConfig(pinCfg.Gpio, false));
            }

            // below is only available on the WALNUT CLIENT. Cannot run on the server
            if (LocationMatchesOperationLocation() == false) return;

            // start each port in turn
            foreach (SCData_PinInputConfig pinCfg in inputGPIOList)
            {
                // sanity check
                if (pinCfg.Gpio == GpioEnum.GPIO_NONE) continue;
                // start the port, this is a polling version so we expect poll worker 
                // to monitor the pin states
                pinCfg.InputPort = new InputPortMM(pinCfg.Gpio);
                // create an input state object to hold any pin status changes we might find
                (globalDataStore as IBehaviour_GPIO_InputStates).GPIOInputStatusList.Add(new SCData_PinInputConfig(pinCfg.Gpio, false));

                Console.WriteLine("Created input port for " + pinCfg.Gpio.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to stop the behaviour cleanly. Override in the 
        /// derived class but always call base.Shutdown() in it.
        /// 
        /// </summary>
        public override void Shutdown()
        {
            // only available on the WALNUT CLIENT. Cannot run on the server
            if (LocationMatchesOperationLocation() == false) return;

            // stop each port in turn
            foreach (SCData_PinInputConfig pinCfg in inputGPIOList)
            {
                // sanity check
                if (pinCfg.Gpio == GpioEnum.GPIO_NONE) continue;
                if (pinCfg.InputPort == null) continue;
                // stop the port
                pinCfg.InputPort.ClosePort();
                // null it out
                pinCfg.InputPort = null;
                Console.WriteLine("Dropped input port for " + pinCfg.Gpio.ToString());
            }

            // call the base
            base.Shutdown();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The poll worker for this behaviour. Here we fetch the input state for 
        /// our pins
        /// 
        /// </summary>
        public override void PollWorker()
        {
            // only available on the WALNUT CLIENT. Cannot run on the server
            if (LocationMatchesOperationLocation() == false) return;

            if (globalDataStore == null) return;
            if ((globalDataStore is IBehaviour_GPIO_InputStates) == false) return;
            // look for each input
            foreach (SCData_PinInputConfig pinCfg in inputGPIOList)
            {
                // sanity check
                if (pinCfg.Gpio == GpioEnum.GPIO_NONE) continue;
                if (pinCfg.InputPort == null) continue;
                // update the pinstatus list held in the global data store
                bool pinState = pinCfg.InputPort.Read();

                bool oldPinState = (globalDataStore as IBehaviour_GPIO_InputStates).GetPinStatusByGPIO(pinCfg.Gpio);
                if (oldPinState != pinState)
                {
                    Console.WriteLine("PinState for " + pinCfg.Gpio.ToString() + " changed to " + pinState);
                }

                (globalDataStore as IBehaviour_GPIO_InputStates).SetPinStatusByGPIO(pinCfg.Gpio, pinState);

            }
        }

    }
}
