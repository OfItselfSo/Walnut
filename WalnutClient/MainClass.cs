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
/// Black. It is designed to interact with a PASM assembly language based program running on 
/// PRU1 to control external devices. In practice this means using the i/o system to 
/// control up to six stepper motors. PWMs and other I/O can also be used. 
/// 
/// The global control direction information is sent to this application by a Windows
/// based Server (Walnut) in the form of an instantiated class (WalnutCommon.ServerClientData.cs)
/// If your main interest is the transfer of an instantiated object full of 
/// information via TCP/IP then you should probably see the RemCon project
/// http://www.OfItselfSo.com/RemCon which is a demonstrator project set up for that
/// purpose. The WalnutCommon code in this application is directly derived from the 
/// RemConCommon sample code. 
/// 
/// This application interacts with Windows based server software named Walnut.
/// Both this client and the server are designed to interact with each other. There is 
/// nothing "generic" about either of them. This is also true of the code which runs 
/// in the PRU1 - it is specific to PRU1..
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
/// WalnutBBB-00A0.dts file for more information. There is information on configuring
/// the uEnv.txt file in the link below:
/// http://www.ofitselfso.com/BeagleNotes/Beaglebone_Black_And_Device_Tree_Overlays.php
/// 
/// WARNING: In order to get sufficient pins operational in the PRU1, the 
/// Beaglebone Black must run headless and without eMMC memory. The supplied
/// WalnutBBB-00A0.dts overlay will interfere with both of these sub-systems. See
/// http://www.ofitselfso.com/BeagleNotes/Disabling_Video_On_The_Beaglebone_Black_And_Running_Headless.php
/// http://www.ofitselfso.com/BeagleNotes/Disabling_The_EMMC_Memory_On_The_Beaglebone_Black.php
/// 
/// The PASMCode directory contains a selection of "test" PASM assembly files
/// used to develop the PRU1_Waldo_IO.p code in this project. They have
/// been left in the though that they may serve as simple examples of PASM code.
/// The PASM assembler can be obtained online and a compiled version from
/// https://github.com/OfItselfSo/PASM_Assembler

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
        private const string APPLICATION_VERSION = "00.02.01";

        // this handles the data transport to and from the server 
        private TCPDataTransporter dataTransporter = null;

        // ###
        // ### these are the offsets into the data store we pass into the PRU
        // ### each data item is a uint (it is simpler that way)
        // ###


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

            // Start the Waldos
            StartWaldosWithDefaults();

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

            // what type of data is it
            if (scData.DataContent == ServerClientDataContentEnum.USER_DATA)
            {
                // user content
                LogMessage("ServerClientDataEventHandler dataStr=" + scData.DataStr + ", Data=" + scData.ToString());
                Console.WriteLine("inbound data received:  dataStr=" + scData.DataStr + ", Data=" + scData.ToString());

                // send the data to the PRU
                //SetWaldosFromServerClientData(scData);

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
                LogMessage("ServerClientDataEventHandler REMOTE_CONNECT");
                Console.WriteLine("ServerClientDataEventHandler REMOTE_CONNECT");
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_DISCONNECT)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_DISCONNECT");
                Console.WriteLine("ServerClientDataEventHandler REMOTE_DISCONNECT");
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
            if(scData==null)
            {
                LogMessage("SetWaldosFromServerClientData, scData==null");
                return;
            }
            Console.WriteLine("SetWaldosFromServerClientData Message Received");
            LogMessage("SetWaldosFromServerClientData Message Received");
            //LogMessage("SetWaldosFromServerClientData Message Received scData.PWM0_Enable=" + scData.PWM0_Enable.ToString() + ", scData.PWM0_PWMDutyCycle=" + scData.PWM0_PWMDutyCycle.ToString() + ", pwmPort0A.RunState=" + pwmPort0A.RunState.ToString());
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts the Waldos
        /// 
        /// </summary>
        public void StartWaldosWithDefaults()
        {
            LogMessage("StartWaldosWithDefaults called");


            LogMessage("StartWaldosWithDefaults Waldos now running.");
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

            // shutdown the Waldos
         }
    }
}
