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
            this.groupBoxDrawGreenCircle = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxGreenCircleLineThickness = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxGreenCircleRadius = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxGreenCircleXY = new System.Windows.Forms.TextBox();
            this.buttonDrawCircle = new System.Windows.Forms.Button();
            this.checkBoxMakeTargetTransparent = new System.Windows.Forms.CheckBox();
            this.checkBoxFindGreen = new System.Windows.Forms.CheckBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.groupBoxAction = new System.Windows.Forms.GroupBox();
            this.radioButtonPathFollow = new System.Windows.Forms.RadioButton();
            this.radioButtonRedToGreen = new System.Windows.Forms.RadioButton();
            this.checkBoxActivate = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxPWMBDir = new System.Windows.Forms.CheckBox();
            this.textBoxPWMBSpeed = new System.Windows.Forms.TextBox();
            this.checkBoxPWMBEnable = new System.Windows.Forms.CheckBox();
            this.checkBoxPWMADir = new System.Windows.Forms.CheckBox();
            this.textBoxPWMASpeed = new System.Windows.Forms.TextBox();
            this.checkBoxPWMAEnable = new System.Windows.Forms.CheckBox();
            this.checkBoxDrawImageOverlay = new System.Windows.Forms.CheckBox();
            this.groupBoxTestRect = new System.Windows.Forms.GroupBox();
            this.radioButtonLocNone = new System.Windows.Forms.RadioButton();
            this.radioButtonLoc4 = new System.Windows.Forms.RadioButton();
            this.radioButtonLoc3 = new System.Windows.Forms.RadioButton();
            this.radioButtonLoc2 = new System.Windows.Forms.RadioButton();
            this.radioButtonLoc1 = new System.Windows.Forms.RadioButton();
            this.checkBoxTestStepperDir = new System.Windows.Forms.CheckBox();
            this.checkBoxTestStepper0 = new System.Windows.Forms.CheckBox();
            this.buttonClientExit = new System.Windows.Forms.Button();
            this.buttonClientMark = new System.Windows.Forms.Button();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
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
            this.textBoxDataTrace = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBoxWaldosEnabled = new System.Windows.Forms.CheckBox();
            this.buttonSendData = new System.Windows.Forms.Button();
            this.ctlTransparentControl1 = new Walnut.ctlTransparentControl();
            this.tabControlMainFrm.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.groupBoxDrawGreenCircle.SuspendLayout();
            this.groupBoxAction.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxTestRect.SuspendLayout();
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
            this.tabPageMain.Controls.Add(this.ctlTransparentControl1);
            this.tabPageMain.Controls.Add(this.groupBoxDrawGreenCircle);
            this.tabPageMain.Controls.Add(this.checkBoxMakeTargetTransparent);
            this.tabPageMain.Controls.Add(this.checkBoxFindGreen);
            this.tabPageMain.Controls.Add(this.buttonTest);
            this.tabPageMain.Controls.Add(this.groupBoxAction);
            this.tabPageMain.Controls.Add(this.groupBox1);
            this.tabPageMain.Controls.Add(this.checkBoxDrawImageOverlay);
            this.tabPageMain.Controls.Add(this.groupBoxTestRect);
            this.tabPageMain.Controls.Add(this.checkBoxTestStepperDir);
            this.tabPageMain.Controls.Add(this.checkBoxTestStepper0);
            this.tabPageMain.Controls.Add(this.buttonClientExit);
            this.tabPageMain.Controls.Add(this.buttonClientMark);
            this.tabPageMain.Controls.Add(this.textBoxStatus);
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
            // groupBoxDrawGreenCircle
            // 
            this.groupBoxDrawGreenCircle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDrawGreenCircle.Controls.Add(this.label9);
            this.groupBoxDrawGreenCircle.Controls.Add(this.textBoxGreenCircleLineThickness);
            this.groupBoxDrawGreenCircle.Controls.Add(this.label8);
            this.groupBoxDrawGreenCircle.Controls.Add(this.textBoxGreenCircleRadius);
            this.groupBoxDrawGreenCircle.Controls.Add(this.label6);
            this.groupBoxDrawGreenCircle.Controls.Add(this.textBoxGreenCircleXY);
            this.groupBoxDrawGreenCircle.Controls.Add(this.buttonDrawCircle);
            this.groupBoxDrawGreenCircle.Location = new System.Drawing.Point(667, 278);
            this.groupBoxDrawGreenCircle.Name = "groupBoxDrawGreenCircle";
            this.groupBoxDrawGreenCircle.Size = new System.Drawing.Size(150, 66);
            this.groupBoxDrawGreenCircle.TabIndex = 73;
            this.groupBoxDrawGreenCircle.TabStop = false;
            this.groupBoxDrawGreenCircle.Text = "Green Circle";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(106, 29);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(27, 13);
            this.label9.TabIndex = 77;
            this.label9.Text = "Line";
            // 
            // textBoxGreenCircleLineThickness
            // 
            this.textBoxGreenCircleLineThickness.Location = new System.Drawing.Point(102, 43);
            this.textBoxGreenCircleLineThickness.Name = "textBoxGreenCircleLineThickness";
            this.textBoxGreenCircleLineThickness.Size = new System.Drawing.Size(42, 20);
            this.textBoxGreenCircleLineThickness.TabIndex = 76;
            this.textBoxGreenCircleLineThickness.Text = "2";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 44);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 75;
            this.label8.Text = "Radius";
            // 
            // textBoxGreenCircleRadius
            // 
            this.textBoxGreenCircleRadius.Location = new System.Drawing.Point(49, 41);
            this.textBoxGreenCircleRadius.Name = "textBoxGreenCircleRadius";
            this.textBoxGreenCircleRadius.Size = new System.Drawing.Size(42, 20);
            this.textBoxGreenCircleRadius.TabIndex = 74;
            this.textBoxGreenCircleRadius.Text = "25";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 13);
            this.label6.TabIndex = 73;
            this.label6.Text = "X,Y";
            // 
            // textBoxGreenCircleXY
            // 
            this.textBoxGreenCircleXY.Location = new System.Drawing.Point(32, 19);
            this.textBoxGreenCircleXY.Name = "textBoxGreenCircleXY";
            this.textBoxGreenCircleXY.Size = new System.Drawing.Size(59, 20);
            this.textBoxGreenCircleXY.TabIndex = 72;
            this.textBoxGreenCircleXY.Text = "320,240";
            // 
            // buttonDrawCircle
            // 
            this.buttonDrawCircle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDrawCircle.Location = new System.Drawing.Point(101, 8);
            this.buttonDrawCircle.Name = "buttonDrawCircle";
            this.buttonDrawCircle.Size = new System.Drawing.Size(43, 19);
            this.buttonDrawCircle.TabIndex = 69;
            this.buttonDrawCircle.Text = "Draw";
            this.buttonDrawCircle.UseVisualStyleBackColor = true;
            this.buttonDrawCircle.Click += new System.EventHandler(this.buttonDrawCircle_Click);
            // 
            // checkBoxMakeTargetTransparent
            // 
            this.checkBoxMakeTargetTransparent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxMakeTargetTransparent.AutoSize = true;
            this.checkBoxMakeTargetTransparent.Location = new System.Drawing.Point(837, 416);
            this.checkBoxMakeTargetTransparent.Name = "checkBoxMakeTargetTransparent";
            this.checkBoxMakeTargetTransparent.Size = new System.Drawing.Size(86, 17);
            this.checkBoxMakeTargetTransparent.TabIndex = 70;
            this.checkBoxMakeTargetTransparent.Text = "Make Trans.";
            this.checkBoxMakeTargetTransparent.UseVisualStyleBackColor = true;
            // 
            // checkBoxFindGreen
            // 
            this.checkBoxFindGreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxFindGreen.AutoSize = true;
            this.checkBoxFindGreen.Location = new System.Drawing.Point(837, 433);
            this.checkBoxFindGreen.Name = "checkBoxFindGreen";
            this.checkBoxFindGreen.Size = new System.Drawing.Size(78, 17);
            this.checkBoxFindGreen.TabIndex = 69;
            this.checkBoxFindGreen.Text = "Find Green";
            this.checkBoxFindGreen.UseVisualStyleBackColor = true;
            this.checkBoxFindGreen.CheckedChanged += new System.EventHandler(this.checkBoxFindGreen_CheckedChanged);
            // 
            // buttonTest
            // 
            this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTest.Location = new System.Drawing.Point(786, 441);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(39, 19);
            this.buttonTest.TabIndex = 68;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // groupBoxAction
            // 
            this.groupBoxAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAction.Controls.Add(this.radioButtonPathFollow);
            this.groupBoxAction.Controls.Add(this.radioButtonRedToGreen);
            this.groupBoxAction.Controls.Add(this.checkBoxActivate);
            this.groupBoxAction.Location = new System.Drawing.Point(803, 196);
            this.groupBoxAction.Name = "groupBoxAction";
            this.groupBoxAction.Size = new System.Drawing.Size(96, 84);
            this.groupBoxAction.TabIndex = 67;
            this.groupBoxAction.TabStop = false;
            this.groupBoxAction.Text = "Action";
            // 
            // radioButtonPathFollow
            // 
            this.radioButtonPathFollow.AutoSize = true;
            this.radioButtonPathFollow.Location = new System.Drawing.Point(3, 35);
            this.radioButtonPathFollow.Name = "radioButtonPathFollow";
            this.radioButtonPathFollow.Size = new System.Drawing.Size(77, 17);
            this.radioButtonPathFollow.TabIndex = 51;
            this.radioButtonPathFollow.Text = "PathFollow";
            this.radioButtonPathFollow.UseVisualStyleBackColor = true;
            // 
            // radioButtonRedToGreen
            // 
            this.radioButtonRedToGreen.AutoSize = true;
            this.radioButtonRedToGreen.Checked = true;
            this.radioButtonRedToGreen.Location = new System.Drawing.Point(3, 17);
            this.radioButtonRedToGreen.Name = "radioButtonRedToGreen";
            this.radioButtonRedToGreen.Size = new System.Drawing.Size(87, 17);
            this.radioButtonRedToGreen.TabIndex = 50;
            this.radioButtonRedToGreen.TabStop = true;
            this.radioButtonRedToGreen.Text = "RedToGreen";
            this.radioButtonRedToGreen.UseVisualStyleBackColor = true;
            // 
            // checkBoxActivate
            // 
            this.checkBoxActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxActivate.AutoSize = true;
            this.checkBoxActivate.Location = new System.Drawing.Point(3, 58);
            this.checkBoxActivate.Name = "checkBoxActivate";
            this.checkBoxActivate.Size = new System.Drawing.Size(65, 17);
            this.checkBoxActivate.TabIndex = 49;
            this.checkBoxActivate.Text = "Activate";
            this.checkBoxActivate.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxPWMBDir);
            this.groupBox1.Controls.Add(this.textBoxPWMBSpeed);
            this.groupBox1.Controls.Add(this.checkBoxPWMBEnable);
            this.groupBox1.Controls.Add(this.checkBoxPWMADir);
            this.groupBox1.Controls.Add(this.textBoxPWMASpeed);
            this.groupBox1.Controls.Add(this.checkBoxPWMAEnable);
            this.groupBox1.Location = new System.Drawing.Point(668, 350);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(157, 86);
            this.groupBox1.TabIndex = 66;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PWM Test";
            // 
            // checkBoxPWMBDir
            // 
            this.checkBoxPWMBDir.AutoSize = true;
            this.checkBoxPWMBDir.Location = new System.Drawing.Point(83, 63);
            this.checkBoxPWMBDir.Name = "checkBoxPWMBDir";
            this.checkBoxPWMBDir.Size = new System.Drawing.Size(39, 17);
            this.checkBoxPWMBDir.TabIndex = 5;
            this.checkBoxPWMBDir.Text = "Dir";
            this.checkBoxPWMBDir.UseVisualStyleBackColor = true;
            this.checkBoxPWMBDir.CheckedChanged += new System.EventHandler(this.checkBoxPWMBDir_CheckedChanged);
            // 
            // textBoxPWMBSpeed
            // 
            this.textBoxPWMBSpeed.Location = new System.Drawing.Point(83, 37);
            this.textBoxPWMBSpeed.Name = "textBoxPWMBSpeed";
            this.textBoxPWMBSpeed.Size = new System.Drawing.Size(62, 20);
            this.textBoxPWMBSpeed.TabIndex = 4;
            this.textBoxPWMBSpeed.Text = "100";
            this.textBoxPWMBSpeed.TextChanged += new System.EventHandler(this.textBoxPWMBSpeed_TextChanged);
            // 
            // checkBoxPWMBEnable
            // 
            this.checkBoxPWMBEnable.AutoSize = true;
            this.checkBoxPWMBEnable.Location = new System.Drawing.Point(83, 19);
            this.checkBoxPWMBEnable.Name = "checkBoxPWMBEnable";
            this.checkBoxPWMBEnable.Size = new System.Drawing.Size(66, 17);
            this.checkBoxPWMBEnable.TabIndex = 3;
            this.checkBoxPWMBEnable.Text = "PWM_B";
            this.checkBoxPWMBEnable.UseVisualStyleBackColor = true;
            this.checkBoxPWMBEnable.CheckedChanged += new System.EventHandler(this.checkBoxPWMBEnable_CheckedChanged);
            // 
            // checkBoxPWMADir
            // 
            this.checkBoxPWMADir.AutoSize = true;
            this.checkBoxPWMADir.Location = new System.Drawing.Point(11, 63);
            this.checkBoxPWMADir.Name = "checkBoxPWMADir";
            this.checkBoxPWMADir.Size = new System.Drawing.Size(39, 17);
            this.checkBoxPWMADir.TabIndex = 2;
            this.checkBoxPWMADir.Text = "Dir";
            this.checkBoxPWMADir.UseVisualStyleBackColor = true;
            this.checkBoxPWMADir.CheckedChanged += new System.EventHandler(this.checkBoxPWMADir_CheckedChanged);
            // 
            // textBoxPWMASpeed
            // 
            this.textBoxPWMASpeed.Location = new System.Drawing.Point(11, 37);
            this.textBoxPWMASpeed.Name = "textBoxPWMASpeed";
            this.textBoxPWMASpeed.Size = new System.Drawing.Size(62, 20);
            this.textBoxPWMASpeed.TabIndex = 1;
            this.textBoxPWMASpeed.Text = "100";
            this.textBoxPWMASpeed.TextChanged += new System.EventHandler(this.textBoxPWMASpeed_TextChanged);
            // 
            // checkBoxPWMAEnable
            // 
            this.checkBoxPWMAEnable.AutoSize = true;
            this.checkBoxPWMAEnable.Location = new System.Drawing.Point(11, 19);
            this.checkBoxPWMAEnable.Name = "checkBoxPWMAEnable";
            this.checkBoxPWMAEnable.Size = new System.Drawing.Size(66, 17);
            this.checkBoxPWMAEnable.TabIndex = 0;
            this.checkBoxPWMAEnable.Text = "PWM_A";
            this.checkBoxPWMAEnable.UseVisualStyleBackColor = true;
            this.checkBoxPWMAEnable.CheckedChanged += new System.EventHandler(this.checkBoxPWMAEnable_CheckedChanged);
            // 
            // checkBoxDrawImageOverlay
            // 
            this.checkBoxDrawImageOverlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxDrawImageOverlay.AutoSize = true;
            this.checkBoxDrawImageOverlay.Location = new System.Drawing.Point(837, 399);
            this.checkBoxDrawImageOverlay.Name = "checkBoxDrawImageOverlay";
            this.checkBoxDrawImageOverlay.Size = new System.Drawing.Size(94, 17);
            this.checkBoxDrawImageOverlay.TabIndex = 65;
            this.checkBoxDrawImageOverlay.Text = "Image Overlay";
            this.checkBoxDrawImageOverlay.UseVisualStyleBackColor = true;
            this.checkBoxDrawImageOverlay.CheckedChanged += new System.EventHandler(this.checkBoxDrawImageOverlay_CheckedChanged);
            // 
            // groupBoxTestRect
            // 
            this.groupBoxTestRect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTestRect.Controls.Add(this.radioButtonLocNone);
            this.groupBoxTestRect.Controls.Add(this.radioButtonLoc4);
            this.groupBoxTestRect.Controls.Add(this.radioButtonLoc3);
            this.groupBoxTestRect.Controls.Add(this.radioButtonLoc2);
            this.groupBoxTestRect.Controls.Add(this.radioButtonLoc1);
            this.groupBoxTestRect.Location = new System.Drawing.Point(831, 283);
            this.groupBoxTestRect.Name = "groupBoxTestRect";
            this.groupBoxTestRect.Size = new System.Drawing.Size(92, 110);
            this.groupBoxTestRect.TabIndex = 64;
            this.groupBoxTestRect.TabStop = false;
            this.groupBoxTestRect.Text = "DrawTestSQ";
            // 
            // radioButtonLocNone
            // 
            this.radioButtonLocNone.AutoSize = true;
            this.radioButtonLocNone.Checked = true;
            this.radioButtonLocNone.Location = new System.Drawing.Point(6, 19);
            this.radioButtonLocNone.Name = "radioButtonLocNone";
            this.radioButtonLocNone.Size = new System.Drawing.Size(51, 17);
            this.radioButtonLocNone.TabIndex = 68;
            this.radioButtonLocNone.TabStop = true;
            this.radioButtonLocNone.Text = "None";
            this.radioButtonLocNone.UseVisualStyleBackColor = true;
            this.radioButtonLocNone.CheckedChanged += new System.EventHandler(this.radioButtonLocNone_CheckedChanged);
            // 
            // radioButtonLoc4
            // 
            this.radioButtonLoc4.AutoSize = true;
            this.radioButtonLoc4.Location = new System.Drawing.Point(6, 87);
            this.radioButtonLoc4.Name = "radioButtonLoc4";
            this.radioButtonLoc4.Size = new System.Drawing.Size(49, 17);
            this.radioButtonLoc4.TabIndex = 67;
            this.radioButtonLoc4.Text = "Loc4";
            this.radioButtonLoc4.UseVisualStyleBackColor = true;
            this.radioButtonLoc4.CheckedChanged += new System.EventHandler(this.radioButtonLoc4_CheckedChanged);
            // 
            // radioButtonLoc3
            // 
            this.radioButtonLoc3.AutoSize = true;
            this.radioButtonLoc3.Location = new System.Drawing.Point(6, 70);
            this.radioButtonLoc3.Name = "radioButtonLoc3";
            this.radioButtonLoc3.Size = new System.Drawing.Size(49, 17);
            this.radioButtonLoc3.TabIndex = 66;
            this.radioButtonLoc3.Text = "Loc3";
            this.radioButtonLoc3.UseVisualStyleBackColor = true;
            this.radioButtonLoc3.CheckedChanged += new System.EventHandler(this.radioButtonLoc3_CheckedChanged);
            // 
            // radioButtonLoc2
            // 
            this.radioButtonLoc2.AutoSize = true;
            this.radioButtonLoc2.Location = new System.Drawing.Point(6, 53);
            this.radioButtonLoc2.Name = "radioButtonLoc2";
            this.radioButtonLoc2.Size = new System.Drawing.Size(49, 17);
            this.radioButtonLoc2.TabIndex = 65;
            this.radioButtonLoc2.Text = "Loc2";
            this.radioButtonLoc2.UseVisualStyleBackColor = true;
            this.radioButtonLoc2.CheckedChanged += new System.EventHandler(this.radioButtonLoc2_CheckedChanged);
            // 
            // radioButtonLoc1
            // 
            this.radioButtonLoc1.AutoSize = true;
            this.radioButtonLoc1.Location = new System.Drawing.Point(6, 36);
            this.radioButtonLoc1.Name = "radioButtonLoc1";
            this.radioButtonLoc1.Size = new System.Drawing.Size(49, 17);
            this.radioButtonLoc1.TabIndex = 64;
            this.radioButtonLoc1.Text = "Loc1";
            this.radioButtonLoc1.UseVisualStyleBackColor = true;
            this.radioButtonLoc1.CheckedChanged += new System.EventHandler(this.radioButtonLoc1_CheckedChanged);
            // 
            // checkBoxTestStepperDir
            // 
            this.checkBoxTestStepperDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxTestStepperDir.AutoSize = true;
            this.checkBoxTestStepperDir.Location = new System.Drawing.Point(774, 329);
            this.checkBoxTestStepperDir.Name = "checkBoxTestStepperDir";
            this.checkBoxTestStepperDir.Size = new System.Drawing.Size(51, 17);
            this.checkBoxTestStepperDir.TabIndex = 62;
            this.checkBoxTestStepperDir.Text = "CCW";
            this.checkBoxTestStepperDir.UseVisualStyleBackColor = true;
            // 
            // checkBoxTestStepper0
            // 
            this.checkBoxTestStepper0.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxTestStepper0.AutoSize = true;
            this.checkBoxTestStepper0.Location = new System.Drawing.Point(679, 329);
            this.checkBoxTestStepper0.Name = "checkBoxTestStepper0";
            this.checkBoxTestStepper0.Size = new System.Drawing.Size(96, 17);
            this.checkBoxTestStepper0.TabIndex = 61;
            this.checkBoxTestStepper0.Text = "Test Stepper 0";
            this.checkBoxTestStepper0.UseVisualStyleBackColor = true;
            this.checkBoxTestStepper0.CheckedChanged += new System.EventHandler(this.checkBoxTestStepper0_CheckedChanged);
            // 
            // buttonClientExit
            // 
            this.buttonClientExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClientExit.Location = new System.Drawing.Point(668, 241);
            this.buttonClientExit.Name = "buttonClientExit";
            this.buttonClientExit.Size = new System.Drawing.Size(39, 19);
            this.buttonClientExit.TabIndex = 51;
            this.buttonClientExit.Text = "Exit";
            this.buttonClientExit.UseVisualStyleBackColor = true;
            this.buttonClientExit.Click += new System.EventHandler(this.buttonClientExit_Click);
            // 
            // buttonClientMark
            // 
            this.buttonClientMark.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClientMark.Location = new System.Drawing.Point(713, 241);
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
            // labelCount
            // 
            this.labelCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCount.AutoSize = true;
            this.labelCount.Location = new System.Drawing.Point(665, 450);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(100, 13);
            this.labelCount.TabIndex = 45;
            this.labelCount.Text = "Processed Count: 0";
            // 
            // buttonResetRecNumber
            // 
            this.buttonResetRecNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetRecNumber.Location = new System.Drawing.Point(847, 144);
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
            this.label5.Location = new System.Drawing.Point(797, 128);
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
            this.textBoxRecNumber.Location = new System.Drawing.Point(800, 144);
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
            this.buttonRecordingOnOff.Location = new System.Drawing.Point(668, 137);
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
            this.ctlTantaEVRStreamDisplay1.Location = new System.Drawing.Point(5, 10);
            this.ctlTantaEVRStreamDisplay1.Name = "ctlTantaEVRStreamDisplay1";
            this.ctlTantaEVRStreamDisplay1.Size = new System.Drawing.Size(640, 480);
            this.ctlTantaEVRStreamDisplay1.TabIndex = 30;
            // 
            // buttonStartStopCapture
            // 
            this.buttonStartStopCapture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartStopCapture.Location = new System.Drawing.Point(668, 207);
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
            this.tabPageTransporter.Controls.Add(this.textBoxDataTrace);
            this.tabPageTransporter.Controls.Add(this.label7);
            this.tabPageTransporter.Controls.Add(this.checkBoxWaldosEnabled);
            this.tabPageTransporter.Controls.Add(this.buttonSendData);
            this.tabPageTransporter.Location = new System.Drawing.Point(4, 22);
            this.tabPageTransporter.Name = "tabPageTransporter";
            this.tabPageTransporter.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTransporter.Size = new System.Drawing.Size(933, 498);
            this.tabPageTransporter.TabIndex = 2;
            this.tabPageTransporter.Text = "Transporter";
            this.tabPageTransporter.UseVisualStyleBackColor = true;
            // 
            // textBoxDataTrace
            // 
            this.textBoxDataTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataTrace.Location = new System.Drawing.Point(108, 282);
            this.textBoxDataTrace.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDataTrace.Multiline = true;
            this.textBoxDataTrace.Name = "textBoxDataTrace";
            this.textBoxDataTrace.ReadOnly = true;
            this.textBoxDataTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDataTrace.Size = new System.Drawing.Size(549, 132);
            this.textBoxDataTrace.TabIndex = 57;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(93, 218);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 59;
            this.label7.Text = "Diagnostics";
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
            // ctlTransparentControl1
            // 
            this.ctlTransparentControl1.Location = new System.Drawing.Point(5, 10);
            this.ctlTransparentControl1.Name = "ctlTransparentControl1";
            this.ctlTransparentControl1.Opacity = 0;
            this.ctlTransparentControl1.Size = new System.Drawing.Size(640, 480);
            this.ctlTransparentControl1.TabIndex = 74;
            this.ctlTransparentControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ctlTransparentControl1_MouseClick);
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
            this.groupBoxDrawGreenCircle.ResumeLayout(false);
            this.groupBoxDrawGreenCircle.PerformLayout();
            this.groupBoxAction.ResumeLayout(false);
            this.groupBoxAction.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxTestRect.ResumeLayout(false);
            this.groupBoxTestRect.PerformLayout();
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
        private System.Windows.Forms.TabPage tabPageTransporter;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxDataTrace;
        private System.Windows.Forms.Button buttonSendData;
        private System.Windows.Forms.CheckBox checkBoxWaldosEnabled;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonClientExit;
        private System.Windows.Forms.Button buttonClientMark;
        private System.Windows.Forms.CheckBox checkBoxTestStepper0;
        private System.Windows.Forms.CheckBox checkBoxTestStepperDir;
        private System.Windows.Forms.GroupBox groupBoxTestRect;
        private System.Windows.Forms.RadioButton radioButtonLoc4;
        private System.Windows.Forms.RadioButton radioButtonLoc3;
        private System.Windows.Forms.RadioButton radioButtonLoc2;
        private System.Windows.Forms.RadioButton radioButtonLoc1;
        private System.Windows.Forms.RadioButton radioButtonLocNone;
        private System.Windows.Forms.CheckBox checkBoxDrawImageOverlay;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxPWMAEnable;
        private System.Windows.Forms.CheckBox checkBoxPWMADir;
        private System.Windows.Forms.TextBox textBoxPWMASpeed;
        private System.Windows.Forms.CheckBox checkBoxPWMBDir;
        private System.Windows.Forms.TextBox textBoxPWMBSpeed;
        private System.Windows.Forms.CheckBox checkBoxPWMBEnable;
        private System.Windows.Forms.GroupBox groupBoxAction;
        private System.Windows.Forms.RadioButton radioButtonRedToGreen;
        private System.Windows.Forms.CheckBox checkBoxActivate;
        private System.Windows.Forms.RadioButton radioButtonPathFollow;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.CheckBox checkBoxFindGreen;
        private System.Windows.Forms.CheckBox checkBoxMakeTargetTransparent;
        private System.Windows.Forms.GroupBox groupBoxDrawGreenCircle;
        private System.Windows.Forms.TextBox textBoxGreenCircleXY;
        private System.Windows.Forms.Button buttonDrawCircle;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxGreenCircleRadius;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxGreenCircleLineThickness;
        private ctlTransparentControl ctlTransparentControl1;
    }
}

