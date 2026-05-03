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
    /// An implementation of a Behaviour Stack for Ex012
    /// 
    /// </summary>
    [SerializableAttribute]
    public class BehaviourStack_Ex012 : Behaviour_StateMachine
        , IBehaviour_IsDirtyOnServer
        , IBehaviour_IsDirtyOnClient
        , IBehaviour_MotorSpeeds
        , IBehaviour_SourcePoint
        , IBehaviour_SourcePointColor
        , IBehaviour_TargetPoint
        , IBehaviour_TargetPointColor
        , IBehaviour_StepperList
        , IBehaviour_ScreenSize
        , IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay
        , IBehaviour_TargetPointStatistics
        , IBehaviour_GPIO_InputStates
        , IBehaviour_ProbeRotationControl
        , IBehaviour_PinOutputControl
    {
        // iBehaviour_ScreenSize implementation
        private int minScreenX = 0;
        private int minScreenY = 0;
        private int maxScreenX = 639;
        private int maxScreenY = 479;

        // IBehaviour_MotorSpeeds implementation
        private uint speed_X = 0;
        private uint speed_Y = 0;
        private uint maxSpeed_X = 0;
        private uint maxSpeed_Y = 0;

        // IBehaviour_TargetPoint implementation
        private Point targetPoint = new Point();

        // IBehaviour_SourcePoint implementation
        private Point sourcePoint = new Point();

        // IBehaviour_TargetPointColor implementation
        private Color targetPointColor = Color.White;
        private Color targetPointColorAlt = Color.White;

        // IBehaviour_SourcePointColor implementation
        private Color sourcePointColor = Color.White;

        // IBehaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay implementation
        private byte sourcePointLowestAlphaValueFoundOnMask_Overlay = 255;

        // IBehaviour_StepperList implementation
        [NonSerialized]
        private List<SCData_Stepper> stepperList = new List<SCData_Stepper>();

        // IBehaviour_TargetPointStatistics implementation
        [NonSerialized]
        private int numTargetPointsFound_Valid = 0;
        [NonSerialized]
        private int numTargetPointsFound_Empty = 0;
        [NonSerialized]
        DateTime timeAtWhichLastValidTargetPointWasFound = DateTime.MinValue;

        // IBehaviour_GPIO_InputStates implementation
        private List<SCData_PinInputConfig> gpioInputStatusList = new List<SCData_PinInputConfig>();
        // this flag gets set if the pins and only the pins are dirty and whatever process
        // is monitoring them should take action
        bool gpioPinsAreDirty = false;

        // this flag gets set if we are dirty on the server and means the global data must be
        // transported to the client
        [NonSerialized]
        private bool isDirtyOnServer = false;
        // this flag gets set if we are dirty on the client and means the global data must be
        // transported to the server
        [NonSerialized]
        private bool isDirtyOnClient = false;

        // indicates if the probe should be allowed to rotate left or right, usually false (no inhibit)
        [NonSerialized]
        private bool probeRotationInhibit_Left = false;
        [NonSerialized]
        private bool probeRotationInhibit_Right = false;

        // these are effectively constants and should only be set at instantiation
        private uint probeRotationDirLeft = 0;
        private uint probeRotationDirRight = 1;

        // indicates if the probe rotation is wanted
        private bool probeRotationWanted = false;
        private uint probeRotationDir = 0;
        private uint probeRotationSteps = 0;

        // the list of IO states to implement IBehaviour_PinOutputControl
        private List<SCData_PinOutputConfig> outputIOStateList = new List<SCData_PinOutputConfig>();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// 
        /// </summary>
        /// <param name="operatingLocationIn">the operating location</param>
        public BehaviourStack_Ex012(BehaviourLocationEnum operatingLocationIn) : base(operatingLocationIn)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Implement a shallow clone for this behaviour
        /// 
        /// </summary>
        public override Behaviour_StateMachine ShallowClone()
        {
            BehaviourStack_Ex012 retObj = (BehaviourStack_Ex012)this.MemberwiseClone();
            // make sure the behaviour list is reset
            retObj.BehaviourList = new LinkedList<Behaviour_Base>();
            // make sure nothing can run
            retObj.WorkerThreadsOKToRun = false;
            return retObj;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Derived classes should implement a shallow copy which 
        /// updates the server variables if we are on the server and the client 
        /// variables if we are on the client
        /// 
        /// NOTE: We have to call the base class version in here as well in order
        ///       to acheive a proper copy
        /// 
        /// </summary>
        /// <param name="currentLocation">the current location</param>
        /// <param name="shallowClonedBehaviourStack">a shallow cloned behaviour stack</param>
        public override void CopyServerClientData(Behaviour_StateMachine shallowClonedBehaviourStack, BehaviourLocationEnum currentLocation)
        {
            if (shallowClonedBehaviourStack == null) return;
            // we cannot deal with other class types here 
            if((shallowClonedBehaviourStack is BehaviourStack_Ex012) == false) return;

            // do the base class data here, doesn't matter what platform we are on
            base.CopyServerClientData(shallowClonedBehaviourStack, currentLocation);

            // we know which variables need to be updated on which platform
            if (currentLocation == BehaviourLocationEnum.WALNUT_CLIENT)
            {
                // we are on the client, give it these variables

                // IBehaviour_MotorSpeeds implementation
                Speed_X = (shallowClonedBehaviourStack as BehaviourStack_Ex012).Speed_X;
                Speed_Y = (shallowClonedBehaviourStack as BehaviourStack_Ex012).Speed_Y;
                MaxSpeed_X = (shallowClonedBehaviourStack as BehaviourStack_Ex012).MaxSpeed_X;
                MaxSpeed_Y = (shallowClonedBehaviourStack as BehaviourStack_Ex012).MaxSpeed_Y;
                // IBehaviour_TargetPoint implementation
                TargetPoint = (shallowClonedBehaviourStack as BehaviourStack_Ex012).TargetPoint;
                // IBehaviour_TargetPointColor implementation
                TargetPointColor = (shallowClonedBehaviourStack as BehaviourStack_Ex012).TargetPointColor;
                // IBehaviour_SourcePoint implementation
                SourcePoint = (shallowClonedBehaviourStack as BehaviourStack_Ex012).SourcePoint;
                // IBehaviour_SourcePointColor implementation
                SourcePointColor = (shallowClonedBehaviourStack as BehaviourStack_Ex012).SourcePointColor;

                ProbeRotationWanted = (shallowClonedBehaviourStack as BehaviourStack_Ex012).ProbeRotationWanted;
                ProbeRotationDir = (shallowClonedBehaviourStack as BehaviourStack_Ex012).ProbeRotationDir;
                ProbeRotationSteps = (shallowClonedBehaviourStack as BehaviourStack_Ex012).ProbeRotationSteps;

        // ignore these
        // stepperList
        // minScreenX
        // minScreenY
        // maxScreenX
        // maxScreenY
        // sourcePointPixelColor
        // numTargetPointsFound_Valid = 0;
        // numTargetPointsFound_Empty = 0;
        // timeAtWhichLastValidTargetPointWasFound

    }
            else if (currentLocation == BehaviourLocationEnum.WALNUT_SERVER)
            {
                // we are on the server, the incoming data is from the client, give it these variables,
                foreach(SCData_PinInputConfig pinStatusObj in (shallowClonedBehaviourStack as BehaviourStack_Ex012).GPIOInputStatusList)
                {
                    if (pinStatusObj.Gpio == GpioEnum.GPIO_NONE) continue;
                    SetPinStatusByGPIO(pinStatusObj.Gpio, pinStatusObj.PinState);
                }
            }
            else { }
        }

        // IBehaviour_ScreenSize implementation
        public int MinScreenX
        {
            get
            {
                lock (lockObject)
                {
                    return minScreenX;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (minScreenX != value)
                    {
                        //Console.WriteLine("aaa isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    minScreenX = value;
                }
            }
        }
        public int MinScreenY
        {
            get
            {
                lock (lockObject)
                {
                    return minScreenY;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (minScreenY != value)
                    {
                        //Console.WriteLine("bbb isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    minScreenY = value;
                }
            }
        }
        public int MaxScreenX
        {
            get
            {
                lock (lockObject)
                {
                    return maxScreenX;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (maxScreenX != value)
                    {
                        //Console.WriteLine("ccc isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    maxScreenX = value;
                }
            }
        }
        public int MaxScreenY
        {
            get
            {
                lock (lockObject)
                {
                    return maxScreenY;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (maxScreenY != value)
                    {
                        //Console.WriteLine("ddd isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    maxScreenY = value;
                }
            }
        }

        // IBehaviour_MotorSpeeds implementation
        public uint Speed_X
        {
            get
            {
                lock (lockObject)
                {
                    return speed_X;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (speed_X != value)
                    {
                        //Console.WriteLine("eee isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    speed_X = value;
                }
            }
        }

        public uint Speed_Y
        {
            get
            {
                lock (lockObject)
                {
                    return speed_Y;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (speed_Y != value)
                    {
                        //Console.WriteLine("fff isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    speed_Y = value;
                }
            }
        }

        public uint MaxSpeed_X
        {
            get
            {
                lock (lockObject)
                {
                    return maxSpeed_X;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (maxSpeed_X != value)
                    {
                        //Console.WriteLine("ggg isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    maxSpeed_X = value;
                }
            }
        }

        public uint MaxSpeed_Y
        {
            get
            {
                lock (lockObject)
                {
                    return maxSpeed_Y;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (maxSpeed_Y != value)
                    {
                        //Console.WriteLine("hhh isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    maxSpeed_Y = value;
                }
            }
        }

        // IBehaviour_TargetPoint implementation
        public Point TargetPoint
        {
            get
            {
                lock (lockObject)
                {
                    return targetPoint;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (targetPoint.Equals(value) != true)
                    {
                        //Console.WriteLine("iii isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    targetPoint = value;
                }
            }
        }

        // IBehaviour_SourcePoint implementation
        public Point SourcePoint
        {
            get
            {
                lock (lockObject)
                {
                    return sourcePoint;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (sourcePoint.Equals(value) != true)
                    {
                        //Console.WriteLine("jjj isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    sourcePoint = value;
                }
            }
        }

        // IBehaviour_TargetPointColor implementation
        public Color TargetPointColor
        {
            get
            {
                lock (lockObject)
                {
                    return targetPointColor;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (targetPointColor.Equals(value) != true)
                    {
                        //Console.WriteLine("kkk isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    targetPointColor = value;
                }
            }
        }
        public Color TargetPointColorAlt
        {
            get
            {
                lock (lockObject)
                {
                    return targetPointColorAlt;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (targetPointColorAlt.Equals(value) != true)
                    {
                        //Console.WriteLine("lll isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    targetPointColorAlt = value;
                }
            }
        }

        // IBehaviour_SourcePointColor sample implementation
        public Color SourcePointColor
        {
            get
            {
                lock (lockObject)
                {
                    return sourcePointColor;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (sourcePointColor.Equals(value) != true)
                    {
                        //Console.WriteLine("mmm isDirtyOnServer change");
                        isDirtyOnServer = true; // record changes
                    }
                    sourcePointColor = value;
                }
            }
        }

        // IBehaviour_SourcePointLowestAlphaValueFoundOnMask sample implementation
        public byte SourcePointLowestAlphaValueFoundOnMask_Overlay
        {
            get
            {
                lock (lockObject)
                {
                    return sourcePointLowestAlphaValueFoundOnMask_Overlay;
                }
            }
            set
            {
                lock (lockObject)
                {
                    sourcePointLowestAlphaValueFoundOnMask_Overlay = value;
                }
            }
        }

        // IBehaviour_StepperList  implementation
        public List<SCData_Stepper> StepperList
        {
            get
            {
                lock (lockObject)
                {
                    return stepperList;
                }
            }
            set
            {
                lock (lockObject)
                {
                    // not checked for IsDirtyOnServer since only gets updated on client and used on client
                    stepperList = value;
                }
            }
        }

        // IBehaviour_TargetPointStatistics sample implementation
        // NOTE: we do not implment IsDirtyOnServer here because the client does not care about this value
        public int NumTargetPointsFound_Valid
        {
            get
            {
                lock (lockObject)
                {
                    return numTargetPointsFound_Valid;
                }
            }
            set
            {
                lock (lockObject)
                {
                    numTargetPointsFound_Valid = value;
                }
            }
        }
        public int NumTargetPointsFound_Empty
        {
            get
            {
                lock (lockObject)
                {
                    return numTargetPointsFound_Empty;
                }
            }
            set
            {
                lock (lockObject)
                {
                    numTargetPointsFound_Empty = value;
                }
            }
        }

        public DateTime TimeAtWhichLastValidTargetPointWasFound
        {
            get
            {
                lock (lockObject)
                {
                    return timeAtWhichLastValidTargetPointWasFound;
                }
            }
            set
            {
                lock (lockObject)
                {
                    timeAtWhichLastValidTargetPointWasFound = value;
                }
            }
        }

        // IBehaviour_IsDirtyOnServer implementation
        public bool IsDirtyOnServer { get => isDirtyOnServer; set => isDirtyOnServer = value; }
        // IBehaviour_IsDirtyOnClient implementation
        public bool IsDirtyOnClient { get => isDirtyOnClient; set => isDirtyOnClient = value; }
        // IBehaviour_GPIO_InputStates implementation
        public List<SCData_PinInputConfig> GPIOInputStatusList { get => gpioInputStatusList;  }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the value he list of input status objects using a specified GPIO
        /// We just return the first one we find. There should be no duplicates!
        /// 
        /// </summary>
        /// <param name="gpioIn">the gpio</param>
        /// <returns> the pin state true or false, false for fail</returns>
        public bool GetPinStatusByGPIO(GpioEnum gpioIn)
        {
            if (GPIOInputStatusList == null) return false;
            lock (lockObject)
            {
                foreach (SCData_PinInputConfig pinStatus in GPIOInputStatusList)
                {
                    if (pinStatus.Gpio == gpioIn) return pinStatus.PinState;
                }
            }
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Indicates if pins are dirty or not. This means have changed since 
        /// the last time some monitoring process checked
        /// 
        /// </summary>
        public bool GPIOPinsAreDirty
        {
            get
            {
                return gpioPinsAreDirty;
            }
            set
            {
                lock (lockObject)
                {
                    gpioPinsAreDirty = value;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Set the value he list of input status objects using a specified GPIO
        /// We just update the first one we find. There should be no duplicates!
        /// 
        /// </summary>
        /// <param name="gpioIn">the gpio</param>
        /// <param name="pinStateIn">the pin state</param>
        public void SetPinStatusByGPIO(GpioEnum gpioIn, bool pinStateIn)
        {
           // Console.WriteLine("SetPinStatusByGPIO gpioIn="+ gpioIn.ToString() + ", state="+ pinStateIn.ToString());
            if (GPIOInputStatusList == null) return;
            lock (lockObject)
            {
                foreach (SCData_PinInputConfig pinStatus in GPIOInputStatusList)
                {
                    if (pinStatus.Gpio == gpioIn)
                    {
                        if (pinStatus.PinState != pinStateIn)
                        {
                            pinStatus.PinState = pinStateIn;
                            // flag it as changed
                            //Console.WriteLine("zzz isDirtyOnClient change");
                            GPIOPinsAreDirty = true;
                            // on the server this does nothing, on the client it transmits the changes to the server
                            IsDirtyOnClient = true;
                        }
                    }
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// indicates if the probe should be allowed to rotate left, usually false (no inhibit)
        /// 
        /// This is the IBehaviour_ProbeRotationControl implementation
        /// </summary>
        public bool ProbeRotationInhibit_Left
        {
            get
            {
                return probeRotationInhibit_Left;
            }
            set
            {
                lock (lockObject)
                {
                    probeRotationInhibit_Left = value;
                }
            }
        }
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// indicates if the probe should be allowed to rotate right, usually false (no inhibit)
        /// 
        /// This is the IBehaviour_ProbeRotationControl implementation
        /// </summary>
        public bool ProbeRotationInhibit_Right
        {
            get
            {
                return probeRotationInhibit_Right;
            }
            set
            {
                lock (lockObject)
                {
                    probeRotationInhibit_Right = value;
                }
            }
        }
        public bool ProbeRotationWanted
        {
            get
            {
                return probeRotationWanted;
            }
            set
            {
                lock (lockObject)
                {
                    probeRotationWanted = value;
                }
            }
        }

        public uint ProbeRotationDir
        {
            get
            {
                return probeRotationDir;
            }
            set
            {
                lock (lockObject)
                {
                    probeRotationDir = value;
                }
            }
        }

        public uint ProbeRotationSteps
        {
            get
            {
                return probeRotationSteps;
            }
            set
            {
                lock (lockObject)
                {
                    probeRotationSteps = value;
                }
            }
        }

        public uint ProbeRotationDirLeft { get => probeRotationDirLeft; set => probeRotationDirLeft = value; }
        public uint ProbeRotationDirRight { get => probeRotationDirRight; set => probeRotationDirRight = value; }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the list which contains all of our pin states. Will never return 
        /// null.
        /// 
        /// NOTE: this also implements IBehaviour_PinOutputControl
        /// </summary>
        public List<SCData_PinOutputConfig> PinStateList_Output
        {
            get
            {
                if (outputIOStateList == null) outputIOStateList = new List<SCData_PinOutputConfig>();
                return outputIOStateList;
            }
            set
            {
                outputIOStateList = value;
                if (outputIOStateList == null) outputIOStateList = new List<SCData_PinOutputConfig>();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we have the specified GPIO in our list
        /// 
        /// NOTE: this also implements IBehaviour_PinOutputControl
        /// </summary>
        /// <param name="gpioIn">the Gpio</param>
        public bool HasGPIO(GpioEnum gpioIn)
        {
            if (gpioIn == GpioEnum.GPIO_NONE) return false;

            foreach (SCData_PinOutputConfig cfgObj in PinStateList_Output)
            {
                if (cfgObj.Gpio == gpioIn) return true;
            }
            return false;
       }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the output state for a pin based on the GPIO
        /// 
        /// NOTE: this also implements IBehaviour_PinOutputControl
        /// </summary>
        /// <param name="gpioIn">the Gpio</param>
        /// <param name="pinStateIn">the pin state</param>
        public void SetPinOutputStateByGPIO(GpioEnum gpioIn, bool pinStateIn)
        {
            if (gpioIn == GpioEnum.GPIO_NONE) return;

            foreach (SCData_PinOutputConfig cfgObj in PinStateList_Output)
            {
                if (cfgObj.Gpio == gpioIn)
                {
                    cfgObj.PinState = pinStateIn;
                    break;
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the ignore state for a pin based on the GPIO
        /// 
        /// NOTE: this also implements IBehaviour_PinOutputControl
        /// </summary>
        /// <param name="gpioIn">the Gpio</param>
        /// <param name="lastPinStateIn">the last pin state</param>
        public void SetPinLastTransmittedPinStateByGPIO(GpioEnum gpioIn, bool lastPinStateIn)
        {
            if (gpioIn == GpioEnum.GPIO_NONE) return;

            foreach (SCData_PinOutputConfig cfgObj in PinStateList_Output)
            {
                if (cfgObj.Gpio == gpioIn)
                {
                    cfgObj.LastTransmittedPinState = lastPinStateIn;
                    break;
                }
            }
        }

    }
}
