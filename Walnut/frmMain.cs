using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using MediaFoundation;
using MediaFoundation.Alt;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using MediaFoundation.OPM;
using MediaFoundation.ReadWrite;
using MediaFoundation.Transform;
using OISCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TantaCommon;
using WalnutBehaviours;
using WalnutCommon;
using BBBCSIO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;


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

/// COMPILATION NOTE: In order to get solution wide conditional complilation defines across multiple projects 
/// even though some projects are used in multiple solutions we use a SolutionDefines.targets file
/// as discussed here: https://stackoverflow.com/questions/5149351/solution-wide-define
/// This means that WALNUT_SERVER is defined in the projects only if we are in the Walnut Server
/// solution
namespace Walnut
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main form for the application
    /// </summary>
    public partial class frmMain : frmOISBase, IBehaviour_WaldosEnabledState
        , IBehaviour_SourcePoint
        , IBehaviour_DetectPointViaColor
        , IBehaviour_TransmitGlobalStack
        , IBehaviour_DetectionActivate
        , IBehaviour_ColorPixelsByColor
        , IBehaviour_LoadOverlayImageBySlot
        , IBehaviour_SourcePointDetectedPixelColor_Screen
        , IBehaviour_SourcePointDetectedPixelColor_Overlay
        , IBehaviour_SourcePointDetectedLowestAlphaValue_Overlay
        , IBehaviour_RecordingOnOff
        , IBehaviour_UpdateScreenWithPinStates
        , IBehaviour_ProcessOutputGPIOControlList
    {
        private const string DEFAULTLOGDIR = @"C:\Dump\Project Logs";
        private const string APPLICATION_NAME = "Walnut";
        private const string APPLICATION_VERSION = "00.02.12";
        private const string EXPERIMENT_NUMBER = "Ex012";
        private const string DEFAULT_SHOT_DESCIPTOR = "Shot00";
        private const int DEFAULT_REC_NUMBER = 0;
        private const string SHOT_DESCRIPTOR_MARKER = "##";
        private const string RUN_INFO_MARKER = "&&";
        private const string REC_NUMBER_MARKER = "$$";
        // default run info, use RUN_NUMBER_MARKER to include run marker 
        private const string DEFAULT_RUN_NAME = "FPath Sample" + " " + SHOT_DESCRIPTOR_MARKER;

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
        private const string DEFAULT_CAPTURE_FILENAME = RUN_INFO_MARKER+@"_" + SHOT_DESCRIPTOR_MARKER + "-" + REC_NUMBER_MARKER + ".mp4";

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
        protected MFTDetectObjectViaHistogram recognitionTransform = null;

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
        private const int MAX_OVERLAY_IMAGE_SLOT = 4;
        private const string DEFAULT_OVERLAY_IMAGE_PATH = @"D:\Dump\FPathData";
        private const string DEFAULT_OVERLAY_IMAGE_FILENAME = @"Overlay_%SLOT%.bin";
        private const string OVERLAY_IMAGE_SLOT_REPVAL = @"%SLOT%";
        private const string OVERLAY_EX_SLOT_REPVAL = @"%EX%";

        // this is the color the overlay image paths are drawn in
        private static Color TARGET_COLOR = Color.FromArgb(0,0,255,0);  // full green
        private static Color TRACKER_COLOR = Color.FromArgb(0, 0, 255, 255);  // full green
        // sometimes we need the alpha channel full on
        private static Color TARGET_COLOR_FULLALPHA = Color.FromArgb(255, TARGET_COLOR.R, TARGET_COLOR.G, TARGET_COLOR.B);
        private static Color TRACKER_COLOR_FULLALPHA = Color.FromArgb(255, TRACKER_COLOR.R, TRACKER_COLOR.G, TRACKER_COLOR.B);

        // make a transparent color, note this has an alpha channel of 0
        private static Color TRANSPARENT_COLOR = Color.FromArgb(0, 255, 255, 255);
        // some pens and brushes we use
        private SolidBrush trackerBrush = new SolidBrush(TRACKER_COLOR_FULLALPHA);
        // make a transparent white brush, note this has an alpha channel of 0
        private SolidBrush whiteTransparentBrush = new SolidBrush(TRANSPARENT_COLOR);

        // some colors, note .net Color.Green is #ff000800" not #ff00ff00" like you would expect. See:
        // https://stackoverflow.com/questions/4342300/why-is-system-drawing-color-green-0-128-0
        // we use the colors as a definitive color statement there
        private const string HTML_GREEN = "#ff00ff00";
        private const string HTML_RED = "#ffff0000";
        private const string HTML_BLUE = "#ff0000ff";
        private Color TRUE_RED = ColorTranslator.FromHtml(HTML_RED);
        private Color TRUE_GREEN = ColorTranslator.FromHtml(HTML_GREEN);
        private Color TRUE_BLUE = ColorTranslator.FromHtml(HTML_BLUE);

        private Point lastClickedTargetPoint = new Point();
        private Point lastDetectedSourcePoint = new Point();
        private Color lastDetectedSourcePointPixelColor_Screen = new Color();

        // we have the ability to draw virtual object on the screen this is the data for them
        private int greenCircleDrawCount = 0;    // if +ve we draw a green circle on the mouse click point and decrement
        private int drawLineCount = 0;        // if +ve we draw a red line on the mouse click point and decrement
        private int maskAlphaChannelCount = 0;   // if +ve we update the mask of the overlay in a rectangle intersecting with a color and zero
        private int drawRectCornerCount = 0;     // tracks if we are updating the lastDrawRectPoint_UL or LR values
        private Point lastDrawRectPoint_UL = new Point();
        private Point lastDrawRectPoint_LR = new Point();

        private const int SMALL_CIRCLE_DIAMETER_IN_PIXELS = 23;
        private const int LARGE_CIRCLE_DIAMETER_IN_PIXELS = 37;

        // set this up to detect the colors
        private const uint DEFAULT_GRAY_DETECTION_RANGE = 15;
        private ColorDetector colorDetectorObj = new ColorDetector(DEFAULT_GRAY_DETECTION_RANGE);
        // used to draw crosses on objects
        public const int DEFAULT_CENTROID_CROSS_BAR_LEN = 10;

        private const int PATH_FOLLOW_MIN_POINTS_NEEDED = 1;

        // as of Ex012 we have implemented a Subsumption Architecture to control the 
        // various behaviours
        private Behaviour_StateMachine globalBehaviourStack = null;
        // we use this lock object to set and reset the Behaviour stack
        private object globalBehaviourStackLockObj = new object();
        private const string BEHAVIOUR_STACK_IS_ACTIVE = "Behaviour Stack is Active";
        private const string BEHAVIOUR_STACK_NOT_ACTIVE = "Behaviour Stack is not Active";

        // #### HARD CODED AT THE MOMENT
        GpioEnum EX012LED1GPIO = GpioEnum.GPIO_50;  

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
                ShotDescriptor = DEFAULT_SHOT_DESCIPTOR;
                RecNumberAsInt = DEFAULT_REC_NUMBER;

                // we always have to initialize MF. The 0x00020070 here is the WMF version 
                // number used by the MF.Net samples. Not entirely sure if it is appropriate
                hr = MFExtern.MFStartup(0x00020070, MFStartup.Full);
                if (hr != 0)
                {
                }

                // init the overlay save file names. These are hard coded for now
                SetOverlaySaveAndLoadSlots();

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

            // stop and drop the current behaviour stack (if present)
            DropBehaviourStack();

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
                return Path.Combine(CaptureDirName, CaptureFileName.Replace(SHOT_DESCRIPTOR_MARKER, ShotDescriptor).Replace(REC_NUMBER_MARKER, RecNumberAsInt.ToString()).Replace(RUN_INFO_MARKER, RunInfoStr));
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
                if (textBoxCaptureFileName.Text.Length == 0) textBoxCaptureFileName.Text = DEFAULT_CAPTURE_FILENAME;
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
            ShotDescriptor = ImplicitUserSettings.LastShotDescriptor;
            RecNumberAsInt = ImplicitUserSettings.LastRecNumber;

            // draw stuff
            GreenCircleRadius = ImplicitUserSettings.DrawGreenCircleRadius;
            GreenCircleDrawMouseClicks = ImplicitUserSettings.DrawGreenCircleDrawMouseClicks;
            DrawGreenOutlineCircleLineWidth = ImplicitUserSettings.DrawGreenOutlineCircleLineWidth;

            // global settings
            CalibratedPixelsPerMicron = ImplicitUserSettings.CalibratedPixelsPerMicron;
            Motor0GlobalPositiveDir = ImplicitUserSettings.Motor0GlobalPositiveDir;
            Motor1GlobalPositiveDir = ImplicitUserSettings.Motor1GlobalPositiveDir;
            Motor2GlobalPositiveDir = ImplicitUserSettings.Motor2GlobalPositiveDir;
            Motor3GlobalPositiveDir = ImplicitUserSettings.Motor3GlobalPositiveDir;

            // grid stuff
            GridCountX = ImplicitUserSettings.GridCountX;
            GridCountY = ImplicitUserSettings.GridCountY;
            GridBarSizeX = ImplicitUserSettings.GridBarSizeX;
            GridBarSizeY = ImplicitUserSettings.GridBarSizeY;
            GridSpacingInMicrons = ImplicitUserSettings.GridSpacingInMicrons;
            GridColor = ImplicitUserSettings.GridColor;

            // stepper control settings
            textBoxStepperControlNumSteps.Text = ImplicitUserSettings.StepperControlNumSteps;
            textBoxStepperControlStepsPerSecond.Text = ImplicitUserSettings.StepperControlStepsPerSecond;
            if (ImplicitUserSettings.StepperControlDirIsCW == true) radioButtonStepperControlDirCW.Checked = true;
            else radioButtonStepperControlDirCCW.Checked = true;

            WASDSpeedX = ImplicitUserSettings.WASDSpeedX;
            WASDSpeedY = ImplicitUserSettings.WASDSpeedY;
            WASDSpeedZ = ImplicitUserSettings.WASDSpeedZ;

            // line detect settings
            if ((ImplicitUserSettings.LineDetectColorHorizTop != null) && (ImplicitUserSettings.LineDetectColorHorizTop.Length > 0)) textBoxColorDetectHorizTop.Text = ImplicitUserSettings.LineDetectColorHorizTop;
            if ((ImplicitUserSettings.LineDetectColorHorizBot != null) && (ImplicitUserSettings.LineDetectColorHorizBot.Length > 0)) textBoxColorDetectHorizBot.Text = ImplicitUserSettings.LineDetectColorHorizBot;
            if ((ImplicitUserSettings.LineDetectColorMinPixelsHoriz != null) && (ImplicitUserSettings.LineDetectColorMinPixelsHoriz.Length > 0)) textBoxColorDetectMinPixelsHoriz.Text = ImplicitUserSettings.LineDetectColorMinPixelsHoriz;
            if ((ImplicitUserSettings.LineDetectColorVertTop != null) && (ImplicitUserSettings.LineDetectColorVertTop.Length > 0)) textBoxColorDetectVertTop.Text = ImplicitUserSettings.LineDetectColorVertTop;
            if ((ImplicitUserSettings.LineDetectColorVertBot != null) && (ImplicitUserSettings.LineDetectColorVertBot.Length > 0)) textBoxColorDetectVertBot.Text = ImplicitUserSettings.LineDetectColorVertBot;
            if ((ImplicitUserSettings.LineDetectColorMinPixelsVert != null) && (ImplicitUserSettings.LineDetectColorMinPixelsVert.Length > 0)) textBoxColorDetectMinPixelsVert.Text = ImplicitUserSettings.LineDetectColorMinPixelsVert;
            HorizLineRecognitionMode = ImplicitUserSettings.HorizLineRecognitionMode;
            VertLineRecognitionMode = ImplicitUserSettings.VertLineRecognitionMode;
            LineDetectHoriz_Floor = ImplicitUserSettings.LineDetectHoriz_Floor;
            LineDetectHoriz_PreDrop = ImplicitUserSettings.LineDetectHoriz_PreDrop;
            LineDetectHoriz_PostDrop = ImplicitUserSettings.LineDetectHoriz_PostDrop;
            LineDetectHoriz_Offset = ImplicitUserSettings.LineDetectHoriz_Offset;
            LineDetectVert_Offset = ImplicitUserSettings.LineDetectVert_Offset;

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
            ImplicitUserSettings.LastShotDescriptor = ShotDescriptor;
            ImplicitUserSettings.LastRecNumber = RecNumberAsInt;

            // draw stuff
            ImplicitUserSettings.DrawGreenCircleRadius = GreenCircleRadius;
            ImplicitUserSettings.DrawGreenCircleDrawMouseClicks = GreenCircleDrawMouseClicks;
            ImplicitUserSettings.DrawGreenOutlineCircleLineWidth = DrawGreenOutlineCircleLineWidth;

            // global settings
            ImplicitUserSettings.CalibratedPixelsPerMicron = CalibratedPixelsPerMicron;
            ImplicitUserSettings.Motor0GlobalPositiveDir = Motor0GlobalPositiveDir;
            ImplicitUserSettings.Motor1GlobalPositiveDir = Motor1GlobalPositiveDir;
            ImplicitUserSettings.Motor2GlobalPositiveDir = Motor2GlobalPositiveDir;
            ImplicitUserSettings.Motor3GlobalPositiveDir = Motor3GlobalPositiveDir;

            // grid stuff
            ImplicitUserSettings.GridCountX = GridCountX;
            ImplicitUserSettings.GridCountY = GridCountY;
            ImplicitUserSettings.GridBarSizeX = GridBarSizeX;
            ImplicitUserSettings.GridBarSizeY = GridBarSizeY;
            ImplicitUserSettings.GridSpacingInMicrons = GridSpacingInMicrons;
            ImplicitUserSettings.GridColor = GridColor;

            // Stepper control settings
            ImplicitUserSettings.StepperControlNumSteps = textBoxStepperControlNumSteps.Text;
            ImplicitUserSettings.StepperControlStepsPerSecond = textBoxStepperControlStepsPerSecond.Text;
            if (radioButtonStepperControlDirCW.Checked == true) ImplicitUserSettings.StepperControlDirIsCW = true;
            ImplicitUserSettings.WASDSpeedX = WASDSpeedX;
            ImplicitUserSettings.WASDSpeedY = WASDSpeedY;
            ImplicitUserSettings.WASDSpeedZ = WASDSpeedZ;

            // line recognition settings
            ImplicitUserSettings.LineDetectColorHorizTop = textBoxColorDetectHorizTop.Text;
            ImplicitUserSettings.LineDetectColorHorizBot = textBoxColorDetectHorizBot.Text;
            ImplicitUserSettings.LineDetectColorMinPixelsHoriz = textBoxColorDetectMinPixelsHoriz.Text;
            ImplicitUserSettings.LineDetectColorVertTop = textBoxColorDetectVertTop.Text;
            ImplicitUserSettings.LineDetectColorVertBot = textBoxColorDetectVertBot.Text;
            ImplicitUserSettings.LineDetectColorMinPixelsVert = textBoxColorDetectMinPixelsVert.Text;

            ImplicitUserSettings.HorizLineRecognitionMode = HorizLineRecognitionMode;
            ImplicitUserSettings.VertLineRecognitionMode = VertLineRecognitionMode;
            ImplicitUserSettings.LineDetectHoriz_Floor = LineDetectHoriz_Floor;
            ImplicitUserSettings.LineDetectHoriz_PreDrop = LineDetectHoriz_PreDrop;
            ImplicitUserSettings.LineDetectHoriz_PostDrop = LineDetectHoriz_PostDrop;
            ImplicitUserSettings.LineDetectHoriz_Offset = LineDetectHoriz_Offset;
            ImplicitUserSettings.LineDetectVert_Offset = LineDetectVert_Offset;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+= VIDEO and WMF =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region VideoAndWMF

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
                // check our source filename is correct and usable
                if ((VideoCaptureDeviceName == null) || (VideoCaptureDeviceName.Length == 0))
                {
                    MessageBox.Show("No Source Filename and path. Cannot continue.");
                    return;
                }
                if (VideoFormatContainer == null)
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
                radioButtonRedToTarget.Enabled = true;
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
                radioButtonRedToTarget.Enabled = false;
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
                    if ((TextOverlayTransform is MFTWriteText_Sync) == true)
                    {
                        // set this so that the transform knows about it
                        (TextOverlayTransform as MFTWriteText_Sync).SetCalibrationBarData(CalibratedPixelsPerMicron);
                        // set our lower left text, we no longer bother with the version info str since the experiment
                        // number pretty much contains that information
                        (TextOverlayTransform as MFTWriteText_Sync).RunInfoStr = this.RunInfoStr+" " + ShotDescriptor;
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
        private MFTDetectObjectViaHistogram CreateRGBAObjectDetectionTransform()
        {
            // hard coded to this. If we wished to inject a different one into the pipeline we
            // could put some logic here.
            return new MFTDetectObjectViaHistogram();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the current image recognition transform object. Can be null
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MFTDetectObjectViaHistogram RecognitionTransform
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
            LineRecognitionSpecificSetupActions();

            // this is what starts the data moving through the pipeline
            HResult hr = mediaSession.Start(Guid.Empty, new PropVariant());
            if (hr != HResult.S_OK)
            {
                throw new Exception("StartVideoCapture call to mediaSession.Start failed. Err=" + hr.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Perform actions specific to line recognition
        /// 
        /// </summary>
        private void LineRecognitionSpecificSetupActions()
        {
            // set the color recognition values on the transform
            SetObjectRecognitionValues();
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
            (RecognitionTransform as MFTDetectObjectViaHistogram).BottomOfScreenSkipHeight = BottomOfScreenSkipHeight();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the bottom of screen skip height from the image recognition transform
        /// This is usually the chyron height. This is so it 
        /// we do not try to do image things in the bar at the bottom of the screen
        /// 
        /// This needs to be done after the topology has been set but before it starts
        /// </summary>
        private int BottomOfScreenSkipHeight()
        {
            if (TextOverlayTransform == null) return 0;
            // get it now, 
            return (TextOverlayTransform as MFTWriteText_Sync).ChyronHeight;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the width of the screen from the text transform. This is 
        /// not zero based - it is a width
        /// 
        /// </summary>
        private int ScreenWidth()
        {
            if (TextOverlayTransform == null) return 640;
            // get it now, 
            return (TextOverlayTransform as MFTWriteText_Sync).ImageWidthInPixels;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the height of the screen from the text transform. This is 
        /// not zero based - it is a height
        /// 
        /// </summary>
        private int ScreenHeight()
        {
            if (TextOverlayTransform == null) return 640;
            // get it now, 
            return (TextOverlayTransform as MFTWriteText_Sync).ImageHeightInPixels;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the coord of the pixel at the far width of the screen from the text transform. 
        /// This is zero based - it is a number
        /// 
        /// </summary>
        private int ScreenWidthMaxCoord()
        {
            int maxCoord = ScreenWidth();
            if (maxCoord <= 0) return 639;
            return maxCoord - 1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the coord of the pixel at the far top of the screen from the text transform. 
        /// This is zero based - it is a number
        /// 
        /// </summary>
        private int ScreenHeightMaxCoord()
        {
            int maxCoord = ScreenHeight();
            if (maxCoord <= 0) return 479;
            return maxCoord - 1;
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
            (ImageOverlayTransform as MFTOverlayImage_Base).BottomOfScreenSkipHeight = BottomOfScreenSkipHeight();
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
            // we just toggle here
            SetScreenRecordingState(!IsRecording);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        ///  Sets the screen recording state
        ///  
        /// NOTE: we also update the recording button state appropriately
        /// 
        /// Note this is part of the IBehaviour_RecordingOnOff implementation
        /// </summary>
        /// <param name="recordingState">if true we turn recording on if it is not on, if false we always turn it off</param>
        public void SetScreenRecordingState(bool recordingState)
        { 
            int retInt;

            // We may not be on teh form thread so, we use the InvokeRequired...Invoke sequence to get us back on the form thread
            if (InvokeRequired == true)
            {
                // call ourselves again but this time be on the form thread.
                Invoke(new MethodInvoker(() => { SetScreenRecordingState(recordingState); }));
                return;
            }

            if (sampleGrabberTransform == null)
            {
                // no transform, recording is always off
                buttonRecordingOnOff.Text = RECORDING_IS_OFF;
                return;
            }

            // if we want to turn recording on and we are not now recording
            if ((recordingState==true) && (IsRecording == false))
            {
                // recording is currently off, turn it on
                buttonRecordingOnOff.Text = RECORDING_IS_ON;

                // crank up the run number
                RecNumberAsInt += 1;
                // set a few things on the VideoTransform now
                if ((TextOverlayTransform is MFTWriteText_Sync) == true)
                {
                    // set our lower left text on the chryron so it syncs with the capture file name
                    (TextOverlayTransform as MFTWriteText_Sync).RunInfoStr = this.RunInfoStr + " " + ShotDescriptor;
                }

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
        /// Get/Set the shot descriptor - will never get/set null
        /// 
        /// Note this is part of the IBehaviour_RecordingOnOff implementation
        /// </summary>
        public string ShotDescriptor
        {
            get
            {
                if (textBoxShotDescriptor.Text == null) textBoxShotDescriptor.Text = DEFAULT_SHOT_DESCIPTOR;
                if (textBoxShotDescriptor.Text.Length == 0) textBoxShotDescriptor.Text = DEFAULT_SHOT_DESCIPTOR;
                return textBoxShotDescriptor.Text;
            }
            set
            {
                textBoxShotDescriptor.Text = value;
                if (textBoxShotDescriptor.Text == null) textBoxShotDescriptor.Text = DEFAULT_SHOT_DESCIPTOR;
                if (textBoxShotDescriptor.Text.Length == 0) textBoxShotDescriptor.Text = DEFAULT_SHOT_DESCIPTOR;
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

            if (currentVideoMediaType == null)
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
        ///  Resets the rec number
        /// </summary>
        private void buttonResetRecNumber_Click(object sender, EventArgs e)
        {
            textBoxRecNumber.Text = DEFAULT_REC_NUMBER.ToString();
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
        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+= GLOBAL FORM HANDLING +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region GlobalFormHandling

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on Show only Overlay Button
        /// </summary>
        private void checkBoxShowOnlyOverlay_CheckedChanged(object sender, EventArgs e)
        {
            SetShowOnlyOverlayCheckBoxAccordingToState();

            if (ImageOverlayTransform == null) return;

            if (checkBoxShowOnlyOverlay.Checked == true)
            {
                (ImageOverlayTransform as MFTOverlayImage_Base).DisplayOnlyOverlayOnImage = true;
            }
            else
            {
                (ImageOverlayTransform as MFTOverlayImage_Base).DisplayOnlyOverlayOnImage = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the visual appearance of the Show Overlays checkbox according to 
        /// the state
        /// </summary>
        private void SetShowOnlyOverlayCheckBoxAccordingToState()
        {
            if (checkBoxShowOnlyOverlay.Checked == true)
            {
                checkBoxShowOnlyOverlay.BackColor = Color.IndianRed;
            }
            else
            {
                checkBoxShowOnlyOverlay.BackColor = Color.LightGreen;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the controls on the form
        /// </summary>
        private void SetupWalnutControls()
        {
            SetShowOnlyOverlayCheckBoxAccordingToState();
            SetEx012StackIsActiveCheckBoxAccordingToState();
            SyncAllWalnutControlsToScreenState(false);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Synchronizes all waldo controls to the screen state
        /// </summary>
        /// <param name="enableState">if true they are all enabled, false they are not</param>
        private void SyncAllWalnutControlsToScreenState(bool enableState)
        {
            // give this a call to set the appearance correctly
            SetWaldosEnabledCheckBoxAccordingToState();
            SetRemoteConnectionCheckBoxVisuals(false);
            // some calibration stuff
            SetMicronDistancesOnUtilsTabToReality();
            // draw stuff
            SyncDrawGreenCircleEnableOptionsToReality();
            // detection stuff
            SyncLineDetectHorizOptionsToReality();
            SyncAllLineDetectOptionsToReality();
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

            // create the message container
            SCM_LogMarker scmData = new SCM_LogMarker();

            //display it
            AppendDataToConnectionTrace("OUT: " + scmData.GetState());
            // send it
            dataTransporter.SendDataMessage(scmData);
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
        /// A quick check to get the Waldos Enabled State as a bool. 
        /// </summary>
        public bool WaldosEnabledState
        {
            get
            {
                return checkBoxWaldosEnabled.Checked;
            }
            set
            {
                checkBoxWaldosEnabled.Checked = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The Waldos Enabled State as a UINT the client side likes it that way
        /// </summary>
        /// <returns>Waldos Enabled State as a UINT</returns>
        public uint WaldosEnabledStateAsUINT()
        {
            return (uint)(WaldosEnabledState ? 1 : 0);
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
                StopAllWaldos();
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
        /// Handle a stop all waldos request. Shuts them all down
        /// </summary>
        private void buttonStopAllWaldos_Click(object sender, EventArgs e)
        {
            LogMessage("buttonStopAllWaldos_Click");
            // this does it all
            StopAllWaldos();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to stop all waldos
        /// </summary>
        private void StopAllWaldos()
        {
            LogMessage("StopAllWaldos called ");

            // now stop the waldos
            if (dataTransporter == null)
            {
                LogMessage("buttonStopAllWaldos_Click, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("buttonStopAllWaldos_Click, Not connected");
                return;
            }

            SCM_StopAllWaldos scmData = new SCM_StopAllWaldos();
            dataTransporter.SendDataMessage(scmData);
            AppendDataToConnectionTrace("OUT: Stop All Waldos message sent");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global positive direction value for  Motor0. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// This is a global setting the set accessor here is normally only called on setup
        /// </summary>
        private uint Motor0GlobalPositiveDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor0.Checked == true) return 0;
                else return 1;
            }
            set
            {
                if (value == 0) { radioButtonPositiveDirIs0_Motor0.Checked = true; }
                else radioButtonPositiveDirIs1_Motor0.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global positive direction value for  Motor1. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// This is a global setting the set accessor here is normally only called on setup
        /// </summary>
        private uint Motor1GlobalPositiveDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor1.Checked == true) return 0;
                else return 1;
            }
            set
            {
                if (value == 0) { radioButtonPositiveDirIs0_Motor1.Checked = true; }
                else radioButtonPositiveDirIs1_Motor1.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global positive direction value for  Motor2. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// This is a global setting the set accessor here is normally only called on setup
        /// </summary>
        private uint Motor2GlobalPositiveDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor2.Checked == true) return 0;
                else return 1;
            }
            set
            {
                if (value == 0) { radioButtonPositiveDirIs0_Motor2.Checked = true; }
                else radioButtonPositiveDirIs1_Motor2.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global positive direction value for  Motor3. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// This is a global setting the set accessor here is normally only called on setup
        /// </summary>
        private uint Motor3GlobalPositiveDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor3.Checked == true) return 0;
                else return 1;
            }
            set
            {
                if (value == 0) { radioButtonPositiveDirIs0_Motor3.Checked = true; }
                else radioButtonPositiveDirIs1_Motor3.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global negative direction value for  Motor0. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// There is no set accessor this is a global setting
        /// </summary>
        private uint Motor0GlobalNegativeDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor0.Checked == true) return 1;
                else return 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global negative direction value for  Motor1. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// There is no set accessor this is a global setting
        /// </summary>
        private uint Motor1GlobalNegativeDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor1.Checked == true) return 1;
                else return 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global negative direction value for  Motor2. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// There is no set accessor this is a global setting
        /// </summary>
        private uint Motor2GlobalNegativeDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor2.Checked == true) return 1;
                else return 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the global negative direction value for  Motor3. This is the direction 
        /// value we need to send to get motor 0 to move in a positive direction. 
        /// 
        /// There is no set accessor this is a global setting
        /// </summary>
        private uint Motor3GlobalNegativeDir
        {
            get
            {
                if (radioButtonPositiveDirIs0_Motor3.Checked == true) return 1;
                else return 0;
            }
        }

        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+ CODE WORKER +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
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
            while (true) // endless loop
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

            Point lastHorizLineCenterPoint = new Point();
            Point lastVertLineCenterPoint = new Point();
            Color lastIntersectionPixelColor_Screen = new Color();

            // call the line recognition change handler
            RecognizeLine_ProcessChangedHandler(out lastVertLineCenterPoint, out lastHorizLineCenterPoint, out lastIntersectionPixelColor_Screen);
            // did we find one?
            if ((lastHorizLineCenterPoint.IsEmpty == false) && (lastVertLineCenterPoint.IsEmpty == false))
            {
                // yes, we did, set it now at the intersection of the detected lines
                lastDetectedSourcePoint = new Point(lastVertLineCenterPoint.X, lastHorizLineCenterPoint.Y);
            }
            else
            {
                // reset this always
                lastDetectedSourcePoint = new Point();
            }
            // doesnt matter if we found a color, it will be returned as Color.IsEmpty for fail
            lastDetectedSourcePointPixelColor_Screen = lastIntersectionPixelColor_Screen;

            // Draw the grid etc.
            FinalizeOverlayComposites();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles progress reports in a way specific to the Recognize object functions
        /// 
        /// NOTE we ARE in the form thread here and it is ok to operate on the screen
        /// controls. 
        /// </summary>
        /// <param name="lastHorizLineCenterPoint">The last detected horizontal center point</param>
        /// <param name="lastVertLineCenterPoint">The last detected vertical center point</param>
        /// <param name="centerPointPixelColor">the color pixel under the last detected center point, will be Color.IsEmpty for not present</param>
        private void RecognizeLine_ProcessChangedHandler(out Point lastVertLineCenterPoint, out Point lastHorizLineCenterPoint, out Color centerPointPixelColor_Screen)
        {
            // set these now
            lastHorizLineCenterPoint = new Point();
            lastVertLineCenterPoint = new Point();
            centerPointPixelColor_Screen = new Color();

            // do we have a recognition transform?
            if (RecognitionTransform == null) return;  // we can do nothing
            // we need the overlayTransform as well
            if (ImageOverlayTransform == null) return;

            // are we even doing line detection, if not leave, save us some work
            if (ObjectDetectionEnabled == false) return;

            // clear out our existing lines
            if (ClearRedLinesEveryFrame == true)
            {
                (ImageOverlayTransform as MFTOverlayImage_Base).ClearColorFromTracker(Color.Red);
            }

            // yes, we do. Get the list of objects from it
            List<ColoredObject_Base> objList = RecognitionTransform.IdentifiedObjects;
            if (objList == null) return;

            // run through each object and draw in the identified largest horiz and vert line
            foreach (ColoredObject_Base crObj in objList)
            {
                if ((crObj is ColoredRotatedLine) == true)
                {
                    // do we have a horiz line?
                    if ((crObj as ColoredRotatedLine).Angle == ColoredRotatedLine.HORIZONTAL_LINE_ANGLE)
                    {
                        // remember this
                        lastHorizLineCenterPoint = (crObj as ColoredRotatedLine).CenterPoint;
                        // do we want to draw one?
                        if (checkBoxColorDetectShowHorizLine.Checked == true)
                        {
                            // yes we do, we draw in the horizontal line
                            DrawLineThroughPointOnTracker(TRUE_RED, (crObj as ColoredRotatedLine).CenterPoint, 1, -1, false);
                        }
                    }
                    // do we have a vertical line?
                    if ((crObj as ColoredRotatedLine).Angle == ColoredRotatedLine.VERTICAL_LINE_ANGLE)
                    {
                        // remember this
                        lastVertLineCenterPoint = (crObj as ColoredRotatedLine).CenterPoint;
                        // do we want to draw one?
                        if (checkBoxColorDetectShowVertLine.Checked == true)
                        {
                            // we draw in the horizontal line
                            DrawLineThroughPointOnTracker(TRUE_RED, (crObj as ColoredRotatedLine).CenterPoint, 1, -1, true);
                        }
                    }
                }
                else if ((crObj is ColoredPointRGB) == true)
                {
                    // in this particular implementation we know it can only be the color of the pixel under the 
                    // intersection of the detected horiz. and vertical lines
                    centerPointPixelColor_Screen = (crObj as ColoredPointRGB).CenterPixelColor;
                }
            } // bottom of foreach (ColoredObject_Base crObj in objList)

            // do we want to draw a circle
            if ((checkBoxColorDetectDrawCircleOnIntersection.Checked == true) && (lastHorizLineCenterPoint.IsEmpty == false) && (lastVertLineCenterPoint.IsEmpty == false))
            {
                // this red circle gets drawn on the tracker overlay and cleared above. This erases everything under the circle
                DrawCircleAtPointOnTracker(new Point(lastVertLineCenterPoint.X, lastHorizLineCenterPoint.Y), ColorDetectRedCircleRadius, TRUE_RED);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything necessary to finalize the overlay before it is written 
        /// onto the actual image in the frame
        /// 
        /// NOTE we ARE in the form thread here and it is ok to operate on the screen
        /// controls. 
        /// </summary>
        private void FinalizeOverlayComposites()
        {
            // we need the overlayTransform as well
            if (ImageOverlayTransform == null) return;

            (ImageOverlayTransform as MFTOverlayImage_GS).DrawGridOnTracker();
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
                if (codeWorker != null)
                {
                    codeWorker.CancelAsync();
                }
            }
            catch { }
        }

        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+ BEHAVIOUR CODE =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region BehaviourCode

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the Clear tracker button
        /// </summary>
        private void buttonTrackerClearTracker_Click(object sender, EventArgs e)
        {
            // we need the overlayTransform to exist
            if (ImageOverlayTransform == null) return;
            (ImageOverlayTransform as MFTOverlayImage_Base).ClearTracker();
        }

        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+= TRANSPORTER =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region Transporter

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles inbound data events.
        /// 
        /// NOTE: You are not on the Main Form Thread here.
        /// 
        /// NOTE: The inbound class is actually a class derived from SCM_Base. We 
        ///      use the class type to figure out what to do with it
        /// </summary>
        /// <param name="scmMessage">a server client message derived vrom SCM_Base</param>
        private void ServerClientDataEventHandler(object sender, SCM_Base scmMessage)
        {
            if (scmMessage == null)
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
                Invoke(new TCPDataTransporter.ServerClientDataEvent_Delegate(ServerClientDataEventHandler), new object[] { sender, scmMessage });
                return;
            }

            // Now we KNOW we are on the main form thread.

            // figure out the class of data and deal with it appropriately
            if ((scmMessage is SCM_RemoteConnect) == true)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_CONNECT");
                // display it
                AppendDataToConnectionTrace("IN: REMOTE_CONNECT");
                // set the screen
                SetScreenVisualsBasedOnConnectionState(true);
            }
            else if ((scmMessage is SCM_RemoteDisConnect) == true)
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
            else if ((scmMessage is SCM_ConnectionTestACK) == true)
            {
                // it is just a connection test ACK, log it
                LogMessage("ServerClientDataEventHandler Connection Test ACK received");
                // display it
                AppendDataToConnectionTrace("IN: Connection Test ACK received");
            }
            else if ((scmMessage is SCM_PinStateList_Input) == true)
            {
                // it is a list of pin status states in from the client
                LogMessage("ServerClientDataEventHandler Pin Status List received");
                //. Not really used on the server. This is handled by the
                //  StackUpdate from the client and the behaviours access it out
                // of the global data
            }
            else if ((scmMessage is SCM_BehaviourStackUpdate) == true)
            {
                // A message of this type updates the global behaviour stack
                // with variables the client is interested in. 
                //
                // NOTE it carries a shallow clone of the behaviour stack on the 
                //      server. So all of the behaviours themselves are missing
                // just copy the data in
                CopyDataToGlobalStack((scmMessage as SCM_BehaviourStackUpdate).BehaviourStack);
                //Console.WriteLine("scmMessage is SCM_BehaviourStackUpdate");
            }
            else
            {
                LogMessage("ServerClientDataEventHandler unknown DataMessage = " + scmMessage.GetType().ToString());
                Console.WriteLine("ServerClientDataEventHandler unknown DataMessage = " + scmMessage.GetType().ToString());
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
                    StopAllWaldos();
                }

                dataTransporter.Shutdown();
                dataTransporter = null;
            }
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

            // send it
            SCM_ConnectionTest scmData = new SCM_ConnectionTest();
            dataTransporter.SendDataMessage(scmData);

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

        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+= PWM CODE +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region PWMCode
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

// disabled now needs to be treated as stepper motors
            //// create the data container
            //ServerClientData scData = new ServerClientData();

            //scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            //scData.UserDataContent = UserDataContentEnum.NO_DATA;

            //// set up some default speeds and dirs
            //scData.PWMB_PWMPercent = GetPWMBSpeed();
            //if (checkBoxPWMBDir.Checked == true)
            //{
            //    scData.PWMB_DirState = 1;
            //}
            //else
            //{
            //    scData.PWMB_DirState = 0;

            //}
            //scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);

            //// set PWMB speed according to the screen
            //if (checkBoxPWMBEnable.Checked == true)
            //{
            //    scData.PWMB_Enable = 1;
            //    scData.DataStr = "Set PWM B State On";
            //    scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMB_DATA;
            //}
            //else
            //{
            //    scData.PWMB_Enable = 0;
            //    scData.DataStr = "Set PWM B State Off";
            //    scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMB_DATA;
            //}

            //// display it
            //AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            //// send it
            //dataTransporter.SendData(scData);

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

// disabled now needs to be treated as stepper motors
            //// create the data container
            //ServerClientData scData = new ServerClientData();

            //scData.DataContent = ServerClientDataContentEnum.USER_DATA;
            //scData.UserDataContent = UserDataContentEnum.NO_DATA;

            //// set up some default speeds and dirs
            //scData.PWMA_PWMPercent = GetPWMASpeed();
            //if (checkBoxPWMADir.Checked == true)
            //{
            //    scData.PWMA_DirState = 1;
            //}
            //else
            //{
            //    scData.PWMA_DirState = 0;

            //}
            //scData.Waldo_Enable = (uint)(checkBoxWaldosEnabled.Checked ? 1 : 0);

            //// set PWMA speed according to the screen
            //if (checkBoxPWMAEnable.Checked == true)
            //{
            //    scData.PWMA_Enable = 1;
            //    scData.DataStr = "Set PWM A State On";
            //    scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMA_DATA;
            //}
            //else
            //{
            //    scData.PWMA_Enable = 0;
            //    scData.DataStr = "Set PWM A State Off";
            //    scData.UserDataContent = scData.UserDataContent | UserDataContentEnum.PWMA_DATA;
            //}

            //// display it
            //AppendDataToConnectionTrace("OUT: dataStr=" + scData.DataStr);
            //// send it
            //dataTransporter.SendData(scData);

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
        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+= DRAWING CODE +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region DrawingCode

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clear a point on the overlay via a color. We operate on everything in a 
        /// rectangular area centered on the pixel point and update only those pixels
        /// whose color matches the current color. 
        /// 
        /// Note we preserve the alpha channel on the replaced color unless we are
        ///      rendering the new pixels transparent
        ///      
        /// This is the IBehaviour_ColorPixelsByColor implementation
        /// 
        /// </summary>
        /// <param name="pixelPoint">the point on the overlay to mask out</param>
        /// <param name="rectHeight">the height of the rectangle</param>
        /// <param name="rectWidth">the width of the mask rectangle</param>
        /// <param name="currentColor">the color of the pixels we operate on</param>
        /// <param name="replacementColor">the replacement color</param>
        /// <param name="wantTransparent">if true we use transparent as the replacement color</param>
        public void ColorPixelsByColor(Point pixelPoint, int rectWidth, int rectHeight, Color currentColor, Color replacementColor, bool wantTransparent)
        {
            if (pixelPoint == null) return;
            if (pixelPoint.IsEmpty == true) return;
            if (rectWidth <= 0) return;
            if (rectHeight <= 0) return;
            if (replacementColor == null) return;
            if (currentColor == null) return;
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            Rectangle rect = Utils.GetRectangleFromCenterPoint(pixelPoint, rectWidth, rectHeight, (ImageOverlayTransform as MFTOverlayImage_GS).GetMinEffectiveScreenWidth, (ImageOverlayTransform as MFTOverlayImage_GS).GetMinEffectiveScreenHeight, (ImageOverlayTransform as MFTOverlayImage_GS).GetMaxEffectiveScreenWidth, (ImageOverlayTransform as MFTOverlayImage_GS).GetMaxEffectiveScreenHeight);
            if (rect.IsEmpty == true) return;

            // do we want to render the values transparent
            if (wantTransparent == true) 
            {
                // yes, we do
                (ImageOverlayTransform as MFTOverlayImage_Base).ConvertColorToColorInRectOnOverlay(currentColor, TRANSPARENT_COLOR, rect, false);
            }
            else
            {
                // no, we do not, replace the currentColor with the replacment
                (ImageOverlayTransform as MFTOverlayImage_Base).ConvertColorToColorInRectOnOverlay(currentColor, replacementColor, rect, true);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle Checked Changed event on the radioButtonDrawGreenCircle_Solid control
        /// </summary>
        private void radioButtonDrawGreenCircle_Solid_CheckedChanged(object sender, EventArgs e)
        {
            SyncDrawGreenCircleEnableOptionsToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle Checked Changed event on the radioButtonDrawGreenCircle_Outline control
        /// </summary>
        private void radioButtonDrawGreenCircle_Outline_CheckedChanged(object sender, EventArgs e)
        {
            SyncDrawGreenCircleEnableOptionsToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the enable state on the all draw green circle controls to reality
        /// </summary>
        private void SyncDrawGreenCircleEnableOptionsToReality()
        {
            if (radioButtonDrawGreenCircle_Outline.Checked == true)
            {
                textBoxDrawGreenCircle_LineWidth.Enabled = true;
                labelDrawGreenCircleLineWidth.Enabled = true;
                labelDrawGreenCircleLineWidthPixels.Enabled = true;
            }
            else
            {
                textBoxDrawGreenCircle_LineWidth.Enabled = false;
                labelDrawGreenCircleLineWidth.Enabled = false;
                labelDrawGreenCircleLineWidthPixels.Enabled = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// If we are drawing a green circle this indicates the type
        /// </summary>
        private bool WantDrawGreenOutlineCircle
        {
            get
            {
                return radioButtonDrawGreenCircle_Outline.Checked;
            }
            set
            {
                radioButtonDrawGreenCircle_Outline.Checked = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// When drawing a green circle this provides the line width
        /// </summary>
        private int DrawGreenOutlineCircleLineWidth
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxDrawGreenCircle_LineWidth.Text);
                }
                catch
                {
                    return 1;
                }
            }
            set
            {
                textBoxDrawGreenCircle_LineWidth.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Apply a rectangular alpha channel mask to the overlay centered at the 
        /// click
        /// </summary>
        private void buttonAlphaMaskRectAtClick_Click(object sender, EventArgs e)
        {
            ClearAllClickPointMarkers();

            try
            {
                // get the value from the screen, and place it in this flag
                maskAlphaChannelCount = 1;
            }
            catch { }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Alpha Mask Value
        /// </summary>
        private int AlphaMaskValue
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxAlphaMaskAlphaValue.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxAlphaMaskAlphaValue.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the alpha mask rectangle width. 
        /// </summary>
        private int AlphaMaskRectWidth
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxAlphaMaskRectWidth.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxAlphaMaskRectWidth.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the alpha mask rectangle Height. 
        /// </summary>
        private int AlphaMaskRectHeight
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxAlphaMaskRectHeight.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxAlphaMaskRectHeight.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the alpha mask intersect color. Note that there are some complications with
        /// Green colors. Color.Green is 0xff000800 not 0xff00ff00 as you would expect
        /// 
        /// See: https://stackoverflow.com/questions/4342300/why-is-system-drawing-color-green-0-128-0
        /// 
        /// Be aware that the default in here takes care of that special case
        /// </summary>
        private Color GetAlphaMaskIntersectColor
        {
            get
            {
                if (radioButtonAlphaMaskColorRed.Checked == true) return TRUE_RED;
                else if (radioButtonAlphaMaskColorBlue.Checked == true) return TRUE_BLUE;
                else return TRUE_GREEN; // default
            }
            set
            {
                if (value == TRUE_RED) radioButtonAlphaMaskColorRed.Checked = true;
                else if (value == TRUE_BLUE) radioButtonAlphaMaskColorBlue.Checked = true;
                else radioButtonAlphaMaskColorGreen.Checked = true; // default
            }
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
            if (WantDrawGreenOutlineCircle == true)
            {
                DrawOutlineCircleAtPoint(centerPoint, radius, DrawGreenOutlineCircleLineWidth, HTML_GREEN);
            }
            else
            {
                DrawCircleAtPointOnOverlay(centerPoint, radius,TRUE_GREEN);
            }
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
            ClearAllClickPointMarkers();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears all of the markers which allow us to draw things on the screen
        /// </summary>
        private void ClearAllClickPointMarkers()
        {
            greenCircleDrawCount = 0;
            drawLineCount = 0;
            maskAlphaChannelCount = 0;
            drawRectCornerCount = 0;
            LastClickedTargetPoint = new Point();
            lastDrawRectPoint_UL = new Point();
            lastDrawRectPoint_LR = new Point();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up to draw a green circle for every mouse click for a specified
        /// number of mouse clicks
        /// </summary>
        private void buttonDrawGreenCircleAtClicks_Click(object sender, EventArgs e)
        {
            ClearAllClickPointMarkers();

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
        private void buttonDrawLineCenteredAtClick_Click(object sender, EventArgs e)
        {
            ClearAllClickPointMarkers();

            try
            {
                // get the value from the screen, and place it in this flag
                drawLineCount = DrawLineMouseClicks;
            }
            catch { }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line through a point. The overlay must be activated and loaded
        /// </summary>
        private void buttonDrawLineThroughPoint_Click(object sender, EventArgs e)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            // get the draw point off the screen
            Point centerPoint = DrawLineManualDrawThroughPoint;
            // get the width off the screen
            int width = DrawLineWidth;
            // get the length off the screen
            int length = DrawLineLength;
            // do we want it to go vertical
            bool wantVert = DrawLineWantVertLine;
            // draw the line
            DrawLineThroughPointOnOverlay(GetDrawLineColor, centerPoint, width, length, wantVert);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a press on the DrawFilledRectFromCorners button. This draws
        /// a filled rectangle using the Upper Left and Lower Right corners
        /// </summary>
        private void buttonDrawFilledRectFromCorners_Click(object sender, EventArgs e)
        {

            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            // just set this ctlTransparentControl1_MouseClick does the rest
            drawRectCornerCount = 2;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line through a point on the overlay at a specified point
        /// and of a specified width. 
        /// </summary>
        /// <param name="lineColor">the line color to draw</param>
        /// <param name="pointIn">the point</param>
        /// <param name="width">the line width</param>
        /// <param name="length">the line length. if <0 we draw fill width or height</param>
        /// <param name="wantVert">if true we draw a vertical line, otherwise horiz.</param>
        private void DrawLineThroughPointOnOverlay(Color lineColor, Point pointIn, int width, int length, bool wantVert)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (pointIn == null) return;
            if (width <= 0) return;

            using (Pen xPen = new Pen(lineColor, width))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).DrawLineThroughPointOnOverlay(xPen, pointIn, length, wantVert);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a line through a point on the tracker at a specified point
        /// and of a specified width. 
        /// </summary>
        /// <param name="lineColor">the line color to draw</param>
        /// <param name="pointIn">the point</param>
        /// <param name="width">the line width</param>
        /// <param name="length">the line length. if <0 we draw fill width or height</param>
        /// <param name="wantVert">if true we draw a vertical line, otherwise horiz.</param>
        private void DrawLineThroughPointOnTracker(Color lineColor, Point pointIn, int width, int length, bool wantVert)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (pointIn == null) return;
            if (width <= 0) return;

            using (Pen xPen = new Pen(lineColor, width))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).DrawLineThroughPointOnTracker(xPen, pointIn, length, wantVert);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a filled rectangle. The overlay must be activated and loaded
        /// </summary>
        /// <param name="corner1">the first corner point, usually UL</param>
        /// <param name="corner2">the second corner point, usually LR</param>
        /// <param name="fillColor">the color to use to fill the retangle</param>
        private void DrawFilledRectangleByCornersOnOverlay(Point corner1, Point corner2, Color fillColor)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            if (fillColor == null) return;
            // some sanity checks
            if (corner1 == null) return;
            if (corner2 == null) return;
            if (corner1.IsEmpty == true) return;
            if (corner2.IsEmpty == true) return;

            Rectangle rect = Utils.GetRectangleFromTwoPoints(corner1, corner2); 

            if (rect.Width == 0) return;
            if (rect.Height == 0) return;

            using (SolidBrush brsh = new SolidBrush(fillColor))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).FillRectangularRegionOnOverlay(brsh, rect);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a rectangle on the screen (on the overlay actually) centered at a specified point
        /// and of the specified width and height. Will not draw down in the Chyron
        /// </summary>
        /// <param name="centerPoint">the center point</param>
        /// <param name="colorAsHTML">the color as an HTML string. IE "#ff00ff00" is green</param>
        /// <param name="width">the width of the rectangle, cannot be 0 or -ve</param>
        /// <param name="height">the height of the rectangle, cannot be 0 or -ve</param>
        private void DrawRectagleAtPointOnOverlay(Point centerPoint, int width, int height, string colorAsHTML)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (centerPoint == null) return;
            if (centerPoint.X < 0) return;
            if (centerPoint.Y < 0) return;
            if (colorAsHTML == null) return;
            if (colorAsHTML.Length == 0) return;

            Rectangle rect = Utils.GetRectangleFromCenterPoint(centerPoint, width, height, (ImageOverlayTransform as MFTOverlayImage_GS).GetMinEffectiveScreenWidth, (ImageOverlayTransform as MFTOverlayImage_GS).GetMinEffectiveScreenHeight, (ImageOverlayTransform as MFTOverlayImage_GS).GetMaxEffectiveScreenWidth, (ImageOverlayTransform as MFTOverlayImage_GS).GetMaxEffectiveScreenHeight);
            if (rect.IsEmpty == true) return;

            using (SolidBrush brsh = new SolidBrush(ColorTranslator.FromHtml(colorAsHTML)))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).FillRectangularRegionOnOverlay(brsh, rect);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a rectangle on the screen (on the overlay actually) centered at a specified point
        /// and of the specified width and height. Will not draw down in the Chyron
        /// </summary>
        /// <param name="centerPoint">the center point</param>
        /// <param name="intersectColor">the color which we mask and update the alpha channel of</param>
        /// <param name="width">the width of the rectangle, cannot be 0 or -ve</param>
        /// <param name="height">the height of the rectangle, cannot be 0 or -ve</param>
        private void SetAlphaValueForColorInRectOnOverlay(Point centerPoint, int width, int height, Color intersectColor, int newAlphaValue)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (centerPoint == null) return;
            if (centerPoint.X < 0) return;
            if (centerPoint.Y < 0) return;
            if (intersectColor == null) return;

            Rectangle rect = Utils.GetRectangleFromCenterPoint(centerPoint, width, height, (ImageOverlayTransform as MFTOverlayImage_GS).GetMinEffectiveScreenWidth, (ImageOverlayTransform as MFTOverlayImage_GS).GetMinEffectiveScreenHeight, (ImageOverlayTransform as MFTOverlayImage_GS).GetMaxEffectiveScreenWidth, (ImageOverlayTransform as MFTOverlayImage_GS).GetMaxEffectiveScreenHeight);
            if (rect.IsEmpty == true) return;

            (ImageOverlayTransform as MFTOverlayImage_GS).SetAlphaValueForColorInRectOnOverlay(intersectColor, newAlphaValue, rect);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a circle on the overlay at a specified point and of a specified radius
        /// 
        /// Note: Uses whatever Alpha channel is in the drawColor
        /// 
        /// </summary>
        /// <param name="pointIn">the point</param>
        /// <param name="radius">the radius</param>
        /// <param name="drawColor">the drawColor</param>
        private void DrawCircleAtPointOnOverlay(Point pointIn, int radius, Color drawColor)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (pointIn == null) return;
            if (radius <= 0) return;
            if (drawColor == null) return;

            using (SolidBrush brsh = new SolidBrush(drawColor))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).FillCircularRegionOnOverlay(brsh, pointIn, radius);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a circle on the tracker at a specified point and of a specified radius
        /// 
        /// Note: Uses whatever Alpha channel is in the drawColor
        /// 
        /// </summary>
        /// <param name="pointIn">the point</param>
        /// <param name="radius">the radius</param>
        /// <param name="drawColor">the drawColor</param>
        private void DrawCircleAtPointOnTracker(Point pointIn, int radius, Color drawColor)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (pointIn == null) return;
            if (radius <= 0) return;
            if (drawColor == null) return;

            using (SolidBrush brsh = new SolidBrush(drawColor))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).FillCircularRegionOnTracker(brsh, pointIn, radius);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Draws a circle as an outline on the screen (on the overlay actually) 
        /// at a specified point and of a specified radius and line thickness
        /// </summary>
        /// <param name="pointIn">the point</param>
        /// <param name="radius">the radius</param>
        /// <param name="lineThickness">the line thickness in pixels</param>
        /// <param name="colorAsHTML">the color as an HTML string. IE "#ff00ff00" is green</param>
        private void DrawOutlineCircleAtPoint(Point pointIn, int radius, int lineThickness, string colorAsHTML)
        {
            // sanity checks
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;
            if (pointIn == null) return;
            if (radius <= 0) return;
            if (lineThickness <= 0) return;
            if (colorAsHTML == null) return;
            if (colorAsHTML.Length == 0) return;

            using (SolidBrush brsh = new SolidBrush(ColorTranslator.FromHtml(colorAsHTML)))
            {
                (ImageOverlayTransform as MFTOverlayImage_GS).DrawCircleOnOverlayAsOutlineOnOverlay(brsh, pointIn, radius, lineThickness);
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
                    return Convert.ToInt32(textBoxDrawGreenCircleDrawMouseClicks.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxDrawGreenCircleDrawMouseClicks.Text = value.ToString();
            }
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the draw line color. Note that there are some complications with
        /// Green colors. Color.Green is 0xff000800 not 0xff00ff00 as you would expect
        /// 
        /// See: https://stackoverflow.com/questions/4342300/why-is-system-drawing-color-green-0-128-0
        /// 
        /// Be aware that the default in here takes care of that special case
        /// </summary>
        private Color GetDrawLineColor
        {
            get
            {
                if (radioButtonDrawLineColorRed.Checked == true) return TRUE_RED;
                else if (radioButtonDrawLineColorBlue.Checked == true) return TRUE_BLUE;
                else return TRUE_GREEN; // default
            }
            set
            {
                if (value == TRUE_RED) radioButtonDrawLineColorRed.Checked = true;
                else if (value == TRUE_BLUE) radioButtonDrawLineColorBlue.Checked = true;
                else radioButtonDrawLineColorGreen.Checked = true; // default
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the number of mouse clicks we can draw red/green/blue lines on
        /// 
        /// Now hard coded to 1
        /// </summary>
        private int DrawLineMouseClicks
        {
            get
            {
                //try
                //{
                //    // get the data off the screen
                //    return Convert.ToInt32(textBoxDrawDrawLineMouseClicks.Text);
                //}
                //catch
                //{
                //    return 0;
                //}
                return 1;
            }
            set
            {
                // simple value
                //textBoxDrawDrawLineMouseClicks.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the draw line width. 
        /// </summary>
        private int DrawLineWidth
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxDrawLineWidth.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxDrawLineWidth.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the draw line length. 
        /// </summary>
        private int DrawLineLength
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxDrawLineLength.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxDrawLineLength.Text = value.ToString();
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
                    return Convert.ToInt32(textBoxDrawGreenCircleRadius.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxDrawGreenCircleRadius.Text = value.ToString();
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
                textBoxDrawGreenCircleXY.Text = value.X.ToString() + "," + value.Y.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the red/green/blue draw line through point. 
        /// </summary>
        private Point DrawLineManualDrawThroughPoint
        {
            get
            {
                return Utils.ConvertBracketTextToPoint(textBoxDrawLineXY.Text);
            }
            set
            {
                // simple comma separated value
                textBoxDrawLineXY.Text = value.X.ToString() + "," + value.Y.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the draw line should be vertical flag
        /// </summary>
        private bool DrawLineWantVertLine
        {
            get
            {
                if (radioButtonDrawLineWantVert.Checked == true) return true;
                else return false;
            }
            set
            {
                if (value == true) radioButtonDrawLineWantVert.Checked = true;
                else radioButtonDrawLineWantHoriz.Checked = true;
            }
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
            textBoxRGBAPixelColor.Text = outColor.R.ToString() + "," + outColor.G.ToString() + "," + outColor.B.ToString() + " (" + outColor.A.ToString() + ")";

            // now get the pixel color on the overlay if we have one. This can be different due to the blended
            // compositing on the actual display
            if ((ImageOverlayTransform != null) && ((ImageOverlayTransform is MFTOverlayImage_GS) == true))
            {
                outColor = (ImageOverlayTransform as MFTOverlayImage_GS).GetColorOnOverlayAtPoint(outPointNonInverted);
                textBoxRGBAOverlayPixelColor.Text = outColor.R.ToString() + "," + outColor.G.ToString() + "," + outColor.B.ToString() + " (" + outColor.A.ToString() + ")";
            }
            else textBoxRGBAOverlayPixelColor.Text = "";

            // ####
            // Now the pixel count difference between mouse clicks

            // we always put the new click in point 2 and the previous click in click 1, the point data is stored in the tag
            textBoxDistanceClick1.Tag = textBoxDistanceClick2.Tag;
            textBoxDistanceClick2.Tag = outPointInverted;
            // set the text, always switch them over
            textBoxDistanceClick1.Text = "";
            textBoxDistanceClick2.Text = "";
            textBoxDistanceInPixelsHoriz.Text = "";
            textBoxDistanceInPixelsVert.Text = "";
            if (textBoxDistanceClick1.Tag != null)
            {
                textBoxDistanceClick1.Text = Utils.ConvertPointToBracketText((Point)textBoxDistanceClick1.Tag);
            }
            if (textBoxDistanceClick2.Tag != null)
            {
                textBoxDistanceClick2.Text = Utils.ConvertPointToBracketText((Point)textBoxDistanceClick2.Tag);
            }

            // ####
            // set the pixel differences between mouse clicks
            SetPixelDistancesOnUtilsTablToReality();

            // ####
            // If we are calibrated we can calc the micron difference between mouse clicks
            SetMicronDistancesOnUtilsTabToReality();

            // ####
            // Now do we need to draw green circles?
            if (greenCircleDrawCount > 0)
            {
                // yes, we do.
                if (WantDrawGreenOutlineCircle == true)
                {
                    DrawOutlineCircleAtPoint(outPointInverted, GreenCircleRadius, DrawGreenOutlineCircleLineWidth, HTML_GREEN);
                }
                else
                {
                    // draw the circle
                    DrawCircleAtPointOnOverlay(outPointInverted, GreenCircleRadius, TRUE_GREEN);
                }
                greenCircleDrawCount--;
                // record this
                LastClickedTargetPoint = outPointInverted;
            }

            // ####
            // Now do we need to draw red/green/blue lines?
            if (drawLineCount > 0)
            {
                // yes, we do.
                // draw the line
                DrawLineThroughPointOnOverlay(GetDrawLineColor, outPointInverted, DrawLineWidth, DrawLineLength, DrawLineWantVertLine);
                drawLineCount--;
            }

            // ####
            // Now do we need to draw a filled rectangle?
            if (drawRectCornerCount == 2)
            {
                // yes, we do.
                lastDrawRectPoint_UL = outPointInverted;
                drawRectCornerCount--;
            }
            else if (drawRectCornerCount == 1)
            {
                // yes, we do.
                lastDrawRectPoint_LR = outPointInverted;
                drawRectCornerCount = 0;
                DrawFilledRectangleByCornersOnOverlay(lastDrawRectPoint_UL, lastDrawRectPoint_LR, GetDrawLineColor);
            }

            // Now do we need to mask an alpha channel?
            if (maskAlphaChannelCount > 0)
            {
                // yes, we do.
                SetAlphaValueForColorInRectOnOverlay(outPointInverted, AlphaMaskRectWidth, AlphaMaskRectHeight, GetAlphaMaskIntersectColor, AlphaMaskValue);
                maskAlphaChannelCount = 0;
            }
        }
        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// += CALIBRATION CODE      =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region CalibrationCode

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calc the pixel distances from the information on the Utils tab. Will return 
        /// 0 if the information cannot be calculated
        /// </summary>
        private void CalcPixelDistancesFromUtilsPanelMouseClicks(out int xDistInPixels, out int yDistInPixels, out int xyDistInPixels)
        {
            xDistInPixels = 0;
            yDistInPixels = 0;
            xyDistInPixels = 0;

            // if we have two measurements then calc the difference
            if ((textBoxDistanceClick1.Tag != null) && (textBoxDistanceClick2.Tag != null)
                && ((textBoxDistanceClick1.Tag is Point) == true) && ((textBoxDistanceClick2.Tag is Point) == true))
            {
                int c1X = ((Point)textBoxDistanceClick1.Tag).X;
                int c1Y = ((Point)textBoxDistanceClick1.Tag).Y;
                int c2X = ((Point)textBoxDistanceClick2.Tag).X;
                int c2Y = ((Point)textBoxDistanceClick2.Tag).Y;
                xDistInPixels = Math.Abs(c2X - c1X);
                yDistInPixels = Math.Abs(c2Y - c1Y);
                xyDistInPixels = (int)Math.Round(Math.Sqrt((xDistInPixels * xDistInPixels) + (yDistInPixels * yDistInPixels)), 0);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the pixel distances field on the Utils tabl to reality. 
        /// </summary>
        /// <param name="xDistInPixels">the current xDist in pixels</param>
        /// <param name="yDistInPixels">the current yDist in pixels</param>
        /// <param name="xyDistInPixels">the current xyDist in pixels</param>
        private void SetPixelDistancesOnUtilsTablToReality()
        {
            // get the current pixel distances, will be zero if not present
            CalcPixelDistancesFromUtilsPanelMouseClicks(out int xDistInPixels, out int yDistInPixels, out int xyDistInPixels);

            // clear all
            textBoxDistanceInPixelsHoriz.Text = "";
            textBoxDistanceInPixelsVert.Text = "";
            textBoxDistInPixelsTotal.Text = "";

            textBoxDistanceInPixelsHoriz.Text = xDistInPixels.ToString();
            textBoxDistanceInPixelsVert.Text = yDistInPixels.ToString();
            textBoxDistInPixelsTotal.Text = xyDistInPixels.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the micron distances field on the Utils tabl to reality. If not calibrated
        /// this just clears the fields
        /// </summary>
        /// <param name="xDistInPixels">the current xDist in pixels</param>
        /// <param name="yDistInPixels">the current yDist in pixels</param>
        /// <param name="xyDistInPixels">the current xyDist in pixels</param>
        private void SetMicronDistancesOnUtilsTabToReality()
        {
            // get the current pixel distances, will be zero if not present
            CalcPixelDistancesFromUtilsPanelMouseClicks(out int xDistInPixels, out int yDistInPixels, out int xyDistInPixels);

            // If we are calibrated we can calc the micron difference between mouse clicks
            double pixelsPerMicron = CalibratedPixelsPerMicron;

            // clear it all down
            textBoxDistanceInMicronsHoriz.Text = "";
            textBoxDistanceInMicronsVert.Text = "";
            textBoxDistInMicronsTotal.Text = "";
            if (pixelsPerMicron > 0)
            {
                // we are calibrated
                textBoxDistanceInMicronsHoriz.Text = Convert.ToInt32((xDistInPixels / pixelsPerMicron)).ToString();
                textBoxDistanceInMicronsVert.Text = Convert.ToInt32((yDistInPixels / pixelsPerMicron)).ToString();
                textBoxDistInMicronsTotal.Text = Convert.ToInt32((xyDistInPixels / pixelsPerMicron)).ToString();
                // make them active
                textBoxDistanceInMicronsHoriz.Enabled = true;
                textBoxDistanceInMicronsVert.Enabled = true;
                textBoxDistInMicronsTotal.Enabled = true;
                labelDistanceInMicrons.Enabled = true;
                labelDistanceInMicronsHoriz.Enabled = true;
                labelDistanceInMicronsVert.Enabled = true;
                labelDistanceInMicronsTotal.Enabled = true;
            }
            else
            {
                // grey them out
                textBoxDistanceInMicronsHoriz.Enabled = false;
                textBoxDistanceInMicronsVert.Enabled = false;
                textBoxDistInMicronsTotal.Enabled = false;
                labelDistanceInMicrons.Enabled = false;
                labelDistanceInMicronsHoriz.Enabled = false;
                labelDistanceInMicronsVert.Enabled = false;
                labelDistanceInMicronsTotal.Enabled = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Calculates the pixels per micron value on the distance calibration panel
        /// and sets the appropriate fields on the main screen.
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
                ClearAllCalibration();
                return;
            }

            if (radioButtonDistVert.Checked == true)
            {
                try
                {
                    knownDist = Convert.ToDouble(textBoxDistanceInPixelsVert.Text);
                }
                catch (Exception ex)
                {
                    LogMessage(" CalcDistanceCalibration (knownDist_V):" + ex.Message);
                    ClearAllCalibration();
                    return;
                }

            }
            else if (radioButtonDistHoriz.Checked == true)
            {
                try
                {
                    knownDist = Convert.ToDouble(textBoxDistanceInPixelsHoriz.Text);
                }
                catch (Exception ex)
                {
                    LogMessage(" CalcDistanceCalibration (knownDist_H):" + ex.Message);
                    ClearAllCalibration();
                    return;
                }
            }
            else
            {
                LogMessage(" CalcDistanceCalibration unknown direction");
                ClearAllCalibration();
                return;
            }

            // divide by zero check
            if (knownMicronLen <= 0)
            {
                ClearAllCalibration();
                return;
            }

            // now do the calc, and load the box
            textBoxScalePixelsPerMicron.Text = Math.Round((knownDist / knownMicronLen), 5).ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears all calibration values
        /// </summary>
        private void ClearAllCalibration()
        {
            CalibratedPixelsPerMicron = 0;
            SetMicronDistancesOnUtilsTabToReality();
            ClearGridFromOverlay();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a micron value to pixels. 
        /// </summary>
        /// <param name="micronValue">the value in microns</param>
        /// <returns>micron value in pixels or -ve for fail</returns>
        private int ConvertMicronsToPixels(int micronValue)
        {
            if (micronValue < 0) return -2;
            if (IsCalibrated() == false) return -1;
            return (int)(Convert.ToDouble(micronValue) * CalibratedPixelsPerMicron);
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
            // set the new calibration setting
            try
            {
                CalibratedPixelsPerMicron = Convert.ToDouble(textBoxScalePixelsPerMicron.Text);
            }
            catch
            {
                CalibratedPixelsPerMicron = 0;
            }
            SetMicronDistancesOnUtilsTabToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears the Calibrated Pixels per micron value on the display. 
        /// </summary>
        private void buttonScaleClear_Click(object sender, EventArgs e)
        {
            CalibratedPixelsPerMicron = 0;
            SetMicronDistancesOnUtilsTabToReality();
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
                textBoxCalibratedPixelsPerMicron.Text = Math.Round(value, 5).ToString();
                // set whatever we have on the text transform
                if (((TextOverlayTransform != null) && (TextOverlayTransform is MFTWriteText_Sync) == true))
                {
                    (TextOverlayTransform as MFTWriteText_Sync).SetCalibrationBarData(CalibratedPixelsPerMicron);
                }
                // was it <= zero? Just clear it
                if (value <= 0) textBoxScalePixelsPerMicron.Text = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we are calibrated
        /// </summary>
        /// <returns>true - we are calibrated, false - not calibrated</returns>
        private bool IsCalibrated()
        {
            if (CalibratedPixelsPerMicron >= 0) return true;
            return false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Enables the grid on the screen
        /// </summary>
        private void checkBoxUtilsGridEnabled_CheckedChanged(object sender, EventArgs e)
        {
            // do we have a proper transform?
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            if (checkBoxUtilsGridEnabled.Checked == false)
            {
                // clear the grid from the image
                ClearGridFromOverlay();
            }
            else
            {
                // we are enabling the grid
                SetGridOnScreen();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Do everything necessary to set the grid on the screen
        /// </summary>
        private void SetGridOnScreen()
        {
            int gridCountX = 0;
            int gridCountY = 0;
            int gridBarSizeX = 0;
            int gridBarSizeY = 0;
            int gridSpacingMicrons = 0;
            int gridSpacingPixels = 0;
            Color? gridColor = null;

            // do we have a proper transform?
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            // clear the grid from the image
            ClearGridFromOverlay();

            // we need the grid color 
            gridColor = GridColor;
            if (gridColor == null)
            {
                OISMessageBox("Invalid Color");
                return;
            }

            // we need the X an Y counts of the grid
            gridCountX = GridCountX;
            if (gridCountX <= 0)
            {
                OISMessageBox("Invalid X grid count");
                return;
            }
            gridCountY = GridCountY;
            if (gridCountY <= 0)
            {
                OISMessageBox("Invalid Y grid count");
                return;
            }

            // we need the X an Y barsize of the grid
            gridBarSizeX = GridBarSizeX;
            if (gridBarSizeX <= 0)
            {
                OISMessageBox("Invalid X grid barsize");
                return;
            }
            gridBarSizeY = GridBarSizeY;
            if (gridBarSizeY <= 0)
            {
                OISMessageBox("Invalid Y grid barsize");
                return;
            }

            // we need the grid spacing in microns
            if (IsCalibrated() == false)
            {
                OISMessageBox("Not Calibrated");
                return;
            }

            gridSpacingMicrons = GridSpacingInMicrons;
            if (gridSpacingMicrons <= 0)
            {
                OISMessageBox("Invalid grid micron spacing");
                return;
            }
            gridSpacingPixels = ConvertMicronsToPixels(gridSpacingMicrons);
            if (gridSpacingPixels <= 0)
            {
                OISMessageBox("Invalid grid to pixel conversion");
                return;
            }

            // now draw the grid
            (ImageOverlayTransform as MFTOverlayImage_GS).SetGrid(true, (Color)gridColor, gridCountX, gridCountY, gridBarSizeX, gridBarSizeY, gridSpacingPixels);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the grid color
        /// </summary>
        private Color? GridColor
        {
            get
            {
                return Utils.ConvertBracketTextToColor(textBoxUtilsGridColor.Text);
            }
            set
            {
                textBoxUtilsGridColor.Text = Utils.ConvertColorToRGBBracketText((Color)value);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the spacing between the grid points in microns
        /// </summary>
        /// <returns>the number of microns between grid points or <=0 for fail</returns>
        private int GridSpacingInMicrons
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxUtilsGridSpacingMicrons.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxUtilsGridSpacingMicrons.Text = value.ToString();
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the number of grid points in the X direction
        /// </summary>
        /// <returns>the number of grid points in X direction or <=0 for fail</returns>
        private int GridCountX
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxUtilsGridSizeX.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxUtilsGridSizeX.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the number of grid points in the Y direction
        /// </summary>
        /// <returns>the number of grid points in Y direction or <=0 for fail</returns>
        private int GridCountY
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxUtilsGridSizeY.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxUtilsGridSizeY.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the grid barsize in the X direction
        /// </summary>
        /// <returns>the grid barsize in X direction or <=0 for fail</returns>
        private int GridBarSizeX
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxUtilsGridBarSizeX.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxUtilsGridBarSizeX.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the grid barsize in the Y direction
        /// </summary>
        /// <returns>the grid barsize in Y direction or <=0 for fail</returns>
        private int GridBarSizeY
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxUtilsGridBarSizeY.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxUtilsGridBarSizeY.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears the grid from the image
        /// </summary>
        private void ClearGridFromOverlay()
        {
            // do we have a proper transform?
            if (ImageOverlayTransform == null) return;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            (ImageOverlayTransform as MFTOverlayImage_GS).GridEnabled = false;
            (ImageOverlayTransform as MFTOverlayImage_GS).ClearGrid();
        }

        #endregion // CALIBRATION

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+ RECOGNITON CODE +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region RecognitionCode

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the buttonColorDetectColorDetectSet button in which we
        /// set the colors the object recognition transform triggers on.
        /// </summary>
        private void buttonColorDetectColorDetectSet_Click(object sender, EventArgs e)
        {
            SetObjectRecognitionValues();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the colors and other values used to recognize objects via a histogram
        /// </summary>
        private void SetObjectRecognitionValues()
        {
            // do we have a recognition transform?
            if (RecognitionTransform == null) return;  // we can do nothing
            if ((RecognitionTransform is MFTDetectObjectViaHistogram) == false) return;

            // convert our colors
            Color? topOfHorizRange = Utils.ConvertBracketTextToColor(textBoxColorDetectHorizTop.Text);
            Color? botOfHorizRange = Utils.ConvertBracketTextToColor(textBoxColorDetectHorizBot.Text);
            Color? topOfVertRange = Utils.ConvertBracketTextToColor(textBoxColorDetectVertTop.Text);
            Color? botOfVertRange = Utils.ConvertBracketTextToColor(textBoxColorDetectVertBot.Text);

            if (topOfHorizRange == null) return;
            if (botOfHorizRange == null) return;
            if (topOfVertRange == null) return;
            if (botOfVertRange == null) return;

            // Set the color boundaries we trigger on to detect the lines
            (RecognitionTransform as MFTDetectObjectViaHistogram).TopOfHorizRange = (Color)topOfHorizRange;
            (RecognitionTransform as MFTDetectObjectViaHistogram).BotOfHorizRange = (Color)botOfHorizRange;
            (RecognitionTransform as MFTDetectObjectViaHistogram).TopOfVertRange = (Color)topOfVertRange;
            (RecognitionTransform as MFTDetectObjectViaHistogram).BotOfVertRange = (Color)botOfVertRange;

            // also set the minimum number acceptable pixels
            try
            {
                (RecognitionTransform as MFTDetectObjectViaHistogram).MinPixelsInLineHoriz = Convert.ToInt32(textBoxColorDetectMinPixelsHoriz.Text);
            }
            catch { }
            // also set the minimum number acceptable pixels
            try
            {
                (RecognitionTransform as MFTDetectObjectViaHistogram).MinPixelsInLineVert = Convert.ToInt32(textBoxColorDetectMinPixelsVert.Text);
            }
            catch { }

            // set the recognition modes
            (RecognitionTransform as MFTDetectObjectViaHistogram).HorizLineRecognitionMode = HorizLineRecognitionMode;
            (RecognitionTransform as MFTDetectObjectViaHistogram).VertLineRecognitionMode = VertLineRecognitionMode;
            (RecognitionTransform as MFTDetectObjectViaHistogram).YValAboveFloorPreDropMinLimit = LineDetectHoriz_PreDrop;
            (RecognitionTransform as MFTDetectObjectViaHistogram).YValBelowFloorPostDropMinLimit = LineDetectHoriz_PostDrop;
            (RecognitionTransform as MFTDetectObjectViaHistogram).YValDropFloor = LineDetectHoriz_Floor;
            (RecognitionTransform as MFTDetectObjectViaHistogram).YValOffset = LineDetectHoriz_Offset;
            (RecognitionTransform as MFTDetectObjectViaHistogram).XValOffset = LineDetectVert_Offset;

            // enabled state
            (RecognitionTransform as MFTDetectObjectViaHistogram).ObjectDetectionEnabled = ObjectDetectionEnabled;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Horiz line detect mode
        /// </summary>
        private LineRecognitionModeEnum HorizLineRecognitionMode
        {
            get
            {
                if (radioButtonLineDetectHoriz_DropOff.Checked == true) return LineRecognitionModeEnum.LRM_LAST_BEFORE_DROP;
                else return LineRecognitionModeEnum.LRM_MAXCOUNT;
            }
            set
            {
                if (value == LineRecognitionModeEnum.LRM_LAST_BEFORE_DROP) radioButtonLineDetectHoriz_DropOff.Checked = true;
                else radioButtonLineDetectHoriz_MaxCount.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Vert line detect mode
        /// </summary>
        private LineRecognitionModeEnum VertLineRecognitionMode
        {
            get
            {
                return LineRecognitionModeEnum.LRM_MAXCOUNT;
            }
            set
            {
                // only option at the moment
                radioButtonLineDetectVert_MaxCount.Checked = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Horiz line detect Floor
        /// </summary>
        /// <returns>the min number of pixels or <0 for fail</returns>
        private int LineDetectHoriz_Floor
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxLineDetectHoriz_Floor.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxLineDetectHoriz_Floor.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Horiz line detect Offset
        /// </summary>
        /// <returns>the offset of the detected line - can be negative</returns>
        private int LineDetectHoriz_Offset
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxLineDetectHoriz_Offset.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxLineDetectHoriz_Offset.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Vert line detect Offset
        /// </summary>
        /// <returns>the offset of the detected line - can be negative</returns>
        private int LineDetectVert_Offset
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxLineDetectVert_Offset.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxLineDetectVert_Offset.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Horiz line detect PostDrop value
        /// </summary>
        /// <returns>the min number of pixels or <0 for fail</returns>
        private int LineDetectHoriz_PostDrop
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxLineDetectHoriz_PostDrop.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxLineDetectHoriz_PostDrop.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the Horiz line detect PreDrop value
        /// </summary>
        /// <returns>the min number of pixels or <0 for fail</returns>
        private int LineDetectHoriz_PreDrop
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxLineDetectHoriz_PreDrop.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxLineDetectHoriz_PreDrop.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the red circle radius for color detection
        /// </summary>
        private int ColorDetectRedCircleRadius
        {
            get
            {
                try
                {
                    // get the draw point off the screen
                    return Convert.ToInt32(textBoxColorDetectRedCircleRadius.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // simple value
                textBoxColorDetectRedCircleRadius.Text = value.ToString();
            }
        }

        public Point LastClickedTargetPoint { get => lastClickedTargetPoint; set => lastClickedTargetPoint = value; }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle Checked Changed event on the radioButtonLineDetectHoriz_MaxCount control
        /// </summary>
        private void radioButtonLineDetectHoriz_MaxCount_CheckedChanged(object sender, EventArgs e)
        {
            SyncLineDetectHorizOptionsToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle Checked Changed event on the radioButtonLineDetectHoriz_DropOff control
        /// </summary>
        private void radioButtonLineDetectHoriz_DropOff_CheckedChanged(object sender, EventArgs e)
        {
            SyncLineDetectHorizOptionsToReality();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the enable state on the horizontal line detect group box to reality
        /// </summary>
        private void SyncLineDetectHorizOptionsToReality()
        {
            if (radioButtonLineDetectHoriz_MaxCount.Checked == true)
            {
                // radioButtonLineDetectHoriz_MaxCount is checked
                textBoxLineDetectHoriz_Floor.Enabled = false;
                textBoxLineDetectHoriz_PreDrop.Enabled = false;
                textBoxLineDetectHoriz_PostDrop.Enabled = false;
                labelLineDetectHoriz_Floor.Enabled = false;
                labelLineDetectHoriz_PreDrop.Enabled = false;
                labelLineDetectHoriz_PostDrop.Enabled = false;
                labelLineDetectHoriz_PostDropCount.Enabled = false;
                labelLineDetectHoriz_PreDropCount.Enabled = false;
                labelLineDetectHoriz_FloorCount.Enabled = false;
            }
            else
            {
                // radioButtonLineDetectHoriz_DropOff is checked
                textBoxLineDetectHoriz_Floor.Enabled = true;
                textBoxLineDetectHoriz_PreDrop.Enabled = true;
                textBoxLineDetectHoriz_PostDrop.Enabled = true;
                labelLineDetectHoriz_Floor.Enabled = true;
                labelLineDetectHoriz_PreDrop.Enabled = true;
                labelLineDetectHoriz_PostDrop.Enabled = true;
                labelLineDetectHoriz_PostDropCount.Enabled = true;
                labelLineDetectHoriz_PreDropCount.Enabled = true;
                labelLineDetectHoriz_FloorCount.Enabled = true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle Checked Changed event on the checkBoxObjectDetection_Enable control
        /// </summary>
        private void checkBoxObjectDetection_Enable_CheckedChanged(object sender, EventArgs e)
        {
            SyncAllLineDetectOptionsToReality();
            // tell the transform about the changes
            SetObjectRecognitionValues();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Syncs the enable state on the all line detect controls to reality
        /// </summary>
        private void SyncAllLineDetectOptionsToReality()
        {
            if (checkBoxObjectDetection_Enable.Checked == true)
            {
                groupBoxLineDetect_Vert.Enabled = true;
                groupBoxLineDetect_Horiz.Enabled = true;
                buttonColorDetectColorDetectSet.Enabled = true;
                checkBoxColorDetectShowVertLine.Enabled = true;
                checkBoxColorDetectShowHorizLine.Enabled = true;
                checkBoxColorDetectDrawCircleOnIntersection.Enabled = true;
                textBoxColorDetectRedCircleRadius.Enabled = true;
                checkBoxColorDetectClearRedEveryFrame.Enabled = true;
                labelLineDetectDrawCircleOnIntersectionPixels.Enabled = true;
            }
            else
            {
                groupBoxLineDetect_Vert.Enabled = false;
                groupBoxLineDetect_Horiz.Enabled = false;
                buttonColorDetectColorDetectSet.Enabled = false;
                checkBoxColorDetectShowVertLine.Enabled = false;
                checkBoxColorDetectShowHorizLine.Enabled = false;
                checkBoxColorDetectDrawCircleOnIntersection.Enabled = false;
                textBoxColorDetectRedCircleRadius.Enabled = false;
                checkBoxColorDetectClearRedEveryFrame.Enabled = false;
                labelLineDetectDrawCircleOnIntersectionPixels.Enabled = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the line recognition value
        /// </summary>
        private bool ObjectDetectionEnabled
        {
            get
            {
                return checkBoxObjectDetection_Enable.Checked;
            }
            set
            {
                checkBoxObjectDetection_Enable.Checked = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// indicates if we have detected a Red point
        /// </summary>
        private bool HaveLastDetectedSourcePoint
        {
            get
            {

                if (lastDetectedSourcePoint.IsEmpty == true) return false;
                else return true;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the last detected source point
        /// 
        /// NOTE: this also implements IBehaviour_SourcePoint
        /// </summary>
        public Point SourcePoint
        {
            get
            {
                return lastDetectedSourcePoint;
            }
            set
            {
                lastDetectedSourcePoint = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the last detected source point pixel color. this is the color
        /// under the source point on the screen not on the overlay or tracker
        /// 
        /// NOTE: this also implements IBehaviour_SourcePointDetectedPixelColor_Screen
        /// </summary>
        /// <returns>the color on the screen or Color.IsEmpty for fail</returns>
        public Color LastDetectedSourcePointPixelColor_Screen()
        {
            return lastDetectedSourcePointPixelColor_Screen;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the last detected source point pixel color. this is the color
        /// under the source point on the screen not on the overlay or tracker
        /// 
        /// NOTE: this also implements IBehaviour_SourcePointDetectedPixelColor_Overlay
        /// </summary>
        /// <returns>the color on the overlay or Color.IsEmpty for fail</returns>
        public Color LastDetectedSourcePointPixelColor_Overlay()
        {
            if (ImageOverlayTransform == null) new Color();
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return new Color();
            if (HaveLastDetectedSourcePoint == false) return new Color();

            return (ImageOverlayTransform as MFTOverlayImage_GS).GetColorOnOverlayAtPoint(SourcePoint);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the last detected alpha value from the pixels masked around the 
        /// source point pixel color. In other words this is the lowest
        /// alpha value from the pixels under the arae source point on the overlay 
        /// which was masked off
        /// 
        /// NOTE: this also implements IBehaviour_SourcePointDetectedLowestAlphaValue_Overlay
        /// </summary>
        /// <returns>the lowest alpha value masked on the overlay or 255 for fail</returns>
        public byte LastDetectedSourcePointDetectedLowestAlphaValue_Overlay()
        {
            if (ImageOverlayTransform == null) return 255;
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return 255;
            if (HaveLastDetectedSourcePoint == false) return 255;

            return (ImageOverlayTransform as MFTOverlayImage_GS).LowestAlphaFoundOnMask;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Activates or deactivates the detection mechanism
        /// 
        /// NOTE: this also implements Behaviour_DetectionActivate
        /// 
        /// </summary>
        /// <param name="activationState">the state of the activation</param>
        /// <param name="markDetectedPoint">if true we mark the detected object</param>
        public void DetectionActivate(bool activationState, bool markDetectedPoint)
        {
            checkBoxObjectDetection_Enable.Checked = activationState;
            checkBoxColorDetectDrawCircleOnIntersection.Checked= markDetectedPoint;
            // make screen updates with the changes
            SyncAllLineDetectOptionsToReality();
            // tell the transform about the changes
            SetObjectRecognitionValues();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects the target point, will return Point.Empty for fail
        /// 
        /// NOTE: this also implements IBehaviour_DetectTargetViaColor
        /// 
        /// </summary>
        /// <param name="colorWithAlphaChannel">the color of the target point with full alpha channel</param>
        /// <param name="startPoint">the point from which we start the search</param>
        /// <returns>The nearest target point or Point.Empty for fail</returns>
        public Point DetectTargetPointViaColor(Point startPoint, Color colorWithAlphaChannel)
        {
            if (startPoint == null) return new Point();
            if (startPoint.IsEmpty == true) return new Point();
            // we have a source point use it to detect the target
            Point workingTargetPoint = (imageOverlayTransform as MFTOverlayImage_GS).GetNearestColorPointFromOrigin(startPoint, colorWithAlphaChannel, PATH_FOLLOW_MIN_POINTS_NEEDED);
            return workingTargetPoint;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the clear red lines every frame value
        /// </summary>
        private bool ClearRedLinesEveryFrame
        {
            get
            {
                return checkBoxColorDetectClearRedEveryFrame.Checked;
            }
            set
            {
                checkBoxColorDetectClearRedEveryFrame.Checked = value;
            }
        }
        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+= STEP CODE =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region StepCode

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the run button of the Stepper Control panel 
        /// </summary>
        private void buttonStepperControlRun_Click(object sender, EventArgs e)
        {
            LogMessage("buttonStepperControlRun_Click");

            if (dataTransporter == null)
            {
                LogMessage("buttonStepperControlRun_Click, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("buttonStepperControlRun_Click, Not connected");
                return;
            }

            //  get the data off the screen
            SCM_StepperRun scmMessage = GetStepperControlDataFromScreen(GetActiveStepperControlID(), true, false, 1);
            if (scmMessage == null)
            {
                LogMessage("buttonStepperControlRun_Click, scmMessage == null");
                return;
            }

            // display it
            AppendDataToConnectionTrace("OUT: " + scmMessage.GetState());
            // send it
            dataTransporter.SendDataMessage(scmMessage);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the run stop button of the Stepper Control panel 
        /// </summary>
        private void buttonStepperControlRunStop_Click(object sender, EventArgs e)
        {
            LogMessage("buttonStepperControlRunStop_Click");

            if (dataTransporter == null)
            {
                LogMessage("buttonStepperControlRunStop_Click, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("buttonStepperControlRunStop_Click, Not connected");
                return;
            }

            //  get the data off the screen
            SCM_StepperRun scmMessage = GetStepperControlDataFromScreen(GetActiveStepperControlID(), false, false, 1);
            if (scmMessage == null)
            {
                LogMessage("buttonStepperControlRunStop_Click, scmMessage == null");
                return;
            }

            // display it
            AppendDataToConnectionTrace("OUT: " + scmMessage.GetState());
            // send it
            dataTransporter.SendDataMessage(scmMessage);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the nudge 1 button of the step panel 
        /// </summary>
        private void buttonStepperControlNudge1_Click(object sender, EventArgs e)
        {
            LogMessage("buttonStepperControlNudge1_Click");

            if (dataTransporter == null)
            {
                LogMessage("buttonStepperControlNudge1_Click, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("buttonStepperControlNudge1_Click, Not connected");
                return;
            }

            // we are hard coded to 1 step here
            SCM_StepperRun scmMessage = GetStepperControlDataFromScreen(GetActiveStepperControlID(), true, true, 1);
            if (scmMessage == null)
            {
                LogMessage("buttonStepperControlNudge1_Click, scmMessage == null");
                return;
            }

            // display it
            AppendDataToConnectionTrace("OUT: " + scmMessage.GetState());
            // send it
            dataTransporter.SendDataMessage(scmMessage);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the stepper control data from the screen and returns the populated
        /// manual run message container, requires the ID to be fed in rather than 
        /// pulling it off the screen
        /// </summary>
        /// <param name="stepperID">the stepper ID we operate on</param>
        /// <param name="stepperEnable">if true we enable the stepper</param>
        /// <param name="wantNumStepsOverride">nz, override the number of steps with numSteps</param>
        /// <param name="numSteps">the number of steps if overriding</param>
        /// <returns>a populated ServerClientData container</returns>
        private SCM_StepperRun GetStepperControlDataFromScreen(StepperIDEnum stepperID, bool stepperEnable, bool wantNumStepsOverride, uint numSteps)
        {
            LogMessage("GetStepperControlDataFromScreen");

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
            SCData_Stepper scData = new SCData_Stepper();
            // create the message container
            SCM_StepperRun scmMessage = new SCM_StepperRun();
            // give the stepper data to the message container
            scmMessage.StepperControlList.Add(scData);

            // tell it which stepper we are operating on
            scData.Stepper_ID = stepperID;

            // set the speed
            try
            {
                scData.Stepper_StepSpeed = Convert.ToUInt32(textBoxStepperControlStepsPerSecond.Text);
            }
            catch (Exception ex)
            {
                OISMessageBox("Error converting step speed: " + ex.Message);
                return null;
            }

            string tmpNumSteps = "0";
            // get the number of steps. Do we have an override
            if (wantNumStepsOverride == false)
            {
                // no, we do not, set it this way
                tmpNumSteps = textBoxStepperControlNumSteps.Text;
            }
            else tmpNumSteps = numSteps.ToString();
            // now do the the conversion on the true value the user wants
            try
            {
                scData.NumSteps = Convert.ToUInt32(tmpNumSteps);
            }
            catch (Exception ex)
            {
                OISMessageBox("Error converting numSteps: " + ex.Message);
                return null;
            }

            // set the direction
            if (radioButtonStepperControlDirCW.Checked == true) scData.Stepper_DirState = 1;
            else scData.Stepper_DirState = 0;

            // enable the stepper
            if (stepperEnable == true) scData.Stepper_Enable = 1;
            else scData.Stepper_Enable = 0;

            // always turn the waldos state correctly
            scmMessage.WaldosEnabled = WaldosEnabledStateAsUINT();
            // hand it back to the caller
            return scmMessage;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get the currently active Stepper manual control id
        /// </summary>
        private StepperIDEnum GetActiveStepperControlID()
        {
            if (radioButtonStepperControlStepper0.Checked == true) return StepperIDEnum.STEPPER_0;
            if (radioButtonStepperControlStepper1.Checked == true) return StepperIDEnum.STEPPER_1;
            if (radioButtonStepperControlStepper2.Checked == true) return StepperIDEnum.STEPPER_2;
            if (radioButtonStepperControlStepper3.Checked == true) return StepperIDEnum.STEPPER_3;
            return StepperIDEnum.STEPPER_None;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the enabled state of the WASD stepper control
        /// </summary>
        private bool WASDStepperControlEnabled
        {
            get
            {
                return checkBoxStepCtrlWASDEnabled.Checked;
            }
            set
            {
                checkBoxStepCtrlWASDEnabled.Checked = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the speed in steps/second of the WASD control
        /// </summary>
        /// <returns>the speed in steps/sec or <=0 for fail</returns>
        private int WASDSpeedX
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxStepCtrlSpeed_X.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxStepCtrlSpeed_X.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the speed in steps/second of the WASD control
        /// </summary>
        /// <returns>the speed in steps/sec or <=0 for fail</returns>
        private int WASDSpeedY
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxStepCtrlSpeed_Y.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxStepCtrlSpeed_Y.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the speed in steps/second of the WASD control
        /// </summary>
        /// <returns>the speed in steps/sec or <=0 for fail</returns>
        private int WASDSpeedZ
        {
            get
            {
                try
                {
                    return Convert.ToInt32(textBoxStepCtrlSpeed_Z.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBoxStepCtrlSpeed_Z.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detect a checked changed on the WASD stepper control
        /// </summary>
        private void checkBoxStepCtrlWASDEnabled_CheckedChanged(object sender, EventArgs e)
        {
            SendAllStepperMotorStop();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends an all stepper motor stop command. Not this is different than a
        /// EnableWaldos=false because it only affects stepper motors not other 
        /// operating things like PWM's
        /// </summary>
        private void SendAllStepperMotorStop()
        {
            LogMessage("SendAllStepperMotorStop");

            if (dataTransporter == null)
            {
                LogMessage("SendAllStepperMotorStop, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("SendAllStepperMotorStop, Not connected");
                return;
            }


            // create the message container
            SCM_StepperRun scmMessage = new SCM_StepperRun();

            // always turn the waldos state correctly
            scmMessage.WaldosEnabled = WaldosEnabledStateAsUINT();

            // create a stepper control container for the X axis
            SCData_Stepper stepperControlX = new SCData_Stepper();
            // tell it which stepper we are operating on
            stepperControlX.Stepper_ID = StepperIDEnum.STEPPER_0;
            // set the speed, steps, dir etc
            stepperControlX.Stepper_StepSpeed = 0;
            stepperControlX.NumSteps = 0;
            stepperControlX.Stepper_DirState = 0;
            stepperControlX.Stepper_Enable = 0;
            // add the X cmd to the list
            scmMessage.StepperControlList.Add(stepperControlX);

            // create a stepper control container for the Y axis
            SCData_Stepper stepperControlY = new SCData_Stepper();
            // tell it which stepper we are operating on
            stepperControlY.Stepper_ID = StepperIDEnum.STEPPER_1;
            // set the speed, steps, dir etc
            stepperControlY.Stepper_StepSpeed = 0;
            stepperControlY.NumSteps = 0;
            stepperControlY.Stepper_DirState = 0;
            stepperControlY.Stepper_Enable = 0;
            // add the Y cmd to the list
            scmMessage.StepperControlList.Add(stepperControlY);

            // create a stepper control container for the Z axis
            SCData_Stepper stepperControlZ = new SCData_Stepper();
            // tell it which stepper we are operating on
            stepperControlZ.Stepper_ID = StepperIDEnum.STEPPER_0;
            // set the speed, steps, dir etc
            stepperControlZ.Stepper_StepSpeed = 0;
            stepperControlZ.NumSteps = 0;
            stepperControlZ.Stepper_DirState = 0;
            stepperControlZ.Stepper_Enable = 0;
            // add the Z cmd to the list
            scmMessage.StepperControlList.Add(stepperControlZ);

            // display it
            AppendDataToConnectionTrace("OUT: " + scmMessage.GetState());
            // send it
            dataTransporter.SendDataMessage(scmMessage);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends a motor start command for a specified motor
        /// </summary>
        /// <param name="stepSpeedHz">the stepper speed in Hz</param>
        /// <param name="stepDir">step direction 0 or 1</param>
        /// <param name="stepperID">the stepper ID we operate on</param>
        private void SendStepperMotorStart(StepperIDEnum stepperID, uint stepSpeedHz, uint stepDir)
        {
            LogMessage("SendStepperMotorStart " + stepperID.ToString());

            if (dataTransporter == null)
            {
                LogMessage("SendStepperMotorStart, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("SendStepperMotorStart, Not connected");
                return;
            }

            // create the data container
            SCData_Stepper scData = new SCData_Stepper();
            // create the message container
            SCM_StepperRun scmMessage = new SCM_StepperRun();
            // give the stepper data to the message container
            scmMessage.StepperControlList.Add(scData);

            // always turn the waldos state correctly
            scmMessage.WaldosEnabled = WaldosEnabledStateAsUINT();

            // tell it which stepper we are operating on
            scData.Stepper_ID = stepperID;
            // set the speed, steps, dir etc
            scData.Stepper_StepSpeed = stepSpeedHz;
            scData.NumSteps = SCData_Stepper.INFINITE_STEPS;
            scData.Stepper_DirState = stepDir;
            scData.Stepper_Enable = 1;

            // display it
            AppendDataToConnectionTrace("OUT: " + scmMessage.GetState());
            // send it
            dataTransporter.SendDataMessage(scmMessage);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends a motor stop command for a specified motor
        /// </summary>
        /// <param name="stepperID">the stepper ID we operate on</param>
        private void SendStepperMotorStop(StepperIDEnum stepperID)
        {
            LogMessage("SendStepperMotorStop " + stepperID.ToString());

            if (dataTransporter == null)
            {
                LogMessage("SendStepperMotorStop, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("SendStepperMotorStop, Not connected");
                return;
            }

            // create the data container
            SCData_Stepper scData = new SCData_Stepper();
            // create the message container
            SCM_StepperRun scmMessage = new SCM_StepperRun();
            // give the stepper data to the message container
            scmMessage.StepperControlList.Add(scData);

            // always turn the waldos state correctly
            scmMessage.WaldosEnabled = WaldosEnabledStateAsUINT();

            // tell it which stepper we are operating on
            scData.Stepper_ID = stepperID;
            // set the speed, steps, dir etc
            scData.Stepper_StepSpeed = 0;
            scData.NumSteps = 0;
            scData.Stepper_DirState = 0;
            scData.Stepper_Enable = 0;

            // display it
            AppendDataToConnectionTrace("OUT: " + scmMessage.GetState());
            // send it
            dataTransporter.SendDataMessage(scmMessage);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects a key down (including repeats) and sends a motor start command 
        /// for a specified motor on the appropriate axis
        /// </summary>
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            // WASD must be enabled. Any change in the enable/disable state turns off
            // alls stepper motors
            if (WASDStepperControlEnabled == true)
            {
                // X axis
                if (e.KeyCode == Keys.A) SendStepperMotorStart(StepperIDEnum.STEPPER_0, (uint)WASDSpeedX, Motor0GlobalNegativeDir);
                if (e.KeyCode == Keys.D) SendStepperMotorStart(StepperIDEnum.STEPPER_0, (uint)WASDSpeedX, Motor0GlobalPositiveDir);
                // Y axis
                if (e.KeyCode == Keys.W) SendStepperMotorStart(StepperIDEnum.STEPPER_1, (uint)WASDSpeedY, Motor1GlobalNegativeDir);
                if (e.KeyCode == Keys.S) SendStepperMotorStart(StepperIDEnum.STEPPER_1, (uint)WASDSpeedY, Motor1GlobalPositiveDir);
                // Z axis
                if (e.KeyCode == Keys.Q) SendStepperMotorStart(StepperIDEnum.STEPPER_2, (uint)WASDSpeedZ, Motor2GlobalNegativeDir);
                if (e.KeyCode == Keys.E) SendStepperMotorStart(StepperIDEnum.STEPPER_2, (uint)WASDSpeedZ, Motor2GlobalPositiveDir);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects a key up and sends a motor stop command for a specified motor
        /// on the appropriate axis
        /// </summary>
        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            // WASD must be enabled. Any change in the enable/disable state turns off
            // alls stepper motors
            if (WASDStepperControlEnabled == true)
            {
                // X axis
                if (e.KeyCode == Keys.A) SendStepperMotorStop(StepperIDEnum.STEPPER_0);
                if (e.KeyCode == Keys.D) SendStepperMotorStop(StepperIDEnum.STEPPER_0);
                // Y axis
                if (e.KeyCode == Keys.W) SendStepperMotorStop(StepperIDEnum.STEPPER_1);
                if (e.KeyCode == Keys.S) SendStepperMotorStop(StepperIDEnum.STEPPER_1);
                // Z axis
                if (e.KeyCode == Keys.Q) SendStepperMotorStop(StepperIDEnum.STEPPER_2);
                if (e.KeyCode == Keys.E) SendStepperMotorStop(StepperIDEnum.STEPPER_2);
            }
        }
        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+ OVERLAY LOADSAVE     =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region OverlayLoadSave

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Returns the overlay path and filename for a specific storage slot. 
        /// Does not check the validity of the path. Will return empty for fail
        /// </summary>
        public string OverlayPathAndFileName(int slotNum)
        {
            if (slotNum < 0) return "";
            if (slotNum > MAX_OVERLAY_IMAGE_SLOT) return "";
            return Path.Combine(DEFAULT_OVERLAY_IMAGE_PATH, DEFAULT_OVERLAY_IMAGE_FILENAME).Replace(OVERLAY_EX_SLOT_REPVAL, EXPERIMENT_NUMBER).Replace(OVERLAY_IMAGE_SLOT_REPVAL, slotNum.ToString("X2"));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the overlay save and load slots. These have hardcoded paths and names
        /// </summary>
        private void SetOverlaySaveAndLoadSlots()
        {
            textBoxOverlayFileName_Slot00.Text = OverlayPathAndFileName(0);
            textBoxOverlayFileName_Slot01.Text = OverlayPathAndFileName(1);
            textBoxOverlayFileName_Slot02.Text = OverlayPathAndFileName(2);
            textBoxOverlayFileName_Slot03.Text = OverlayPathAndFileName(3);
            textBoxOverlayFileName_Slot04.Text = OverlayPathAndFileName(4);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Loads the overlay from a slot file. Much of this such as the path
        /// are hard coded
        /// 
        /// Note this is also a IBehaviour_LoadOverlayImageBySlot implementation
        /// </summary>
        /// <param name="slotNum">the slot number, forms part of the filename</param>
        public void LoadOverlayImageFromSlot(int slotNum)
        {
            // sanity checks
            if (ImageOverlayTransform == null) throw new Exception("Overlay transform is null");
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) throw new Exception("Overlay transform is not MFTOverlayImage_GS");
            if ((slotNum < 0) || (slotNum > MAX_OVERLAY_IMAGE_SLOT)) throw new Exception("Overlay transform is null");

            string overlayImagePathAndFilename = OverlayPathAndFileName(slotNum);
            // some sanity checks. We do not check if it exists. We always overwrite
            if (overlayImagePathAndFilename.Length < 10) throw new Exception("Overlay path and name is too short");
            if (Path.IsPathRooted(overlayImagePathAndFilename) == false) throw new Exception("Overlay path is not rooted");
            // load the image
            (ImageOverlayTransform as MFTOverlayImage_Base).LoadOverlayImageAsBinary(overlayImagePathAndFilename);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Saves the overlay as an PNG to a slot file. Much of this such as the path
        /// are hard coded
        /// </summary>
        /// <param name="slotNum">the slot number, forms part of the filename</param>
        private void SaveOverlayImageToSlot(int slotNum)
        {
            // sanity checks
            if (ImageOverlayTransform == null) throw new Exception("Overlay transform is null");
            if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) throw new Exception("Overlay transform is not MFTOverlayImage_GS");
            if ((slotNum < 0) || (slotNum > MAX_OVERLAY_IMAGE_SLOT)) throw new Exception("Overlay transform is null");

            string overlayImagePathAndFilename = OverlayPathAndFileName(slotNum);
            // some sanity checks. We do not check if it exists. We always overwrite
            if (overlayImagePathAndFilename.Length < 10) throw new Exception("Overlay path and name is too short");
            if (Path.IsPathRooted(overlayImagePathAndFilename) == false) throw new Exception("Overlay path is not rooted");
            // delete the file if it exists, the save does not like to overwrite
            if (File.Exists(overlayImagePathAndFilename) == true) File.Delete(overlayImagePathAndFilename);
            // save the image
            (ImageOverlayTransform as MFTOverlayImage_Base).SaveOverlayImageAsBinary(overlayImagePathAndFilename);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a save overlay into slot button
        /// </summary>
        private void buttonSaveCurrentOverlay_Slot00_Click(object sender, EventArgs e)
        {
            SaveOverlayImageToSlot(0);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a save overlay into slot button
        /// </summary>
        private void buttonSaveCurrentOverlay_Slot01_Click(object sender, EventArgs e)
        {
            SaveOverlayImageToSlot(1);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a save overlay into slot button
        /// </summary>
        private void buttonSaveCurrentOverlay_Slot02_Click(object sender, EventArgs e)
        {
            SaveOverlayImageToSlot(2);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a save overlay into slot button
        /// </summary>
        private void buttonSaveCurrentOverlay_Slot03_Click(object sender, EventArgs e)
        {
            SaveOverlayImageToSlot(3);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a save overlay into slot button
        /// </summary>
        private void buttonSaveCurrentOverlay_Slot04_Click(object sender, EventArgs e)
        {
            SaveOverlayImageToSlot(4);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a load overlay from slot button
        /// </summary>
        private void buttonLoadFileIntoOverlay_Slot00_Click(object sender, EventArgs e)
        {
            LoadOverlayImageFromSlot(0);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a load overlay from slot button
        /// </summary>
        private void buttonLoadFileIntoOverlay_Slot01_Click(object sender, EventArgs e)
        {
            LoadOverlayImageFromSlot(1);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a load overlay from slot button
        /// </summary>
        private void buttonLoadFileIntoOverlay_Slot02_Click(object sender, EventArgs e)
        {
            LoadOverlayImageFromSlot(2);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a load overlay from slot button
        /// </summary>
        private void buttonLoadFileIntoOverlay_Slot03_Click(object sender, EventArgs e)
        {
            LoadOverlayImageFromSlot(3);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on a load overlay from slot button
        /// </summary>
        private void buttonLoadFileIntoOverlay_Slot04_Click(object sender, EventArgs e)
        {
            LoadOverlayImageFromSlot(4);
        }
        #endregion

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        #region BehaviourStackCode

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a click on the checkBoxEx012StackIsActive
        /// </summary>
        private void checkBoxEx012StackIsActive_CheckedChanged(object sender, EventArgs e)
        {
            LogMessage("checkBoxEx012StackIsActive_CheckedChanged called");

            SetEx012StackIsActiveCheckBoxAccordingToState();
            // now create or stop our behaviour stack
            if (checkBoxEx012StackIsActive.Checked == true)
            {
                //Behaviour_StateMachine workingBehaviourStack = CreateBehaviourStackEx012Shot02();
                //Behaviour_StateMachine workingBehaviourStack = CreateBehaviourStackEx012Shot06();
                Behaviour_StateMachine workingBehaviourStack = CreateBehaviourStackEx012Shot10();

                if (workingBehaviourStack == null)
                {
                    LogMessage("checkBoxEx012StackIsActive_CheckedChanged workingBehaviourStack == null");
                    return;
                }

                // send the newly built BehaviourStack to the client
                CreateBehaviourStackOnClient(workingBehaviourStack);

                // lock the global behaviour stack
                lock (globalBehaviourStackLockObj)
                {
                    // set it now as the global behaviour stack
                    globalBehaviourStack = workingBehaviourStack;

                    // enable the stateMachine
                    globalBehaviourStack.WorkerThreadsOKToRun = true;
                    // set our current location on each behaviour in the stack
                    globalBehaviourStack.SetCurrentLocation(BehaviourLocationEnum.WALNUT_SERVER);
                    // run all of the startup actions. This includes the BehaviourStack actor itself
                    globalBehaviourStack.RunStartupActions();
                    // start all behaviour Threads. This includes the BehaviourStack actor itself
                    globalBehaviourStack.StartBehaviourThreads();
                }

            }
            else
            {
                // stack not active
                DropBehaviourStack();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// An interface to insure that an object can update the screen with
        /// some pin states
        /// 
        /// Note this is the IBehaviour_UpdateScreenWithPinStates implementation
        /// 
        /// Note: we are not necessarily on the form thread so we Invoke if needed
        /// 
        /// </summary>
        /// <param name="pin1State">state of pin 1</param>
        /// <param name="pin2State">state of pin 2</param>
        public void UpdateScreenWithPinStates(bool pin1State, bool pin2State)
        {
            // So, we use the InvokeRequired...Invoke sequence to get us back on the form thread
            if (InvokeRequired == true)
            {
                // call ourselves again but this time be on the form thread.
                Invoke(new MethodInvoker(() => { UpdateScreenWithPinStates(pin1State, pin2State); })); 
                return;
            }

            // this is tool head left sensor
            radioButtonToolHeadLeft.Checked = pin1State;
            // this is tool head right sensor
            radioButtonToolHeadRight.Checked = pin2State;

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Code to transmit output pin states to the client
        /// 
        /// Note this is the IBehaviour_ProcessOutputGPIOControlList implementation
        /// 
        /// </summary>
        /// <param name="gpioOutputList">the list of gpio output containers</param>
        public void ProcessOutputGPIOControlList(List<SCData_PinOutputConfig> gpioOutputList)
        {
            if (gpioOutputList == null)
            {
                LogMessage("ProcessOutputGPIOControlList, dataTransporter == null");
                return;
            }

            if (dataTransporter == null)
            {
                LogMessage("ProcessOutputGPIOControlList, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("ProcessOutputGPIOControlList, Not connected");
                return;
            }

            // create the container
            SCM_PinStateList_Output scmMessage = new SCM_PinStateList_Output();
            // set the list
            scmMessage.PinStateList = gpioOutputList;

            // display it
            AppendDataToConnectionTrace("OUT: " + scmMessage.GetState());
            // send it
            dataTransporter.SendDataMessage(scmMessage);

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the visual appearance of the Show Overlays checkbox according to 
        /// the state
        /// </summary>
        private void SetEx012StackIsActiveCheckBoxAccordingToState()
        {
            if (checkBoxEx012StackIsActive.Checked == true)
            {
                checkBoxEx012StackIsActive.BackColor = Color.IndianRed;
                checkBoxEx012StackIsActive.Text = BEHAVIOUR_STACK_IS_ACTIVE;
            }
            else
            {
                checkBoxEx012StackIsActive.BackColor = Color.LightGreen;
                checkBoxEx012StackIsActive.Text = BEHAVIOUR_STACK_NOT_ACTIVE;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Does everything to drop and remove the current behaviour stack
        /// 
        /// If we do not have one that is ok.
        /// </summary>
        private void DropBehaviourStack()
        {
            LogMessage("DropBehaviourStack called");

            // also drop the stack on the client
            DropBehaviourStackOnClient();

            // lock the global behaviour stack
            lock (globalBehaviourStackLockObj)
            {
                // do we have a global Behaviour Stack? If so, kill it off
                if (globalBehaviourStack != null)
                {
                    LogMessage("DropBehaviourStack, dropping existing stack");
                    // yes we do drop it
                    globalBehaviourStack.WorkerThreadsOKToRun = false;
                    globalBehaviourStack.StopAllBehaviours();
                    globalBehaviourStack = null;
                }
            }
            LogMessage("DropBehaviourStack ends");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Creates a behaviour stack. 
        /// 
        /// Note the behaviour stack will not have been started
        /// </summary>
        /// <returns> a new behaviour state machine or null for fail</returns>
        private Behaviour_StateMachine CreateBehaviourStackEx012Shot10()
        {
            LogMessage("CreateBehaviourStack called");

            const byte ALPHA_FOR_REDLED_ACTIVATE = 254; // hard coded at the moment

            // create a statemachine to hold our behaviours. This, in itself, is a behaviour too
            // the operating location is WALNUT_BOTH because a copy will be instantiated over on the client as well
            BehaviourStack_Ex012 workingBehaviourStack = new BehaviourStack_Ex012(BehaviourLocationEnum.WALNUT_BOTH);

            // set the main object
            workingBehaviourStack.MainObject = this;

            //// build and add the DataClasses for the behaviour stack.
            //// NOTE: the add order matters here. The later ones supersede the earlier ones
            ////       so put the basic behaviours last. The behaviours interact with each 
            ////       other by setting values the global data class. They never interact 
            ////       with each other directly

            // add a behaviour to ensure the global data knows the correct screen sizes. This is the size of the capture display
            // from the USB microscope. This is not the windows display. Note the Chryron height affects the minimum Y value
            Behaviour_ScreenSize behaviourScreenSize = new Behaviour_ScreenSize(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourScreenSize);
            // now dynamically populate the screensize behaviour
            behaviourScreenSize.MinScreenX = 0;
            behaviourScreenSize.MinScreenY = BottomOfScreenSkipHeight();
            behaviourScreenSize.MaxScreenX = ScreenWidthMaxCoord();
            behaviourScreenSize.MaxScreenY = ScreenHeightMaxCoord();

            // add a behaviour to automatically load a previously saved overlay image from a slot this is just 
            // a timesaver so we do not have to do the manual setup prep for each test run.
            Behaviour_LoadOverlayImageBySlot behaviourLoadOverlayImageBySlot = new Behaviour_LoadOverlayImageBySlot(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourLoadOverlayImageBySlot);
            // now dynamically populate the screensize behaviour
            behaviourLoadOverlayImageBySlot.OverlayImageSlot = 3;   // using this one for now

            // add a behaviour to automatically start recording this is just  a timesaver so we
            // do not have to do the manual setup prep for each test run.
            Behaviour_RecordingOnOff behaviourRecordingOnOff = new Behaviour_RecordingOnOff(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourRecordingOnOff);
            // now dynamically populate our runtime values
            behaviourRecordingOnOff.WorkingShotDescriptor = "Shot10"; // this stack is specifically configured to get this shot
            behaviourRecordingOnOff.WantRecordingOnOffAction = true;   // we want recording

            // add a behaviour to start the detection mechansim. This runs only on the server. It does not pull the detected points
            // into the global data it just kicks off the detection mechanism once at the start and then does nothing
            Behaviour_DetectionActivate behaviourDetectionActivate = new Behaviour_DetectionActivate(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourDetectionActivate);
            // now set values specific for this run
            behaviourDetectionActivate.WantMarkDetectedPoint = true;

            // add a behaviour to define the GPIO outputs. This runs only on the server. It does not send anything to the 
            // client it just sets up the output GPIOs for future use
            Behaviour_IO_OutputStateSetup behaviourOutputStatesSetup = new Behaviour_IO_OutputStateSetup(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourOutputStatesSetup);
            // now set values specific for this run
            behaviourOutputStatesSetup.AddOutputGPIO(EX012LED1GPIO);

            // add the base motor speed variables. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            Behaviour_MotorSpeeds behaviourSetMotorSpeeds = new Behaviour_MotorSpeeds(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSetMotorSpeeds);
            // now set values specific for this run
            behaviourSetMotorSpeeds.WorkingMaxSpeed_X = 20;
            behaviourSetMotorSpeeds.WorkingMaxSpeed_Y = 15;

            // add the source point pixel color variable. This is the color of the pixel on the screen under the source point rather than
            // the color of whatever marker is on top of it on the overlay or tracker
            Behaviour_SourcePointPixelColor_Screen behaviourSourcePointPixelColorScreen = new Behaviour_SourcePointPixelColor_Screen(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointPixelColorScreen);

            // add the source point pixel color variable. This is the color of the pixel on the overlay under the source point rather than
            // the color of whatever marker is on top of it on the screen or tracker
            Behaviour_SourcePointPixelColor_Overlay behaviourSourcePointPixelColorOverlay = new Behaviour_SourcePointPixelColor_Overlay(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointPixelColorOverlay);

            // add the source point color variable. This behaviour can change the color we use to mark the source point
            // note this goes higher than Behaviour_SourcePoint - we want the color set before we figure out the source point 
            Behaviour_SourcePointColor behaviourSourcePointColor = new Behaviour_SourcePointColor(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointColor);

            // add the source point variable. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            // note this goes higher than Behaviour_TargetPoint - we need the SourcePoint to figure out the best target
            Behaviour_SourcePoint behaviourSourcePoint = new Behaviour_SourcePoint(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePoint);

            // add the target point color variable. This behaviour can change the color we use for the target point
            // note this goes higher than Behaviour_TargetPoint - we want the color set before we figure out the target point 
            Behaviour_TargetPointColor behaviourTargetPointColor = new Behaviour_TargetPointColor(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPointColor);

            // add the target point variable. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            Behaviour_TargetPoint behaviourTargetPoint = new Behaviour_TargetPoint(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPoint);

            // add the detect target point. This behaviour is an action to figure out the best target point. We use both the 
            // SourcePoint and the TargetPointColor so it is best to put it after those behaviours do their thing
            Behaviour_TargetPointDetector behaviourDetectTargetPoint = new Behaviour_TargetPointDetector(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourDetectTargetPoint);

            // add the removal of the target pixels at the source point
            Behaviour_RemoveTargetPixelsAtSourcePoint behaviourRemoveTargetPixelsAtSourcePoint = new Behaviour_RemoveTargetPixelsAtSourcePoint(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourRemoveTargetPixelsAtSourcePoint);
            // now set values specific for this run
            behaviourRemoveTargetPixelsAtSourcePoint.WantTransparent = false;
            behaviourRemoveTargetPixelsAtSourcePoint.ErasureRectWidth = 10;
            behaviourRemoveTargetPixelsAtSourcePoint.ErasureRectHeight = 20;

            // add the detect lowest alpha value we removed. This behaviour is an action to acquire the lowest of the alpha values we just removed while
            // clearing the target pixels on the overlay
            Behaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay behaviourSourcePointLowestAlphaValueFoundOnMaskOverlay = new Behaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointLowestAlphaValueFoundOnMaskOverlay);

            // add the color decider. This is the code the decides what color we use to detect the target point and what color we lay down
            // as we consume the target pixels by moving over them
            Behaviour_TargetPointColorDecider behaviourTargetPointColorDecider = new Behaviour_TargetPointColorDecider(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPointColorDecider);

            // add the move source to target. This behaviour figures out the motor control states to move the source to the 
            // target. Does not actually do the move
            Behaviour_MoveSourceToTarget behaviourMoveSourceToTarget = new Behaviour_MoveSourceToTarget(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourMoveSourceToTarget);

            // add the behaviour to trigger the activation of an GPIO output when we see a certain bit in the alpha channel.
            Behaviour_IO_OutputState_OnAlpha behaviourSetIOOutputState_OnAlpha = new Behaviour_IO_OutputState_OnAlpha(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSetIOOutputState_OnAlpha);
            // now set values specific for this run
            behaviourSetIOOutputState_OnAlpha.TriggerValueForIOHigh = ALPHA_FOR_REDLED_ACTIVATE;
            // this is the gpio that gets set high if the alpha indicates to do so
            behaviourSetIOOutputState_OnAlpha.Gpio = EX012LED1GPIO;

            // add the behaviour to trigger the activation of an GPIO output when we see a certain bit in the alpha channel.
            // this will inhibit the EX012LED1GPIO on a trigger and so must come after any code that enables it
            Behaviour_ChangeSeekColor_OnAlpha behaviourChangeSeekColorOn_OnAlpha = new Behaviour_ChangeSeekColor_OnAlpha(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourChangeSeekColorOn_OnAlpha);
            // now set values specific for this run
            behaviourChangeSeekColorOn_OnAlpha.TriggerValue = ALPHA_FOR_REDLED_ACTIVATE;
            // this is the gpio that gets set high if the alpha indicates to do so
            behaviourChangeSeekColorOn_OnAlpha.GpioToInhibit = EX012LED1GPIO;
            // this is the color we have to be seeking on in order to trigger
            behaviourChangeSeekColorOn_OnAlpha.TriggerSeekColor = TRUE_BLUE;
            // this is the replacement seek color if we are currently seeking
            behaviourChangeSeekColorOn_OnAlpha.NewSeekColor = TRUE_RED;

            // Add our monitor for the waldos enabled state. 
            Behaviour_WaldosEnabledState behaviourWaldosEnabledState = new Behaviour_WaldosEnabledState(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourWaldosEnabledState);

            // Add our action to process a gpio output list. This will also obey the waldos off flag. This uses the gpio output list 
            // in the globalDataStore
            Behaviour_ProcessOutputGPIOControlList behaviourProcessOutputGPIOList = new Behaviour_ProcessOutputGPIOControlList(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourProcessOutputGPIOList);

            // Add our action to process a stepper control list. This will also turn off all waldos if flagged. This uses the stepper control list 
            // created by the Behaviour_DetectTargetPoint
            Behaviour_ProcessStepperControlList behaviourProcessStepperControlList = new Behaviour_ProcessStepperControlList(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourProcessStepperControlList);

            // Add our action to turn off the waldos if we need to. 
            Behaviour_WaldosAllStop behaviourWaldosAllStop = new Behaviour_WaldosAllStop(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourWaldosAllStop);

            LogMessage("CreateBehaviourStack ends");
            return workingBehaviourStack;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Creates a behaviour stack. 
        /// 
        /// Note the behaviour stack will not have been started
        /// 
        /// This stack has reciprocating motion but initiates and activation of a 
        /// output GPIO upon running over an imbedded command. This also handles
        /// shot 07
        /// </summary>
        /// <returns> a new behaviour state machine or null for fail</returns>
        private Behaviour_StateMachine CreateBehaviourStackEx012Shot06()
        {
            LogMessage("CreateBehaviourStack called");

            // create a statemachine to hold our behaviours. This, in itself, is a behaviour too
            // the operating location is WALNUT_BOTH because a copy will be instantiated over on the client as well
            BehaviourStack_Ex012 workingBehaviourStack = new BehaviourStack_Ex012(BehaviourLocationEnum.WALNUT_BOTH);

            // set the main object
            workingBehaviourStack.MainObject = this;

            //// build and add the DataClasses for the behaviour stack.
            //// NOTE: the add order matters here. The later ones supersede the earlier ones
            ////       so put the basic behaviours last. The behaviours interact with each 
            ////       other by setting values the global data class. They never interact 
            ////       with each other directly

            // add a behaviour to ensure the global data knows the correct screen sizes. This is the size of the capture display
            // from the USB microscope. This is not the windows display. Note the Chryron height affects the minimum Y value
            Behaviour_ScreenSize behaviourScreenSize = new Behaviour_ScreenSize(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourScreenSize);
            // now dynamically populate the screensize behaviour
            behaviourScreenSize.MinScreenX = 0;
            behaviourScreenSize.MinScreenY = BottomOfScreenSkipHeight();
            behaviourScreenSize.MaxScreenX = ScreenWidthMaxCoord();
            behaviourScreenSize.MaxScreenY = ScreenHeightMaxCoord();

            // add a behaviour to automatically load a previously saved overlay image from a slot this is just 
            // a timesaver so we do not have to do the manual setup prep for each test run.
            Behaviour_LoadOverlayImageBySlot behaviourLoadOverlayImageBySlot = new Behaviour_LoadOverlayImageBySlot(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourLoadOverlayImageBySlot);
            // now dynamically populate the screensize behaviour
            behaviourLoadOverlayImageBySlot.OverlayImageSlot = 2;   // using this one for now

            // add a behaviour to automatically start recording this is just  a timesaver so we
            // do not have to do the manual setup prep for each test run.
            Behaviour_RecordingOnOff behaviourRecordingOnOff = new Behaviour_RecordingOnOff(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourRecordingOnOff);
            // now dynamically populate our runtime values
            behaviourRecordingOnOff.WorkingShotDescriptor = "Shot07"; // this stack is specifically configured to get this shot
            behaviourRecordingOnOff.WantRecordingOnOffAction = true;   // we want recording

            // add a behaviour to start the detection mechansim. This runs only on the server. It does not pull the detected points
            // into the global data it just kicks off the detection mechanism once at the start and then does nothing
            Behaviour_DetectionActivate behaviourDetectionActivate = new Behaviour_DetectionActivate(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourDetectionActivate);
            // now set values specific for this run
            behaviourDetectionActivate.WantMarkDetectedPoint = true;

            // add a behaviour to define the GPIO outputs. This runs only on the server. It does not send anything to the 
            // client it just sets up the output GPIOs for future use
            Behaviour_IO_OutputStateSetup behaviourOutputStatesSetup = new Behaviour_IO_OutputStateSetup(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourOutputStatesSetup);
            // now set values specific for this run
            behaviourOutputStatesSetup.AddOutputGPIO(EX012LED1GPIO);

            // add the base motor speed variables. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            Behaviour_MotorSpeeds behaviourSetMotorSpeeds = new Behaviour_MotorSpeeds(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSetMotorSpeeds);
            // now set values specific for this run
            behaviourSetMotorSpeeds.WorkingMaxSpeed_X = 20;
            behaviourSetMotorSpeeds.WorkingMaxSpeed_Y = 15;

            // add the source point pixel color variable. This is the color of the pixel on the screen under the source point rather than
            // the color of whatever marker is on top of it on the overlay or tracker
            Behaviour_SourcePointPixelColor_Screen behaviourSourcePointPixelColorScreen = new Behaviour_SourcePointPixelColor_Screen(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointPixelColorScreen);

            // add the source point pixel color variable. This is the color of the pixel on the overlay under the source point rather than
            // the color of whatever marker is on top of it on the screen or tracker
            Behaviour_SourcePointPixelColor_Overlay behaviourSourcePointPixelColorOverlay = new Behaviour_SourcePointPixelColor_Overlay(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointPixelColorOverlay);

            // add the source point color variable. This behaviour can change the color we use to mark the source point
            // note this goes higher than Behaviour_SourcePoint - we want the color set before we figure out the source point 
            Behaviour_SourcePointColor behaviourSourcePointColor = new Behaviour_SourcePointColor(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointColor);

            // add the source point variable. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            // note this goes higher than Behaviour_TargetPoint - we need the SourcePoint to figure out the best target
            Behaviour_SourcePoint behaviourSourcePoint = new Behaviour_SourcePoint(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePoint);

            // add the target point color variable. This behaviour can change the color we use for the target point
            // note this goes higher than Behaviour_TargetPoint - we want the color set before we figure out the target point 
            Behaviour_TargetPointColor behaviourTargetPointColor = new Behaviour_TargetPointColor(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPointColor);

            // add the target point variable. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            Behaviour_TargetPoint behaviourTargetPoint = new Behaviour_TargetPoint(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPoint);

            // add the detect target point. This behaviour is an action to figure out the best target point. We use both the 
            // SourcePoint and the TargetPointColor so it is best to put it after those behaviours do their thing
            Behaviour_TargetPointDetector behaviourDetectTargetPoint = new Behaviour_TargetPointDetector(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourDetectTargetPoint);

            // add the removal of the target pixels at the source point
            Behaviour_RemoveTargetPixelsAtSourcePoint behaviourRemoveTargetPixelsAtSourcePoint = new Behaviour_RemoveTargetPixelsAtSourcePoint(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourRemoveTargetPixelsAtSourcePoint);
            // now set values specific for this run
            behaviourRemoveTargetPixelsAtSourcePoint.WantTransparent = false;
            behaviourRemoveTargetPixelsAtSourcePoint.ErasureRectWidth = 10;
            behaviourRemoveTargetPixelsAtSourcePoint.ErasureRectHeight = 20;

            // add the detect lowest alpha value we removed. This behaviour is an action to acquire the lowest of the alpha values we just removed while
            // clearing the target pixels on the overlay
            Behaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay behaviourSourcePointLowestAlphaValueFoundOnMaskOverlay = new Behaviour_SourcePointLowestAlphaValueFoundOnMask_Overlay(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointLowestAlphaValueFoundOnMaskOverlay);

            // add the color decider. This is the code the decides what color we use to detect the target point and what color we lay down
            // as we consume the target pixels by moving over them
            Behaviour_TargetPointColorDecider behaviourTargetPointColorDecider = new Behaviour_TargetPointColorDecider(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPointColorDecider);

            // add the move source to target. This behaviour figures out the motor control states to move the source to the 
            // target. Does not actually do the move
            Behaviour_MoveSourceToTarget behaviourMoveSourceToTarget = new Behaviour_MoveSourceToTarget(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourMoveSourceToTarget);

            // add the behaviour to trigger the activation of an GPIO output when we see a certain bit in the alpha channel.
            Behaviour_IO_OutputState_OnAlpha behaviourSetIOOutputState_OnAlpha = new Behaviour_IO_OutputState_OnAlpha(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSetIOOutputState_OnAlpha);
            // now set values specific for this run
            const byte ALPHA_FOR_REDLED_ACTIVATE = 254; // hard coded at the moment
            behaviourSetIOOutputState_OnAlpha.TriggerValueForIOHigh = ALPHA_FOR_REDLED_ACTIVATE;
            // this is the gpio that gets set high if the alpha indicates to do so
            behaviourSetIOOutputState_OnAlpha.Gpio = EX012LED1GPIO; 

            // Add our monitor for the waldos enabled state. 
            Behaviour_WaldosEnabledState behaviourWaldosEnabledState = new Behaviour_WaldosEnabledState(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourWaldosEnabledState);

            // Add our action to process a gpio output list. This will also obey the waldos off flag. This uses the gpio output list 
            // in the globalDataStore
            Behaviour_ProcessOutputGPIOControlList behaviourProcessOutputGPIOList = new Behaviour_ProcessOutputGPIOControlList(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourProcessOutputGPIOList);

            // Add our action to process a stepper control list. This will also turn off all waldos if flagged. This uses the stepper control list 
            // created by the Behaviour_DetectTargetPoint
            Behaviour_ProcessStepperControlList behaviourProcessStepperControlList = new Behaviour_ProcessStepperControlList(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourProcessStepperControlList);

            // Add our action to turn off the waldos if we need to. 
            Behaviour_WaldosAllStop behaviourWaldosAllStop = new Behaviour_WaldosAllStop(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourWaldosAllStop);

            LogMessage("CreateBehaviourStack ends");
            return workingBehaviourStack;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Creates a behaviour stack. 
        /// 
        /// This stack is intended to implement a simple follow of a virtual path 
        /// moving the red dot which indicates the end of the probe onto the nearest
        /// green pixel. Instead of rendering the green pixels transparent as it moves
        /// over them it renders them blue. A subsequent behaviour detects if no more
        /// green pixels can be found and sets things up to seek blue pixels. Thus we
        /// get reciprocating motion.
        /// </summary>
        /// <returns> a new behaviour state machine or null for fail</returns>
        private Behaviour_StateMachine CreateBehaviourStackEx012Shot02()
        {
            LogMessage("CreateBehaviourStack called");

            // #### HARD CODED AT THE MOMENT

            // create a statemachine to hold our behaviours. This, in itself, is a behaviour too
            // the operating location is WALNUT_BOTH because a copy will be instantiated over on the client as well
            BehaviourStack_Ex012 workingBehaviourStack = new BehaviourStack_Ex012(BehaviourLocationEnum.WALNUT_BOTH);

            // set the main object
            workingBehaviourStack.MainObject = this;

            //// build and add the DataClasses for the behaviour stack.
            //// NOTE: the add order matters here. The later ones supersede the earlier ones
            ////       so put the basic behaviours last. The behaviours interact with each 
            ////       other by setting values the global data class. They never interact 
            ////       with each other directly

            // add a behaviour to ensure the global data knows the correct screen sizes. This is the size of the capture display
            // from the USB microscope. This is not the windows display. Note the Chryron height affects the minimum Y value
            Behaviour_ScreenSize behaviourScreenSize = new Behaviour_ScreenSize(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourScreenSize);
            // now dynamically populate the screensize behaviour
            behaviourScreenSize.MinScreenX = 0;
            behaviourScreenSize.MinScreenY = BottomOfScreenSkipHeight();
            behaviourScreenSize.MaxScreenX = ScreenWidthMaxCoord();
            behaviourScreenSize.MaxScreenY = ScreenHeightMaxCoord();

            // add a behaviour to automatically load a previously saved overlay image from a slot. this is just 
            // a timesaver so we do not have to do the manual setup prep for each test run.
            Behaviour_LoadOverlayImageBySlot behaviourLoadOverlayImageBySlot = new Behaviour_LoadOverlayImageBySlot(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourLoadOverlayImageBySlot);
            // now dynamically populate the screensize behaviour
            behaviourLoadOverlayImageBySlot.OverlayImageSlot = 0;   // using this one for now

            // add a behaviour to automatically start recording. this is just  a timesaver so we
            // do not have to do the manual setup prep for each test run.
            Behaviour_RecordingOnOff behaviourRecordingOnOff = new Behaviour_RecordingOnOff(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourRecordingOnOff);
            // now dynamically populate our runtime values
            behaviourRecordingOnOff.WorkingShotDescriptor = "Shot03"; // this stack is specifically configured to get this shot
            behaviourRecordingOnOff.WantRecordingOnOffAction = true;   // we want recording

            // add a behaviour to start the detection mechansim. This runs only on the server. It does not pull the detected points
            // into the global data it just kicks off the detection mechanism once at the start and then does nothing
            Behaviour_DetectionActivate behaviourDetectionActivate = new Behaviour_DetectionActivate(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourDetectionActivate);
            // now set values specific for this run
            behaviourDetectionActivate.WantMarkDetectedPoint = true;

            // add the base motor speed variables. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            Behaviour_MotorSpeeds behaviourSetMotorSpeeds = new Behaviour_MotorSpeeds(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSetMotorSpeeds);
            // now set values specific for this run
            behaviourSetMotorSpeeds.WorkingMaxSpeed_X = 20;
            behaviourSetMotorSpeeds.WorkingMaxSpeed_Y = 15;

            // add the source point pixel color variable. This is the color of the pixel on the screen under the source point rather than
            // the color of whatever marker is on top of it on the overlay or tracker. This is reported back to us
            Behaviour_SourcePointPixelColor_Screen behaviourSourcePointPixelColor = new Behaviour_SourcePointPixelColor_Screen(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointPixelColor);

            // add the source point color variable. This behaviour can change the color we use for the source point
            // note this goes higher than Behaviour_SourcePoint - we want the color set before we figure out the source point 
            Behaviour_SourcePointColor behaviourSourcePointColor = new Behaviour_SourcePointColor(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePointColor);

            // add the source point variable. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            // note this goes higher than Behaviour_TargetPoint - we need the SourcePoint to figure out the best target
            Behaviour_SourcePoint behaviourSourcePoint = new Behaviour_SourcePoint(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourSourcePoint);

            // add the target point color variable. This behaviour can change the color we use for the target point
            // note this goes higher than Behaviour_TargetPoint - we want the color set before we figure out the target point 
            Behaviour_TargetPointColor behaviourTargetPointColor = new Behaviour_TargetPointColor(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPointColor);

            // add the target point variable. This behaviour does not take actions but it does force
            // the behaviour stack to define the variables - otherwise an exception will be thrown on 
            // construction.
            Behaviour_TargetPoint behaviourTargetPoint = new Behaviour_TargetPoint(BehaviourLocationEnum.WALNUT_BOTH, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPoint);

            // add the detect target point. This behaviour is an action to figure out the best target point. We use both the 
            // SourcePoint and the TargetPointColor so it is best to put it after those behaviours do their thing
            Behaviour_TargetPointDetector behaviourDetectTargetPoint = new Behaviour_TargetPointDetector(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourDetectTargetPoint);

            // add the removal of the target pixels at the source point
            Behaviour_RemoveTargetPixelsAtSourcePoint behaviourRemoveTargetPixelsAtSourcePoint = new Behaviour_RemoveTargetPixelsAtSourcePoint(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourRemoveTargetPixelsAtSourcePoint);
            // now set values specific for this run
            behaviourRemoveTargetPixelsAtSourcePoint.WantTransparent = false;
            behaviourRemoveTargetPixelsAtSourcePoint.ErasureRectWidth = 10;
            behaviourRemoveTargetPixelsAtSourcePoint.ErasureRectHeight = 20;

            // add the color decider. This is the code the decides what color we use to detect the target point and what color we lay down
            // as we consume the target pixels by moving over them
            Behaviour_TargetPointColorDecider behaviourTargetPointColorDecider = new Behaviour_TargetPointColorDecider(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourTargetPointColorDecider);

            // add the move source to target. This behaviour figures out the motor control states to move the source to the 
            // target. Does not actually do the move
            Behaviour_MoveSourceToTarget behaviourMoveSourceToTarget = new Behaviour_MoveSourceToTarget(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourMoveSourceToTarget);

            // Add our monitor for the waldos enabled state. 
            Behaviour_WaldosEnabledState behaviourWaldosEnabledState = new Behaviour_WaldosEnabledState(BehaviourLocationEnum.WALNUT_SERVER, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourWaldosEnabledState);

            // Add our action to process a stepper control list. This will also turn off all waldos if flagged. This uses the stepper control list 
            // created by the Behaviour_DetectTargetPoint
            Behaviour_ProcessStepperControlList behaviourProcessStepperControlList = new Behaviour_ProcessStepperControlList(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourProcessStepperControlList);

            // Add our action to turn off the waldos if we need to. 
            Behaviour_WaldosAllStop behaviourWaldosAllStop = new Behaviour_WaldosAllStop(BehaviourLocationEnum.WALNUT_CLIENT, workingBehaviourStack);
            workingBehaviourStack.BehaviourList.AddLast(behaviourWaldosAllStop);

            LogMessage("CreateBehaviourStack ends");
            return workingBehaviourStack;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Copies the data from a specified behaviour stack to the global behaviour
        /// stack.
        /// 
        /// Note the incoming behaviour stack should be shallow cloned and will have 
        /// an empty BehaviourList.
        /// </summary>
        /// <param name="shallowClonedBehaviourStack">the inbound statemaching behaviourStack</param>
        private void CopyDataToGlobalStack(Behaviour_StateMachine shallowClonedBehaviourStack)
        {
            // lock the global behaviour stack
            lock (globalBehaviourStackLockObj)
            {
                // haven't got one? do nothing
                if (globalBehaviourStack == null) return;
                // ok we have one, copy over the appropriate data while in the lock
                globalBehaviourStack.CopyServerClientData(shallowClonedBehaviourStack, BehaviourLocationEnum.WALNUT_SERVER);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends the BehaviourStack to the client which will create it there
        /// </summary>
        /// <param name="behaviourStack">the fully populated behaviour stack we send</param>
        private void CreateBehaviourStackOnClient(Behaviour_StateMachine behaviourStack)
        {
            LogMessage("CreateBehaviourStackOnClient");

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

            if (behaviourStack == null)
            {
                LogMessage("CreateBehaviourStackOnClient behaviourStack==null");
                OISMessageBox("No behaviour stack to send");
                return;
            }

            // create the message container
            SCM_BehaviourStackBuild scmData = new SCM_BehaviourStackBuild(behaviourStack);

            //display it
            AppendDataToConnectionTrace("OUT: " + scmData.GetState());
            // send it
            dataTransporter.SendDataMessage(scmData);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends the a message to the client to drop the behaviour stack
        /// </summary>
        private void DropBehaviourStackOnClient()
        {
            LogMessage("DropBehaviourStackOnClient");

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

            // create the message container
            SCM_BehaviourStackDrop scmData = new SCM_BehaviourStackDrop();

            //display it
            AppendDataToConnectionTrace("OUT: " + scmData.GetState());
            // send it
            dataTransporter.SendDataMessage(scmData);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends the global behaviour stack data to the walnut client
        /// </summary>
        public void TransmitGlobalStackData()
        {
            if (dataTransporter == null)
            {
                LogMessage("TransmitGlobalStackData, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("TransmitGlobalStackData, Not connected");
                return;
            }

            SCM_BehaviourStackUpdate scmMessage = null;
            lock (globalBehaviourStackLockObj)
            {
                // create a transfer message object, with the global behaviour stack data only - empty BehaviourList
                scmMessage = new SCM_BehaviourStackUpdate(globalBehaviourStack);
            }

            // send it
            dataTransporter.SendDataMessage(scmMessage);
        }

        #endregion

        private void buttonDrawTest_Click(object sender, EventArgs e)
        {
            //// sanity checks
            //if (ImageOverlayTransform == null) return;
            //if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            //Rectangle rect = new Rectangle(320, 240, 100, 100);

            //(ImageOverlayTransform as MFTOverlayImage_Base).ConvertColorToColorInRectOnOverlay(ColorTranslator.FromHtml(HTML_GREEN), ColorTranslator.FromHtml(HTML_RED), rect, true);

            ////   public void ConvertColorToColorInRectOnOverlay(Color color, Color toColor, Rectangle rect)
        }

        private void buttonTest2_Click(object sender, EventArgs e)
        {
            //// sanity checks
            //if (ImageOverlayTransform == null) return;
            //if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;


            ////    (ImageOverlayTransform as MFTOverlayImage_Base).SaveOverlayImageAsBMP("foobar");
        }

        private void buttonTest3_Click(object sender, EventArgs e)
        {
            //if (ImageOverlayTransform == null) return;
            //if ((ImageOverlayTransform is MFTOverlayImage_GS) == false) return;

            //(ImageOverlayTransform as MFTOverlayImage_Base).SetOverlayImage(@"C:\Dump\test1.png", null);

        }

        private void buttonTestBehaviours_Click(object sender, EventArgs e)
        {
            //if (dataTransporter == null)
            //{
            //    LogMessage("SendDataFromScreenToClient, dataTransporter == null");
            //    return;
            //}
            //if (IsConnected() == false)
            //{
            //    LogMessage("SendDataFromScreenToClient, Not connected");
            //    return;
            //}

            //SCM_BehaviourStackUpdate scmMessage = null;
            //lock (globalBehaviourStackLockObj)
            //{
            //    // create a transfer message object, with the global behaviour stack data only - empty BehaviourList
            //    scmMessage = new SCM_BehaviourStackUpdate(globalBehaviourStack);
            //}

            //// display it
            //AppendDataToConnectionTrace("OUT: dataStr=" + scmMessage.ToString());
            //// send it
            //dataTransporter.SendDataMessage(scmMessage);
        }

        private void buttonEx012Test_Click(object sender, EventArgs e)
        {
            // lock the global behaviour stack
            lock (globalBehaviourStackLockObj)
            {
                // haven't got one? do nothing
                if (globalBehaviourStack == null) return;
                // ok we have one, copy over the appropriate data while in the lock
                (globalBehaviourStack as IBehaviour_ProbeRotationControl).ProbeRotationSteps = 20000;
                (globalBehaviourStack as IBehaviour_ProbeRotationControl).ProbeRotationWanted = true;
                (globalBehaviourStack as IBehaviour_ProbeRotationControl).ProbeRotationDir = 1;
            }

        }

        private void buttonEx012Test_a_Click(object sender, EventArgs e)
        {
            // lock the global behaviour stack
            lock (globalBehaviourStackLockObj)
            {
                // haven't got one? do nothing
                if (globalBehaviourStack == null) return;
                // ok we have one, copy over the appropriate data while in the lock
                (globalBehaviourStack as IBehaviour_ProbeRotationControl).ProbeRotationSteps = 20000;
                (globalBehaviourStack as IBehaviour_ProbeRotationControl).ProbeRotationWanted = true;
                (globalBehaviourStack as IBehaviour_ProbeRotationControl).ProbeRotationDir = 0;
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a state change on the checkBoxManualOutputControl box
        /// </summary>
        private void checkBoxManualOutputControl_CheckedChanged(object sender, EventArgs e)
        {
            // the checked state of this is the enable state of the LED1 toggle
            checkBoxLED1State.Enabled = checkBoxManualOutputControl.Checked;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a state change on the checkBoxLED1State control
        /// </summary>
        private void checkBoxLED1State_CheckedChanged(object sender, EventArgs e)
        {
            LogMessage("checkBoxLED1State_CheckedChanged_Click");

            // create a new list
            List<SCData_PinOutputConfig> gpioOutputList = new List<SCData_PinOutputConfig>();

            // create a new PinState data object and set it to the state we want
            SCData_PinOutputConfig gpioCfg = new SCData_PinOutputConfig(EX012LED1GPIO, checkBoxLED1State.Checked);
            gpioOutputList.Add(gpioCfg);
            
            // send it
            ProcessOutputGPIOControlList(gpioOutputList);
        }
    }
}
