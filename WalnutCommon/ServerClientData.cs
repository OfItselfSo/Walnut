using System;
using System.Text;
using System.Collections.Generic;

namespace WalnutCommon
{
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

    /// NOTE: this class and the entire WalnutCommon project is shared with the client which runs on the Beaglebone Black. If your primary
    /// interest is in working out how a Typed object is sent between a Server and Client (and back) to transmit complex data you should
    /// have a look at the RemCon demonstrator project at http://www.OfItselfSo.com/RemCon which is devoted to that topic. This class 
    /// is directly derived from that project. If your primary interest is in seeing how a C# program running on Windows can control 
    /// stepper motors see the Tilo project - it is specifically set up to demo that. http://www.OfItselfSo.com/Tilo

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// </summary>
    [SerializableAttribute]
    public class ServerClientData
    {
        public const uint DEFAULT_SPEED = 50000;

        // this is what this class means
        // NOTE: this enum also uses the [SerializableAttribute]
        private ServerClientDataContentEnum dataContent = ServerClientDataContentEnum.NO_DATA;
        private string dataStr = "";

        // this is the what the user data means only valid if dataContent == USER_DATA
        UserDataContentEnum userDataContent = UserDataContentEnum.NO_DATA;
        // this is the user flag data only valid if dataContent == USER_FLAG
        UserDataFlagEnum userFlag = UserDataFlagEnum.NO_FLAG;

        private List<ColoredRotatedRect> rectList = null;

        // NOTE: normally the decision of which steppers to run is at the 
        // discretion of the code in the WalnutClient. However the server 
        // can force a stepper on or off it it wishes

