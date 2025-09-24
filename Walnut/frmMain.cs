using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MediaFoundation;
using MediaFoundation.ReadWrite;
using MediaFoundation.Misc;
using MediaFoundation.Transform;
using MediaFoundation.Alt;
using MediaFoundation.EVR;
using OISCommon;
using TantaCommon;
using WalnutCommon;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Text.RegularExpressions;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MediaFoundation.OPM;
using Emgu.CV;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Reflection;


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

/// The function of this app is to interact with a BeagleBone Black (BBB) which is controlling a remote robotics system.
/// This app performs the image recognition and high level path planning functions and communicates to the BBB which 
/// handles the low level path planning and realtime stepper motor and robotic controls.
/// 
/// The image of the objects being operated on is streamed the screen.
/// 
/// The screen can be recorded to disk.
/// 
/// A Windows Media Foundation (WMF) transform injects logging and run information into the stream at the bottom each frame.
/// 
/// A WMF transform in this app makes calls to EmguCV code to identify the contents of the image 
/// and generate positional data of the various components. 
/// 
/// The positional information is made available to the application and decisions regarding the path of the 
/// robotic end effectors can be made. 
/// 
/// The end location of the path and way points are communicated to the BBB. Alternately, the coordinates of 
/// various image recognised components are communicated to the BBB and it knows what to do with them
/// 
/// The BBB moves the end effector to the location via the end points.
/// 
/// Normally the speed and direction of stepper motors controlled by the BBB is left up to the code running on the 
/// WalnutClient. However, this program can force a stepper to turn on at a specific speed and direction 
///
/// If your main interest is the transfer of an instantiated object full of 
/// information via TCP/IP then you should probably see the RemCon project
/// http://www.OfItselfSo.com/RemCon which is a demonstrator project set up for that
/// purpose. The Walnut Server code in this application is partly derived from the 
/// RemConClient sample code. 
///
/// If your main interest is the use of Windows Media Foundation to intercept a video stream and 
/// modify it and make it available for processing then you should probably see the Tanta project
/// http://www.OfItselfSo.com/Tanta which is a demonstrator project set up for this
/// purpose. The Walnut Server code in this application is partly derived from the 
/// Tanta sample code. 

/// SUPER IMPORTANT NOTE: You MUST use the [MTAThread] to decorate your entry point method. If you use the default [STAThread] 
/// you may get errors - WMF requires this. See the Program.cs file for details.

/// If your main interest is the use of EmguCV to process a video stream for image recognition
/// then you should probably see the Prism project http://www.OfItselfSo.com/Prism which is a demonstrator 
/// project set up for this purpose. The Walnut Server code in this application is partly derived from the 
/// Prism sample code. 


