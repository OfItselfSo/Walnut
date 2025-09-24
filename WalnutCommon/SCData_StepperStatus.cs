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
    /// This class encapsulates the state of the steppers in the PRU
    /// 
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// </summary>
    [SerializableAttribute]
    public class SCData_StepperStatus
    {

        private uint allSteppersDirRegister = 0;
        private uint step0_Enabled = 0;
        private uint step0_StepCount = 0;
        private uint step1_Enabled = 0;
        private uint step1_StepCount = 0;
        private uint step2_Enabled = 0;
        private uint step2_StepCount = 0;
        private uint step3_Enabled = 0;
        private uint step3_StepCount = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public SCData_StepperStatus()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state as a string
        /// </summary>
        public virtual void GetState(StringBuilder sb)
        {
            if (sb == null) return;

            sb.Append("DirReg=0x"+ AllSteppersDirRegister.ToString("X8") + ", Step0Ena=" + Step0_Enabled.ToString() + ", Step0Cnt=" + Step0_StepCount.ToString() + ", Step1Ena = " + Step1_Enabled.ToString() + ", Step1Cnt = " + Step1_StepCount.ToString() + ", Step2Ena = " + Step2_Enabled.ToString() + ", Step2Cnt = " + Step2_StepCount.ToString() + ", Step3Ena = " + Step3_Enabled.ToString() + ", Step3Cnt = " + Step3_StepCount.ToString());
        }

        public uint Step0_Enabled { get => step0_Enabled; set => step0_Enabled = value; }
        public uint Step0_StepCount { get => step0_StepCount; set => step0_StepCount = value; }
        public uint Step1_Enabled { get => step1_Enabled; set => step1_Enabled = value; }
        public uint Step1_StepCount { get => step1_StepCount; set => step1_StepCount = value; }
        public uint Step2_Enabled { get => step2_Enabled; set => step2_Enabled = value; }
        public uint Step2_StepCount { get => step2_StepCount; set => step2_StepCount = value; }
        public uint Step3_Enabled { get => step3_Enabled; set => step3_Enabled = value; }
        public uint Step3_StepCount { get => step3_StepCount; set => step3_StepCount = value; }
        public uint AllSteppersDirRegister { get => allSteppersDirRegister; set => allSteppersDirRegister = value; }
    }
}
