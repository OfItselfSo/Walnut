using BBBCSIO;
using OISCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using WalnutCommon;
using System.Threading;
using System.Xml.Linq;

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

/// This application acts as a command line based client which runs on the Beaglebone
/// Black. It is designed to interact via TCP/IP with software running on a Windows PC.
/// 
/// The global control direction information is sent to this application by a Windows
/// based Server (Walnut) in the form of an instantiated class (WalnutCommon.ServerClientData.cs)
/// 
/// If your main interest is the transfer of an instantiated object full of 
/// information via TCP/IP then you should probably see the RemCon project
/// http://www.OfItselfSo.com/RemCon which is a demonstrator project set up for that
/// purpose. The WalnutCommon code in this application is partly derived from the 
/// RemConClient sample code. 
/// 
/// If your main interest is the control of stepper motors in the PRU's of a 
/// BeagleboneBlack then you should probably see the Tilo project
/// http://www.OfItselfSo.com/Tilo which is a demonstrator project set up for that
/// purpose. The WalnutCommon code in this application is partly derived from the 
/// TiloClient sample code.
/// 
/// Ultimately, the purpose of this code is to output control signals for the FPath 
/// Waldos. This PRU code for this functionality is present in the PRU1_Waldo_IO.p The 
/// Post Build event of this project compiles up the PRU1_Waldo_IO.p file using the 
/// PASM.exe file and ensures it is present in the output directory. The PASM.exe
/// executable is available from https://github.com/OfItselfSo/PASM_Assembler
/// 
/// The PRU1_Waldo_IO.p is designed to be able to run up to 6 stepper motors 
/// simultaneously. It will send pulses at the specified stable rate irregardless
/// of how many motors are operational. In other words, if you set a 1Hz train
/// of pulses going on stepper 0 that is the stable frequency you will get even if 
/// other motors are taken on and off line in while it is running.
/// 
/// This software uses the BBBCSIO library to start the program in the PRU and to
/// pass the incoming pulse and direction information to it so the pulses can be
/// generated. http://www.OfItselfSo.com/BBBCSIO
/// 
/// This library requires the UIO Drivers to be enabled so that the PRU can be 
/// accessed. The link below provides information on this topic.
/// http://www.OfItselfSo.com/BeagleNotes/Enabling_the_UIO_Drivers_on_the_Beaglebone_Black.php
/// 
/// In order to produce output, an overlay will need to be configured in the 
/// /boot/uEnv.txt file of the Beaglebone Black otherwise the pin states changed
/// by the PRU program simply will not be visible on the P8 or P9 headers. The
/// Pins used are necessarily hard coded into the PRU1_Waldo_IO.p program. A 
/// suitable overlay is included with this source code repository in the DTS
/// directory. See the readme.txt file in that directory and the comments in the 
/// Waldo-00A0.dts file for more information. There is information on configuring
/// the uEnv.txt file in the link below:
/// http://www.ofitselfso.com/BeagleNotes/Beaglebone_Black_And_Device_Tree_Overlays.php
/// 
/// WARNING: In order to get sufficient pins operational in the PRU1, the 
/// Beaglebone Black must run headless and without eMMC memory. The supplied
/// WalnutBBB-00A0.dts overlay will interfere with both of these sub-systems. See
/// http://www.ofitselfso.com/BeagleNotes/Disabling_Video_On_The_Beaglebone_Black_And_Running_Headless.php
/// http://www.ofitselfso.com/BeagleNotes/Disabling_The_EMMC_Memory_On_The_Beaglebone_Black.php
/// 

