using BBBCSIO;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// An interface to insure that a behaviour stack can maintain a list of 
    /// gpio input states
    /// 
    /// </summary>
    public interface IBehaviour_GPIO_InputStates
    {
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the list of our input status objects
        /// 
        /// Note: there is no set here. The implementor is expected to create a 
        /// non null version during construction
        /// 
        /// </summary>
        List<SCData_PinInputConfig> GPIOInputStatusList { get;  }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the value he list of input status objects using a specified GPIO
        /// 
        /// </summary>
        /// <param name="gpioIn">the gpio</param>
        /// <returns> the pin state true or false, false for fail</returns>
        bool GetPinStatusByGPIO(GpioEnum gpioIn);

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Update the list of input status objects using a specified GPIO
        /// 
        /// </summary>
        /// <param name="gpioIn">the gpio</param>
        /// <param name="pinStateIn">the pin state</param>
        void SetPinStatusByGPIO(GpioEnum gpioIn, bool pinStateIn);


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Indicates if pins are dirty or not. This means have changed since 
        /// the last time some monitoring process checked
        /// 
        /// </summary>
        bool GPIOPinsAreDirty { get; set; }

        // IBehaviour_GPIO_InputStates sample implementation
        //public List<SCData_PinInputConfig> GPIOInputStatusList { get => gpioInputStatusList; set => gpioInputStatusList = value; }
        //public bool GPIOPinsAreDirty();

        ///// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        ///// <summary>
        ///// Get the value he list of input status objects using a specified GPIO
        ///// We just return the first one we find. There should be no duplicates!
        ///// 
        ///// </summary>
        ///// <param name="gpioIn">the gpio</param>
        ///// <returns> the pin state true or false, false for fail</returns>
        //public bool GetPinStatusByGPIO(GpioEnum gpioIn)
        //{
        //    if (GPIOInputStatusList == null) return false;
        //    foreach (SCData_PinInputConfig pinStatus in GPIOInputStatusList)
        //    {
        //        if (pinStatus.Gpio == gpioIn) return pinStatus.PinState;
        //    }
        //    return false;
        //}

        ///// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        ///// <summary>
        ///// Set the value he list of input status objects using a specified GPIO
        ///// We just update the first one we find. There should be no duplicates!
        ///// 
        ///// </summary>
        ///// <param name="gpioIn">the gpio</param>
        ///// <param name="pinStateIn">the pin state</param>
        //public void SetPinStatusByGPIO(GpioEnum gpioIn, bool pinStateIn)
        //{
        //    if (GPIOInputStatusList == null) return;
        //    foreach (SCData_PinInputConfig pinStatus in GPIOInputStatusList)
        //    {
        //if (pinStatus.PinState != pinStateIn)
        //{
        //    pinStatus.PinState = pinStateIn;
        //    // flag it as changed
        //    pinStatus.IsDirty = true;
        //}
        //    }
        //}

    }
}