namespace Walnut
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main form for the application
    /// </summary>
    public partial class frmMain : frmOISBase
    {
        private const string DEFAULTLOGDIR = @"C:\Dump\Project Logs";
        private const string APPLICATION_NAME = "Walnut";
        private const string APPLICATION_VERSION = "00.02.09";
        private const int DEFAULT_RUN_NUMBER = 0;
        private const int DEFAULT_REC_NUMBER = 0;
        private const string RUN_NUMBER_MARKER = "##";
        private const string REC_NUMBER_MARKER = "$$";
        // default run info, use RUN_NUMBER_MARKER to include run marker 
        private const string DEFAULT_RUN_NAME = "FPath Sample"+ " "+ RUN_NUMBER_MARKER;

        private const string START_CAPTURE = "Start Capture";
        private const string STOP_CAPTURE = "Stop Capture";
        private const string RECORDING_IS_ON = "Recording is ON";
        private const string RECORDING_IS_OFF = "Recording is OFF";

        private const string DEFAULT_VIDEO_DEVICE = "USB camera";
//        private const string DEFAULT_VIDEO_DEVICE = "HD Pro Webcam C920";
        private const string DEFAULT_VIDEO_FORMAT = "YUY2";
        private const int DEFAULT_VIDEO_FRAME_WIDTH = 640;
        private const int DEFAULT_VIDEO_FRAME_HEIGHT = 480;
//        private const int DEFAULT_VIDEO_FRAMES_PER_SEC = 10;
        private const int DEFAULT_VIDEO_FRAMES_PER_SEC = 30;

        private const string DEFAULT_SOURCE_DEVICE = @"<No Video Device Selected>";

        private const string DEFAULT_CAPTURE_DIRNAME = @"D:\Dump\FPathData";
        // default capture filename, use RUN_NUMBER_MARKER to include run marker in name
        // use REC_NUMBER_MARKER to include rec marker in name
        private const string DEFAULT_CAPTURE_FILENAME = @"WalnutCapture_" + RUN_NUMBER_MARKER + "-" + REC_NUMBER_MARKER+".mp4";

        // the call back handler for the mediaSession
        private TantaAsyncCallbackHandler mediaSessionAsyncCallbackHandler = null;

        // A session provides playback controls for the media content. The Media Session and the protected media path (PMP) session objects 
        // expose this interface. This interface is the primary interface that applications use to control the Media Foundation pipeline.
        // In this app we want the copy to proceed as fast as possible so we do not implement any of the usual session control items.
        protected IMFMediaSession mediaSession;

        // Media sources are objects that generate media data. For example, the data might come from a video file, a network stream, 
        // or a hardware device, such as a camera. Each media source contains one or more streams, and each stream delivers 
        // data of one type, such as audio or video.
        protected IMFMediaSource mediaSource;

        // The Enhanced Video Renderer(EVR) implements this interface and it controls how the EVR presenter displays video.
        // The EVR also a sink but we do not really use it as one - that functionality is largely internal to the pipeline.
        // we only get access to this object once the topology has been resolved. We still have to release it though!
        protected IMFVideoDisplayControl evrVideoDisplay;

        // we are using a custom transform to intercept the information as it moves through the
        // pipeline. If recording is enabled, it takes a copy of the media samples and then presents 
        // this data to a SinkWriter to be saved. This is an IMFTransform
        protected MFTTantaSampleGrabber_Sync sampleGrabberTransform = null;

        // if we are using a text overlay transform (as a binary) this will be non-null
        protected IMFTransform textOverlayTransform = null;

        // if we are using an image overlay transform (as a binary) this will be non-null
        protected IMFTransform imageOverlayTransform = null;

        // if we are using an image recognition transform (as a binary) this will be non-null
        protected MFTDetectHorizLines recognitionTransform = null;

        // this is the current type of the video stream. We need this to set up the sink writer
        // properly. This must be released at the end
        protected IMFMediaType currentVideoMediaType = null;

        // our thread safe screen update delegate
        public delegate void ThreadSafeScreenUpdate_Delegate(object obj, bool captureIsActive, string displayText);

        // these are settings the user does not explicitly configure such as form size
        // or some boolean screen control states
        private ApplicationImplicitSettings implictUserSettings = null;
        //// these are settings the user configures 
        //private ApplicationExplicitSettings explictUserSettings = null;

        // the worker that recognises the screen data
        BackgroundWorker codeWorker = null;
        //private const int CODEWORKER_UPDATE_TIME_MSEC = 1000;
        private const int CODEWORKER_UPDATE_TIME_MSEC = 50;

        // this handles the data transport to and from the client 
        private TCPDataTransporter dataTransporter = null;
        //private bool inhibitAutoSend = false;

        //private const int DEFAULT_STEPPER_SPEED_HZ = 60;
        private const int DEFAULT_STEPPER_SPEED_HZ = 200;
        private const int STEPPER_SPEED_1HZ = 1;
        private const int DEFAULT_STEPPER_DIR = 0;

        // used for diagnostics message speed testing
   //     DateTime diagnosticStartTime = DateTime.Now;
   //     int diagnosticMessageCount = 0;
        const int MAX_DIAGNOSTIC_MESSAGE_COUNT = 100;
        //   private const string OVERLAY_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex004\Line1.png";
        //  private const string OVERLAY_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex004\WavePath1.png";
        private const string OVERLAY_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex005\AllTransparent.png";
        // private const string OVERLAY_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex005\CircleLowLeft.png";
        //  private const string OVERLAY_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex005\SmallGreenDot_LL.png";
        private const string TRACKER_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex004\AllTransparent640x480.png";
        //private const string TRACKER_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex005\Rectangle.png";
       // private const string TRACKER_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex005\WavePath1.png";
        //private const string OVERLAY_IMAGE_FILENAME = @"D:\Dump\FPathData\Walnut_003\CirclePath.png";
        // private const string OVERLAY_IMAGE_FILENAME = @"D:\Dump\FPathData\FPath_Ex004\AllGreen640x480.png";

        // this is the color the overlay image paths are drawn in
        private static Color OVERLAY_TARGET_COLOR = Color.FromArgb(255, 0, 255, 0);
        private static Color TRACKER_COLOR = Color.Cyan;
        private static int DEFAULT_TARGET_COLOR_ARGB = Color.FromArgb(255, 0, 255, 0).ToArgb();
        private static int ALT_TARGET_COLOR_ARGB = TRACKER_COLOR.ToArgb();

        // this is the current color we are tracking
        private int currentTargetColorARGB = DEFAULT_TARGET_COLOR_ARGB;
        private int countOfDefaultTargetPixelsFound = 0;

        // some pens and brushes we use
        private Pen blackPen = new Pen(Color.Black);
        private Pen trackerPen = new Pen(TRACKER_COLOR);
        private SolidBrush blackBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
        private SolidBrush trackerBrush = new SolidBrush(TRACKER_COLOR);
        // make a transparent white brush, note this has an alpha channel of 0
        private SolidBrush whiteTransparentBrush = new SolidBrush(Color.FromArgb(0, 255, 255, 255));

        // some colors
        private const string HTML_GREEN = "#ff00ff00";
        private const string HTML_RED = "#ffff0000";

        private Point lastGreenPoint = new Point();

        // we have the ability to draw virtual object on the screen this is the data for them
        private int greenCircleDrawCount = 0;    // if +ve we draw a green circle on the mouse click point and decrement
        private int redLineDrawCount = 0;        // if +ve we draw a red line on the mouse click point and decrement

        private const int SMALL_CIRCLE_DIAMETER_IN_PIXELS = 23;
        private const int LARGE_CIRCLE_DIAMETER_IN_PIXELS = 37;

        // set this up to detect the colors
        private const uint DEFAULT_GRAY_DETECTION_RANGE = 15;
        private ColorDetector colorDetectorObj = new ColorDetector(DEFAULT_GRAY_DETECTION_RANGE);
        // used to draw crosses on objects
        public const int DEFAULT_CENTROID_CROSS_BAR_LEN = 10;


        private const int PATH_FOLLOW_MIN_POINTS_NEEDED = 3;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        public frmMain()
        {
            bool retBOOL = false;
            HResult hr = 0;

            if (DesignMode == false)
            {
                // set the current directory equal to the exe directory. We do this because
                // people can start from a link 
                Directory.SetCurrentDirectory(Application.StartupPath);

                // set up the Singleton g_Logger instance. Simply using it in a test
                // creates it.
                if (g_Logger == null)
                {
                    // did not work, nothing will start say so now in a generic way
                    MessageBox.Show("Logger Class Failed to Initialize. Nothing will work well.");
                    return;
                }
                // record this in the logger for everybodys use
                g_Logger.ApplicationMainForm = this;
                g_Logger.DefaultDialogBoxTitle = APPLICATION_NAME;
                try
                {
                    // set the icon for this form and for all subsequent forms
                    g_Logger.AppIcon = new Icon(GetType(), "App.ico");
                    this.Icon = new Icon(GetType(), "App.ico");
                }
                catch (Exception)
                {
                }

                // Register the global error handler as soon as we can in Main
                // to make sure that we catch as many exceptions as possible
                // this is a last resort. All execeptions should really be trapped
                // and handled by the code.
                OISGlobalExceptions ex1 = new OISGlobalExceptions();
                Application.ThreadException += new ThreadExceptionEventHandler(ex1.OnThreadException);

                // set the culture so our numbers convert consistently
                System.Threading.Thread.CurrentThread.CurrentCulture = g_Logger.GetDefaultCulture();

            }

            InitializeComponent();

            if (DesignMode == false)
            {

                // set up our logging
                retBOOL = g_Logger.InitLogging(DEFAULTLOGDIR, APPLICATION_NAME, false);
                if (retBOOL == false)
                {
                    // did not work, nothing will start say so now in a generic way
                    MessageBox.Show("The log file failed to create. No log file will be recorded.");
                }
                // pump out the header
                g_Logger.EmitStandardLogfileheader(APPLICATION_NAME);
                LogMessage("");
                LogMessage("Version: " + APPLICATION_VERSION);
                LogMessage("");

                // a bit of setup
                buttonStartStopCapture.Text = START_CAPTURE;
                textBoxPickedVideoDeviceURL.Text = DEFAULT_SOURCE_DEVICE;
                buttonRecordingOnOff.Text = RECORDING_IS_OFF;
                SyncScreenControlsToCaptureState(false, null);
                textBoxCaptureDirName.Text = DEFAULT_CAPTURE_DIRNAME;
                textBoxCaptureFileName.Text = DEFAULT_CAPTURE_FILENAME;
                RunInfoStr = DEFAULT_RUN_NAME;
                RunNumberAsInt = DEFAULT_RUN_NUMBER;
                RecNumberAsInt = DEFAULT_REC_NUMBER;

                // we always have to initialize MF. The 0x00020070 here is the WMF version 
                // number used by the MF.Net samples. Not entirely sure if it is appropriate
                hr = MFExtern.MFStartup(0x00020070, MFStartup.Full);
                if (hr != 0)
                {
                }

                // set up our Video Picker Control
                ctlTantaVideoPicker1.VideoDevicePickedEvent += new ctlTantaVideoPicker.VideoDevicePickedEventHandler(VideoDevicePickedHandler);
                ctlTantaVideoPicker1.VideoFormatPickedEvent += new ctlTantaVideoPicker.VideoFormatPickedEventHandler(VideoFormatPickedHandler);

                // now recover the last configuration settings - if saved, we only do this if 
                // the shift key is not pressed. This allows the user to start with the
                // Shift key pressed and reset to defaults
                if ((Control.ModifierKeys & Keys.Shift) == 0)
                {
                    try
                    {
                        implictUserSettings = new ApplicationImplicitSettings();
                        try
                        {
                            // we do not want to trigger user activated events when setting things
                            // up on startup
                            //suppressUserActivatedEvents = true;
                            // if we got here the above lines did not fail
                            MoveImplicitUserSettingsToScreen();
                        }
                        finally
                        {
                            //suppressUserActivatedEvents = false;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The form loaded handler
        /// </summary>
        private void frmMain_Load(object sender, EventArgs e)
        {
            // Set up the Walnut Controls
            SetupWalnutControls();

            try
            {
                // enumerate all video devices and display their formats
                ctlTantaVideoPicker1.DisplayVideoCaptureDevices();

                ctlTantaEVRStreamDisplay1.InitMediaPlayer();
            }
            catch (Exception ex)
            {
                // something went wrong
                MessageBox.Show("An error occurred\n\n" + ex.Message + "\n\nPlease see the logs");
            }

            // init the video picker
            ctlTantaVideoPicker1.ChooseCurrentDeviceByFriendlyName(DEFAULT_VIDEO_DEVICE);
            TantaMFVideoFormatContainer videoFormatCont = ctlTantaVideoPicker1.ChooseCurrentFormatByFormat(DEFAULT_VIDEO_FORMAT, DEFAULT_VIDEO_FRAME_WIDTH, DEFAULT_VIDEO_FRAME_HEIGHT, DEFAULT_VIDEO_FRAMES_PER_SEC);
            // trigger the change event manually
            VideoFormatPickedHandler(this, videoFormatCont);

            try
            {
                LogMessage("frmMain_Load Setting up the Data Transporter");

                // set up our data transporter
                dataTransporter = new TCPDataTransporter(TCPDataTransporterModeEnum.TCPDATATRANSPORT_SERVER, WalnutConstants.SERVER_TCPADDR, WalnutConstants.SERVER_PORT_NUMBER);
                // set up the event so the data transporter can send us the data it recevies
                dataTransporter.ServerClientDataEvent += ServerClientDataEventHandler;
                LogMessage("frmMain_Load Data Transporter Setup complete");
            }
            catch (Exception ex)
            {
                LogMessage("frmMain_Load exception: " + ex.Message);
                LogMessage("frmMain_Load exception: " + ex.StackTrace);
                OISMessageBox("Exception setting up the data transporter: " + ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The form closing handler
        /// </summary>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // do everything to close all media devices
                CloseAllMediaDevices();

                // Shut down MF
                MFExtern.MFShutdown();

                // put the non user specified configuration settings in place now
                SetImplicitUserSettings();

                // we always save implicit settings on close, unless the Shift key is pressed
                if ((Control.ModifierKeys & Keys.Shift) == 0)
                {
                    ImplicitUserSettings.Save();
                }
            }
            catch
            {
            }

            ShutdownDataTransporter();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the output filename and path. Will never return null, will return ""
        /// There is no set accessor, This is obtained off the screen.
        /// </summary>
        public string OutputFileNameAndPath
        {
            get
            {
                return Path.Combine(CaptureDirName, CaptureFileName.Replace(RUN_NUMBER_MARKER, RunNumberAsInt.ToString()).Replace(REC_NUMBER_MARKER, RecNumberAsInt.ToString()));
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the capture filename. Never returns null or empty
        /// </summary>
        public string CaptureFileName
        {
            get
            {
                if (textBoxCaptureFileName.Text == null) textBoxCaptureFileName.Text = DEFAULT_CAPTURE_FILENAME;
                if (textBoxCaptureFileName.Text.Length==0) textBoxCaptureFileName.Text = DEFAULT_CAPTURE_FILENAME;
                return textBoxCaptureFileName.Text;
            }
            set
            {
                textBoxCaptureFileName.Text = value;
                if (textBoxCaptureFileName.Text == null) textBoxCaptureFileName.Text = DEFAULT_CAPTURE_FILENAME;
                if (textBoxCaptureFileName.Text.Length == 0) textBoxCaptureFileName.Text = DEFAULT_CAPTURE_FILENAME;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the capture dirname. Never returns null or empty
        /// </summary>
        public string CaptureDirName
        {
            get
            {
                if (textBoxCaptureDirName.Text == null) textBoxCaptureDirName.Text = DEFAULT_CAPTURE_DIRNAME;
                if (textBoxCaptureDirName.Text.Length == 0) textBoxCaptureDirName.Text = DEFAULT_CAPTURE_DIRNAME;
                return textBoxCaptureDirName.Text;
            }
            set
            {
                textBoxCaptureDirName.Text = value;
                if (textBoxCaptureDirName.Text == null) textBoxCaptureDirName.Text = DEFAULT_CAPTURE_DIRNAME;
                if (textBoxCaptureDirName.Text.Length == 0) textBoxCaptureDirName.Text = DEFAULT_CAPTURE_DIRNAME;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the full capture directory path and filename
        /// </summary>
        public string CaptureFileNameAndPath
        {
            get
            {
                return Path.Combine(CaptureDirName, CaptureFileName);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the implicit user config settings object. Will never get or set null
        /// </summary>
        public ApplicationImplicitSettings ImplicitUserSettings
        {
            get
            {
                if (implictUserSettings == null) implictUserSettings = new ApplicationImplicitSettings();
                return implictUserSettings;
            }
            set
            {
                implictUserSettings = value;
                if (implictUserSettings == null) implictUserSettings = new ApplicationImplicitSettings();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Moves the implicit configuration settings from settings file to the screen
        /// </summary>
        private void MoveImplicitUserSettingsToScreen()
        {
            // implicit settings
            this.Size = ImplicitUserSettings.FormSize;
            CaptureDirName = ImplicitUserSettings.LastCaptureDirectory;
            CaptureFileName = ImplicitUserSettings.LastCaptureFileName;
            RunInfoStr = ImplicitUserSettings.LastRunName;
            RunNumberAsInt = ImplicitUserSettings.LastRunNumber;
            GreenCircleRadius = ImplicitUserSettings.LastGreenCircleRadius;
            GreenCircleDrawMouseClicks = ImplicitUserSettings.LastGreenCircleDrawMouseClicks;
//            GreenCircleCenterPoint = ImplicitUserSettings.LastGreenCircleCenterPoint;

            CalibratedPixelsPerMicron = ImplicitUserSettings.CalibratedPixelsPerMicron;

            // experiment 008 settings
            textBoxStepperControl008NumSteps.Text = ImplicitUserSettings.Ex008NumSteps;
            textBoxStepperControl008StepsPerSecond.Text = ImplicitUserSettings.Ex008StepsPerSecond;
            if (ImplicitUserSettings.Ex008DirIsCW == true) radioButtonStepperControl008DirCW.Checked = true;
            else radioButtonStepperControl008DirCCW.Checked = true;
            if ((ImplicitUserSettings.Ex008ColorDetectHorizTop!=null) && (ImplicitUserSettings.Ex008ColorDetectHorizTop.Length>0)) textBoxEx008ColorDetectHorizTop.Text = ImplicitUserSettings.Ex008ColorDetectHorizTop;
            if ((ImplicitUserSettings.Ex008ColorDetectHorizBot != null) && (ImplicitUserSettings.Ex008ColorDetectHorizBot.Length > 0)) textBoxEx008ColorDetectHorizBot.Text = ImplicitUserSettings.Ex008ColorDetectHorizBot;
            if ((ImplicitUserSettings.Ex008ColorDetectMinPixelsHoriz != null) && (ImplicitUserSettings.Ex008ColorDetectMinPixelsHoriz.Length > 0))  textBoxEx008ColorDetectMinPixelsHoriz.Text = ImplicitUserSettings.Ex008ColorDetectMinPixelsHoriz;
            if ((ImplicitUserSettings.Ex008ColorDetectVertTop != null) && (ImplicitUserSettings.Ex008ColorDetectVertTop.Length > 0)) textBoxEx008ColorDetectVertTop.Text = ImplicitUserSettings.Ex008ColorDetectVertTop;
            if ((ImplicitUserSettings.Ex008ColorDetectVertBot != null) && (ImplicitUserSettings.Ex008ColorDetectVertBot.Length > 0)) textBoxEx008ColorDetectVertBot.Text = ImplicitUserSettings.Ex008ColorDetectVertBot;
            if ((ImplicitUserSettings.Ex008ColorDetectMinPixelsVert != null) && (ImplicitUserSettings.Ex008ColorDetectMinPixelsVert.Length > 0)) textBoxEx008ColorDetectMinPixelsVert.Text = ImplicitUserSettings.Ex008ColorDetectMinPixelsVert;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the form settings which the user does not really specify. These
        /// are things like form size etc.
        /// </summary>
        private void SetImplicitUserSettings()
        {
            ImplicitUserSettings.FormSize = this.Size;
            ImplicitUserSettings.LastCaptureDirectory = CaptureDirName;
            ImplicitUserSettings.LastCaptureFileName = CaptureFileName;
            ImplicitUserSettings.LastRunName = RunInfoStr;
            ImplicitUserSettings.LastRunNumber = RunNumberAsInt;
            ImplicitUserSettings.LastGreenCircleRadius = GreenCircleRadius;
            ImplicitUserSettings.LastGreenCircleDrawMouseClicks = GreenCircleDrawMouseClicks;
//            ImplicitUserSettings.LastGreenCircleCenterPoint = GreenCircleCenterPoint;

            ImplicitUserSettings.CalibratedPixelsPerMicron = CalibratedPixelsPerMicron;

           // experiment 008 settings
            ImplicitUserSettings.Ex008NumSteps = textBoxStepperControl008NumSteps.Text;
            ImplicitUserSettings.Ex008StepsPerSecond = textBoxStepperControl008StepsPerSecond.Text;
            if (radioButtonStepperControl008DirCW.Checked == true) ImplicitUserSettings.Ex008DirIsCW = true;
            ImplicitUserSettings.Ex008ColorDetectHorizTop = textBoxEx008ColorDetectHorizTop.Text;
            ImplicitUserSettings.Ex008ColorDetectHorizBot = textBoxEx008ColorDetectHorizBot.Text;
            ImplicitUserSettings.Ex008ColorDetectMinPixelsHoriz = textBoxEx008ColorDetectMinPixelsHoriz.Text;
            ImplicitUserSettings.Ex008ColorDetectVertTop = textBoxEx008ColorDetectVertTop.Text;
            ImplicitUserSettings.Ex008ColorDetectVertBot = textBoxEx008ColorDetectVertBot.Text;
            ImplicitUserSettings.Ex008ColorDetectMinPixelsVert = textBoxEx008ColorDetectMinPixelsVert.Text;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A centralized place to close down all media devices.
        /// </summary>
        private void CloseAllMediaDevices()
        {
            HResult hr;

            // if we are processing in the code worker we had better stop now
            StopCodeWorker();

            // if we are recording we had better stop now
            StopRecording();

            // close and release our call back handler
            if (mediaSessionAsyncCallbackHandler != null)
            {
                // stop any messaging or events in the call back handler
                mediaSessionAsyncCallbackHandler.ShutDown();
                mediaSessionAsyncCallbackHandler = null;
            }

            // close the session (this is NOT the same as shutting it down)
            if (mediaSession != null)
            {
                hr = mediaSession.Close();
                if (hr != HResult.S_OK)
                {
                    // just log it
                }
            }

            // Shut down the media source
            if (mediaSource != null)
            {
                hr = mediaSource.Shutdown();
                if (hr != HResult.S_OK)
                {
                    // just log it
                }
                Marshal.ReleaseComObject(mediaSource);
                mediaSource = null;
            }

            // Shut down the media session (note we only closed it before).
            if (mediaSession != null)
            {
                hr = mediaSession.Shutdown();
                if (hr != HResult.S_OK)
                {
                    // just log it
                }
                Marshal.ReleaseComObject(mediaSession);
                mediaSession = null;
            }

            // close down the display
            ctlTantaEVRStreamDisplay1.ShutDownFilePlayer();

            // close the evrvideodisplay
            if (evrVideoDisplay != null)
            {
                Marshal.ReleaseComObject(evrVideoDisplay);
                evrVideoDisplay = null;
            }

            if (currentVideoMediaType != null)
            {
                Marshal.ReleaseComObject(currentVideoMediaType);
                currentVideoMediaType = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the runName - will never get/set null
        /// </summary>
        public string RunInfoStr
        {
            get
            {
                if (textBoxRunName.Text == null) textBoxRunName.Text = DEFAULT_RUN_NAME;
                if (textBoxRunName.Text.Length == 0) textBoxRunName.Text = DEFAULT_RUN_NAME;
                return textBoxRunName.Text;
            }
            set
            {
                textBoxRunName.Text = value;
                if (textBoxRunName.Text == null) textBoxRunName.Text = DEFAULT_RUN_NAME;
                if (textBoxRunName.Text.Length == 0) textBoxRunName.Text = DEFAULT_RUN_NAME;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the source filename. Will never return null, will return ""
        /// There is no set accessor, This is obtained off the screen.
        /// </summary>
      public string VideoCaptureDeviceName
        {
            get
            {
                if (textBoxPickedVideoDeviceURL.Text == null) textBoxPickedVideoDeviceURL.Text = "";
                return textBoxPickedVideoDeviceURL.Text;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the video device
        /// </summary>
        public TantaMFVideoFormatContainer VideoFormatContainer
        {
            get
            {
                if ((textBoxPickedVideoDeviceURL.Tag is TantaMFVideoFormatContainer) == false)
                {
                    textBoxPickedVideoDeviceURL.Tag = null;
                    return null;
                }
                return (textBoxPickedVideoDeviceURL.Tag as TantaMFVideoFormatContainer);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts/Stops the capture
        /// 
        /// Because this code is intended for demo purposes, and in the interests of
        /// reducing complexity, it is extremely linear, step-by-step and kicked off
        /// directly from a button press in the main form. Doubtless there is much 
        /// refactoring that could be done.
        /// 
        /// </summary>
        private void buttonStartStopCapture_Click(object sender, EventArgs e)
        {
            // this code toggles both the start and stop capture. Since the
            // STOP code is much simpler we test for it first. We use the 
            // text on the button to detect if we are capturing or not. 
            if (buttonStartStopCapture.Text == STOP_CAPTURE)
            {
                // do everything to close all media devices
                // the MF itself is still active.
                CloseAllMediaDevices();

                // re-enable our screen controls
                SyncScreenControlsToCaptureState(false, null);
                return;
            }

            // ####
            // #### below here we assume we are starting the capture
            // ####

            try
            {
                // crank up the run number
                RunNumberAsInt += 1;
                RecNumberAsInt = DEFAULT_REC_NUMBER;

                // check our source filename is correct and usable
                if ((VideoCaptureDeviceName == null) || (VideoCaptureDeviceName.Length == 0))
                {
                    MessageBox.Show("No Source Filename and path. Cannot continue.");
                    return;
                }
                if(VideoFormatContainer == null)
                {
                    MessageBox.Show("The video device and format is unknown.\n\nHave you selected a video device and format?");
                    return;
                }

                // check our output filename is correct and usable
                if ((OutputFileNameAndPath == null) || (OutputFileNameAndPath.Length == 0))
                {
                    MessageBox.Show("No Output Filename and path. Cannot continue.");
                    return;
                } 
                if (Path.IsPathRooted(OutputFileNameAndPath) == false)
                {
                    MessageBox.Show("No Output Filename and path is not rooted. A full directory and path is required. Cannot continue.");
                    return;
                }

                // check the directory of the path exists
                string dirName = Path.GetDirectoryName(OutputFileNameAndPath);
                if (Directory.Exists(dirName) == false)
                {
                    MessageBox.Show("The output directory does not exist. A full directory and path is required. Cannot continue.");
                    return;
                }
 
                // set up a session, topology and open the media source and sink etc
                PrepareSessionAndTopology(VideoFormatContainer);

                // disable our screen controls
                SyncScreenControlsToCaptureState(true, null);

                // start our codeWorker
                StartCodeWorker();

            }
            finally
            {

            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sync the state on the screen controls to the current capture state
        /// </summary>
        /// <param name="captureIsActive">if true we set the controls to enabled</param>
        /// <param name="displayText">Text to display in a message box. If null we ignore</param>
        private void SyncScreenControlsToCaptureState(bool captureIsActive, string displayText)
        {

            if (captureIsActive == false)
            {
                textBoxCaptureFileName.Enabled = true;
                labelVideoCaptureDeviceName.Enabled = true;
                textBoxCaptureFileName.Enabled = true;
                labelOutputFileName.Enabled = true;
                ctlTantaVideoPicker1.Enabled = true;
                buttonStartStopCapture.Text = START_CAPTURE;
                buttonRecordingOnOff.Enabled = false;
                buttonRecordingOnOff.Text = RECORDING_IS_OFF;
                checkBoxActivate.Enabled = false;
                checkBoxActivate.Checked = false;
                radioButtonRedToGreen.Enabled = true;
                radioButtonPathFollow.Enabled = true;
            }
            else
            {
                // set this
                textBoxPickedVideoDeviceURL.Enabled = false;
                labelVideoCaptureDeviceName.Enabled = false;
                textBoxCaptureFileName.Enabled = false;
                labelOutputFileName.Enabled = false;
                ctlTantaVideoPicker1.Enabled = false;
                buttonStartStopCapture.Text = STOP_CAPTURE;
                buttonRecordingOnOff.Enabled = true;
                buttonRecordingOnOff.Text = RECORDING_IS_OFF;
                checkBoxActivate.Enabled = true;
                radioButtonRedToGreen.Enabled = false;
                radioButtonPathFollow.Enabled = false;
            }

            if ((displayText != null) && (displayText.Length != 0))
            {
                MessageBox.Show(displayText);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the enabled state on the screen controls in a thread safe way
        /// </summary>
        /// <param name="captureIsActive">if true we set the controls to enabled</param>
        /// <param name="displayText">Text to display in a message box. If null we ignore</param>
        public void ThreadSafeScreenUpdate(object caller, bool captureIsActive, string displayText)
        {

            // Ok, you probably already know this but I'll note it here because this is so important
            // You do NOT want to update any form controls from a thread that is not the forms main
            // thread. Very odd, intermittent and hard to debug problems will result. Even if your 
            // handler does not actually update any form controls do not do it! Sooner or later you 
            // or someone else will make changes that calls something that eventually updates a
            // form or control and then you will have introduced a really hard to find bug.

            // So, we always use the InvokeRequired...Invoke sequence to get us back on the form thread
            if (InvokeRequired == true)
            {
                // call ourselves again but this time be on the form thread.
                Invoke(new ThreadSafeScreenUpdate_Delegate(ThreadSafeScreenUpdate), new object[] { caller, captureIsActive, displayText });
                return;
            }

            // if we get here we are assured we are on the form thread.
            SyncScreenControlsToCaptureState(captureIsActive, displayText);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Opens and prepares the media session and topology and opens the media source
        /// and media sink.
        /// 
        /// Once the session and topology are setup, a MESessionTopologySet event
        /// will be triggered in the callback handler. After that the events there
        /// trigger other events and everything rolls along automatically.
        /// </summary>
        /// <param name="videoCaptureDevice">the video capture device name</param>
        public void PrepareSessionAndTopology(TantaMFVideoFormatContainer videoFormatContainer)
        {
            HResult hr;
            IMFSourceResolver pSourceResolver = null;
            IMFTopology topologyObj = null;
            IMFPresentationDescriptor sourcePresentationDescriptor = null;
            int sourceStreamCount = 0;
            bool streamIsSelected = false;
            IMFStreamDescriptor videoStreamDescriptor = null;
            IMFTopologyNode sourceVideoNode = null;
            IMFTopologyNode outputSinkNodeVideo = null;
            IMFTopologyNode sampleGrabberTransformNode = null;
            IMFTopologyNode textOverlayTransformNode = null;
            IMFTopologyNode imageOverlayTransformNode = null;
            IMFTopologyNode recognitionTransformNode = null;

            // we sanity check the video source device 
            if (videoFormatContainer == null)
            {
                throw new Exception("PrepareSessionAndTopology: videoFormatContainer is invalid. Cannot continue.");
            }
            if (videoFormatContainer.VideoDevice == null)
            {
                throw new Exception("PrepareSessionAndTopology: VideoDevice is invalid. Cannot continue.");
            }
            if ((videoFormatContainer.VideoDevice.SymbolicName == null) || (videoFormatContainer.VideoDevice.SymbolicName.Length == 0))
            {
                throw new Exception("PrepareSessionAndTopology: VideoDevice.SymbolicName is invalid. Cannot continue.");
            }

            try
            {
                // reset everything
                CloseAllMediaDevices();

                // Create the media session.
                hr = MFExtern.MFCreateMediaSession(null, out mediaSession);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateMediaSession failed. Err=" + hr.ToString());
                }
                if (mediaSession == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateMediaSession failed. mediaSession == null");
                }

                // set up our media session call back handler.
                mediaSessionAsyncCallbackHandler = new TantaAsyncCallbackHandler();
                mediaSessionAsyncCallbackHandler.Initialize();
                mediaSessionAsyncCallbackHandler.MediaSession = mediaSession;
                mediaSessionAsyncCallbackHandler.MediaSessionAsyncCallBackError = HandleMediaSessionAsyncCallBackErrors;
                mediaSessionAsyncCallbackHandler.MediaSessionAsyncCallBackEvent = HandleMediaSessionAsyncCallBackEvent;

                // Register the callback handler with the session and tell it that events can
                // start. This does not actually trigger an event it just lets the media session 
                // know that it can now send them if it wishes to do so.
                hr = mediaSession.BeginGetEvent(mediaSessionAsyncCallbackHandler, null);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to mediaSession.BeginGetEvent failed. Err=" + hr.ToString());
                }

                // Create a new topology.  A topology describes a collection of media sources, sinks, and transforms that are 
                // connected in a certain order. These objects are represented within the topology by topology nodes, 
                // which expose the IMFTopologyNode interface. A topology describes the path of multimedia data through these nodes.
                hr = MFExtern.MFCreateTopology(out topologyObj);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateTopology failed. Err=" + hr.ToString());
                }
                if (topologyObj == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateTopology failed. topologyObj == null");
                }

                // ####
                // #### we now create the media source, this is video device (camera)
                // ####

                // use the device symbolic name to create the media source for the video device. Media sources are objects that generate media data. 
                // For example, the data might come from a video file, a network stream, or a hardware device, such as a camera. Each 
                // media source contains one or more streams, and each stream delivers data of one type, such as audio or video.                
                mediaSource = TantaWMFUtils.GetMediaSourceFromTantaDevice(videoFormatContainer.VideoDevice);
                if (mediaSource == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to mediaSource == null");
                }

                // A presentation is a set of related media streams that share a common presentation time.  We now get a copy of the media 
                // source's presentation descriptor. Applications can use the presentation descriptor to select streams 
                // and to get information about the source content.
                hr = mediaSource.CreatePresentationDescriptor(out sourcePresentationDescriptor);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to mediaSource.CreatePresentationDescriptor failed. Err=" + hr.ToString());
                }
                if (sourcePresentationDescriptor == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to mediaSource.CreatePresentationDescriptor failed. sourcePresentationDescriptor == null");
                }

                // Now we get the number of stream descriptors in the presentation. A presentation descriptor contains a list of one or more 
                // stream descriptors. These describe the streams in the presentation. Streams can be either selected or deselected. Only the 
                // selected streams produce data. Deselected streams are not active and do not produce any data. 
                hr = sourcePresentationDescriptor.GetStreamDescriptorCount(out sourceStreamCount);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to sourcePresentationDescriptor.GetStreamDescriptorCount failed. Err=" + hr.ToString());
                }
                if (sourceStreamCount == 0)
                {
                    throw new Exception("PrepareSessionAndTopology call to sourcePresentationDescriptor.GetStreamDescriptorCount failed. sourceStreamCount == 0");
                }

                // Look at each stream, there can be more than one stream here
                // Usually only one is enabled. This app uses the first "selected"  
                // stream we come to which has the appropriate media type

                // look for the video stream
                for (int i = 0; i < sourceStreamCount; i++)
                {
                    // we require the major type to be video
                    Guid guidMajorType = TantaWMFUtils.GetMajorMediaTypeFromPresentationDescriptor(sourcePresentationDescriptor, i);
                    if (guidMajorType != MFMediaType.Video) continue;

                    // we also require the stream to be enabled
                    hr = sourcePresentationDescriptor.GetStreamDescriptorByIndex(i, out streamIsSelected, out videoStreamDescriptor);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to sourcePresentationDescriptor.GetStreamDescriptorByIndex(v) failed. Err=" + hr.ToString());
                    }
                    if (videoStreamDescriptor == null)
                    {
                        throw new Exception("PrepareSessionAndTopology call to sourcePresentationDescriptor.GetStreamDescriptorByIndex(v) failed. videoStreamDescriptor == null");
                    }
                    // if the stream is selected, leave now we will release the videoStream descriptor later
                    if (streamIsSelected == true) break;

                    // release the one we are not using
                    if (videoStreamDescriptor != null)
                    {
                        Marshal.ReleaseComObject(videoStreamDescriptor);
                        videoStreamDescriptor = null;
                    }
                }

                // by the time we get here we should have a video StreamDescriptor if
                // we do not, then we cannot proceed. 
                if (videoStreamDescriptor == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to sourcePresentationDescriptor.GetStreamDescriptorByIndex failed. videoStreamDescriptor == null");
                }

                // sets the current media type on a stream descriptor by matching
                // its mediaTypes to the video format container contents. We know we will
                // get a match because our video format picker enumerated all the formats
                // for us and thus we chose one we already know exists.
                hr = TantaWMFUtils.SetCurrentMediaTypeOnIMFStreamDescriptorByFormatContainer(videoStreamDescriptor, videoFormatContainer);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to SetCurrentMediaTypeOnIMFStreamDescriptorByFormatContainer failed. Err=" + hr.ToString());
                }

                // ####
                // #### when we create the sink writer to record the video data we will need the types from the stream to do 
                // #### this which is why we get this now.
                // ####

                currentVideoMediaType = TantaWMFUtils.GetCurrentMediaTypeFromStreamDescriptor(videoStreamDescriptor);
                if (currentVideoMediaType == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to currentVideoMediaType == null");
                }

                // ####
                // #### Create the custom sample grabber transform which will send a copy of the data
                // #### to the SinkWriter for recording purposes
                // ####
                sampleGrabberTransform = new MFTTantaSampleGrabber_Sync();

                // ####
                // #### we now make up a topology branch for the video stream
                // ####

                // Create a source Video node for this stream.
                sourceVideoNode = TantaWMFUtils.CreateSourceNodeForStream(mediaSource, sourcePresentationDescriptor, videoStreamDescriptor);
                if (sourceVideoNode == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to CreateSourceNodeForStream(v) failed. sourceAudioNode == null");
                }

                // Create the Video sink node. 
                outputSinkNodeVideo = TantaWMFUtils.CreateEVRRendererOutputNodeForStream(this.ctlTantaEVRStreamDisplay1.DisplayPanelHandle);
                if (outputSinkNodeVideo == null)
                {
                    throw new Exception("PrepareSessionAndTopology call to MFCreateTopologyNode(v) failed.  outputSinkNodeVideo == null");
                }

                // Create the sample grabber transform node.
                hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out sampleGrabberTransformNode);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateTopologyNode failed. Err=" + hr.ToString());
                }

                // set the sample grabber transform object (it is an IMFTransform) as an object on the transform node. Since it is already
                // instantiated the topology does not need a GUID or activator to create it
                hr = sampleGrabberTransformNode.SetObject(sampleGrabberTransform);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to pTransformNode.SetObject failed. Err=" + hr.ToString());
                }

                // set the text overlay transform
                TextOverlayTransform = CreateRGBATextOverlayTransform();
                // do we have one?
                if (TextOverlayTransform != null)
                {
                    // yes, we do. Create a video Transform node for it
                    hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out textOverlayTransformNode);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateTopologyNode failed. Err=" + hr.ToString());
                    }
                    if (textOverlayTransformNode == null)
                    {
                        throw new Exception("PrepareSessionAndTopology call to MFCreateTopologyNode(t) failed.  textOverlayTransformNode == null");
                    }

                    // set the transform object (it is an IMFTransform) as an object on the transform node. Since it already exists as an
                    // object the topology does not need a GUID or activator to create it
                    hr = textOverlayTransformNode.SetObject(TextOverlayTransform);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to pTransformNode.SetObject failed. Err=" + hr.ToString());
                    }

                    // set a few things on the VideoTransform now
                    if ((TextOverlayTransform is MFTWriteText_Sync)== true)
                    {
                        (TextOverlayTransform as MFTWriteText_Sync).VersionInfoStr = APPLICATION_NAME + " " + APPLICATION_VERSION;
                        // the run info gets the run number inserted if the user used the MARKER in the string
                        (TextOverlayTransform as MFTWriteText_Sync).RunInfoStr = RunInfoStr.Replace(RUN_NUMBER_MARKER, RunNumberAsInt.ToString()); 
                    }
                }

                // set the image overlay transform
                ImageOverlayTransform = CreateImageOverlayTransform();
                // do we have one?
                if (ImageOverlayTransform != null)
                {
                    // yes, we do. Create a video Transform node for it
                    hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out imageOverlayTransformNode);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateTopologyNode failed. Err=" + hr.ToString());
                    }
                    if (imageOverlayTransformNode == null)
                    {
                        throw new Exception("PrepareSessionAndTopology call to MFCreateTopologyNode(t) failed.  imageOverlayTransformNode == null");
                    }

                    // set the transform object (it is an IMFTransform) as an object on the transform node. Since it already exists as an
                    // object the topology does not need a GUID or activator to create it
                    hr = imageOverlayTransformNode.SetObject(ImageOverlayTransform);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to pTransformNode.SetObject failed. Err=" + hr.ToString());
                    }

                    // set a few things on the VideoTransform now
                    //if ((ImageOverlayTransform is MFTWriteText_Sync) == true)
                    //{
                    //    (ImageOverlayTransform as MFTWriteText_Sync).VersionInfoStr = APPLICATION_NAME + " " + APPLICATION_VERSION;
                    //    // the run info gets the run number inserted if the user used the MARKER in the string
                    //    (ImageOverlayTransform as MFTWriteText_Sync).RunInfoStr = RunInfoStr.Replace(RUN_NUMBER_MARKER, RunNumberAsInt.ToString());
                    //}
                }

                // set the image recognition transform
                RecognitionTransform = CreateRGBAObjectDetectionTransform();
                // do we have one?
                if (RecognitionTransform != null)
                {
                    // set it up for (0,0) in lower left
                    RecognitionTransform.WantOriginLowerLeft = true;

                    // yes, we do. Create a video Transform node for it
                    hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out recognitionTransformNode);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to MFExtern.MFCreateTopologyNode failed. Err=" + hr.ToString());
                    }
                    if (recognitionTransformNode == null)
                    {
                        throw new Exception("PrepareSessionAndTopology call to MFCreateTopologyNode(t) failed.  recognitionTransformNode == null");
                    }

                    // set the transform object (it is an IMFTransform) as an object on the transform node. Since it already exists as an
                    // object the topology does not need a GUID or activator to create it
                    hr = recognitionTransformNode.SetObject(RecognitionTransform);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to pTransformNode.SetObject failed. Err=" + hr.ToString());
                    }

                    //// set a few things on the VideoTransform now
                    //if ((RecognitionTransform is MFTDetectHorizLines) == true)
                    //{
                    //    //(RecognitionTransform as MFTDetectColoredBlobs_Sync).VersionInfoStr = APPLICATION_NAME + " " + APPLICATION_VERSION;
                    //    //// the run info gets the run number inserted if the user used the MARKER in the string
                    //    //(RecognitionTransform as MFTDetectColoredBlobs_Sync).RunInfoStr = RunInfoStr.Replace(RUN_NUMBER_MARKER, RunNumberAsInt.ToString());
                    //}
                }

                // Add the nodes to the topology. First the source node
                hr = topologyObj.AddNode(sourceVideoNode);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to topologyObj.AddNode(sourceAudioNode) failed. Err=" + hr.ToString());
                }

                // add the output Node
                hr = topologyObj.AddNode(outputSinkNodeVideo);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to topologyObj.AddNode(outputSinkNodeVideo) failed. Err=" + hr.ToString());
                }

                // add the samplegrabber transform Node
                hr = topologyObj.AddNode(sampleGrabberTransformNode);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to topologyObj.AddNode(sampleGrabberTransformNode) failed. Err=" + hr.ToString());
                }

                // now we connect the nodes. The way we do this depends on whether we have certain node types

                // inject the text overlay transform node into the topology
                hr = topologyObj.AddNode(textOverlayTransformNode);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to topologyObj.AddNode(textOverlayTransformNode) failed. Err=" + hr.ToString());
                }
                // connect first transform node to the source node
                hr = sourceVideoNode.ConnectOutput(0, textOverlayTransformNode, 0);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to  sourceVideoNode.ConnectOutput failed. Err=" + hr.ToString());
                }

                // record this, so we can chain the transforms properly. We always chain last to next
                IMFTopologyNode lastTransformNode = textOverlayTransformNode;

                // do we have an image recognition transform? 
                if (RecognitionTransform != null)
                {
                    // inject the image recognition transform node into the topology
                    hr = topologyObj.AddNode(recognitionTransformNode);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to topologyObj.AddNode(recognitionTransformNode) failed. Err=" + hr.ToString());
                    }
                    // connect the recognition transform node to the last transform node
                    hr = lastTransformNode.ConnectOutput(0, recognitionTransformNode, 0);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call(a) to  lastTransformNode.ConnectOutput failed. Err=" + hr.ToString());
                    }

                    // record this
                    lastTransformNode = recognitionTransformNode;
                }

                // note we are putting the overlay transform after the image recognition. This is more
                // appropriate for following a path since the green does not interfere with the image recognition
                // if we want to have virtual targets on the overlay it has to go before the recognition transform

                // do we have an image overlay transform? 
                if (ImageOverlayTransform != null)
                {
                    // yes, we do
                    // inject the image overlay transform node into the topology
                    hr = topologyObj.AddNode(imageOverlayTransformNode);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call to topologyObj.AddNode(imageOverlayTransformNode) failed. Err=" + hr.ToString());
                    }
                    // connect the image overlay transform node to the last transform node
                    hr = lastTransformNode.ConnectOutput(0, imageOverlayTransformNode, 0);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("PrepareSessionAndTopology call(a) to lastTransformNode.ConnectOutput failed. Err=" + hr.ToString());
                    }
                    // record this
                    lastTransformNode = imageOverlayTransformNode;
                }

                // now connect the last transform node to sample grabber transform node
                hr = lastTransformNode.ConnectOutput(0, sampleGrabberTransformNode, 0);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to lastTransformNode.ConnectOutput failed. Err=" + hr.ToString());
                }


                // the sample grabber always connects to the sink node. The samples are grabbed internally in that node 
                // and copied off to a file (if necessary). Other than that it just acts as a regular pass through 
                // transform
                hr = sampleGrabberTransformNode.ConnectOutput(0, outputSinkNodeVideo, 0);
                if (hr != HResult.S_OK)
                {
                    throw new Exception("PrepareSessionAndTopology call to  sampleGrabberTransformNode.ConnectOutput failed. Err=" + hr.ToString());
                }

                // Set the topology on the media session.
                // If SetTopology succeeds, the media session will queue an
                // MESessionTopologySet event. We can use that to discover the
                // EVR display object
                hr = mediaSession.SetTopology(0, topologyObj);
                if (hr != HResult.S_OK)
                {
                    // we failed
                    throw new Exception("PrepareSessionAndTopology mediaSession.SetTopology failed, retVal=" + hr.ToString());
                }

                // Release the topology
                if (topologyObj != null)
                {
                    Marshal.ReleaseComObject(topologyObj);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Clean up
                if (pSourceResolver != null)
                {
                    Marshal.ReleaseComObject(pSourceResolver);
                }
                if (sourcePresentationDescriptor != null)
                {
                    Marshal.ReleaseComObject(sourcePresentationDescriptor);
                }
                if (videoStreamDescriptor != null)
                {
                    Marshal.ReleaseComObject(videoStreamDescriptor);
                }
                if (sourceVideoNode != null)
                {
                    Marshal.ReleaseComObject(sourceVideoNode);
                }
                if (outputSinkNodeVideo != null)
                {
                    Marshal.ReleaseComObject(outputSinkNodeVideo);
                }
                if (sampleGrabberTransformNode != null)
                {
                    Marshal.ReleaseComObject(sampleGrabberTransformNode);
                }
                if (textOverlayTransformNode != null)
                {
                    Marshal.ReleaseComObject(textOverlayTransformNode);
                }
                if (imageOverlayTransformNode != null)
                {
                    Marshal.ReleaseComObject(imageOverlayTransformNode);
                }
                if (recognitionTransformNode != null)
                {
                    Marshal.ReleaseComObject(recognitionTransformNode);
                }

            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the text overlay transform. This is the transform that injects the 
        /// logging and other information at the bottom of the image stream.
        /// 
        /// The topology build process assumes we have one of these so this can never
        /// be null.
        /// </summary>
        /// <returns> the a text overlay transform object according to the display settings</returns>
        private IMFTransform CreateRGBATextOverlayTransform()
        {
            // hard coded to this. 
            return new MFTWriteText_Sync(); 
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the image overlay transform. This is the transform that can overlay
        /// an image (from a file) onto the image stream.
        /// 
        /// </summary>
        /// <returns> the a text overlay transform object according to the display settings</returns>
        private IMFTransform CreateImageOverlayTransform()
        {
            // hard coded to this. 
            return new MFTOverlayImage_GS();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the transform we are currently using to detect objects in 
        /// the image stream
        /// 
        /// This does not necessarily implement it in the topology. Just creates 
        /// it. The addition comes later
        /// 
        /// This can be null if we do not have one.
        /// </summary>
        /// <returns> the a new transform object according to the display settings or null for none</returns>
        private MFTDetectHorizLines CreateRGBAObjectDetectionTransform()
        {
            // hard coded to this. If we wished to inject a different one into the pipeline we
            // could put some logic here.
            return new MFTDetectHorizLines();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current image recognition transform object. Can be null
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MFTDetectHorizLines RecognitionTransform
        {
            get
            {
                return recognitionTransform;
            }
            set
            {
                recognitionTransform = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current text overlay transform object. Can be null
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IMFTransform TextOverlayTransform
        {
            get
            {
                return textOverlayTransform;
            }
            set
            {
                textOverlayTransform = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current image overlay transform object. Can be null
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IMFTransform ImageOverlayTransform
        {
            get
            {
                return imageOverlayTransform;
            }
            set
            {
                imageOverlayTransform = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles events reported by the media session TantaAsyncCallbackHandler 
        /// </summary>
        /// <param name="sender">the object sending the event</param>
        /// <param name="mediaEvent">the event generated by the media session. Do NOT release this here.</param>
        /// <param name="mediaEventType">the eventType, this is just an enum</param>
        private void HandleMediaSessionAsyncCallBackEvent(object sender, IMFMediaEvent pEvent, MediaEventType mediaEventType)
        {

            switch (mediaEventType)
            {
                case MediaEventType.MESessionTopologyStatus:
                    // Raised by the Media Session when the status of a topology changes. 
                    // Get the topology changed status code. This is an enum in the event
                    int i;
                    HResult hr = pEvent.GetUINT32(MFAttributesClsid.MF_EVENT_TOPOLOGY_STATUS, out i);
                    if (hr != HResult.S_OK)
                    {
                        throw new Exception("HandleMediaSessionAsyncCallBackEvent call to pEvent to get the status code failed. Err=" + hr.ToString());
                    }
                    // the one we are most interested in is i == MFTopoStatus.Ready
                    // which we get then the Topology is built and ready to run
                    HandleTopologyStatusChanged(pEvent, mediaEventType, (MFTopoStatus)i);
                    break;

                case MediaEventType.MESessionStarted:
                    // Raised when the IMFMediaSession::Start method completes asynchronously. 
                    //       PlayerState = TantaEVRPlayerStateEnum.Started;
                    break;

                case MediaEventType.MESessionPaused:
                    // Raised when the IMFMediaSession::Pause method completes asynchronously. 
                    //        PlayerState = TantaEVRPlayerStateEnum.Paused;
                    break;

                case MediaEventType.MESessionStopped:
                    // Raised when the IMFMediaSession::Stop method completes asynchronously.
                    break;

                case MediaEventType.MESessionClosed:
                    // Raised when the IMFMediaSession::Close method completes asynchronously. 
                    break;

                case MediaEventType.MESessionCapabilitiesChanged:
                    // Raised by the Media Session when the session capabilities change.
                    // You can use IMFMediaEvent::GetValue to figure out what they are
                    break;

                case MediaEventType.MESessionTopologySet:
                    // Raised after the IMFMediaSession::SetTopology method completes asynchronously. 
                    // The Media Session raises this event after it resolves the topology into a full topology and queues the topology for playback. 
                    break;

                case MediaEventType.MESessionNotifyPresentationTime:
                    // Raised by the Media Session when a new presentation starts. 
                    // This event indicates when the presentation will start and the offset between the presentation time and the source time.      
                    break;

                case MediaEventType.MEEndOfPresentation:
                    // Raised by a media source when a presentation ends. This event signals that all streams 
                    // in the presentation are complete. The Media Session forwards this event to the application.

                    // we cannot sucessfully .Finalize_ on the SinkWriter
                    // if we call CloseAllMediaDevices directly from this thread
                    // so we use an asynchronous method
                    Task taskA = Task.Run(() => CloseAllMediaDevices());
                    // we have to be on the form thread to update the screen
                    ThreadSafeScreenUpdate(this, false, null);
                    break;

                case MediaEventType.MESessionRateChanged:
                    // Raised by the Media Session when the playback rate changes. This event is sent after the 
                    // IMFRateControl::SetRate method completes asynchronously. 
                    break;

                default:
                    break;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles topology status changes reported by the media session TantaAsyncCallbackHandler 
        /// </summary>
        /// <param name="sender">the object sending the event</param>
        /// <param name="mediaEvent">the event generated by the media session. Do NOT release this here.</param>
        /// <param name="mediaEventType">the eventType</param>
        /// <param name="topoStatus">the topology status flag</param>
        private void HandleTopologyStatusChanged(IMFMediaEvent mediaEvent, MediaEventType mediaEventType, MFTopoStatus topoStatus)
        {

            if (topoStatus == MFTopoStatus.Ready)
            {
                MediaSessionTopologyNowReady(mediaEvent);
            }
            else
            {
                // we are not interested in any other status changes
                return;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Called when the topology status changes to ready. This status change
        /// is generally signaled by the media session when it is fully configured.
        /// </summary>
        /// <param name="sender">the object sending the event</param>
        /// <param name="mediaEvent">the event generated by the media session. Do NOT release this here.</param>
        /// <param name="mediaEventType">the eventType</param>
        /// <param name="topoStatus">the topology status flag</param>
        private void MediaSessionTopologyNowReady(IMFMediaEvent mediaEvent)
        {
            HResult hr;
            object evrVideoService;

            // we need to obtain a reference to the EVR Video Display Control.
            // We used an Activator to configure this in the Topology and so
            // there is no reference to it at this point. However the media session
            // knows about it and so we can get it from that.

            // Ask for the IMFVideoDisplayControl interface. This interface is implemented by the EVR and is
            // exposed by the media session as a service.

            // Some interfaces in Media Foundation must be obtained by calling IMFGetService::GetService instead 
            // of by calling QueryInterface. The GetService method works like QueryInterface, but 
            // the big difference is that if an object is returning itself as a different interface 
            // you can use QueryInterface. If, as in this case where the media session is NOT the
            // evrVideoDisplay object, an object is returning another object you obtain that object
            // as a service.            

            // Note: This call is expected to fail if the source does not have video.

            try
            {
                // we need to get the active IMFVideoDisplayControl. The EVR presenter implements this interface
                // and it controls how the Enhanced Video Renderer (EVR) displays video.
                hr = MFExtern.MFGetService(
                    mediaSession,
                    MFServices.MR_VIDEO_RENDER_SERVICE,
                    typeof(IMFVideoDisplayControl).GUID,
                    out evrVideoService
                    );
                if (hr != HResult.S_OK)
                {
                    throw new Exception("MediaSessionTopologyNowReady call to MFExtern.MFGetService failed. Err=" + hr.ToString());
                }
                if (evrVideoService == null)
                {
                    throw new Exception("MediaSessionTopologyNowReady call to MFExtern.MFGetService failed. evrVideoService == null");
                }

                // set the video display now for later use
                evrVideoDisplay = evrVideoService as IMFVideoDisplayControl;
                // also give this to the display control
                ctlTantaEVRStreamDisplay1.EVRVideoDisplay = evrVideoDisplay;
            }
            catch (Exception)
            {
                evrVideoDisplay = null;
                ctlTantaEVRStreamDisplay1.EVRVideoDisplay = evrVideoDisplay;
            }

            try
            {
                StartVideoCapture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts the capture of the media data and gets it moving through
        /// the pipeline from source to sink.
        /// </summary>
        private void StartVideoCapture()
        {

            if (mediaSession == null)
            {
                return;
            }

            if (evrVideoDisplay != null)
            {
                // the aspect ratio can be changed by uncommenting either of these lines
                // evrVideoDisplay.SetAspectRatioMode(MFVideoAspectRatioMode.None);
                // evrVideoDisplay.SetAspectRatioMode(MFVideoAspectRatioMode.PreservePicture);
            }

            // set this now
            GiveChyronHeightToImageRecognitionTransform();
            GiveChyronHeightToOverlayTransform();

            // experiment specific setup actions
            Ex008SpecificSetupActions();

            // this is what starts the data moving through the pipeline
            HResult hr = mediaSession.Start(Guid.Empty, new PropVariant());
            if (hr != HResult.S_OK)
            {
                throw new Exception("StartVideoCapture call to mediaSession.Start failed. Err=" + hr.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Perform actions specific to Ex008
        /// 
        /// This needs to be done after the topology has been set but before it starts
        /// </summary>
        private void Ex008SpecificSetupActions()
        {
            // set the color recognition values on the transform
            SetEx008ColorRecognitionValues();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tell the image recognition transform the chyron height. This is so it 
        /// does not try to image recognise in the bar at the bottom of the screen
        /// 
        /// This needs to be done after the topology has been set but before it starts
        /// </summary>
        private void GiveChyronHeightToImageRecognitionTransform()
        {
            if (RecognitionTransform == null) return;
            if (TextOverlayTransform == null) return;
            // set it now, 
            (RecognitionTransform as MFTDetectHorizLines).BottomOfScreenSkipHeight = (TextOverlayTransform as MFTWriteText_Sync).ChyronHeight;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Tell the overlay transform the chyron height. This is so it 
        /// does not try to process in the bar at the bottom of the screen
        /// 
        /// This needs to be done after the topology has been set but before it starts
        /// </summary>
        private void GiveChyronHeightToOverlayTransform()
        {
            if (ImageOverlayTransform == null) return;
            if (TextOverlayTransform == null) return;
            // set it now, 
            (ImageOverlayTransform as MFTOverlayImage_Base).BottomOfScreenSkipHeight = (TextOverlayTransform as MFTWriteText_Sync).ChyronHeight;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles errors reported by the media session TantaAsyncCallbackHandler 
        /// </summary>
        /// <param name="sender">the object sending the event</param>
        /// <param name="errMsg">the error message</param>
        /// <param name="ex">the exception. Can be null</param>
        private void HandleMediaSessionAsyncCallBackErrors(object sender, string errMsg, Exception ex)
        {
            if (errMsg == null) errMsg = "unknown error";

            if (ex != null)
            {
            }
            MessageBox.Show("The media session reported an error\n\nPlease see the logfile.");
            // do everything to close all media devices
            CloseAllMediaDevices();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle changes on the input filename so we can set our output filename.
        /// </summary>
        private void textBoxPickedVideoDeviceURL_TextChanged(object sender, EventArgs e)
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Handle a picked video device 
        /// </summary>
        /// <param name="videoDevice">the video device</param>
        private void VideoDevicePickedHandler(object sender, TantaMFDevice videoDevice)
        {
           // we do nothing here. The user has to also pick a format from that device
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Handle a picked video device and format
        /// </summary>
        /// <param name="videoFormatCont">the video format container. Also contains the device</param>
        private void VideoFormatPickedHandler(object sender, TantaMFVideoFormatContainer videoFormatCont)
        {
            string mfDeviceName = "<unknown device>";
            string formatSummary = "<unknown format>";

            // set these now
            if (videoFormatCont != null)
            {
                formatSummary = videoFormatCont.DisplayString();
                if (videoFormatCont.VideoDevice != null) mfDeviceName = videoFormatCont.VideoDevice.FriendlyName;
                // set the button text appropriately
                textBoxPickedVideoDeviceURL.Text = mfDeviceName + " " + formatSummary;
                // save the container here - this is the last one that came in
                textBoxPickedVideoDeviceURL.Tag = videoFormatCont;
            }
            else
            {
                // set the button text appropriately
                textBoxPickedVideoDeviceURL.Text = "Use: " + mfDeviceName + " " + formatSummary;
                // save the container here - this is the last one that came in
                textBoxPickedVideoDeviceURL.Tag = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Detects if we are currently recording
        /// </summary>
        private bool IsRecording
        {
            get
            {
                if (sampleGrabberTransform == null) return false;
                return sampleGrabberTransform.IsRecording;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Toggles the recording on and off
        /// </summary>
        private void buttonRecordingOnOff_Click(object sender, EventArgs e)
        {
            int retInt;

            if (sampleGrabberTransform == null)
            {
                // no transform, recording is always off
                buttonRecordingOnOff.Text = RECORDING_IS_OFF;
                return;
            }

            if (IsRecording == false)
            {
                // recording is currently off, turn it on
                buttonRecordingOnOff.Text = RECORDING_IS_ON;

                // crank up the run number
                RecNumberAsInt += 1;

                // start recording
                retInt = StartRecording();
                if (retInt != 0)
                {
                    // we errored
                    StopRecording();
                    buttonRecordingOnOff.Text = RECORDING_IS_OFF;
                    MessageBox.Show("Error " + retInt.ToString() + " occurred. Cannot continue. Please see the logs");
                    return;
                }
            }
            else
            {
                // recording is currently on, turn it off
                buttonRecordingOnOff.Text = RECORDING_IS_OFF;
                // just do this
                StopRecording();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Starts the recording process. Does not update the screen to say it is
        ///  doing this.
        /// </summary>
        /// <returns>z success, nz fail</returns>
        private int StartRecording()
        {
            // we have to have this.
            if (sampleGrabberTransform == null) return 100;

            // check our output filename is correct and usable
            if ((OutputFileNameAndPath == null) || (OutputFileNameAndPath.Length == 0))
            {
                MessageBox.Show("No Output Filename and path. Cannot continue.");
                return 200;
            }
            // check the path is rooted
            if (Path.IsPathRooted(OutputFileNameAndPath) == false)
            {
                MessageBox.Show("No Output Filename and path is not rooted. A full directory and path is required. Cannot continue.");
                return 300;
            }

            if(currentVideoMediaType == null)
            {
                MessageBox.Show("No current video type. Something went wrong. Cannot continue.");
                return 400;
            }

            // ask the sampleGrabberTransform to start recording
            sampleGrabberTransform.StartRecording(OutputFileNameAndPath, currentVideoMediaType, false);
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Starts the recording process. Does not update the screen to say it is
        ///  doing this.
        /// </summary>
        /// <returns>z success, nz fail</returns>
        private void StopRecording()
        {
            // we have to have this.
            if (sampleGrabberTransform == null) return;

            // ask the sampleGrabberTransform to start recording
            sampleGrabberTransform.StopRecording();

            if (buttonStartStopCapture.Text == STOP_CAPTURE)
            {
//                checkBoxTimeBaseRebase.Enabled = true;
            }
            else
            {
 //               checkBoxTimeBaseRebase.Enabled = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Resets the run number
        /// </summary>
        private void buttonResetRunNumber_Click(object sender, EventArgs e)
        {
            textBoxRunNumber.Text = DEFAULT_RUN_NUMBER.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Resets the rec number
        /// </summary>
        private void buttonResetRecNumber_Click(object sender, EventArgs e)
        {
            textBoxRecNumber.Text = DEFAULT_REC_NUMBER.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Returns the run number as an integer
        /// </summary>
        private int RunNumberAsInt
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxRunNumber.Text);
                }
                catch
                {
                    // if it is not convertable to an int we give it the default 
                    // and return that
                    textBoxRunNumber.Text = DEFAULT_RUN_NUMBER.ToString();
                    return Convert.ToInt32(textBoxRunNumber.Text);
                }
            }
            set
            {
                // we know it is an int so set it now
                textBoxRunNumber.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Returns the rec number as an integer
        /// </summary>
        private int RecNumberAsInt
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxRecNumber.Text);
                }
                catch
                {
                    // if it is not convertable to an int we give it the default 
                    // and return that
                    textBoxRecNumber.Text = DEFAULT_REC_NUMBER.ToString();
                    return Convert.ToInt32(textBoxRecNumber.Text);
                }
            }
            set
            {
                // we know it is an int so set it now
                textBoxRecNumber.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// A region of code which runs in a background worker and figures out 
        /// the moves the CNC device needs to execute
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region CodeWorker

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Starts the CodeWorker. 
        /// </summary>
        private void StartCodeWorker()
        {
            // are we already running?
            if (codeWorker != null)
            {
                StopCodeWorker();
            }

            codeWorker = new BackgroundWorker();
            codeWorker.DoWork += new DoWorkEventHandler(codeWorker_DoWork);
            codeWorker.ProgressChanged += new ProgressChangedEventHandler(codeWorker_ProgressChanged);
            codeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(codeWorker_RunWorkerCompleted);
            codeWorker.WorkerReportsProgress = true;
            codeWorker.WorkerSupportsCancellation = true;
            codeWorker.RunWorkerAsync();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does the work for the CodeWorker. NOTE we are NOT in the form thread here
        /// you CANNOT operate on any screen controls in here.
        /// </summary>
        void codeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int processedCount = 0;
            while(true) // endless loop
            {
                // are we to cancel?
                if (codeWorker.CancellationPending)
                {
                    // this will cancel it
                    e.Cancel = true;                
                    return;
                }
                processedCount++;

                // we only update the screen every so often
                System.Threading.Thread.Sleep(CODEWORKER_UPDATE_TIME_MSEC);

                // handle the output
                codeWorker.ReportProgress(0, processedCount);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Reports the progress for the CodeWorker 
        /// 
        /// NOTE we ARE in the form thread here and it is ok to operate on the screen
        /// controls. 
        /// </summary>
        void codeWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelCount.Text = String.Format("Processed Count: {0}", e.UserState);

            // call the change handler for the current experiment
            Ex008_ProcessChangedHandler();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles progress reports in a way specific to Ex008
        /// 
        /// NOTE we ARE in the form thread here and it is ok to operate on the screen
        /// controls. 
        /// </summary>
        private void Ex008_ProcessChangedHandler()
        {
            // do we have a recognition transform?
            if (RecognitionTransform == null) return;  // we can do nothing
            // we need the overlayTransform as well
            if (ImageOverlayTransform == null) return;

            // clear out our existing lines
            if (checkBoxActions008ClearRedEveryFrame.Checked == true)
            {
                (ImageOverlayTransform as MFTOverlayImage_Base).ClearColorFromOverlay(Color.Red);
            }

            // yes, we do. Get the list of objects from it
            List<ColoredRotatedObject> objList = RecognitionTransform.IdentifiedObjects;
            if (objList == null) return;

            Point lastHorizLineCenterPoint=new Point();
            Point lastVertLineCenterPoint=new Point();

            // run through each object and draw in the identified largest horiz and vert line
            foreach (ColoredRotatedObject crObj in objList)
            {
                if ((crObj is ColoredRotatedLine) == false) return;
                if ((crObj as ColoredRotatedLine).Angle == ColoredRotatedLine.HORIZONTAL_LINE_ANGLE)
                {
                    // remember this
                    lastHorizLineCenterPoint = (crObj as ColoredRotatedLine).CenterPoint;
                    // do we want to draw one?
                    if (checkBoxActions008ShowHorizLine.Checked == true)
                    {
                        // yes we do, we draw in the horizontal line
                        DrawRedLineThroughPoint((crObj as ColoredRotatedLine).CenterPoint, 1, false);
                    }
                }
                if ((crObj as ColoredRotatedLine).Angle == ColoredRotatedLine.VERTICAL_LINE_ANGLE)
                {
                    // remember this
                    lastVertLineCenterPoint = (crObj as ColoredRotatedLine).CenterPoint;
                    // do we want to draw one?
                    if (checkBoxActions008ShowVertLine.Checked == true)
                    {
                        // we draw in the horizontal line
                        DrawRedLineThroughPoint((crObj as ColoredRotatedLine).CenterPoint, 1, true);
                    }
                }
            } // bottom of foreach (ColoredRotatedObject crObj in objList)

            // do we want to draw a circle
            if ((checkBoxActions008DrawCircleOnIntersection.Checked == true) && (lastHorizLineCenterPoint.IsEmpty == false) && (lastVertLineCenterPoint.IsEmpty == false))
            {
                // just draw a red circle
                DrawCircleAtPoint(new Point(lastVertLineCenterPoint.X, lastHorizLineCenterPoint.Y), Ex008RedCircleRadius, HTML_RED);
            }

            // now we move on to sending the SRC/TGT data container to the Walnut client

            if ((checkBoxActions008MoveRedOntoGreen.Checked == true) && (LastGreenPoint.IsEmpty==false))
            {
                // do we have anything to send?
                if((lastHorizLineCenterPoint.IsEmpty == true) || (lastVertLineCenterPoint.IsEmpty == true))
                {
                    // we cannot move red onto green
                    // create an empty scData obj, this will shut it down
                    SendSrcTgtData(new SrcTgtData());
                    return;
                }

                // we are good to go, create and populate an scData obj
                Point redPoint = new Point(lastVertLineCenterPoint.X, lastHorizLineCenterPoint.Y);
                SrcTgtData scData = new SrcTgtData(redPoint, LastGreenPoint);
                // send it
                SendSrcTgtData(scData);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Send SrcTgtData to the Walnut Client
        /// 
        /// </summary>
        /// <param name="srcTgtData">the srcTgt data container</param>
        private void SendSrcTgtData(SrcTgtData srcTgtData)
        {
            if (srcTgtData == null)
            {
                LogMessage("SendSrcTgtData, srcTgtData == null");
                return;
            }
            if (dataTransporter == null)
            {
                LogMessage("SendSrcTgtData, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("SendSrcTgtData, Not connected");
                return;
            }

            // create the data container
            ServerClientData scData = new ServerClientData("SrcTgt Data from Server to Client");
            // tell it we are carrying a srcTgt list
            scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.SRCTGT_DATA;
            scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);
            scData.SrcTgtList = new List<SrcTgtData>();
            scData.SrcTgtList.Add(srcTgtData);

            // display it
            LogMessage("SendSrcTgtData, OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);            
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles progress reports in a way specific to some older experiments
        /// 
        /// NOTE we ARE in the form thread here and it is ok to operate on the screen
        /// controls. 
        /// </summary>
        private void ExOLD_ProcessChangedHandler()
        { 
            //StringBuilder sb = new StringBuilder();

        //// do we have a recognition transform?
        //if (RecognitionTransform != null)
        //{
        //    // yes, we do. Get the list of objects from it
        //    List<ColoredRotatedObject> objList = RecognitionTransform.IdentifiedObjects;
        //    if (objList == null)
        //    {
        //        return;
        //    }
        //    // convert to src-tgt format
        //    List<SrcTgtData> srcTgtDataList = new List<SrcTgtData>();

        //    // do we want to check for the nearest green point?
        //    if ((objList.Count != 0) && (imageOverlayTransform != null))
        //    {
        //        Point centerPoint = new Point((int)objList[0].Center.X, (int)objList[0].Center.Y);
        //     //   int objRadius = (int)objList[0].Radius + 3;
        //        int objRadius = 3;

        //        // do we want to make it transparent
        //        if (checkBoxMakeTargetTransparent.Checked == true)
        //        {
        //            // make the target transparent
        //            (imageOverlayTransform as MFTOverlayImage_GS).FillCircularRegionOnOverlay(whiteTransparentBrush, centerPoint, objRadius);
        //            // only write on the tracker if we are not actually following the track
        //            if (currentTargetColorARGB != ALT_TARGET_COLOR_ARGB) (imageOverlayTransform as MFTOverlayImage_GS).FillCircularRegionOnTracker(trackerBrush, centerPoint, 1);
        //        }
        //        if (checkBoxFindGreen.Checked == true)
        //        {
        //            // look for the nearest green point. This is a spiral algorythm from the start point
        //            // it is faster than a raster scan from (0,0) and because the overlay uses a DirectBitmap
        //            // the GetPixel() calls are reasonably fast.
        //            Point nearestGreenPoint = (imageOverlayTransform as MFTOverlayImage_GS).GetNearestColorPointFromOrigin(centerPoint, currentTargetColorARGB, PATH_FOLLOW_MIN_POINTS_NEEDED);
        //            if (nearestGreenPoint.IsEmpty == false)
        //            {
        //                // found one, count it
        //                countOfDefaultTargetPixelsFound++;
        //                // load up the srcTgt object
        //                srcTgtDataList.Add(new SrcTgtData(centerPoint, nearestGreenPoint));

        //                // temporary
        //                // (imageOverlayTransform as MFTOverlayImage_GS).DrawLineBetweenPoints(blackPen, centerPoint, nearestGreenPoint);
        //            }
        //            else
        //            {
        //                // commented out for experiment 006
        //                //// not found. Have we moved enough to consider switching colors and toggling the 
        //                //// operation to find our way back.
        //                //const int MIN_TARGET_PIXELS_FOUND_TO_SWITCH_COLORS = 20;
        //                //if (countOfDefaultTargetPixelsFound>MIN_TARGET_PIXELS_FOUND_TO_SWITCH_COLORS)
        //                //{
        //                //    // switch to the alt color
        //                //    currentTargetColorARGB = ALT_TARGET_COLOR_ARGB;
        //                //    countOfDefaultTargetPixelsFound = 0;
        //                //    (imageOverlayTransform as MFTOverlayImage_GS).CopyTrackerOntoOverlay();
        //                //    (imageOverlayTransform as MFTOverlayImage_GS).ClearTracker();
        //                //}
        //            }
        //        }
        //    }

        //    // do we want to transmit this data to the client?
        //    if (TransmitToClientEnabled == true)
        //    {

        //        if (dataTransporter == null)
        //        {
        //            LogMessage("codeWorker_ProgressChanged, dataTransporter == null");
        //            return;
        //        }
        //        if (IsConnected() == false)
        //        {
        //            LogMessage("codeWorker_ProgressChanged, Not connected");
        //            return;
        //        }

        //        // create the data container
        //        ServerClientData scData = new ServerClientData("SrcTgt Data from Server to Client");
        //        // tell it we are carrying a srcTgt list
        //        scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.SRCTGT_DATA;
        //        scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);
        //        scData.SrcTgtList = srcTgtDataList;

        //        // display it
        //        LogMessage("codeWorker_ProgressChanged, OUT: dataStr=" + scData.DataStr);
        //        // send it
        //        dataTransporter.SendData(scData);

        //        // set diagnostics going
        //        if (diagnosticMessageCount == 0) diagnosticStartTime = DateTime.Now;
        //        if (diagnosticMessageCount >= MAX_DIAGNOSTIC_MESSAGE_COUNT)
        //        {
        //            TimeSpan timeItTook = DateTime.Now - diagnosticStartTime;
        //            this.textBoxStatus.Text = "Elapsed=" + timeItTook.TotalSeconds + ", avg/sec=" + diagnosticMessageCount / timeItTook.TotalSeconds;
        //            diagnosticMessageCount = 0;
        //            return;
        //        }
        //        diagnosticMessageCount++;

        //    }
        //}
        }

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Handles completion actions for the CodeWorker
    /// 
    /// NOTE we ARE in the form thread here and it is ok to operate on the screen
    /// controls. 
    /// </summary>
    void codeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //he codeworker has completed - we clean up
                BackgroundWorker tmpWorker = codeWorker;
                codeWorker = null;
                if (tmpWorker != null) tmpWorker.Dispose();
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Stops the CodeWorker. NOTE we are NOT in the form thread here
        /// you CANNOT operate on any screen controls in here.
        /// </summary>
        private void StopCodeWorker()
        {
            try
            {
                if(codeWorker!=null)
                {
                    codeWorker.CancelAsync();
                }
            }
            catch { }
        }

        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the controls on the form
        /// </summary>
        private void SetupWalnutControls()
        {
            SyncAllWalnutControlsToScreenState(false);
            // give this a call to set the appearance correctly
            SetWaldosEnabledCheckBoxAccordingToState();
            SetRemoteConnectionCheckBoxVisuals(false);
        }

 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles presses on the buttonTestConnection button
        /// </summary>
        private void buttonTestConnection_Click(object sender, EventArgs e)
        {
            LogMessage("buttonTestConnection_Click");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return;
            }

            // test the connection
            ConnectionTest();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends the data from the screen to the client
        /// </summary>
        private void SendDataFromScreenToClient()
        {
            if (dataTransporter == null)
            {
                LogMessage("SendDataFromScreenToClient, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("SendDataFromScreenToClient, Not connected");
                return;
            }

            // get the server client data from the screen
            ServerClientData scData = GetSCDataFromScreen("Data from server to client");

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a objectList to a srcTgtList. Just looks for the first red and green
        /// object we can find and returns those
        /// </summary>
        /// <param name="objList">the objList to convert</param>
        /// <returns>a populated List<SrcTgtData> container, will never be null, might be empty</returns>
        private List<SrcTgtData> ConvertObjectListToSrcTgtList(List<ColoredRotatedObject> objList)
        {
            ColoredRotatedObject greenObj = null;
            ColoredRotatedObject redObj = null;

            List<SrcTgtData> outList = new List<SrcTgtData>();

            // sanity check
            if (objList == null) return outList;
            if (objList.Count == 0) return outList;

            // we consider the green obj to be the tgt and red to be the src, look for them
            foreach (ColoredRotatedObject foundObj in objList)
            {
                // we just take the first one we find
                if ((greenObj == null) && (foundObj.ObjColor == KnownColor.Green)) greenObj = foundObj;
                if ((redObj == null) && (foundObj.ObjColor == KnownColor.Red)) redObj = foundObj;
            }
            // create our output class
            SrcTgtData workingSrcTgt = new SrcTgtData();

            // if we found either a red or a green object then add them
            if (greenObj != null) workingSrcTgt.TgtPoint = greenObj.CenterPoint;
            if (redObj != null) workingSrcTgt.SrcPoint = redObj.CenterPoint;

            // do we have at least one of these? if not return empty list
            if(workingSrcTgt.IsMinimallyPopulated() == false) return outList;
            // we do, add it
            outList.Add(workingSrcTgt);
            // return it
            return outList;
         }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the server client data from the screen and returns the populated
        /// container
        /// </summary>
        /// <returns>a populated ServerClientData container</returns>
        private ServerClientData GetSCDataFromScreen(string scDataText)
        {
            //List<ColoredRotatedObject> rectList = null;

            //if (scDataText == null) scDataText = "Rect Data from Server to Client";
            ServerClientData scData = new ServerClientData(scDataText);

            //// check if we are recognizing
            //if (RecognitionTransform == null) return scData;
            //else
            //{
            //    rectList = RecognitionTransform.IdentifiedObjects;
            //    // tell it we are carrying a rect list
            //    scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.RECT_DATA;
            //    scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);
            //}

            //// get the global enable
            //scData.RectList = rectList;

            return scData;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles inbound data events.
        /// 
        /// NOTE: You are not on the Main Form Thread here.
        /// </summary>
        private void ServerClientDataEventHandler(object sender, ServerClientData scData)
        {
            if (scData == null)
            {
                LogMessage("ServerClientDataEventHandler scData==null");
                return;
            }

            // Ok, you probably already know this but I'll note it here because this is so important
            // You do NOT want to update any form controls from a thread that is not the forms main
            // thread. Very odd, intermittent and hard to debug problems will result. Even if your 
            // handler does not actually update any form controls do not do it! Sooner or later you 
            // or someone else will make changes that calls something that eventually updates a
            // form or control and then you will have introduced a really hard to find bug.

            // So, we always use the InvokeRequired...Invoke sequence to get us back on the form thread
            if (InvokeRequired == true)
            {
                // call ourselves again but this time be on the form thread.
                Invoke(new TCPDataTransporter.ServerClientDataEvent_Delegate(ServerClientDataEventHandler), new object[] { sender, scData });
                return;
            }

            // Now we KNOW we are on the main form thread.

            // what type of data is it
            if (scData.DataContent == ServerClientDataContentEnum.USER_DATA)
            {
                // it is user defined data, log it
                LogMessage("ServerClientDataEventHandler dataStr=" + scData.DataStr);
                // display it
                AppendDataToConnectionTrace("IN: dataStr=" + scData.DataStr);
            }
            else if (scData.DataContent == ServerClientDataContentEnum.CONNECTION_TEST_ACK)
            {
                // it is just a connection test ACK, log it
                LogMessage("ServerClientDataEventHandler " + scData.DataStr);
                // display it
                AppendDataToConnectionTrace("IN: " + scData.DataStr);
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_CONNECT)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_CONNECT");
                // display it
                AppendDataToConnectionTrace("IN: REMOTE_CONNECT");
                // set the screen
                SetScreenVisualsBasedOnConnectionState(true);
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_DISCONNECT)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_DISCONNECT");
                // display it
                AppendDataToConnectionTrace("IN: REMOTE_DISCONNECT");
                // set the screen
                SetScreenVisualsBasedOnConnectionState(false);
                // shut things down on our end
                ShutdownDataTransporter();
            }
            else
            {
                LogMessage("ServerClientDataEventHandler unknown DataContent = " + scData.DataContent.ToString());
                Console.WriteLine("ServerClientDataEventHandler  unknown DataContent = " + scData.DataContent.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Shuts down the data transporter safely
        /// </summary>
        private void ShutdownDataTransporter()
        {
            LogMessage("ShutdownDataTransporter called");

            // shutdown the data transporter
            if (dataTransporter != null)
            {
                // are we connected? we want to tell the client to exit 
                if (IsConnected() == true)
                {
                    // disable all waldos
                    DisableAllWaldos();
                }

                dataTransporter.Shutdown();
                dataTransporter = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Disables all Waldos
        /// </summary>
        private void DisableAllWaldos()
        {
            LogMessage("DisableAllWaldos called");
            // do we have a data transporter
            if (dataTransporter == null)
            {
                LogMessage("DisableAllWaldos dataTransporter == null");
                return;
            }

            // shutdown all waldos
            ServerClientData scData = new ServerClientData("Disable all Waldos");
            scData.UserDataContent = UserDataContentEnum.NO_DATA;
            scData.Waldo_Enable = 0;
            // send it
            dataTransporter.SendData(scData);

            // display it
            AppendDataToConnectionTrace("OUT: Disable All Waldos requested");

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we have a connection. 
        /// </summary>
        private bool IsConnected()
        {
            if (dataTransporter == null) return false;
            if (dataTransporter.IsConnected() == false) return false;
            if (buttonTestConnection.Enabled == false) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends a connection TEST message
        /// </summary>
        private void ConnectionTest()
        {
            LogMessage("ConnectionTest called");
            // do we have a data transporter
            if (dataTransporter == null)
            {
                LogMessage("DisableAllWaldos dataTransporter == null");
                return;
            }

            // shutdown all waldos
            ServerClientData scData = new ServerClientData("Connection Test");
            scData.DataContent = ServerClientDataContentEnum.CONNECTION_TEST;
            scData.UserDataContent = UserDataContentEnum.NO_DATA;
            scData.Waldo_Enable = 0;
            // send it
            dataTransporter.SendData(scData);

            // display it
            AppendDataToConnectionTrace("OUT: Connection test requested");

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the screen visuals based on the connections state
        /// </summary>
        private void SetScreenVisualsBasedOnConnectionState(bool connectionState)
        {
            if (connectionState == true)
            {
                buttonTestConnection.Enabled = true;
            }
            else
            {
                buttonTestConnection.Enabled = false;
            }
            SetRemoteConnectionCheckBoxVisuals(connectionState);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Appends data to our data connection trace
        /// </summary>
        private void AppendDataToConnectionTrace(string dataToAppend)
        {
            if ((dataToAppend == null) || (dataToAppend.Length == 0)) return;
            textBoxDataTrace.Text = textBoxDataTrace.Text + "\r\n" + dataToAppend;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Synchronizes all waldo controls to the screen state
        /// </summary>
        /// <param name="enableState">if true they are all enabled, false they are not</param>
        private void SyncAllWalnutControlsToScreenState(bool enableState)
        {
            // set them now
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Toggles a stepper motor 0 on and off. Hard Coded mostly for testing
        /// </summary>
        private void checkBoxTestStepper0_CheckedChanged(object sender, EventArgs e)
        {
            //LogMessage("checkBoxTestStepper0_CheckedChanged");

            //if (dataTransporter == null)
            //{
            //    OISMessageBox("No data transporter");
            //    return;
            //}
            //if (IsConnected() == false)
            //{
            //    OISMessageBox("Not connected");
            //    return;
            //}

            //// create the data container
            //ServerClientData scData = new ServerClientData();
            //scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            //scData.UserDataContent = UserDataContentEnum.NO_DATA;

            //// set up some default speeds and dirs
            //scData.Step0_StepSpeed = Utils.ConvertHzToCycles(DEFAULT_STEPPER_SPEED_HZ/4);
            //if(checkBoxTestStepperDir.Checked==true)
            //{
            //    scData.Step0_DirState = 1;
            //}
            //else
            //{
            //    scData.Step0_DirState = 0;

            //}
            //scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked?1:0);

            //// set stepper 0 state according to the screen
            //if (checkBoxTestStepper0.Checked == true)
            //{
            //    scData.Step0_Enable = 1;
            //    scData.DataStr = "Toggle Stepper Motor 0 State On";
            //    scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.STEP0_DATA;
            //}
            //else
            //{
            //    scData.Step0_Enable = 0;
            //    scData.DataStr = "Toggle Stepper Motor 0 State Off";
            //    scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.STEP0_DATA;
            //}

            //// display it
            //AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            //// send it
            //dataTransporter.SendData(scData);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends a mark request to the client. The client should mark the console
        /// output and the log. Used for diagnostics
        /// </summary>
        private void buttonClientMark_Click(object sender, EventArgs e)
        {
            LogMessage("buttonClientMark_Click");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return;
            }

            // create the data container
            ServerClientData scData = new ServerClientData();
            scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            scData.UserDataContent = UserDataContentEnum.FLAG_DATA;
            scData.UserFlag = UserDataFlagEnum.MARK_FLAG;

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends a shutdown request to the client. The client should mark the console
        /// output and the log and then exit. 
        /// </summary>
        private void buttonClientExit_Click(object sender, EventArgs e)
        {
            LogMessage("buttonClientExit_Click");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return;
            }

            // create the data container
            ServerClientData scData = new ServerClientData();
            scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            scData.UserDataContent = UserDataContentEnum.FLAG_DATA;
            scData.UserFlag = UserDataFlagEnum.EXIT_FLAG;
            scData.UserFlag = UserDataFlagEnum.EXIT_FLAG;

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        // detect changes on our draw square radio buttons, these are just temporary
        // for testing
        //private void radioButtonLoc1_CheckedChanged(object sender, EventArgs e)
        //{
        //    // some sanity checks
        //    if (radioButtonLoc1.Checked == false) return;
        //    if (ImageOverlayTransform == null) return;
        //    if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
        //    // set the rectangle
        //    (ImageOverlayTransform as MFTOverlayImage_GS).SetRectangle(new Point(150, 150));
        //}

        //private void radioButtonLoc2_CheckedChanged(object sender, EventArgs e)
        //{
        //    // some sanity checks
        //    if (radioButtonLoc2.Checked == false) return;
        //    if (ImageOverlayTransform == null) return;
        //    if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
        //    // set the rectangle
        //    (ImageOverlayTransform as MFTOverlayImage_GS).SetRectangle(new Point(200, 200));

        //}

        //private void radioButtonLoc3_CheckedChanged(object sender, EventArgs e)
        //{
        //    // some sanity checks
        //    if (radioButtonLoc3.Checked == false) return;
        //    if (ImageOverlayTransform == null) return;
        //    if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
        //    // set the rectangle
        //    (ImageOverlayTransform as MFTOverlayImage_GS).SetRectangle(new Point(350, 100));

        //}

        //private void radioButtonLoc4_CheckedChanged(object sender, EventArgs e)
        //{
        //    // some sanity checks
        //    if (radioButtonLoc4.Checked == false) return;
        //    if (ImageOverlayTransform == null) return;
        //    if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
        //    // set the rectangle
        //    (ImageOverlayTransform as MFTOverlayImage_GS).SetRectangle(new Point(350, 200));

        //}

        //private void radioButtonLocNone_CheckedChanged(object sender, EventArgs e)
        //{
        //    // some sanity checks
        //    if (radioButtonLocNone.Checked == false) return;
        //    if (ImageOverlayTransform == null) return;
        //    if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
        //    // set the rectangle
        //    (ImageOverlayTransform as MFTOverlayImage_GS).SetRectangle(new Point(-1, -1));

        //}

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draw on the image overlay
        /// </summary>
        private void checkBoxDrawImageOverlay_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawImageOverlay.Checked == false )
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).SetOverlayImage(null,null);
                return;
            }
            // set the overlay image - just hard coded for now
            (ImageOverlayTransform as MFTOverlayImage_GS).SetOverlayImage(OVERLAY_IMAGE_FILENAME, TRACKER_IMAGE_FILENAME);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Find nearest Green as target
        /// </summary>
        private void checkBoxFindGreen_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawImageOverlay.Checked == false)
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).SetOverlayImage(null, null);
                return;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a checked changed event on the PWMA test option
        /// </summary>
        /// 
        private void checkBoxPWMAEnable_CheckedChanged(object sender, EventArgs e)
        {
            LogMessage("checkBoxPWMAEnable_CheckedChanged");

            // send the PWMA test data
            SendPWMATestData();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle an update on the PWMA speed test option
        /// </summary>
        /// 
        private void textBoxPWMASpeed_TextChanged(object sender, EventArgs e)
        {
            LogMessage("textBoxPWMASpeed_TextChanged");

            // send the PWMA test data
            SendPWMATestData();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle an update on the PWMA dir test option
        /// </summary>
        /// 
        private void checkBoxPWMADir_CheckedChanged(object sender, EventArgs e)
        {
            LogMessage("textBoxPWMASpeed_TextChanged");

            // send the PWMA test data
            SendPWMATestData();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a checked changed event on the PWMB test option
        /// </summary>
        /// 
        private void checkBoxPWMBEnable_CheckedChanged(object sender, EventArgs e)
        {
            LogMessage("checkBoxPWMBEnable_CheckedChanged");

            // send the PWMB test data
            SendPWMBTestData();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle an update on the PWMB speed test option
        /// </summary>
        /// 
        private void textBoxPWMBSpeed_TextChanged(object sender, EventArgs e)
        {
            LogMessage("textBoxPWMBSpeed_TextChanged");

            // send the PWMB test data
            SendPWMBTestData();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle an update on the PWMB dir test option
        /// </summary>
        /// 
        private void checkBoxPWMBDir_CheckedChanged(object sender, EventArgs e)
        {
            LogMessage("textBoxPWMBSpeed_TextChanged");

            // send the PWMB test data
            SendPWMBTestData();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populates a ServerClientData object with PWMB test data and sends it
        /// </summary>
        /// <returns>a ServerClientData object</returns>
        /// 
        private void SendPWMBTestData()
        {
            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return;
            }

            // create the data container
            ServerClientData scData = new ServerClientData();

            scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            scData.UserDataContent = UserDataContentEnum.NO_DATA;

            // set up some default speeds and dirs
            scData.PWMB_PWMPercent = GetPWMBSpeed();
            if (checkBoxPWMBDir.Checked == true)
            {
                scData.PWMB_DirState = 1;
            }
            else
            {
                scData.PWMB_DirState = 0;

            }
            scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);

            // set PWMB speed according to the screen
            if (checkBoxPWMBEnable.Checked == true)
            {
                scData.PWMB_Enable = 1;
                scData.DataStr = "Set PWM B State On";
                scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMB_DATA;
            }
            else
            {
                scData.PWMB_Enable = 0;
                scData.DataStr = "Set PWM B State Off";
                scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMB_DATA;
            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Populates a ServerClientData object with PWMA test data and sends it
        /// </summary>
        /// <returns>a ServerClientData object</returns>
        /// 
        private void SendPWMATestData()
        {
            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return;
            }

            // create the data container
            ServerClientData scData = new ServerClientData();

            scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            scData.UserDataContent = UserDataContentEnum.NO_DATA;

            // set up some default speeds and dirs
            scData.PWMA_PWMPercent = GetPWMASpeed();
            if (checkBoxPWMADir.Checked == true)
            {
                scData.PWMA_DirState = 1;
            }
            else
            {
                scData.PWMA_DirState = 0;

            }
            scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);

            // set PWMA speed according to the screen
            if (checkBoxPWMAEnable.Checked == true)
            {
                scData.PWMA_Enable = 1;
                scData.DataStr = "Set PWM A State On";
                scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMA_DATA;
            }
            else
            {
                scData.PWMA_Enable = 0;
                scData.DataStr = "Set PWM A State Off";
                scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMA_DATA;
            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

            return;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the speed for PWM A
        /// </summary>
        private uint GetPWMASpeed()
        {
            try
            {
                return Convert.ToUInt32(textBoxPWMASpeed.Text);
            }
            catch
            {
                return 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the speed for PWM B
        /// </summary>
        private uint GetPWMBSpeed()
        {
            try
            {
                return Convert.ToUInt32(textBoxPWMASpeed.Text);
            }
            catch
            {
                return 0;
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            //if (ImageOverlayTransform == null) return;

            //using (SolidBrush brsh = new SolidBrush(ColorTranslator.FromHtml("#ff00ff00")))
            //{
            //    (ImageOverlayTransform as MFTOverlayImage_GS).DrawCircleOnOverlay(brsh, new Point(125,125), 25, 2);
            //}

            //Graphics overlayGraphicsObj = (ImageOverlayTransform as MFTOverlayImage_GS).OverlayGraphicsObj;
            //if(overlayGraphicsObj == null) return;
            
            //using (Brush brsh = new SolidBrush(ColorTranslator.FromHtml("#ff00ff00")))
            //{
            //    overlayGraphicsObj.FillEllipse(brsh, 100, 100, 50, 50);
            //    overlayGraphicsObj.FillEllipse(whiteTransparentBrush, 102, 102, 46, 46);
            //}
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a green circle. The overlay must be activated and loaded
        /// </summary>
        private void buttonDrawGreenCircleAtPoint_Click(object sender, EventArgs e)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            // get the draw point off the screen
            Point centerPoint = GreenCircleManualDrawCenterPoint;
            // get the radius off the screen
            int radius = GreenCircleRadius;
            // draw the circle
            DrawCircleAtPoint(centerPoint, radius, HTML_GREEN);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears the green circles and everything else on the overlay
        /// </summary>
        private void buttonDrawClearOverlay_Click(object sender, EventArgs e)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            (ImageOverlayTransform as MFTOverlayImage_GS).ClearOverlay();
            // also clear this
            greenCircleDrawCount = 0;
            redLineDrawCount = 0;
            LastGreenPoint = new Point();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up to draw a green circle for every mouse click for a specified
        /// number of mouse clicks
        /// </summary>
        private void buttonDrawGreenCircleAtClicks_Click(object sender, EventArgs e)
        {
            greenCircleDrawCount = 0;

            try
            {
                // get the value from the screen, and place it in this flag
                greenCircleDrawCount = GreenCircleDrawMouseClicks;
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up to draw a red line for every mouse click for a specified
        /// number of mouse clicks
        /// </summary>
        private void buttonDrawRedLineAtClicks_Click(object sender, EventArgs e)
        {
            redLineDrawCount = 0;

            try
            {
                // get the value from the screen, and place it in this flag
                redLineDrawCount = RedLineDrawMouseClicks;
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a red line through a point. The overlay must be activated and loaded
        /// </summary>
        private void buttonDrawRedLineThroughPoint_Click(object sender, EventArgs e)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            // get the draw point off the screen
            Point centerPoint = RedLineManualDrawThroughPoint;
            // get the width off the screen
            int radius = RedLineWidth;
            // do we want it to go vertical
            bool wantVert = RedLineWantVertLine;
            // draw the line
            DrawRedLineThroughPoint(centerPoint, radius, wantVert);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a red line through a point (on the overlay actually) at a specified point
        /// and of a specified width
        /// </summary>
        /// <param name="pointIn">the point</param>
        /// <param name="width">the line width</param>
        /// <param name="wantVert">if true we draw a vertical line, otherwise horiz.</param>
        private void DrawRedLineThroughPoint(Point pointIn, int width, bool wantVert)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (pointIn == null) return;
            if (width <= 0) return;

            using (Pen xPen = new Pen(Color.Red, width))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).DrawLineThroughPointOnOnOverlay(xPen, pointIn, wantVert);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a circle on the screen (on the overlay actuall) at a specified point
        /// and of a specified radius
        /// </summary>
        /// <param name="pointIn">the point</param>
        /// <param name="radius">the radius</param>
        /// <param name="colorAsHTML">the color as an HTML string. IE "#ff00ff00" is green</param>
        private void DrawCircleAtPoint(Point pointIn, int radius, string colorAsHTML)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (pointIn == null) return;
            if (radius <= 0) return;
            if(colorAsHTML==null) return;
            if (colorAsHTML.Length == 0) return;

            using (SolidBrush brsh = new SolidBrush(ColorTranslator.FromHtml(colorAsHTML)))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).FillCircularRegionOnOverlay(brsh, pointIn, radius);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the number of mouse clicks we can draw green circles on
        /// </summary>
        private int GreenCircleDrawMouseClicks
        {
            get
            {
                try
                {
                    // get the data off the screen
                    return Convert.ToInt32(textBoxGreenCircleDrawMouseClicks.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxGreenCircleDrawMouseClicks.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the number of mouse clicks we can draw red lines on
        /// </summary>
        private int RedLineDrawMouseClicks
        {
            get
            {
                try
                {
                    // get the data off the screen
                    return Convert.ToInt32(textBoxRedLineDrawMouseClicks.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxRedLineDrawMouseClicks.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the red line width. 
        /// </summary>
        private int RedLineWidth
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxRedLineWidth.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxRedLineWidth.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the green circle radius. 
        /// </summary>
        private int GreenCircleRadius
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxGreenCircleRadius.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxGreenCircleRadius.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the green circle centerpoint. 
        /// </summary>
        private Point GreenCircleManualDrawCenterPoint
        {
            get
            {
                return Utils.ConvertBracketTextToPoint(textBoxDrawGreenCircleXY.Text);
            }
            set
            {
                // simple comma separated value
                textBoxDrawGreenCircleXY.Text=value.X.ToString()+","+value.Y.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the red line through point. 
        /// </summary>
        private Point RedLineManualDrawThroughPoint
        {
            get
            {
                return Utils.ConvertBracketTextToPoint(textBoxDrawRedLineXY.Text);
            }
            set
            {
                // simple comma separated value
                textBoxDrawRedLineXY.Text = value.X.ToString() + "," + value.Y.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the red line should be vertical flag
        /// </summary>
        private bool RedLineWantVertLine
        {
            get
            {
                if (radioButtonDrawRedLineVert.Checked == true) return true;
                else return false;
            }
            set
            {
                if (value == true) radioButtonDrawRedLineVert.Checked = true;
                else radioButtonDrawRedLineHoriz.Checked = true;
            }
        }

        void BitmapTest()
        {
            //int testX = 100;
            //int testY = 100;
            //int radius = 10;

            //SolidBrush blackBrush = new SolidBrush(Color.Red);
            //SolidBrush whiteBrush = new SolidBrush(Color.Orange);

            //DirectBitmap overlayImage = new DirectBitmap(@"D:\Dump\FPathData\FPath_Ex004\AllGreen640x480.png");

            //// set up the bitmap for transparency
            //overlayImage.Bitmap.MakeTransparent(Color.White);

            //Graphics bitmapGraphicsObj = Graphics.FromImage(overlayImage.Bitmap);

            //bitmapGraphicsObj = Graphics.FromImage(overlayImage.Bitmap);
            //bitmapGraphicsObj.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            //// we have to flip the Y axis or the graphics draw calls will be inverted
            //bitmapGraphicsObj.ScaleTransform(1.0F, -1.0F);
            //bitmapGraphicsObj.TranslateTransform(0.0F, -(float)overlayImage.Height);

            //Color color1 = overlayImage.GetPixel(testX, testY);

            //// Transparent fill circle on screen.
            //bitmapGraphicsObj.FillEllipse(whiteBrush, testX - radius, testY - radius, radius * 2, radius * 2);

            ////      overlayImage.SetPixel(testX, testY, Color.Red);
            ////    overlayImage.SetPixelInvertedY(testX, testY, Color.Blue);

            //Color color2 = overlayImage.GetPixel(testX, testY);
            //Color color3 = overlayImage.Bitmap.GetPixel(testX, testY);

            //Color color4 = overlayImage.GetPixel(testX, 480 - testY);
            //Color color5 = overlayImage.Bitmap.GetPixel(testX, 480 - testY);
            //Color color6 = overlayImage.GetPixel(testX, 479 - testY);
            //Color color7 = overlayImage.Bitmap.GetPixel(testX, 479 - testY);

            //Color color8 = overlayImage.GetPixelInvertedY(testX, testY);

            //// it appears that 

            //int foo = 1;

        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on our transparent overlay on the EVR display. This allows
        /// us to get mouse clicks from it. 
        /// </summary>
        private void ctlTransparentControl1_MouseClick(object sender, MouseEventArgs e)
        {
            //       MessageBox.Show("Mouse Click (" + e.X.ToString() + "," + e.Y.ToString()+")");

            // we get the click point Y inverted and non Inverted. The ConvertPoint call also takes care of 
            // stretched images. It will give true non stretched pixel hits so it can be referenced 
            // back to the video frames
            Point outPointNonInverted = this.ctlTransparentControl1.ConvertPoint(new Point(e.X, e.Y), new Size(DEFAULT_VIDEO_FRAME_WIDTH, DEFAULT_VIDEO_FRAME_HEIGHT), false);
            Point outPointInverted = this.ctlTransparentControl1.ConvertPoint(new Point(e.X, e.Y), new Size(DEFAULT_VIDEO_FRAME_WIDTH, DEFAULT_VIDEO_FRAME_HEIGHT), true);

            // Now do various things with mouse clicks

            // ####
            // Get the color of the pixel and put it over on our  Utils tab. This is just a useful
            // little utility for debugging and programming

            textBoxRGBAPixelColorLocInverted.Text = outPointInverted.X.ToString() + "," + outPointInverted.Y.ToString();
            textBoxRGBAPixelColorLocNonInverted.Text = outPointNonInverted.X.ToString() + "," + outPointNonInverted.Y.ToString();
            // now get the color. We have access to the DisplayPanelHandle down in the ctlTantaEVRStreamDisplay control
            Color outColor = Utils.GetPixelFromHandle(ctlTantaEVRStreamDisplay1.DisplayPanelHandle, outPointNonInverted.X, outPointNonInverted.Y);
            // set it on the utils tab
            textBoxRGBAPixelColor.Text = outColor.R.ToString() + "," + outColor.G.ToString() + "," + outColor.B.ToString() + " (" + outColor.B.ToString() + ")";

            // ####
            // Now the pixel count difference between mouse clicks

            // we always put the new click in point 2 and the previous click in click 1, the point data is stored in the tag
            textBoxDistanceClick1.Tag = textBoxDistanceClick2.Tag;
            textBoxDistanceClick2.Tag = outPointInverted;
            // set the text, always switch them over
            textBoxDistanceClick1.Text = "";
            textBoxDistanceClick2.Text = "";
            textBoxDistanceHoriz.Text = "";
            textBoxDistanceVert.Text = "";
            if (textBoxDistanceClick1.Tag != null)
            {
                textBoxDistanceClick1.Text = Utils.ConvertPointToBracketText((Point)textBoxDistanceClick1.Tag);
            }
            if (textBoxDistanceClick2.Tag != null) textBoxDistanceClick2.Text = Utils.ConvertPointToBracketText((Point)textBoxDistanceClick2.Tag);
            // if we have two measurements then calc the difference
            if ((textBoxDistanceClick1.Tag != null) && (textBoxDistanceClick2.Tag != null))
            {
                int c1X = ((Point)textBoxDistanceClick1.Tag).X;
                int c1Y = ((Point)textBoxDistanceClick1.Tag).Y;
                int c2X = ((Point)textBoxDistanceClick2.Tag).X;
                int c2Y = ((Point)textBoxDistanceClick2.Tag).Y;
                int xDist = Math.Abs(c2X - c1X);
                int yDist = Math.Abs(c2Y - c1Y);
                textBoxDistanceHoriz.Text = xDist.ToString();
                textBoxDistanceVert.Text = yDist.ToString();
                textBoxDistTotal.Text = Math.Round(Math.Sqrt((xDist * xDist) + (yDist * yDist)),0).ToString();
            }

            // ####
            // Now do we need to draw green circles?
            if(greenCircleDrawCount>0)
            {
                // yes, we do.
                // draw the circle
                DrawCircleAtPoint(outPointInverted, GreenCircleRadius, HTML_GREEN);
                greenCircleDrawCount--;
                // record this
                LastGreenPoint = outPointInverted;
            }

            // ####
            // Now do we need to draw red lines?
            if (redLineDrawCount > 0)
            {
                // yes, we do.
                // draw the line
                DrawRedLineThroughPoint(outPointInverted, RedLineWidth, RedLineWantVertLine);
                redLineDrawCount--;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the run button of the Ex008 panel 
        /// </summary>
        private void buttonStepperControl008Run_Click(object sender, EventArgs e)
        {
            LogMessage("buttonStepperControl008Run_Click");

            if (dataTransporter == null)
            {
                LogMessage("buttonStepperControl008Run_Click, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("buttonStepperControl008Run_Click, Not connected");
                return;
            }

            //  get the data off the screen
            ServerClientData scData = GetTestEx008StepperDataFromScreen(StepperIDEnum.STEPPER_0, true, false, 1);
            if (scData == null)
            {
                LogMessage("buttonStepperControl008Run_Click, scData == null");
                return;

            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the run stop button of the Ex008 panel 
        /// </summary>
        private void buttonStepperControl008Stop_Click(object sender, EventArgs e)
        {
            LogMessage("buttonStepperControl008Stop_Click");

            if (dataTransporter == null)
            {
                LogMessage("buttonStepperControl008Stop_Click, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("buttonStepperControl008Stop_Click, Not connected");
                return;
            }

            //  get the data off the screen
            ServerClientData scData = GetTestEx008StepperDataFromScreen(StepperIDEnum.STEPPER_0, false, false, 1);
            if (scData == null)
            {
                LogMessage("buttonStepperControl008Stop_Click, scData == null");
                return;

            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the nudge 1 button of the Ex008 panel 
        /// </summary>
        private void buttonStepperControl008Nudge1_Click(object sender, EventArgs e)
        {
            LogMessage("buttonStepperControl008Nudge1_Click");

            if (dataTransporter == null)
            {
                LogMessage("buttonStepperControl008Nudge1_Click, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("buttonStepperControl008Nudge1_Click, Not connected");
                return;
            }

            // we are hard coded to 1 step here
            ServerClientData scData = GetTestEx008StepperDataFromScreen(StepperIDEnum.STEPPER_0, true, true, 1);
            if(scData == null)
            {
                LogMessage("buttonStepperControl008Nudge1_Click, scData == null");
                return;

            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a click on the checkBoxStepper0Test checkBox
        /// </summary>
        private void checkBoxStepper0Test_CheckedChanged(object sender, EventArgs e)
        {
            if (dataTransporter == null)
            {
                LogMessage("checkBoxStepper0Test_CheckedChanged, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("checkBoxStepper0Test_CheckedChanged, Not connected");
                return;
            }

            // get the server client data from the screen
            ServerClientData scData = GetTestStepper0DataFromScreen();
            if (scData == null)
            {
                LogMessage("checkBoxStepper0Test_CheckedChanged, scData == null");
                return;
            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a change of state on the test stepper motor CW radio button
        /// </summary>
        private void radioButtonStepper0TestCW_CheckedChanged(object sender, EventArgs e)
        {
            if (dataTransporter == null)
            {
                LogMessage("radioButtonStepper0TestCW_CheckedChanged, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("radioButtonStepper0TestCW_CheckedChanged, Not connected");
                return;
            }

            // get the server client data from the screen
            ServerClientData scData = GetTestStepper0DataFromScreen();
            if (scData == null)
            {
                LogMessage("radioButtonStepper0TestCW_CheckedChanged, scData == null");
                return;
            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a change of state on the test stepper motor CCW radio button
        /// </summary>
        private void radioButtonStepper0TestCCW_CheckedChanged(object sender, EventArgs e)
        {
            if (dataTransporter == null)
            {
                LogMessage("radioButtonStepper0TestCCW_CheckedChanged, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("radioButtonStepper0TestCCW_CheckedChanged, Not connected");
                return;
            }

            // get the server client data from the screen
            ServerClientData scData = GetTestStepper0DataFromScreen();
            if (scData == null)
            {
                LogMessage("radioButtonStepper0TestCCW_CheckedChanged, scData == null");
                return;
            }

            // display it
            AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the stepper 0 test data from the screen and returns the populated
        /// container
        /// </summary>
        /// <returns>a populated ServerClientData container</returns>
        private ServerClientData GetTestStepper0DataFromScreen()
        {
            const uint DEFAULT_STEPPER_TEST_SPEED_HZ = 10;

            LogMessage("GetTestStepper0DataFromScreen");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return null;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return null;
            }

            // create the data container
            ServerClientData scData = new ServerClientData();
            scData.DataStr = "Stepper 0 Test Commands";
            scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            scData.UserDataContent = UserDataContentEnum.STEPPER_CONTROL;

            // create a stepper control container
            SCData_StepperControl stepperControl = new SCData_StepperControl();
            // tell it which stepper we are operating on
            stepperControl.Stepper_ID = StepperIDEnum.STEPPER_0;  // always this

            // set the speed
            stepperControl.Stepper_StepSpeed = DEFAULT_STEPPER_TEST_SPEED_HZ;
            stepperControl.Num_Steps = SCData_StepperControl.INFINITE_STEPS;

            // set the direction
            if (radioButtonStepper0TestCW.Checked == true) stepperControl.Stepper_DirState = 1;
            else stepperControl.Stepper_DirState = 0;

            // set enable state according to the screen
            if (checkBoxStepper0Test.Checked == true)
            {
                stepperControl.Stepper_Enable = 1;
                scData.DataStr = "Toggle Stepper Motor 0 State On";
            }
            else
            {
                stepperControl.Stepper_Enable = 0;
                scData.DataStr = "Toggle Stepper Motor 0 State Off";
            }

            // always turn the waldos state correctly
            scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);

            // make up a list for the SCData_StepperControl even though we only have one
            List< SCData_StepperControl > stControlList = new List< SCData_StepperControl >();
            // put it in the data container
            scData.StepperControlList = stControlList;
            // add the one we have to the list
            stControlList.Add( stepperControl );
            return scData;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the Ex008 stepper data from the screen and returns the populated
        /// container
        /// </summary>
        /// <param name="stepperID">the stepper ID we operate on</param>
        /// <param name="stepperEnable">if true we enable the stepper</param>
        /// <param name="wantNumStepsOverride">nz, override the number of steps with numSteps</param>
        /// <param name="numSteps">the number of steps if overriding</param>
        /// <returns>a populated ServerClientData container</returns>
        private ServerClientData GetTestEx008StepperDataFromScreen(StepperIDEnum stepperID, bool stepperEnable, bool wantNumStepsOverride, uint numSteps)
        {
            LogMessage("GetTestEx008StepperDataFromScreen");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return null;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return null;
            }

            // create the data container
            ServerClientData scData = new ServerClientData();
            scData.DataStr = "Ex0008 Stepper Commands";
            scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            scData.UserDataContent = UserDataContentEnum.STEPPER_CONTROL;

            // create a stepper control container
            SCData_StepperControl stepperControl = new SCData_StepperControl();
            // tell it which stepper we are operating on
            stepperControl.Stepper_ID = stepperID;  

            // set the speed
            try
            {
                stepperControl.Stepper_StepSpeed = Convert.ToUInt32(textBoxStepperControl008StepsPerSecond.Text);
            }
            catch(Exception ex)
            {
                OISMessageBox("Error converting step speed: "+ex.Message);
                return null;
            }

            string tmpNumSteps ="0";
            // get the number of steps. Do we have an override
            if (wantNumStepsOverride == false)
            {
                // no, we do not, set it this way
                tmpNumSteps = textBoxStepperControl008NumSteps.Text;
            }
            else tmpNumSteps = numSteps.ToString();
            // now do the the conversion on the true value the user wants
            try
            {
                stepperControl.Num_Steps = Convert.ToUInt32(tmpNumSteps);
            }
            catch (Exception ex)
            {
                OISMessageBox("Error converting numSteps: " + ex.Message);
                return null;

            }

            // set the direction
            if (radioButtonStepperControl008DirCW.Checked == true) stepperControl.Stepper_DirState = 1;
            else stepperControl.Stepper_DirState = 0;

            // enable the stepper
            if (stepperEnable == true) stepperControl.Stepper_Enable = 1;
            else stepperControl.Stepper_Enable = 0;
            scData.DataStr = "Ex008 Set Stepper Motor State";

            // always turn the waldos state correctly
            scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);

            // make up a list for the SCData_StepperControl even though we only have one
            List<SCData_StepperControl> stControlList = new List<SCData_StepperControl>();
            // put it in the data container
            scData.StepperControlList = stControlList;
            // add the one we have to the list
            stControlList.Add(stepperControl);
            return scData;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Set the commands for a stepper motor
        /// </summary>
        /// <param name="numSteps">the number of steps, can use SCData_StepperControl.INFINITE_STEPS</param>
        /// <param name="stepDir">step direction 0 or 1</param>
        /// <param name="stepEna">stepper enable state 0 or 1</param>
        /// <param name="stepID">the id of the stepper</param>
        /// <param name="stepSpeedHz">the stepper speed in Hz</param>
        private SCData_StepperControl SetCommandsForStepper(StepperIDEnum stepID, uint stepEna, uint stepSpeedHz, uint numSteps, uint stepDir)
        {
            LogMessage("SetCommandsForStepper");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return null;
            }
            if (IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return null;
            }

            // create a stepper control container
            SCData_StepperControl stepperControl = new SCData_StepperControl();
            // tell it which stepper we are operating on
            stepperControl.Stepper_ID = stepID; 

            // set the speed, steps, dir etc
            stepperControl.Stepper_StepSpeed = stepSpeedHz;
            stepperControl.Num_Steps = numSteps;
            stepperControl.Stepper_DirState = stepDir;
            stepperControl.Stepper_Enable = 1;

            // set enable state 
            stepperControl.Stepper_Enable = stepEna;
           
            return stepperControl;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a click on the Waldos enabled check box
        /// </summary>
        private void checkBoxWaldosEnabled_CheckedChanged(object sender, EventArgs e)
        {
            SetWaldosEnabledCheckBoxAccordingToState();
            // are we newly disabled?
            if (checkBoxWaldosEnabled.Checked == false)
            {
                // yes, we are, we must force a turn off of all waldos. Note that 
                // turning off all waldos requires each one to be individually re-enabled
                DisableAllWaldos();
            }
            else
            {
                // there is no re-enable here. The individual command sent always include
                // the waldo enable state.
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the visual appearance of the Waldos enabled checkbox according to 
        /// the state
        /// </summary>
        private void SetWaldosEnabledCheckBoxAccordingToState()
        {
            if (checkBoxWaldosEnabled.Checked == true)
            {
                checkBoxWaldosEnabled.BackColor = Color.LightGreen;
                checkBoxWaldosEnabled.Text = "Waldos Enabled";
            }
            else
            {
                checkBoxWaldosEnabled.BackColor = Color.IndianRed;
                checkBoxWaldosEnabled.Text = "Waldos Disabled";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the visual appearance of the remote connections state checkbox
        /// </summary>
        /// <param name="connState">the connection state</param>
        private void SetRemoteConnectionCheckBoxVisuals(bool connState)
        {
            if (connState == true)
            {
                checkBoxRemoteConnectionState.BackColor = Color.LightGreen;
                checkBoxRemoteConnectionState.Text = "Remote Conn...";
            }
            else
            {
                checkBoxRemoteConnectionState.BackColor = Color.IndianRed;
                checkBoxRemoteConnectionState.Text = "Remote DisCon...";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the pixels per micron value on the distance calibration panel
        /// and sets the appropriate fields
        /// </summary>
        private void CalcDistanceCalibration()
        {
            double knownMicronLen = 0;
            double knownDist = 0;

            // reset some things
            textBoxScalePixelsPerMicron.Text = "";
            try
            {
                knownMicronLen = Convert.ToInt32(textBoxDistInKnownMicrons.Text);
            }
            catch (Exception ex) 
            {
                LogMessage(" CalcDistanceCalibration (knownMicronLen):" + ex.Message);
                return;
            }

            if (radioButtonDistVert.Checked == true)
            {
                try
                {
                    knownDist = Convert.ToDouble(textBoxDistanceVert.Text);
                }
                catch (Exception ex)
                {
                    LogMessage(" CalcDistanceCalibration (knownDist_V):" + ex.Message);
                    return;
                }

            }
            else if (radioButtonDistHoriz.Checked == true)
            {
                try
                {
                    knownDist = Convert.ToDouble(textBoxDistanceHoriz.Text);
                }
                catch (Exception ex)
                {
                    LogMessage(" CalcDistanceCalibration (knownDist_H):" + ex.Message);
                    return;
                }

            }
            else 
            {
                LogMessage(" CalcDistanceCalibration unknown direction");
                return; 
            }

            // divide by zero check
            if(knownMicronLen==0) return; 

            // now do the calc, and load the box
            textBoxScalePixelsPerMicron.Text = Math.Round((knownDist / knownMicronLen), 5).ToString();

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the get scale from vertical distance radio button
        /// </summary>
        private void radioButtonDistVert_CheckedChanged(object sender, EventArgs e)
        {
            // just re-do the calcs
            CalcDistanceCalibration();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the get scale from horizontal distance radio button
        /// </summary>
        private void radioButtonDistHoriz_CheckedChanged(object sender, EventArgs e)
        {
            // just re-do the calcs
            CalcDistanceCalibration();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the known microns per distance text box
        /// </summary>
        private void textBoxDistInKnownMicrons_TextChanged(object sender, EventArgs e)
        {
            // just re-do the calcs
            CalcDistanceCalibration();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the button which calculates the pixels/micron value
        /// </summary>
        private void buttonScaleCalc_Click(object sender, EventArgs e)
        {
            // just re-do the calcs
            CalcDistanceCalibration();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the Calibrated Pixels per micron value on the display. This is 
        /// derived from the distance and scale panel on the utils tab. This can 
        /// change all the time with every mouse click so we have a "set" to fix 
        /// it in place once we have done it accurately.
        /// </summary>
        private void buttonScaleSet_Click(object sender, EventArgs e)
        {
            // we are sure it is good, copy it over
            textBoxCalibratedPixelsPerMicron.Text = textBoxScalePixelsPerMicron.Text;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the calibrated number pixels per micron. Pulls this off the 
        /// screen (which is a text field). Any problems, it returns 0;
        /// </summary>
        private double CalibratedPixelsPerMicron
        {
            get
            {
                double pixelsPerMicron = 0;
                try
                {
                    pixelsPerMicron = Convert.ToDouble(textBoxCalibratedPixelsPerMicron.Text);
                }
                catch (Exception ex)
                {
                    LogMessage(" CalibratedPixelsPerMicron (pixelsPerMicron):" + ex.Message);
                    return 0;
                }
                return pixelsPerMicron;

            }
            set
            {
                textBoxCalibratedPixelsPerMicron.Text = Math.Round(value,5).ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the buttonEx008ColorDetectSet button in which we
        /// set the colors the line recognition transform triggers on.
        /// </summary>
        private void buttonEx008ColorDetectSet_Click(object sender, EventArgs e)
        {
            SetEx008ColorRecognitionValues();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the color recognition values used to recognize lines in Ex008
        /// </summary>
        private void SetEx008ColorRecognitionValues()
        { 
            // do we have a recognition transform?
            if (RecognitionTransform == null) return;  // we can do nothing
            if ((RecognitionTransform is MFTDetectHorizLines) == false) return;

            // convert our colors
            Color? topOfHorizRange = Utils.ConvertBracketTextToColor(textBoxEx008ColorDetectHorizTop.Text);
            Color? botOfHorizRange = Utils.ConvertBracketTextToColor(textBoxEx008ColorDetectHorizBot.Text);
            Color? topOfVertRange = Utils.ConvertBracketTextToColor(textBoxEx008ColorDetectVertTop.Text);
            Color? botOfVertRange = Utils.ConvertBracketTextToColor(textBoxEx008ColorDetectVertBot.Text);

            if (topOfHorizRange == null) return;
            if (botOfHorizRange == null) return;
            if (topOfVertRange == null) return;
            if (botOfVertRange == null) return;

            // Set the color boundaries we trigger on to detect the lines
            (RecognitionTransform as MFTDetectHorizLines).TopOfHorizRange = (Color)topOfHorizRange;
            (RecognitionTransform as MFTDetectHorizLines).BotOfHorizRange = (Color)botOfHorizRange;
            (RecognitionTransform as MFTDetectHorizLines).TopOfVertRange = (Color)topOfVertRange;
            (RecognitionTransform as MFTDetectHorizLines).BotOfVertRange = (Color)botOfVertRange;

            // also set the minimum number acceptable pixels
            try
            {
                (RecognitionTransform as MFTDetectHorizLines).MinPixelsInLineHoriz = Convert.ToInt32(textBoxEx008ColorDetectMinPixelsHoriz.Text);
            }
            catch { }
            // also set the minimum number acceptable pixels
            try
            {
                (RecognitionTransform as MFTDetectHorizLines).MinPixelsInLineVert = Convert.ToInt32(textBoxEx008ColorDetectMinPixelsVert.Text);
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the red circle radius for Ex008 
        /// </summary>
        private int Ex008RedCircleRadius
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxActions008RedCircleRadius.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxActions008RedCircleRadius.Text = value.ToString();
            }
        }

        public Point LastGreenPoint { get => lastGreenPoint; set => lastGreenPoint = value; }
    }

}
