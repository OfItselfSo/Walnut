using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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

namespace WalnutCommon
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// </summary>
    [SerializableAttribute]
    public class SCData_PWMControl
    {
        private PWMIDEnum pwmID = PWMIDEnum.PWM_None;

        // there is an enable, direction flag for the PWM and also a percent value
        // the percent can be between 0 and 100. Values over 100 are considered to be 100
        private uint pwm_Enable = 0;
        private uint pwm_DirState = 0;
        private uint pwm_PWMPercent = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public SCData_PWMControl()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pwmIDIn">the id of the PWM motor</param>
        public SCData_PWMControl(PWMIDEnum pwmIDIn)
        {
            PWMID = pwmIDIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pwmIDIn">the id of the PWM motor</param>
        /// <param name="pwm_DirStateIn">the direction state 0 or 1</param>
        /// <param name="pwm_EnableIn"> the enable state 0 or 1</param>
        /// <param name="pwm_PWMPercentIn">the percentage speed 0 to 100</param>
        public SCData_PWMControl(PWMIDEnum pwmIDIn, uint pwm_EnableIn, uint pwm_DirStateIn, uint pwm_PWMPercentIn)
        {
            PWMID = pwmIDIn;
            pwm_Enable = pwm_EnableIn;
            pwm_DirState = pwm_DirStateIn;
            pwm_PWMPercent = pwm_PWMPercentIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwm_Enable data value. 
        /// </summary>
        public uint PWM_Enable
        {
            get
            {
                return pwm_Enable;
            }
            set
            {
                pwm_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwm_DirState data value. 
        /// </summary>
        public uint PWM_DirState
        {
            get
            {
                return pwm_DirState;
            }
            set
            {
                pwm_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwm_PWMPercent data value. 
        /// </summary>
        public uint PWM_PWMPercent
        {
            get
            {
                if (pwm_PWMPercent > 100) return 100;
                return pwm_PWMPercent;
            }
            set
            {
                pwm_PWMPercent = value;
            }
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state as a string
        /// </summary>
        public virtual void GetState(StringBuilder sb)
        {
            if (sb == null) return;

            sb.Append(", PWM_ID=" + PWMID.ToString() + ", PWM_Enable=" + PWM_Enable.ToString() + ", PWMA_PWMPercent=" + PWM_PWMPercent.ToString() + ", PWMA_DirState=" + PWM_DirState.ToString());
        }
        public PWMIDEnum PWMID { get => pwmID; set => pwmID = value; }

    }
}
