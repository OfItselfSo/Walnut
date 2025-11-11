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
    /// This class encapsulates the configuration (speed, dir etc) for a single
    /// stepper
    /// 
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// </summary>
    [SerializableAttribute]
    public class SCData_StepperBase
    {

        // the enable, direction and step speed values are each is treated as a 4
        // byte uint. This makes it easy for the client to dig it out
        // and give it to the PRU
        private uint stepper_Enable = 0;
        private uint stepper_DirState = 0;
        private uint stepper_StepSpeed = 0;
        private StepperIDEnum stepper_ID = StepperIDEnum.STEPPER_None;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public SCData_StepperBase()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stepper_IDIn">the ID of the stepper</param>
        public SCData_StepperBase(StepperIDEnum stepper_IDIn)
        {
            Stepper_ID = stepper_IDIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stepperIDIn">the ID of the stepper</param>
        /// <param name="dirState">the direction 0 or 1</param>
        /// <param name="enableState">the enabled state 0 or 1</param>
        /// <param name="stepSpeed">the stepper speed in Hz</param>
        public SCData_StepperBase(StepperIDEnum stepperIDIn, uint enableState, uint dirState, uint stepSpeed)
        {
            // set the values
            Stepper_ID = stepperIDIn;
            Stepper_Enable = enableState;
            Stepper_DirState = dirState;
            Stepper_StepSpeed = stepSpeed;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state as a string
        /// </summary>
        public virtual void GetState(StringBuilder sb)
        {
            if (sb == null) return;

            sb.Append(", StepperID=" + Stepper_ID.ToString() + ", Stepper_Enable=" + Stepper_Enable.ToString() + ", Stepper_StepSpeed=" + Stepper_StepSpeed.ToString() + ", Stepper_DirState=" + Stepper_DirState.ToString());
        }

        public uint Stepper_Enable { get => stepper_Enable; set => stepper_Enable = value; }
        public uint Stepper_DirState { get => stepper_DirState; set => stepper_DirState = value; }
        public uint Stepper_StepSpeed { get => stepper_StepSpeed; set => stepper_StepSpeed = value; }
        public StepperIDEnum Stepper_ID { get => stepper_ID; set => stepper_ID = value; }
    }
}
