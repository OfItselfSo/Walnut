namespace Walnut
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.tabControlMainFrm = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.checkBoxTestStepper0 = new System.Windows.Forms.CheckBox();
            this.buttonClientExit = new System.Windows.Forms.Button();
            this.buttonClientMark = new System.Windows.Forms.Button();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.checkBoxTransmitToClient = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxFoundObjects = new System.Windows.Forms.TextBox();
            this.labelCount = new System.Windows.Forms.Label();
            this.buttonResetRecNumber = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxRecNumber = new System.Windows.Forms.TextBox();
            this.buttonResetRunNumber = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxRunNumber = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxCaptureDirName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxRunName = new System.Windows.Forms.TextBox();
            this.labelOutputFileName = new System.Windows.Forms.Label();
            this.textBoxCaptureFileName = new System.Windows.Forms.TextBox();
            this.buttonRecordingOnOff = new System.Windows.Forms.Button();
            this.ctlTantaEVRStreamDisplay1 = new TantaCommon.ctlTantaEVRStreamDisplay();
            this.buttonStartStopCapture = new System.Windows.Forms.Button();
            this.tabPageSetup = new System.Windows.Forms.TabPage();
            this.labelVideoCaptureDeviceName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ctlTantaVideoPicker1 = new TantaCommon.ctlTantaVideoPicker();
            this.textBoxPickedVideoDeviceURL = new System.Windows.Forms.TextBox();
            this.tabPageTransporter = new System.Windows.Forms.TabPage();
            this.checkBoxWaldosEnabled = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxDataTrace = new System.Windows.Forms.TextBox();
            this.buttonSendData = new System.Windows.Forms.Button();
            this.checkBoxTestStepperDir = new System.Windows.Forms.CheckBox();
            this.tabControlMainFrm.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.tabPageSetup.SuspendLayout();
            this.tabPageTransporter.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMainFrm
            // 
            this.tabControlMainFrm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMainFrm.Controls.Add(this.tabPageMain);
            this.tabControlMainFrm.Controls.Add(this.tabPageSetup);
            this.tabControlMainFrm.Controls.Add(this.tabPageTransporter);
            this.tabControlMainFrm.Location = new System.Drawing.Point(2, 2);
            this.tabControlMainFrm.Name = "tabControlMainFrm";
            this.tabControlMainFrm.SelectedIndex = 0;
            this.tabControlMainFrm.Size = new System.Drawing.Size(941, 524);
            this.tabControlMainFrm.TabIndex = 29;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.checkBoxTestStepperDir);
            this.tabPageMain.Controls.Add(this.checkBoxTestStepper0);
            this.tabPageMain.Controls.Add(this.buttonClientExit);
            this.tabPageMain.Controls.Add(this.buttonClientMark);
            this.tabPageMain.Controls.Add(this.textBoxStatus);
            this.tabPageMain.Controls.Add(this.checkBoxTransmitToClient);
            this.tabPageMain.Controls.Add(this.label8);
            this.tabPageMain.Controls.Add(this.textBoxFoundObjects);
            this.tabPageMain.Controls.Add(this.labelCount);
            this.tabPageMain.Controls.Add(this.buttonResetRecNumber);
            this.tabPageMain.Controls.Add(this.label5);
            this.tabPageMain.Controls.Add(this.textBoxRecNumber);
            this.tabPageMain.Controls.Add(this.buttonResetRunNumber);
            this.tabPageMain.Controls.Add(this.label4);
            this.tabPageMain.Controls.Add(this.textBoxRunNumber);
            this.tabPageMain.Controls.Add(this.label3);
            this.tabPageMain.Controls.Add(this.textBoxCaptureDirName);
            this.tabPageMain.Controls.Add(this.label2);
            this.tabPageMain.Controls.Add(this.textBoxRunName);
            this.tabPageMain.Controls.Add(this.labelOutputFileName);
            this.tabPageMain.Controls.Add(this.textBoxCaptureFileName);
            this.tabPageMain.Controls.Add(this.buttonRecordingOnOff);
            this.tabPageMain.Controls.Add(this.ctlTantaEVRStreamDisplay1);
            this.tabPageMain.Controls.Add(this.buttonStartStopCapture);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(933, 498);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Main";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // checkBoxTestStepper0
            // 
            this.checkBoxTestStepper0.AutoSize = true;
            this.checkBoxTestStepper0.Location = new System.Drawing.Point(679, 379);
            this.checkBoxTestStepper0.Name = "checkBoxTestStepper0";
            this.checkBoxTestStepper0.Size = new System.Drawing.Size(96, 17);
            this.checkBoxTestStepper0.TabIndex = 61;
            this.checkBoxTestStepper0.Text = "Test Stepper 0";
            this.checkBoxTestStepper0.UseVisualStyleBackColor = true;
            this.checkBoxTestStepper0.CheckedChanged += new System.EventHandler(this.checkBoxTestStepper0_CheckedChanged);
            // 
            // buttonClientExit
            // 
            this.buttonClientExit.Location = new System.Drawing.Point(874, 159);
            this.buttonClientExit.Name = "buttonClientExit";
            this.buttonClientExit.Size = new System.Drawing.Size(39, 19);
            this.buttonClientExit.TabIndex = 51;
            this.buttonClientExit.Text = "Exit";
            this.buttonClientExit.UseVisualStyleBackColor = true;
            this.buttonClientExit.Click += new System.EventHandler(this.buttonClientExit_Click);
            // 
            // buttonClientMark
            // 
            this.buttonClientMark.Location = new System.Drawing.Point(874, 139);
            this.buttonClientMark.Name = "buttonClientMark";
            this.buttonClientMark.Size = new System.Drawing.Size(39, 19);
            this.buttonClientMark.TabIndex = 50;
            this.buttonClientMark.Text = "Mark";
            this.buttonClientMark.UseVisualStyleBackColor = true;
            this.buttonClientMark.Click += new System.EventHandler(this.buttonClientMark_Click);
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.Location = new System.Drawing.Point(668, 466);
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.Size = new System.Drawing.Size(231, 20);
            this.textBoxStatus.TabIndex = 49;
            // 
            // checkBoxTransmitToClient
            // 
            this.checkBoxTransmitToClient.AutoSize = true;
            this.checkBoxTransmitToClient.Location = new System.Drawing.Point(806, 151);
            this.checkBoxTransmitToClient.Name = "checkBoxTransmitToClient";
            this.checkBoxTransmitToClient.Size = new System.Drawing.Size(66, 17);
            this.checkBoxTransmitToClient.TabIndex = 48;
            this.checkBoxTransmitToClient.Text = "Transmit";
            this.checkBoxTransmitToClient.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(665, 221);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 13);
            this.label8.TabIndex = 47;
            this.label8.Text = "Found Objects";
            // 
            // textBoxFoundObjects
            // 
            this.textBoxFoundObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFoundObjects.Location = new System.Drawing.Point(668, 237);
            this.textBoxFoundObjects.Multiline = true;
            this.textBoxFoundObjects.Name = "textBoxFoundObjects";
            this.textBoxFoundObjects.Size = new System.Drawing.Size(231, 56);
            this.textBoxFoundObjects.TabIndex = 46;
            // 
            // labelCount
            // 
            this.labelCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCount.AutoSize = true;
            this.labelCount.Location = new System.Drawing.Point(665, 296);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(100, 13);
            this.labelCount.TabIndex = 45;
            this.labelCount.Text = "Processed Count: 0";
            // 
            // buttonResetRecNumber
            // 
            this.buttonResetRecNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetRecNumber.Location = new System.Drawing.Point(847, 193);
            this.buttonResetRecNumber.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetRecNumber.Name = "buttonResetRecNumber";
            this.buttonResetRecNumber.Size = new System.Drawing.Size(52, 20);
            this.buttonResetRecNumber.TabIndex = 43;
            this.buttonResetRecNumber.Text = "Reset";
            this.buttonResetRecNumber.UseVisualStyleBackColor = true;
            this.buttonResetRecNumber.Click += new System.EventHandler(this.buttonResetRecNumber_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(797, 177);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 42;
            this.label5.Text = "Rec $$";
            // 
            // textBoxRecNumber
            // 
            this.textBoxRecNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRecNumber.Location = new System.Drawing.Point(800, 193);
            this.textBoxRecNumber.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxRecNumber.Name = "textBoxRecNumber";
            this.textBoxRecNumber.Size = new System.Drawing.Size(43, 20);
            this.textBoxRecNumber.TabIndex = 41;
            // 
            // buttonResetRunNumber
            // 
            this.buttonResetRunNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetRunNumber.Location = new System.Drawing.Point(876, 103);
            this.buttonResetRunNumber.Margin = new System.Windows.Forms.Padding(2);
            this.buttonResetRunNumber.Name = "buttonResetRunNumber";
            this.buttonResetRunNumber.Size = new System.Drawing.Size(52, 20);
            this.buttonResetRunNumber.TabIndex = 40;
            this.buttonResetRunNumber.Text = "Reset";
            this.buttonResetRunNumber.UseVisualStyleBackColor = true;
            this.buttonResetRunNumber.Click += new System.EventHandler(this.buttonResetRunNumber_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(826, 88);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Run ##";
            // 
            // textBoxRunNumber
            // 
            this.textBoxRunNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRunNumber.Location = new System.Drawing.Point(829, 104);
            this.textBoxRunNumber.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxRunNumber.Name = "textBoxRunNumber";
            this.textBoxRunNumber.Size = new System.Drawing.Size(43, 20);
            this.textBoxRunNumber.TabIndex = 38;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(651, 13);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 37;
            this.label3.Text = "Capture Output Directory";
            // 
            // textBoxCaptureDirName
            // 
            this.textBoxCaptureDirName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCaptureDirName.Location = new System.Drawing.Point(668, 29);
            this.textBoxCaptureDirName.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxCaptureDirName.Name = "textBoxCaptureDirName";
            this.textBoxCaptureDirName.Size = new System.Drawing.Size(260, 20);
            this.textBoxCaptureDirName.TabIndex = 36;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(651, 89);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 35;
            this.label2.Text = "Run Info Tag";
            // 
            // textBoxRunName
            // 
            this.textBoxRunName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRunName.Location = new System.Drawing.Point(668, 103);
            this.textBoxRunName.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxRunName.Name = "textBoxRunName";
            this.textBoxRunName.Size = new System.Drawing.Size(157, 20);
            this.textBoxRunName.TabIndex = 34;
            // 
            // labelOutputFileName
            // 
            this.labelOutputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelOutputFileName.AutoSize = true;
            this.labelOutputFileName.Location = new System.Drawing.Point(651, 51);
            this.labelOutputFileName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelOutputFileName.Name = "labelOutputFileName";
            this.labelOutputFileName.Size = new System.Drawing.Size(94, 13);
            this.labelOutputFileName.TabIndex = 33;
            this.labelOutputFileName.Text = "Capture File Name";
            // 
            // textBoxCaptureFileName
            // 
            this.textBoxCaptureFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCaptureFileName.Location = new System.Drawing.Point(668, 66);
            this.textBoxCaptureFileName.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxCaptureFileName.Name = "textBoxCaptureFileName";
            this.textBoxCaptureFileName.Size = new System.Drawing.Size(260, 20);
            this.textBoxCaptureFileName.TabIndex = 32;
            // 
            // buttonRecordingOnOff
            // 
            this.buttonRecordingOnOff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRecordingOnOff.Location = new System.Drawing.Point(668, 186);
            this.buttonRecordingOnOff.Margin = new System.Windows.Forms.Padding(2);
            this.buttonRecordingOnOff.Name = "buttonRecordingOnOff";
            this.buttonRecordingOnOff.Size = new System.Drawing.Size(128, 29);
            this.buttonRecordingOnOff.TabIndex = 31;
            this.buttonRecordingOnOff.Text = "Recording is OFF";
            this.buttonRecordingOnOff.UseVisualStyleBackColor = true;
            this.buttonRecordingOnOff.Click += new System.EventHandler(this.buttonRecordingOnOff_Click);
            // 
            // ctlTantaEVRStreamDisplay1
            // 
            this.ctlTantaEVRStreamDisplay1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlTantaEVRStreamDisplay1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctlTantaEVRStreamDisplay1.Location = new System.Drawing.Point(5, 10);
            this.ctlTantaEVRStreamDisplay1.Name = "ctlTantaEVRStreamDisplay1";
            this.ctlTantaEVRStreamDisplay1.Size = new System.Drawing.Size(640, 476);
            this.ctlTantaEVRStreamDisplay1.TabIndex = 30;
            // 
            // buttonStartStopCapture
            // 
            this.buttonStartStopCapture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartStopCapture.Location = new System.Drawing.Point(668, 144);
            this.buttonStartStopCapture.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStartStopCapture.Name = "buttonStartStopCapture";
            this.buttonStartStopCapture.Size = new System.Drawing.Size(128, 29);
            this.buttonStartStopCapture.TabIndex = 29;
            this.buttonStartStopCapture.Text = "Start Capture";
            this.buttonStartStopCapture.UseVisualStyleBackColor = true;
            this.buttonStartStopCapture.Click += new System.EventHandler(this.buttonStartStopCapture_Click);
            // 
            // tabPageSetup
            // 
            this.tabPageSetup.Controls.Add(this.labelVideoCaptureDeviceName);
            this.tabPageSetup.Controls.Add(this.label1);
            this.tabPageSetup.Controls.Add(this.ctlTantaVideoPicker1);
            this.tabPageSetup.Controls.Add(this.textBoxPickedVideoDeviceURL);
            this.tabPageSetup.Location = new System.Drawing.Point(4, 22);
            this.tabPageSetup.Name = "tabPageSetup";
            this.tabPageSetup.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSetup.Size = new System.Drawing.Size(933, 498);
            this.tabPageSetup.TabIndex = 1;
            this.tabPageSetup.Text = "Setup";
            this.tabPageSetup.UseVisualStyleBackColor = true;
            // 
            // labelVideoCaptureDeviceName
            // 
            this.labelVideoCaptureDeviceName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelVideoCaptureDeviceName.AutoSize = true;
            this.labelVideoCaptureDeviceName.Location = new System.Drawing.Point(5, 456);
            this.labelVideoCaptureDeviceName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelVideoCaptureDeviceName.Name = "labelVideoCaptureDeviceName";
            this.labelVideoCaptureDeviceName.Size = new System.Drawing.Size(107, 13);
            this.labelVideoCaptureDeviceName.TabIndex = 33;
            this.labelVideoCaptureDeviceName.Text = "Picked Video Device";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, -50);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "Choose a Video Source";
            // 
            // ctlTantaVideoPicker1
            // 
            this.ctlTantaVideoPicker1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ctlTantaVideoPicker1.Location = new System.Drawing.Point(5, 6);
            this.ctlTantaVideoPicker1.Name = "ctlTantaVideoPicker1";
            this.ctlTantaVideoPicker1.Size = new System.Drawing.Size(448, 447);
            this.ctlTantaVideoPicker1.TabIndex = 31;
            // 
            // textBoxPickedVideoDeviceURL
            // 
            this.textBoxPickedVideoDeviceURL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxPickedVideoDeviceURL.Location = new System.Drawing.Point(5, 471);
            this.textBoxPickedVideoDeviceURL.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPickedVideoDeviceURL.Name = "textBoxPickedVideoDeviceURL";
            this.textBoxPickedVideoDeviceURL.Size = new System.Drawing.Size(448, 20);
            this.textBoxPickedVideoDeviceURL.TabIndex = 30;
            // 
            // tabPageTransporter
            // 
            this.tabPageTransporter.Controls.Add(this.checkBoxWaldosEnabled);
            this.tabPageTransporter.Controls.Add(this.label7);
            this.tabPageTransporter.Controls.Add(this.textBoxDataTrace);
            this.tabPageTransporter.Controls.Add(this.buttonSendData);
            this.tabPageTransporter.Location = new System.Drawing.Point(4, 22);
            this.tabPageTransporter.Name = "tabPageTransporter";
            this.tabPageTransporter.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTransporter.Size = new System.Drawing.Size(933, 498);
            this.tabPageTransporter.TabIndex = 2;
            this.tabPageTransporter.Text = "Transporter";
            this.tabPageTransporter.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaldosEnabled
            // 
            this.checkBoxWaldosEnabled.AutoSize = true;
            this.checkBoxWaldosEnabled.Checked = true;
            this.checkBoxWaldosEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWaldosEnabled.Location = new System.Drawing.Point(51, 50);
            this.checkBoxWaldosEnabled.Name = "checkBoxWaldosEnabled";
            this.checkBoxWaldosEnabled.Size = new System.Drawing.Size(104, 17);
            this.checkBoxWaldosEnabled.TabIndex = 61;
            this.checkBoxWaldosEnabled.Text = "Waldos Enabled";
            this.checkBoxWaldosEnabled.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(48, 219);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 59;
            this.label7.Text = "Diagnostics";
            // 
            // textBoxDataTrace
            // 
            this.textBoxDataTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataTrace.Location = new System.Drawing.Point(51, 234);
            this.textBoxDataTrace.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDataTrace.Multiline = true;
            this.textBoxDataTrace.Name = "textBoxDataTrace";
            this.textBoxDataTrace.ReadOnly = true;
            this.textBoxDataTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDataTrace.Size = new System.Drawing.Size(549, 132);
            this.textBoxDataTrace.TabIndex = 57;
            // 
            // buttonSendData
            // 
            this.buttonSendData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendData.Location = new System.Drawing.Point(482, 156);
            this.buttonSendData.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSendData.Name = "buttonSendData";
            this.buttonSendData.Size = new System.Drawing.Size(115, 24);
            this.buttonSendData.TabIndex = 56;
            this.buttonSendData.Text = "Force Send Data";
            this.buttonSendData.UseVisualStyleBackColor = true;
            this.buttonSendData.Click += new System.EventHandler(this.buttonSendData_Click);
            // 
            // checkBoxTestStepperDir
            // 
            this.checkBoxTestStepperDir.AutoSize = true;
            this.checkBoxTestStepperDir.Location = new System.Drawing.Point(774, 379);
            this.checkBoxTestStepperDir.Name = "checkBoxTestStepperDir";
            this.checkBoxTestStepperDir.Size = new System.Drawing.Size(51, 17);
            this.checkBoxTestStepperDir.TabIndex = 62;
            this.checkBoxTestStepperDir.Text = "CCW";
            this.checkBoxTestStepperDir.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(955, 527);
            this.Controls.Add(this.tabControlMainFrm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "frmMain";
            this.Text = "Walnut: Remote Robotic Manipulator Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.tabControlMainFrm.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.tabPageMain.PerformLayout();
            this.tabPageSetup.ResumeLayout(false);
            this.tabPageSetup.PerformLayout();
            this.tabPageTransporter.ResumeLayout(false);
            this.tabPageTransporter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMainFrm;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.Label labelOutputFileName;
        private System.Windows.Forms.TextBox textBoxCaptureFileName;
        private System.Windows.Forms.Button buttonRecordingOnOff;
        private TantaCommon.ctlTantaEVRStreamDisplay ctlTantaEVRStreamDisplay1;
        private System.Windows.Forms.Button buttonStartStopCapture;
        private System.Windows.Forms.TabPage tabPageSetup;
        private System.Windows.Forms.Label labelVideoCaptureDeviceName;
        private System.Windows.Forms.Label label1;
        private TantaCommon.ctlTantaVideoPicker ctlTantaVideoPicker1;
        private System.Windows.Forms.TextBox textBoxPickedVideoDeviceURL;
        private System.Windows.Forms.TextBox textBoxRunName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxCaptureDirName;
        private System.Windows.Forms.Button buttonResetRunNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxRunNumber;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxRecNumber;
        private System.Windows.Forms.Button buttonResetRecNumber;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.TextBox textBoxFoundObjects;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPageTransporter;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxDataTrace;
        private System.Windows.Forms.Button buttonSendData;
        private System.Windows.Forms.CheckBox checkBoxWaldosEnabled;
        private System.Windows.Forms.CheckBox checkBoxTransmitToClient;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonClientExit;
        private System.Windows.Forms.Button buttonClientMark;
        private System.Windows.Forms.CheckBox checkBoxTestStepper0;
        private System.Windows.Forms.CheckBox checkBoxTestStepperDir;
    }
}

