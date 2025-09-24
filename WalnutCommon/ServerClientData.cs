using System;
using System.Text;
using System.Collections.Generic;

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
    public class ServerClientData
    {
        // this enables or disables all steppers, pwms etc
        private uint waldo_Enable = 0;

         // this is what this class means
        // NOTE: this enum also uses the [SerializableAttribute]
        private ServerClientDataContentEnum dataContent = ServerClientDataContentEnum.NO_DATA;
        private string dataStr = "";

        // this is the what the user data means only valid if dataContent == USER_DATA
        UserDataContentEnum userDataContent = UserDataContentEnum.NO_DATA;
        // this is the user flag data only valid if dataContent == USER_FLAG
        UserDataFlagEnum userFlag = UserDataFlagEnum.NO_FLAG;

        private List<SCData_StepperControl> stepperControlList = null;

        private List<ColoredRotatedObject> rectList = null;

        private List<SrcTgtData> srcTgtList = null;

        // NOTE: normally the decision of which steppers to run is at the 
        // discretion of the code in the WalnutClient. However the server 
        // can force a stepper on or off it it wishes

        // there is an enable, direction flag for each PWM and also a percent value
        // the percent can be between 0 and 100. Values over 100 are considered to be 100
        private uint pwmA_Enable = 0;
        private uint pwmA_DirState = 0;
        private uint pwmA_PWMPercent= 0;

        private uint pwmB_Enable = 0;
        private uint pwmB_DirState = 0;
        private uint pwmB_PWMPercent = 0;

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
        public List<ColoredRotatedObject> RectList
        {
            get
            {
                if (rectList == null) rectList = new List<ColoredRotatedObject>();
                return rectList;
            }
            set
            {
                rectList = value;
                if (rectList == null) rectList = new List<ColoredRotatedObject>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the srcTgtList data value. Will never return null 
        /// </summary>
        public List<SrcTgtData> SrcTgtList
        {
            get
            {
                if (srcTgtList == null) srcTgtList = new List<SrcTgtData>();
                return srcTgtList;
            }
            set
            {
                srcTgtList = value;
                if (srcTgtList == null) srcTgtList = new List<SrcTgtData>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the stepperControlList data value. Will never return null 
        /// </summary>
        public List<SCData_StepperControl> StepperControlList
        {
            get
            {
                if (stepperControlList == null) stepperControlList = new List<SCData_StepperControl>();
                return stepperControlList;
            }
            set
            {
                stepperControlList = value;
                if (stepperControlList == null) stepperControlList = new List<SCData_StepperControl>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the waldo_Enable data value. 
        /// </summary>
        public uint Waldo_Enable
        {
            get
            {
                return waldo_Enable;
            }
            set
            {
                waldo_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwmA_Enable data value. 
        /// </summary>
        public uint PWMA_Enable
        {
            get
            {
                return pwmA_Enable;
            }
            set
            {
                pwmA_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwmA_DirState data value. 
        /// </summary>
        public uint PWMA_DirState
        {
            get
            {
                return pwmA_DirState;
            }
            set
            {
                pwmA_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwmA_PWMPercent data value. 
        /// </summary>
        public uint PWMA_PWMPercent
        {
            get
            {
                if (pwmA_PWMPercent > 100) return 100;
                return pwmA_PWMPercent;
            }
            set
            {
                pwmA_PWMPercent = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwmB_Enable data value. 
        /// </summary>
        public uint PWMB_Enable
        {
            get
            {
                return pwmB_Enable;
            }
            set
            {
                pwmB_Enable = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwmB_DirState data value. 
        /// </summary>
        public uint PWMB_DirState
        {
            get
            {
                return pwmB_DirState;
            }
            set
            {
                pwmB_DirState = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the pwmB_PWMPercent data value. 
        /// </summary>
        public uint PWMB_PWMPercent
        {
            get
            {
                if (pwmB_PWMPercent > 100) return 100;
                return pwmB_PWMPercent;
            }
            set
            {
                pwmB_PWMPercent = value;
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

            if ((DataContent == ServerClientDataContentEnum.USER_DATA))
            {
                sb.Append(", UserDataContent=" + UserDataContent.ToString());

                sb.Append(", Waldo_Enable=" + Waldo_Enable.ToString());

                // what toes the UserDataContent value say we are carrying
                if (UserDataContent.HasFlag(UserDataContentEnum.STEPPER_CONTROL))
                {
                    sb.Append("\r\n");
                    foreach (SCData_StepperControl stControlObj in StepperControlList)
                    {
                        stControlObj.GetState(sb);
                        sb.Append("\r\n");
                    }
                }
                if (UserDataContent.HasFlag(UserDataContentEnum.PWMA_DATA))
                {
                    sb.Append(", PWMA_Enable=" + PWMA_Enable.ToString() + ", PWMA_PWMPercent=" + PWMA_PWMPercent.ToString() + ", PWMA_DirState=" + PWMA_DirState.ToString());
                }
                if (UserDataContent.HasFlag(UserDataContentEnum.RECT_DATA))
                {
                    if (RectList == null) sb.Append(", RectList=null");
                    else sb.Append(", RectCount=" + RectList.Count.ToString());
                }
                if (UserDataContent.HasFlag(UserDataContentEnum.SRCTGT_DATA))
                {
                    if (SrcTgtList == null) sb.Append(", SrcTgtList=null");
                    else sb.Append(", SrcTgtCount=" + SrcTgtList.Count.ToString());
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