        // this enables or disables all steppers
        private uint allStep_Enable = 0;
        // there is an enable, direction flag for each stepper and also a step speed value
        // each is treated as a 4 byte uint. This makes it easy for the client to dig it out
        // and give it to the PRU
        private uint step0_Enable = 0;
        private uint step0_DirState = 0;
        private uint step0_StepSpeed = 0;
        private uint step1_Enable = 1;
        private uint step1_DirState = 1;
        private uint step1_StepSpeed = 1;
        private uint step2_Enable = 1;
        private uint step2_DirState = 1;
        private uint step2_StepSpeed = 1;
        private uint step3_Enable = 1;
        private uint step3_DirState = 1;
        private uint step3_StepSpeed = 1;
        private uint step4_Enable = 1;
        private uint step4_DirState = 1;
        private uint step4_StepSpeed = 1;
        private uint step5_Enable = 1;
        private uint step5_DirState = 1;
        private uint step5_StepSpeed = 1;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public ServerClientData()
        { }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor. Assumes ServerClientDataContentEnum.USER_DATA
        /// </summary>
        /// <param name="dataStrIn">the string data value</param>
        public ServerClientData(string dataStrIn)
        {
            DataContent = ServerClientDataContentEnum.USER_DATA;
            DataStr = dataStrIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataContentIn">the data content type</param>
        public ServerClientData(ServerClientDataContentEnum dataContentIn)
        {
            DataContent = dataContentIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the data content flag
        /// </summary>
        public ServerClientDataContentEnum DataContent
        {
            get
            {
                return dataContent;
            }
            set
            {
                dataContent = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the DataStr data value. Will never return null will return empty
        /// </summary>
        public string DataStr
        {
            get
            {
                if (dataStr == null) dataStr = "";
                return dataStr;
            }
            set
            {
                dataStr = value;
                if (dataStr == null) dataStr = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the user data content flag. This is what the data means as opposed
        /// to what the whole class means (DataContent). Only valid if DataContent == USER_DATA
        /// 
        /// Note: you are dealing with [Flags]
        /// </summary>
        public UserDataContentEnum UserDataContent
        {
            get
            {
                return userDataContent;
            }
            set
            {
                userDataContent = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the user data flag. This is the flag data as opposed
        /// to what the whole class means (DataContent). Only valid if DataContent == USER_FLAG
        /// 
        /// Note: you are dealing with [Flags]
        /// </summary>
        public UserDataFlagEnum UserFlag
        {
            get
            {
                return userFlag;
            }
            set
            {
                userFlag = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the rectList data value. Will never return null 
        /// </summary>
        public List<ColoredRotatedRect> RectList
        {
            get
            {
                if (rectList == null) rectList = new List<ColoredRotatedRect>();
                return rectList;
            }
            set
            {
                rectList = value;
                if (rectList == null) rectList = new List<ColoredRotatedRect>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the allStep_Enable data value. 
        /// </summary>
        public uint Waldo_Enable
        {
            get
            {
                return allStep_Enable;
            }
            set
            {
                allStep_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step0_Enable data value. 
        /// </summary>
        public uint Step0_Enable
        {
            get
            {
                return step0_Enable;
            }
            set
            {
                step0_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step0_DirState data value. 
        /// </summary>
        public uint Step0_DirState
        {
            get
            {
                return step0_DirState;
            }
            set
            {
                step0_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step0_StepSpeed data value. 
        /// </summary>
        public uint Step0_StepSpeed
        {
            get
            {
                return step0_StepSpeed;
            }
            set
            {
                step0_StepSpeed = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step1_Enable data value. 
        /// </summary>
        public uint Step1_Enable
        {
            get
            {
                return step1_Enable;
            }
            set
            {
                step1_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step1_DirState data value. 
        /// </summary>
        public uint Step1_DirState
        {
            get
            {
                return step1_DirState;
            }
            set
            {
                step1_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step1_StepSpeed data value. 
        /// </summary>
        public uint Step1_StepSpeed
        {
            get
            {
                return step1_StepSpeed;
            }
            set
            {
                step1_StepSpeed = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step2_Enable data value. 
        /// </summary>
        public uint Step2_Enable
        {
            get
            {
                return step2_Enable;
            }
            set
            {
                step2_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step2_DirState data value. 
        /// </summary>
        public uint Step2_DirState
        {
            get
            {
                return step2_DirState;
            }
            set
            {
                step2_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step2_StepSpeed data value. 
        /// </summary>
        public uint Step2_StepSpeed
        {
            get
            {
                return step2_StepSpeed;
            }
            set
            {
                step2_StepSpeed = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step3_Enable data value. 
        /// </summary>
        public uint Step3_Enable
        {
            get
            {
                return step3_Enable;
            }
            set
            {
                step3_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step3_DirState data value. 
        /// </summary>
        public uint Step3_DirState
        {
            get
            {
                return step3_DirState;
            }
            set
            {
                step3_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step3_StepSpeed data value. 
        /// </summary>
       public uint Step3_StepSpeed
        {
            get
            {
                return step3_StepSpeed;
            }
            set
            {
                step3_StepSpeed = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step4_Enable data value. 
        /// </summary>
        public uint Step4_Enable
        {
            get
            {
                return step4_Enable;
            }
            set
            {
                step4_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step4_DirState data value. 
        /// </summary>
        public uint Step4_DirState
        {
            get
            {
                return step4_DirState;
            }
            set
            {
                step4_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step4_StepSpeed data value. 
        /// </summary>
        public uint Step4_StepSpeed
        {
            get
            {
                return step4_StepSpeed;
            }
            set
            {
                step4_StepSpeed = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step5_Enable data value. 
        /// </summary>
        public uint Step5_Enable
        {
            get
            {
                return step5_Enable;
            }
            set
            {
                step5_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step5_DirState data value. 
        /// </summary>
        public uint Step5_DirState
        {
            get
            {
                return step5_DirState;
            }
            set
            {
                step5_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the step5_StepSpeed data value. 
        /// </summary>
        public uint Step5_StepSpeed
        {
            get
            {
                return step5_StepSpeed;
            }
            set
            {
                step5_StepSpeed = value;
            }
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state
        /// </summary>
        public string GetState()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DataContent=" + DataContent.ToString());
            //Console.WriteLine("scData.Waldo_Enable=" + scData.Waldo_Enable.ToString());
            if ((DataContent == ServerClientDataContentEnum.USER_DATA))
            {
                sb.Append(", UserDataContent=" + UserDataContent.ToString());

                sb.Append(", Waldo_Enable=" + Waldo_Enable.ToString());

                // DataContent is a Flags enum so we can have multiple meanings
                if (UserDataContent.HasFlag(UserDataContentEnum.STEP0_DATA))
                {
                    sb.Append(", Step0_Enable=" + Step0_Enable.ToString() + ", Step0_StepSpeed=" + Step0_StepSpeed.ToString() +", Step0_DirState=" + Step0_DirState.ToString());
                }
                if (UserDataContent.HasFlag(UserDataContentEnum.RECT_DATA))
                {
                    if (RectList == null) sb.Append(", RectList=null");
                    else sb.Append(", RectCount=" + RectList.Count.ToString());
                }
                if (UserDataContent.HasFlag(UserDataContentEnum.FLAG_DATA))
                {
                    if (UserFlag.HasFlag(UserDataFlagEnum.MARK_FLAG))
                    {
                        sb.Append(", MARK_FLAG");
                    }
                    if (UserFlag.HasFlag(UserDataFlagEnum.EXIT_FLAG))
                    {
                        sb.Append(", EXIT_FLAG");
                    }
                }
            }
            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the current state
        /// </summary>
        public override string ToString()
        {
            return GetState();
        }
    }
}
