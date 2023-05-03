using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using OISCommon;
using BBBCSIO;
using WalnutCommon;

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

namespace WalnutBBBClient
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main class for the application, the data transport mechanism
    /// for this class is derived from the RemCon project
    /// http://www.ofitselfso.com/RemCon/RemCon.php
    /// </summary>
    public class MainClass : OISObjBase
    {
        private const string DEFAULTLOGDIR = @"/home/devuser/Dump/ProjectLogs";
        private const string APPLICATION_NAME = "WalnutBBBClient";
        private const string APPLICATION_VERSION = "00.02.02";

        // this handles the data transport to and from the server 
        private TCPDataTransporter dataTransporter = null;

        // ###
        // ### these are the offsets into the data store we pass into the PRU
        // ### each data item is a uint (it is simpler that way)
        // ###

        // this is what controls the PRU
        private PRUDriver pruDriver = null;

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
            // this is a last resort. All execeptions should really be trapped
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
        /// application execution starts (other than the constructer above).
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

            // we sit and wait for the user to press return. The handler is dealing with the responses
            Console.WriteLine("Press <Return> to quit");
            Console.ReadLine();

            ShutDown();
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
            Console.WriteLine("inbound data received:  Data=" + scData.ToString());

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
                // the remote side has connected
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
            if (scData==null)
            {
                LogMessage("SetWaldosFromServerClientData, scData==null");
                return;
            }

            //Console.WriteLine("SetWaldosFromServerClientData Message Received");
            LogMessage("SetWaldosFromServerClientData Message Received");

            // are we dealing with stepper0 data?
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
                //Console.WriteLine("sscData.Step0_Enable=" + scData.Step0_Enable.ToString());
                //Console.WriteLine("scData.Step0_StepSpeed=" + scData.Step0_StepSpeed.ToString());
                //Console.WriteLine("scData.Step0_DirState=" + scData.Step0_DirState.ToString());

                // write the semaphore. This must come last, the code running in the 
                // PRU will see this change and set things up according to the
                // other configuration items above
                pruDriver.WritePRUDataUInt32(1, SEMAPHORE_OFFSET);
            }

            // are we dealing with rectangle data
            if (scData.UserDataContent.HasFlag(UserDataContentEnum.RECT_DATA))
            {
            }

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

        }
    }
}