namespace WalnutClient
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main class for the application
    /// </summary>
    public class MainClass : OISObjBase
    {
        private const string DEFAULTLOGDIR = @"/home/devuser/Dump/ProjectLogs";
        private const string APPLICATION_NAME = "WalnutClient";
        private const string APPLICATION_VERSION = "00.02.06";

        // this handles the data transport to and from the server 
        private TCPDataTransporter dataTransporter = null;

        // this is what controls the PRU
        private PRUDriver pruDriver = null;

        // This is what controls the PWM ports
        private const int DEFAULT_PWM_FREQ = 1000;
        // we map PwmPortA on the incoming data to this BBB pwm port
        private const PWMPortEnum PWMPORT_A = PWMPortEnum.PWM0_A;
        // we map PwmPortB on the incoming data to this BBB pwm port
        private const PWMPortEnum PWMPORT_B = PWMPortEnum.PWM0_B;
        // we map PwmPortA Direction to this gpio
        private const GpioEnum PWMPORTDIR_A = GpioEnum.GPIO_15;
        // we map PwmPortB Direction to this gpio
        private const GpioEnum PWMPORTDIR_B = GpioEnum.GPIO_49;

        // the pwm port A
        private PWMPortFS pwmPortA = null;
        private OutputPortMM pwmPortADir = null;

        // the pwm port B
        private PWMPortFS pwmPortB = null;
        private OutputPortMM pwmPortBDir = null;

        // ###
        // ### these are the offsets into the data store we pass into the PRU
        // ### each data item is a uint (it is simpler that way)
        // ###

        // our semaphore flag is stored at this offset
        private const uint SEMAPHORE_OFFSET = 0;
        // the all steppers enabled flag is stored at this offset
        private const uint WALDO_ENABLE_OFFSET = 4;

        // STEP0
        private const uint STEP0_ENABLED_OFFSET = 8;     // 0 disabled, 1 enabled
        private const uint STEP0_FULLCOUNT = 12;         // this is the count we reset to when we toggle
        private const uint STEP0_DIRSTATE = 16;          // this is the state of the direction pin

        // STEP1
        private const uint STEP1_ENABLED_OFFSET = 20;     // 1 disabled, 1 enabled
        private const uint STEP1_FULLCOUNT = 24;          // this is the count we reset to when we toggle
        private const uint STEP1_DIRSTATE = 28;           // this is the state of the direction pin

        // STEP2
        private const uint STEP2_ENABLED_OFFSET = 32;     // 1 disabled, 1 enabled
        private const uint STEP2_FULLCOUNT = 36;          // this is the count we reset to when we toggle
        private const uint STEP2_DIRSTATE = 40;           // this is the state of the direction pin

        // STEP3
        private const uint STEP3_ENABLED_OFFSET = 44;     // 1 disabled, 1 enabled
        private const uint STEP3_FULLCOUNT = 48;          // this is the count we reset to when we toggle
        private const uint STEP3_DIRSTATE = 52;           // this is the state of the direction pin

        // STEP4
        private const uint STEP4_ENABLED_OFFSET = 56;     // 1 disabled, 1 enabled
        private const uint STEP4_FULLCOUNT = 60;          // this is the count we reset to when we toggle
        private const uint STEP4_DIRSTATE = 64;           // this is the state of the direction pin

        // STEP5
        private const uint STEP5_ENABLED_OFFSET = 68;     // 1 disabled, 1 enabled
        private const uint STEP5_FULLCOUNT = 72;          // this is the count we reset to when we toggle
        private const uint STEP5_DIRSTATE = 76;           // this is the state of the direction pin

        // ###
        // ### this is the END of the data items, we need to allocate space for the 
        // ### above number of UINTS
        // ###
        private const int NUM_DATA_UINTS = 20;

        // in this version the software only cares about two squares. These contain the
        // discovered information regarding the squares
        private MarkedObject redSquare = new MarkedObject();
        private MarkedObject greenSquare = new MarkedObject();

        // this is the behaviour. It gets instantiated at the class level because it is 
        // NOT stateless. It remembers past red and green square coordinate values and
        // the last outputs (speed and dir) it returned and makes decisions based on them
        private Behaviour_MoveClose behaviourMoveClose = null;
        private const uint MAX_STEPPER_SPEED = 200;

        // this is the level behaviour. It gets instantiated at the class level because it is 
        // NOT stateless. It remembers past red and green square coordinate values and
        // the last outputs (speed and dir) it returned and makes decisions based on them
        private Behaviour_MoveLevel behaviourMoveLevelX = null;
        private Behaviour_MoveLevel behaviourMoveLevelY = null;
        private const uint MAX_MOTOR_SPEED = 100;

        // this is a point tracker. It gets instantiated at the class level because it is 
        // NOT stateless. It remembers past N points it is presented with and can detect
        // things like movment etc
        private Behaviour_TrackTarget behaviourTrackTarget = null;
        private const float DEFAULT_TARGET_MOVED_THRESHOLD = 10;
        private const int DEFAULT_TARGET_QUEUE_SIZE = 5;

        // we can skip over missing src points if we wish to do so. This copes with missing
        // data. 
        private const int MAX_MISSING_SRC_POINTS = 5;
        private int numMissingSrcPoints = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public MainClass()
        {
            bool retBOOL = false;

            Console.WriteLine(APPLICATION_NAME + " started");

            // set the current directory equal to the exe directory. We do this because
            // people can start from a link and if the start-in directory is not right
            // it can put the log file in strange places
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // set up the Singleton g_Logger instance. Simply using it in a test
            // creates it.
            if (g_Logger == null)
            {
                // did not work, nothing will start say so now in a generic way
                Console.WriteLine("Logger Class Failed to Initialize. Nothing will work well.");
                return;
            }

            // Register the global error handler as soon as we can in Main
            // to make sure that we catch as many exceptions as possible
            // this is a last resort. All exceptions should really be trapped
            // and handled by the code.
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            // set up our logging
            retBOOL = g_Logger.InitLogging(DEFAULTLOGDIR, APPLICATION_NAME, false, true);
            if (retBOOL == false)
            {
                // did not work, nothing will start say so now in a generic way
                Console.WriteLine("The log file failed to create. No log file will be recorded.");
            }

            // pump out the header
            g_Logger.EmitStandardLogfileheader(APPLICATION_NAME);
            LogMessage("");
            LogMessage("Version: " + APPLICATION_VERSION);
            LogMessage("");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Called out of the program main() function. This is where all of the 
        /// application execution starts (other than the constructor above).
        /// </summary>
        public void BeginProcessing()
        {
            Console.WriteLine(APPLICATION_NAME + " processing begins");

            // set up our data transporter
            dataTransporter = new TCPDataTransporter(TCPDataTransporterModeEnum.TCPDATATRANSPORT_CLIENT, WalnutConstants.SERVER_TCPADDR, WalnutConstants.SERVER_PORT_NUMBER);
            // set up the event so the data transporter can send us the data it recevies
            dataTransporter.ServerClientDataEvent += ServerClientDataEventHandler;

            // Start the PRU
            StartPRUWithDefaults(PRUEnum.PRU_1);

            // start the PWM ports
            StartPWMPortA();
            StartPWMPortB();

            // we sit and wait for the user to press return. The handler is dealing with the responses
            Console.WriteLine("Press <Return> to quit");
            Console.ReadLine();

            ShutDown();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts and configures the PWM A port
        /// </summary>
        private void StartPWMPortA()
        {
            pwmPortA = new PWMPortFS(PWMPORT_A);
            // we use FrequencyHz
            pwmPortA.FrequencyHz = DEFAULT_PWM_FREQ;
            // we set the DutyPercent so it is always low
            pwmPortA.DutyPercent = 0;
            // set the run state off
            pwmPortA.RunState = false;
            // set up the direction GPIO pin
            pwmPortADir = new OutputPortMM(PWMPORTDIR_A);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts and configures the PWM B port
        /// </summary>
        private void StartPWMPortB()
        {
            pwmPortB = new PWMPortFS(PWMPORT_B);
            // we use FrequencyHz
            pwmPortB.FrequencyHz = DEFAULT_PWM_FREQ;
            // we set the DutyPercent so it is always low
            pwmPortB.DutyPercent = 0;
            // set the run state off
            pwmPortB.RunState = false;
            // set up the direction GPIO pin
            pwmPortBDir = new OutputPortMM(PWMPORTDIR_B);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles inbound data events
        /// </summary>
        /// <param name="scData">the server client data object</param>
        private void ServerClientDataEventHandler(object sender, ServerClientData scData)
        {
            if (scData == null)
            {
                LogMessage("ServerClientDataEventHandler scData==null");
                Console.WriteLine("ServerClientDataEventHandler scData==null");
                return;
            }

            LogMessage("ServerClientDataEventHandler: Data=" + scData.ToString());
            //  Console.WriteLine("inbound data received:  Data=" + scData.ToString());

            // what type of data is it
            if (scData.DataContent == ServerClientDataContentEnum.USER_DATA)
            {
                // user content

                // send the data to the PRU
                SetWaldosFromServerClientData(scData);

                // for the purposes of demonstration, send an ack now
                if (dataTransporter == null)
                {
                    LogMessage("ServerClientDataEventHandler dataTransporter==null");
                    Console.WriteLine("ServerClientDataEventHandler dataTransporter==null");
                    return;
                }

                // send it
                ServerClientData ackData = new ServerClientData("ACK from client to server");
                dataTransporter.SendData(ackData);
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_CONNECT)
            {
                // the remote side has connected
                //LogMessage("ServerClientDataEventHandler REMOTE_CONNECT");
                //  Console.WriteLine("ServerClientDataEventHandler REMOTE_CONNECT");
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_DISCONNECT)
            {
                // the remote side has disconnected
                // LogMessage("ServerClientDataEventHandler REMOTE_DISCONNECT");
                // Console.WriteLine("ServerClientDataEventHandler REMOTE_DISCONNECT");
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A function to send data received from the server over to the 
        /// code running in the PRU
        /// 
        /// </summary>
        /// <param name="scData">the server client data object</param>
        public void SetWaldosFromServerClientData(ServerClientData scData)
        {
            // sanity check
            if (scData == null)
            {
                LogMessage("SetWaldosFromServerClientData, scData==null");
                return;
            }

            //Console.WriteLine("SetWaldosFromServerClientData Message Received");
            LogMessage("SetWaldosFromServerClientData Message Received");

            // are we dealing with raw request stepper0 data?
            if (scData.UserDataContent.HasFlag(UserDataContentEnum.STEP0_DATA))
            {
                // write the waldo_enable flag
                pruDriver.WritePRUDataUInt32(scData.Waldo_Enable, WALDO_ENABLE_OFFSET);

                // write the STEP0 enable/disable flag
                pruDriver.WritePRUDataUInt32(scData.Step0_Enable, STEP0_ENABLED_OFFSET);
                // write the STEP0 fullcount value
                pruDriver.WritePRUDataUInt32(scData.Step0_StepSpeed, STEP0_FULLCOUNT);
                // write the STEP0 direction flag
                pruDriver.WritePRUDataUInt32(scData.Step0_DirState, STEP0_DIRSTATE);
                Console.WriteLine("scData.Waldo_Enable=" + scData.Waldo_Enable.ToString());

                // write the semaphore. This must come last, the code running in the 
                // PRU will see this change and set things up according to the
                // other configuration items above
                pruDriver.WritePRUDataUInt32(1, SEMAPHORE_OFFSET);
            }

            // are we dealing with PWMA data?
            if (scData.UserDataContent.HasFlag(UserDataContentEnum.PWMA_DATA))
            {
                if (pwmPortA != null)
                {
                    Console.WriteLine("PWMA DATA PWMA_Enable=" + scData.PWMA_Enable.ToString() + ",DutyPercent="+ scData.PWMA_PWMPercent.ToString() + ", Dir="+ scData.PWMA_DirState.ToString());

                    // set the pwmPercent, the frequency was set to a constant when the port was opened
                    pwmPortA.DutyPercent = scData.PWMA_PWMPercent;
                    // now enable or disable as appropriate
                    if (scData.PWMA_Enable != 0) pwmPortA.RunState = true;
                    else pwmPortA.RunState = false;
                    // set the direction
                    if(scData.PWMA_DirState != 0) pwmPortADir.Write(true);
                    else pwmPortADir.Write(false);
                }
            }

            // are we dealing with PWMB data?
            if (scData.UserDataContent.HasFlag(UserDataContentEnum.PWMB_DATA))
            {
                if (pwmPortB != null)
                {
                    Console.WriteLine("PWMB DATA PWMB_Enable=" + scData.PWMB_Enable.ToString() + ",DutyPercent=" + scData.PWMB_PWMPercent.ToString() + ", Dir=" + scData.PWMB_DirState.ToString());

                    // set the pwmPercent, the frequency was set to a constant when the port was opened
                    pwmPortB.DutyPercent = scData.PWMB_PWMPercent;
                    // now enable or disable as appropriate
                    if (scData.PWMB_Enable != 0) pwmPortB.RunState = true;
                    else pwmPortB.RunState = false;
                    // set the direction
                    if (scData.PWMB_DirState != 0) pwmPortBDir.Write(true);
                    else pwmPortBDir.Write(false);
                }
            }

            // are we dealing with rectangle data, this is data that has come off
            // the image recognition algorythm in the Walnut Server
            if (scData.UserDataContent.HasFlag(UserDataContentEnum.RECT_DATA))
            {
                // sanity check
                if (scData.RectList == null)
                {
                    Console.WriteLine("Null rectList with content flag RECT_DATA");
                    LogMessage("SetWaldosFromServerClientData, Null rectList with content flag RECT_DATA");
                    return;
                }
                // this is the action at the moment, we have the centerpoint of the squares and the 
                // known color of the centerpoint. We have to set this information in objects
                // so we can use it
                IdentifySquaresByColor(scData.RectList);
                // now we move the red square to the green square. This is a stated goal
                //MoveRedToGreen();
            }

            // are we dealing with srcTgt data, this is data that has come off
            // decided on by the Walnut Server
            if (scData.UserDataContent.HasFlag(UserDataContentEnum.SRCTGT_DATA))
            {
                // sanity check
                if (scData.SrcTgtList == null)
                {
                    Console.WriteLine("Null srcTgtList with content flag SRCTGT_DATA");
                    LogMessage("SetWaldosFromServerClientData, Null srcTgtList with content flag SRCTGT_DATA");
                    return;
                }
                if (scData.SrcTgtList.Count == 0)
                {
                    Console.WriteLine("zero len srcTgtList with content flag SRCTGT_DATA");
                    LogMessage("SetWaldosFromServerClientData, zero len srcTgtList with content flag SRCTGT_DATA");
                    // send in a dummy, there is code in the called procedure to handle this
                    MoveSourceToTarget(new SrcTgtData());
                    return;
                }
                else
                {
                    // now we move the source to the target with the green square. This is a stated goal
                    MoveSourceToTarget(scData.SrcTgtList[0]);
                }
            }

            // are we dealing with a flag
            if (scData.UserDataContent.HasFlag(UserDataContentEnum.FLAG_DATA))
            {
                if (scData.UserFlag.HasFlag(UserDataFlagEnum.MARK_FLAG))
                {
                    Console.WriteLine("MARK_FLAG");
                    LogMessage("SetWaldosFromServerClientData, MARK_FLAG");
                }
                if (scData.UserFlag.HasFlag(UserDataFlagEnum.EXIT_FLAG))
                {
                    Console.WriteLine("EXIT_FLAG");
                    LogMessage("SetWaldosFromServerClientData, EXIT_FLAG");
                    ShutDown();
                    // force a quit
                    Environment.Exit(0);
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A Waldo action to identify squares by color. At the moment we just use
        /// the first one we find.
        /// 
        /// </summary>
        /// <param name="rectList">The list of rectangles</param>
        private void IdentifySquaresByColor(List<ColoredRotatedObject> rectList)
        {
            // run through the list to find the first red square
            foreach (ColoredRotatedObject rectObj in rectList)
            {
                if (rectObj.ObjColor != KnownColor.Red) continue;
                redSquare.SetCenterLocation(rectObj.Center);
                break;
            }
            // run through the list to find the first green square
            foreach (ColoredRotatedObject rectObj in rectList)
            {
                if (rectObj.ObjColor != KnownColor.Green) continue;
                greenSquare.SetCenterLocation(rectObj.Center);
                break;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A Waldo action to move the red rectangle as close to the green rectangle
        /// as possible. We invoke the Behaviour_MoveClose() behaviour in order to
        /// help us do this
        /// 
        /// </summary>
        private void MoveRedToGreen()
        {
            uint outSpeed = 0;
            uint outDirection = 0;

            // this is a test action. The Red square is assumed to be on a rotating 
            // turntable and the green square is stationary. We will turn the table
            // so that the red square is as close to the green square as possible and 
            // then stop.

            if (redSquare.IsValid() == false)
            {
                Console.WriteLine("No Red Square Data");
                return;
            }
            if (greenSquare.IsValid() == false)
            {
                Console.WriteLine("No Green Square Data");
                return;
            }

            // note the center locations will have been rounded to the nearest int
            // when stored in the Marked Object. There will be no decimals here
            PointF redCenter = redSquare.CenterLocation;
            PointF greenCenter = greenSquare.CenterLocation;

            float redCenterX = redCenter.X;
            float redCenterY = redCenter.Y;
            float greenCenterX = greenCenter.X;
            float greenCenterY = greenCenter.Y;

            // do we have both, if not do nothing, the motor could still be 
            // turning at this point but we deal with so much noise we cannot 
            // stop every time there is dodgy data
            if (redCenterX < 0) return;
            if (redCenterY < 0) return;
            if (greenCenterX < 0) return;
            if (greenCenterY < 0) return;

            // set up our behaviour if we need to
            if (behaviourMoveClose == null) behaviourMoveClose = new Behaviour_MoveClose(MAX_STEPPER_SPEED);
            // get the result 
            int retVal = behaviourMoveClose.GetOutput(greenCenter, redCenter, out outSpeed, out outDirection);

            // write the STEP0 enable/disable flag
            pruDriver.WritePRUDataUInt32(1, WALDO_ENABLE_OFFSET);
            // write the STEP0 speed value
            pruDriver.WritePRUDataUInt32(WalnutCommon.Utils.ConvertHzToCycles((uint)outSpeed), STEP0_FULLCOUNT);
            // write the direction state
            pruDriver.WritePRUDataUInt32(outDirection, STEP0_DIRSTATE);
            pruDriver.WritePRUDataUInt32(1, STEP0_ENABLED_OFFSET);
            pruDriver.WritePRUDataUInt32(1, SEMAPHORE_OFFSET);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A Waldo action to move the source onto the target
        /// 
        /// </summary>
        private void MoveSourceToTarget(SrcTgtData stData)
        {
            uint outSpeedX = 0;
            uint outDirectionX = 0;
            uint outSpeedY = 0;
            uint outDirectionY = 0;
            PointF srcCenter = new PointF(float.NaN, float.NaN);
            PointF tgtCenter = new PointF(float.NaN, float.NaN);


            // set up our behaviours if we need to
            if (behaviourMoveLevelX == null) behaviourMoveLevelX = new Behaviour_MoveLevel(AxisEnum.AXIS_X, MAX_MOTOR_SPEED);
            if (behaviourMoveLevelY == null) behaviourMoveLevelY = new Behaviour_MoveLevel(AxisEnum.AXIS_Y, MAX_MOTOR_SPEED);
            // set up our target tracker if we need to
            int workingTargetMovedThreshold = 5;
            if (behaviourTrackTarget == null) behaviourTrackTarget = new Behaviour_TrackTarget(workingTargetMovedThreshold, DEFAULT_TARGET_QUEUE_SIZE);

            if (stData == null)
            {
                Console.WriteLine("No stData");
                // turn off the ports
                pwmPortA.RunState = false;
                pwmPortB.RunState = false;
                // and leave
                return;
            }

            Console.WriteLine("*(" + stData.SrcPoint.X.ToString() + "," + stData.SrcPoint.Y.ToString() + ")" + " (" + stData.TgtPoint.X.ToString() + "," + stData.TgtPoint.Y.ToString() + ")");

            if ((stData == null) || (stData.SrcIsPopulated() == false))
            {
                // we do not have source data
                Console.WriteLine("No Src Data");
                // turn off the ports
                pwmPortA.RunState = false;
                pwmPortB.RunState = false;
                // and leave
                return;
            }

            // do we have a target
            if (stData.TgtIsPopulated() == false)
            {
                Console.WriteLine("No incoming Tgt Data");
                // turn off the ports
                pwmPortA.RunState = false;
                pwmPortB.RunState = false;
                // and leave
                return;
            }
            srcCenter = stData.SrcPoint;
            tgtCenter = stData.TgtPoint;
            // feed our target tracker with the static point. We want to be able to see if it has moved
            behaviourTrackTarget.SetTargetPoint(stData.TgtPoint);

        // tmp   if (behaviourTrackTarget.HasMoved() == true)
            if ( true)
            {
                // reset the MoveLevel behaviour
                behaviourMoveLevelX.Reset();
                behaviourMoveLevelY.Reset();
                // reset the target point tracker
                behaviourTrackTarget.Reset();
                behaviourTrackTarget.SetTargetPoint(tgtCenter);
            }

            // Process X, have we already reached a point where we can stop?
            if (behaviourMoveLevelX.CanStop() == true)
            {
                // turn off the port
                pwmPortA.RunState = false;
            }
            else
            {
                // can't stop, process this input
                // get the result for X direction
                int retVal = behaviourMoveLevelX.GetOutput(tgtCenter, srcCenter, out outSpeedX, out outDirectionX);
                if (retVal != 0)
                {
                    pwmPortA.RunState = false;
                    return;
                }
                else
                {
                    // set the pwmPercent, the frequency was set to a constant when the port was opened
                    pwmPortA.DutyPercent = (double)outSpeedX;

                    // set the direction
                    if (outDirectionX != 0) pwmPortADir.Write(true);
                    else pwmPortADir.Write(false);

                    // now enable the port
                    pwmPortA.RunState = true;
                }
            }

            // Process Y, have we already reached a point where we can stop?
            if (behaviourMoveLevelY.CanStop() == true)
            {
                // turn off the port
                pwmPortB.RunState = false;
            }
            else
            {
                // can't stop, process this input
                // get the result for Y direction
                int retVal = behaviourMoveLevelY.GetOutput(tgtCenter, srcCenter, out outSpeedY, out outDirectionY);
                if (retVal != 0)
                {
                    pwmPortB.RunState = false;
                    return;
                }
                else
                {
                    // set the pwmPercent, the frequency was set to a constant when the port was opened
                    pwmPortB.DutyPercent = (double)outSpeedY;

                    // set the direction
                    if (outDirectionY != 0) pwmPortBDir.Write(true);
                    else pwmPortBDir.Write(false);

                    // now enable the port
                    pwmPortB.RunState = true;
                }
            }
            // write this out for diagnostics
            Console.WriteLine("");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts the PRU with the  PRU1_StepperIO binary. Very specific to the 
        /// data needs of the PRU1_StepperIO binary
        /// 
        /// In this function, the PASM binary to load is hard coded. It is designed
        /// to monitor incoming data from the client and control step and dir pins
        /// for up to six stepper motors.
        /// 
        /// </summary>
        /// <param name="pruID">The pruID</param>
        public void StartPRUWithDefaults(PRUEnum pruID)
        {

            // this is the array we use to pass in the data to the PRU. The
            // single byte will act as a toggle flag. Because we are only
            // transmitting a byte (an atomic value) we do not need to set
            // up any complicated semaphore system.

            // The size of this array is the number of 
            byte[] dataBytes = new byte[NUM_DATA_UINTS * sizeof(UInt32)];

            // sanity checks
            if (pruID == PRUEnum.PRU_NONE)
            {
                throw new Exception("No such PRU: " + pruID.ToString());
            }
            string binaryToRun = "./PRU1_Waldo_IO.bin";

            // build the driver
            pruDriver = new PRUDriver(pruID);

            // initialize the dataBytes array. the PRU code expects to see a 
            // zero semaphore, and enable flags when it starts
            Array.Clear(dataBytes, 0, dataBytes.Length);

            // run the binary, pass in our initial array
            pruDriver.ExecutePRUProgram(binaryToRun, dataBytes);

            Console.WriteLine("PRU now running.");
            LogMessage("PRU now running.");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This is where we handle exceptions
        /// </summary>
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// We perform all shutdown operations in here
        /// </summary>
        public void ShutDown()
        {
            // shut down the data transporter
            if (dataTransporter != null)
            {
                dataTransporter.Shutdown();
                dataTransporter = null;
            }

            // shutdown the PRU driver
            if (pruDriver != null)
            {
                pruDriver.PRUStop();
                pruDriver.Dispose();
                pruDriver = null;
            }

            // shutdown the PWMPorts
            if (pwmPortA != null)
            {
                // we are done, stop running
                if(pwmPortA.RunState==true) pwmPortA.RunState = false;

                // close the port
                pwmPortA.ClosePort();
                pwmPortA.Dispose();
            }
            if (pwmPortB != null)
            {
                // we are done, stop running
                if (pwmPortB.RunState == true) pwmPortB.RunState = false;

                // close the port
                pwmPortB.ClosePort();
                pwmPortB.Dispose();
            }
            if (pwmPortADir != null)
            {
                // close the port
                pwmPortADir.ClosePort();
                pwmPortADir.Dispose();
            }
            if (pwmPortBDir != null)
            {
                // close the port
                pwmPortBDir.ClosePort();
                pwmPortBDir.Dispose();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tests a PWM Port (and presumably a servo attached to that port)
        /// by changing the pulse width duty cycle to 25%, 50% and 75% and
        /// back again. Once that is done, the pulse width is changed over the
        /// same range but much more smoothly. 
        /// 
        /// NOTE: 
        ///   This code assumes that the PWM device has been configured in the 
        ///   device tree. If it is not, then the PWM Port will probably not
        ///   be available. Usually this is done by adding the 
        ///   overlay to the uEnv.txt file. The Beaglebone Black and Device Tree Overlays
        ///   technical note has more information.
        /// 
        /// http://www.ofitselfso.com/BeagleNotes/Beaglebone_Black_And_Device_Tree_Overlays.php
        ///
        /// Any of the entries below placed in the /boot/uEnv.txt file will enable the 
        /// specified PWM ports. You may need to edit the .dts source if you only want A and 
        /// not B etc.
        /// uboot_overlay_addr4=/lib/firmware/BB-PWM0-00A0.dtbo (PWM0_A & PWM0_B)
        /// uboot_overlay_addr5=/lib/firmware/BB-PWM1-00A0.dtbo (PWM1_A & PWM1_B)
        /// uboot_overlay_addr6=/lib/firmware/BB-PWM2-00A0.dtbo (PWM2_A & PWM2_B)
        ///   
        ///   You should conduct the usual tests to make sure the PWM header pin
        ///   you wish to use is available and not in use by anything else on 
        ///   the Beaglebone Black PinMux. 
        /// 
        /// NOTE:
        ///    Be aware that although there are 12 pins available for PWM output
        ///    on the Beaglebone Black P8 and P9 headers in many cases two pins
        ///    share the same PWM output. In addition, in many cases two PWM 
        ///    outputs share the same PWM module. P8/P9 Header pins that 
        ///    share the same PWM output will always have identical signals in
        ///    frequency, pulse width and timing. They _are_ the same signal!
        /// 
        ///    PWM Outputs that share the same PWM OCP device MUST have the same
        ///    frequency but can have independent pulse widths. The trigger
        ///    timing of the start of the high part of pulse waveform is 
        ///    simultaneous.
        /// 
        ///    PWM Name         H8/H9 Pins          PWMDevice_Output
        ///    PWM0_A,        PWM_P9_22_or_P9_31     (EHRPWM0_A)
        ///    PWM0_B,        PWM_P9_21_or_P9_29     (EHRPWM0_B)    
        ///                 
        ///    PWM1_A,        PWM_P9_14_or_P8_36     (EHRPWM1_A)
        ///    PWM1_B,        PWM_P9_16_or_P8_34     (EHRPWM1_B)
        /// 
        ///    PWM2_A,        PWM_P8_19_or_P8_45     (EHRPWM2_A)
        ///    PWM2_B,        PWM_P8_13_or_P8_46     (EHRPWM2_B)
        /// 
        ///    This is IMPORTANT so I will say it again. If you configure 
        ///    two PWM outputs in the same device (PWM1_A, PWM1_B for example) 
        ///    then they MUST be configured with the same frequency. 
        /// 
        ///    They will use the same frequency anyways and if
        ///    you change one you change the other. Changing the frequency on the
        ///    B output will instantly change the frequency on the A output and
        ///    will really mess up any pulse widths/duty cycles the A output is using
        /// 
        ///    Always set the frequency first then the Pulse Width/duty cycle. The pulse 
        ///    width is calculated from whatever frequency is currently set it is
        ///    not adjusted if the frequency is later changed.
        ///
        /// From the above information, if you connect servos to both P9_14 and 
        /// P8_36 you will see the servos behave identically. If you connect
        /// servos to P9_14 and P9_16 the frequency must be identical (because
        /// the are both on PWM module EHRPWM1 but the pulse widths can be
        /// independently controlled. If you connect servos to P9_22 and
        /// P9_14 you can have distinct frequencies and pulse widths because
        /// the two PWM devices are fully independent.
        /// 
        /// </summary>
        /// <param name="pwmID">The pwmID</param>
        /// <history>
        ///    07 Mar 19  Cynic - Originally written
        /// </history>
        public void SimpleTestPWM_FS(PWMPortEnum pwmID)
        {
            const uint DEFAULT_PERIOD_NS = 250000;
            const uint DEFAULT_DUTY_50PERCENT = (uint)(DEFAULT_PERIOD_NS * (0.5));
            const uint DEFAULT_DUTY_75PERCENT = (uint)(DEFAULT_PERIOD_NS * (0.75));
            const uint DEFAULT_DUTY_25PERCENT = (uint)(DEFAULT_PERIOD_NS * (0.25));

            // open the port
            PWMPortFS pwmPort = new PWMPortFS(pwmID);

            // set the PWM waveform period
            pwmPort.PeriodNS = DEFAULT_PERIOD_NS;
            // we could also use FrequencyHz which does the same thing
            // pwmPort.FrequencyHz = 4000;

            // set the PWM waveform Duty Cycle
            pwmPort.DutyNS = DEFAULT_DUTY_50PERCENT;
            // we could also use DutyPercent which does the same thing
            // pwmPort.DutyPercent = 50;

            // set the run state to begin the output of the PWM waveform
            pwmPort.RunState = true;

            Console.WriteLine("PeriodNS is: " + pwmPort.PeriodNS.ToString());
            Console.WriteLine("DutyNS is: " + pwmPort.DutyNS.ToString());
            Console.WriteLine("FrequencyHz is: " + pwmPort.FrequencyHz.ToString());
            Console.WriteLine("DutyPercent is: " + pwmPort.DutyPercent.ToString());
            Console.WriteLine("RunState is: " + pwmPort.RunState.ToString());

            // change the PWM duty cycle (i.e. rotate the servo)
            // until we get a key press on the console
            while (true)
            {
                // first we rotate the servo in steps
                Console.WriteLine("");
                Console.WriteLine("Now Step Rotating Servo");

                // set the Duty Cycle low 
                pwmPort.DutyNS = DEFAULT_DUTY_25PERCENT;
                if (Console.KeyAvailable == true) break;
                Console.WriteLine("DutyPercent is: " + pwmPort.DutyPercent.ToString());
                Thread.Sleep(1000);
                // set the Duty Cycle midway
                pwmPort.DutyNS = DEFAULT_DUTY_50PERCENT;
                if (Console.KeyAvailable == true) break;
                Console.WriteLine("DutyPercent is: " + pwmPort.DutyPercent.ToString());
                Thread.Sleep(1000);
                // set the Duty Cycle high
                pwmPort.DutyNS = DEFAULT_DUTY_75PERCENT;
                if (Console.KeyAvailable == true) break;
                Console.WriteLine("DutyPercent is: " + pwmPort.DutyPercent.ToString());
                Thread.Sleep(1000);
                // set the Duty Cycle midway
                pwmPort.DutyNS = DEFAULT_DUTY_50PERCENT;
                if (Console.KeyAvailable == true) break;
                Console.WriteLine("DutyPercent is: " + pwmPort.DutyPercent.ToString());
                Thread.Sleep(1000);

                Console.WriteLine("");
                Console.WriteLine("Now Smoothly Rotating Servo");

                // now we rotate the servo smoothly from
                // 25.123% to 75.456% using a totally arbitrary increment
                for (float i = 25.123f; i < 75.345f; i = i + 0.678f)
                {
                    pwmPort.DutyPercent = i;
                    if (Console.KeyAvailable == true) break;
                    Console.WriteLine("DutyPercent is: " + pwmPort.DutyPercent.ToString());
                    Thread.Sleep(50);
                }
            }

            // we are done, stop running
            pwmPort.RunState = false;

            // close the port
            pwmPort.ClosePort();
            pwmPort.Dispose();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Produces a series of 1 sec pulses on a GPIO port. Memory mapped version
        /// 
        /// NOTE: 
        ///   This code assumes that the port associated with the gpioID has been 
        ///   properly configured in the device tree as an output. If it is not
        ///   then the output may not work correctly. See the associated documentation
        /// 
        /// NOTE:
        ///    Be aware of the BBB output voltage and max output current. In general
        ///    you cannot directly switch any meaningful output device. You have to
        ///    run it through a transistor or some other current/voltage amplifier.
        /// 
        /// </summary>
        /// <param name="gpioID">The gpioID</param>
        /// <history>
        ///    28 Aug 14  Cynic - Originally written
        /// </history>
        public void SimplePulsePortMM(GpioEnum gpioID)
        {
            // open the port
            OutputPortMM outPort = new OutputPortMM(gpioID);

            // run until we have a keypress on the console
            while (Console.KeyAvailable == false)
            {
                // put the port low
                outPort.Write(false);
                // sleep for half a second
                Thread.Sleep(500);
                // put the port high
                outPort.Write(true);
                // sleep for half a second
                Thread.Sleep(500);
            }
            // close the port
            outPort.ClosePort();
            outPort.Dispose();
        }
    }
}
