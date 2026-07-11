
namespace Wit.Example_BWT901BLE
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            // ── 实例化所有控件 ──
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.groupBoxSensorData = new System.Windows.Forms.GroupBox();
            this.dataRichTextBox = new System.Windows.Forms.RichTextBox();
            this.groupBoxCameraLog = new System.Windows.Forms.GroupBox();
            this.cameraLogRichTextBox = new System.Windows.Forms.RichTextBox();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.sensorStatusLight = new System.Windows.Forms.Panel();
            this.sensorLabel = new System.Windows.Forms.Label();
            this.sensorNameTextBox = new System.Windows.Forms.TextBox();
            this.sensorConnectButton = new System.Windows.Forms.Button();
            this.cameraLabel = new System.Windows.Forms.Label();
            this.cameraStatusLight = new System.Windows.Forms.Panel();
            this.cameraIpTextBox = new System.Windows.Forms.TextBox();
            this.cameraConnectButton = new System.Windows.Forms.Button();
            this.groupBoxSampling = new System.Windows.Forms.GroupBox();
            this.imuSamplingButton = new System.Windows.Forms.Button();
            this.cameraSamplingButton = new System.Windows.Forms.Button();
            this.logStatusLabel = new System.Windows.Forms.Label();
            this.logCountLabel = new System.Windows.Forms.Label();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.imuSettingsHeaderLabel = new System.Windows.Forms.Label();
            this.returnRateLabel = new System.Windows.Forms.Label();
            this.returnRateComboBox = new System.Windows.Forms.ComboBox();
            this.bandWidthLabel = new System.Windows.Forms.Label();
            this.bandWidthComboBox = new System.Windows.Forms.ComboBox();
            this.chipTimeCalibrationButton = new System.Windows.Forms.Button();
            this.appliedCalibrationButton = new System.Windows.Forms.Button();
            this.cameraSettingsHeaderLabel = new System.Windows.Forms.Label();
            this.captureIntervalLabel = new System.Windows.Forms.Label();
            this.captureIntervalTextBox = new System.Windows.Forms.TextBox();
            this.saveDirectoryLabel = new System.Windows.Forms.Label();
            this.saveDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.browseSaveDirButton = new System.Windows.Forms.Button();
            this.baseFileNameLabel = new System.Windows.Forms.Label();
            this.baseFileNameTextBox = new System.Windows.Forms.TextBox();
            this.showPreviewButton = new System.Windows.Forms.Button();
            this.groupBoxCalibration = new System.Windows.Forms.GroupBox();
            this.magCalibrationButton = new System.Windows.Forms.Button();
            this.spacerPanel = new System.Windows.Forms.Panel();

            // ── 挂起布局 ──
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.groupBoxSensorData.SuspendLayout();
            this.groupBoxCameraLog.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.groupBoxConnection.SuspendLayout();
            this.groupBoxSampling.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.groupBoxCalibration.SuspendLayout();
            this.SuspendLayout();

            // ================================================================
            // mainSplitContainer (左右可拖拽分割)
            // ================================================================
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.mainSplitContainer.Size = new System.Drawing.Size(1024, 600);
            this.mainSplitContainer.SplitterDistance = 260;
            this.mainSplitContainer.TabIndex = 2;

            // ── mainSplitContainer.Panel1: 左侧面板 ──
            this.mainSplitContainer.Panel1.Controls.Add(this.leftPanel);
            this.mainSplitContainer.Panel1MinSize = 200;

            // ── mainSplitContainer.Panel2: 右侧面板 ──
            this.mainSplitContainer.Panel2.Controls.Add(this.splitContainer);

            // ================================================================
            // splitContainer (右侧上下分割: 传感器数据 / 相机日志)
            // ================================================================
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(240, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer.Size = new System.Drawing.Size(784, 600);
            this.splitContainer.SplitterDistance = 250;
            this.splitContainer.TabIndex = 1;

            // ── splitContainer.Panel1: 传感器数据 ──
            this.splitContainer.Panel1.Controls.Add(this.groupBoxSensorData);

            // ── splitContainer.Panel2: 相机拍照日志 ──
            this.splitContainer.Panel2.Controls.Add(this.groupBoxCameraLog);

            // ================================================================
            // groupBoxSensorData
            // ================================================================
            this.groupBoxSensorData.Controls.Add(this.dataRichTextBox);
            this.groupBoxSensorData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSensorData.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSensorData.Name = "groupBoxSensorData";
            this.groupBoxSensorData.Size = new System.Drawing.Size(784, 250);
            this.groupBoxSensorData.TabIndex = 0;
            this.groupBoxSensorData.TabStop = false;
            this.groupBoxSensorData.Text = "传感器数据 Sensor Data";

            // ── dataRichTextBox ──
            this.dataRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataRichTextBox.Location = new System.Drawing.Point(3, 17);
            this.dataRichTextBox.Name = "dataRichTextBox";
            this.dataRichTextBox.ReadOnly = true;
            this.dataRichTextBox.Size = new System.Drawing.Size(778, 230);
            this.dataRichTextBox.TabIndex = 0;
            this.dataRichTextBox.Text = "";

            // ================================================================
            // groupBoxCameraLog
            // ================================================================
            this.groupBoxCameraLog.Controls.Add(this.cameraLogRichTextBox);
            this.groupBoxCameraLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCameraLog.Location = new System.Drawing.Point(0, 0);
            this.groupBoxCameraLog.Name = "groupBoxCameraLog";
            this.groupBoxCameraLog.Size = new System.Drawing.Size(784, 346);
            this.groupBoxCameraLog.TabIndex = 0;
            this.groupBoxCameraLog.TabStop = false;
            this.groupBoxCameraLog.Text = "相机拍照日志 Camera Log";

            // ── cameraLogRichTextBox ──
            this.cameraLogRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraLogRichTextBox.Location = new System.Drawing.Point(3, 17);
            this.cameraLogRichTextBox.Name = "cameraLogRichTextBox";
            this.cameraLogRichTextBox.ReadOnly = true;
            this.cameraLogRichTextBox.Size = new System.Drawing.Size(778, 326);
            this.cameraLogRichTextBox.TabIndex = 0;
            this.cameraLogRichTextBox.Text = "";

            // ================================================================
            // leftPanel (Dock=Fill, 在mainSplitContainer.Panel1内)
            // ================================================================
            // Dock顺序：后添加的先Dock。Fill必须最先添加（最后Dock）
            this.leftPanel.Controls.Add(this.spacerPanel);          // Fill - 填充剩余空间
            this.leftPanel.Controls.Add(this.groupBoxSettings);     // Top - 自适应内容
            this.leftPanel.Controls.Add(this.groupBoxCalibration);  // Bottom - 底部
            this.leftPanel.Controls.Add(this.groupBoxSampling);     // Top
            this.leftPanel.Controls.Add(this.groupBoxConnection);   // Top - 最先Dock
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(260, 600);
            this.leftPanel.TabIndex = 0;

            // ================================================================
            // groupBoxConnection (Height=120)
            // ================================================================
            this.groupBoxConnection.Controls.Add(this.sensorLabel);
            this.groupBoxConnection.Controls.Add(this.sensorStatusLight);
            this.groupBoxConnection.Controls.Add(this.sensorNameTextBox);
            this.groupBoxConnection.Controls.Add(this.sensorConnectButton);
            this.groupBoxConnection.Controls.Add(this.cameraLabel);
            this.groupBoxConnection.Controls.Add(this.cameraStatusLight);
            this.groupBoxConnection.Controls.Add(this.cameraIpTextBox);
            this.groupBoxConnection.Controls.Add(this.cameraConnectButton);
            this.groupBoxConnection.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxConnection.Location = new System.Drawing.Point(0, 0);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(260, 120);
            this.groupBoxConnection.TabIndex = 0;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "连接";

            // ── sensorLabel ──
            this.sensorLabel.AutoSize = true;
            this.sensorLabel.Location = new System.Drawing.Point(10, 24);
            this.sensorLabel.Name = "sensorLabel";
            this.sensorLabel.Size = new System.Drawing.Size(44, 13);
            this.sensorLabel.TabIndex = 0;
            this.sensorLabel.Text = "传感器";

            // ── sensorStatusLight ──
            this.sensorStatusLight.BackColor = System.Drawing.Color.Gray;
            this.sensorStatusLight.Location = new System.Drawing.Point(75, 22);
            this.sensorStatusLight.Name = "sensorStatusLight";
            this.sensorStatusLight.Size = new System.Drawing.Size(16, 16);
            this.sensorStatusLight.TabIndex = 1;

            // ── sensorNameTextBox (蓝牙名称输入框，与相机IP对齐) ──
            this.sensorNameTextBox.Location = new System.Drawing.Point(10, 45);
            this.sensorNameTextBox.Name = "sensorNameTextBox";
            this.sensorNameTextBox.Size = new System.Drawing.Size(120, 20);
            this.sensorNameTextBox.TabIndex = 2;
            this.sensorNameTextBox.Text = "WT901BLE68";

            // ── sensorConnectButton (连接按钮，与相机连接对齐) ──
            this.sensorConnectButton.Location = new System.Drawing.Point(135, 43);
            this.sensorConnectButton.Name = "sensorConnectButton";
            this.sensorConnectButton.Size = new System.Drawing.Size(70, 24);
            this.sensorConnectButton.TabIndex = 3;
            this.sensorConnectButton.Text = "连接";
            this.sensorConnectButton.UseVisualStyleBackColor = true;
            this.sensorConnectButton.Click += new System.EventHandler(this.sensorConnectButton_Click);

            // ── cameraLabel ──
            this.cameraLabel.AutoSize = true;
            this.cameraLabel.Location = new System.Drawing.Point(10, 80);
            this.cameraLabel.Name = "cameraLabel";
            this.cameraLabel.Size = new System.Drawing.Size(32, 13);
            this.cameraLabel.TabIndex = 4;
            this.cameraLabel.Text = "相机";

            // ── cameraStatusLight ──
            this.cameraStatusLight.BackColor = System.Drawing.Color.Gray;
            this.cameraStatusLight.Location = new System.Drawing.Point(75, 78);
            this.cameraStatusLight.Name = "cameraStatusLight";
            this.cameraStatusLight.Size = new System.Drawing.Size(16, 16);
            this.cameraStatusLight.TabIndex = 5;

            // ── cameraIpTextBox ──
            this.cameraIpTextBox.Location = new System.Drawing.Point(10, 98);
            this.cameraIpTextBox.Name = "cameraIpTextBox";
            this.cameraIpTextBox.Size = new System.Drawing.Size(120, 20);
            this.cameraIpTextBox.TabIndex = 6;
            this.cameraIpTextBox.Text = "192.168.0.38";

            // ── cameraConnectButton ──
            this.cameraConnectButton.Location = new System.Drawing.Point(135, 96);
            this.cameraConnectButton.Name = "cameraConnectButton";
            this.cameraConnectButton.Size = new System.Drawing.Size(70, 24);
            this.cameraConnectButton.TabIndex = 7;
            this.cameraConnectButton.Text = "连接";
            this.cameraConnectButton.UseVisualStyleBackColor = true;
            this.cameraConnectButton.Click += new System.EventHandler(this.cameraConnectButton_Click);

            // ================================================================
            // groupBoxSampling (Height=90)
            // ================================================================
            this.groupBoxSampling.Controls.Add(this.imuSamplingButton);
            this.groupBoxSampling.Controls.Add(this.cameraSamplingButton);
            this.groupBoxSampling.Controls.Add(this.logStatusLabel);
            this.groupBoxSampling.Controls.Add(this.logCountLabel);
            this.groupBoxSampling.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxSampling.Location = new System.Drawing.Point(0, 120);
            this.groupBoxSampling.Name = "groupBoxSampling";
            this.groupBoxSampling.Size = new System.Drawing.Size(260, 90);
            this.groupBoxSampling.TabIndex = 1;
            this.groupBoxSampling.TabStop = false;
            this.groupBoxSampling.Text = "采样记录";

            // ── imuSamplingButton ──
            this.imuSamplingButton.Location = new System.Drawing.Point(10, 22);
            this.imuSamplingButton.Name = "imuSamplingButton";
            this.imuSamplingButton.Size = new System.Drawing.Size(100, 28);
            this.imuSamplingButton.TabIndex = 0;
            this.imuSamplingButton.Text = "IMU采样";
            this.imuSamplingButton.UseVisualStyleBackColor = false;
            this.imuSamplingButton.Click += new System.EventHandler(this.imuSamplingButton_Click);

            // ── cameraSamplingButton ──
            this.cameraSamplingButton.Location = new System.Drawing.Point(115, 22);
            this.cameraSamplingButton.Name = "cameraSamplingButton";
            this.cameraSamplingButton.Size = new System.Drawing.Size(100, 28);
            this.cameraSamplingButton.TabIndex = 1;
            this.cameraSamplingButton.Text = "相机采样";
            this.cameraSamplingButton.UseVisualStyleBackColor = false;
            this.cameraSamplingButton.Click += new System.EventHandler(this.cameraSamplingButton_Click);

            // ── logStatusLabel ──
            this.logStatusLabel.AutoSize = true;
            this.logStatusLabel.Location = new System.Drawing.Point(10, 55);
            this.logStatusLabel.Name = "logStatusLabel";
            this.logStatusLabel.Size = new System.Drawing.Size(79, 13);
            this.logStatusLabel.TabIndex = 2;
            this.logStatusLabel.Text = "状态: 未记录";

            // ── logCountLabel ──
            this.logCountLabel.AutoSize = true;
            this.logCountLabel.Location = new System.Drawing.Point(130, 55);
            this.logCountLabel.Name = "logCountLabel";
            this.logCountLabel.Size = new System.Drawing.Size(79, 13);
            this.logCountLabel.TabIndex = 3;
            this.logCountLabel.Text = "已记录: 0 条";

            // ================================================================
            // groupBoxSettings (Height=230, 校准按钮已移出)
            // ================================================================
            this.groupBoxSettings.Controls.Add(this.imuSettingsHeaderLabel);
            this.groupBoxSettings.Controls.Add(this.returnRateLabel);
            this.groupBoxSettings.Controls.Add(this.returnRateComboBox);
            this.groupBoxSettings.Controls.Add(this.bandWidthLabel);
            this.groupBoxSettings.Controls.Add(this.bandWidthComboBox);
            this.groupBoxSettings.Controls.Add(this.cameraSettingsHeaderLabel);
            this.groupBoxSettings.Controls.Add(this.captureIntervalLabel);
            this.groupBoxSettings.Controls.Add(this.captureIntervalTextBox);
            this.groupBoxSettings.Controls.Add(this.saveDirectoryLabel);
            this.groupBoxSettings.Controls.Add(this.saveDirectoryTextBox);
            this.groupBoxSettings.Controls.Add(this.browseSaveDirButton);
            this.groupBoxSettings.Controls.Add(this.baseFileNameLabel);
            this.groupBoxSettings.Controls.Add(this.baseFileNameTextBox);
            this.groupBoxSettings.Controls.Add(this.showPreviewButton);
            this.groupBoxSettings.AutoSize = true;
            this.groupBoxSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxSettings.MinimumSize = new System.Drawing.Size(0, 245);
            this.groupBoxSettings.Padding = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.groupBoxSettings.Location = new System.Drawing.Point(0, 210);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(260, 280);
            this.groupBoxSettings.TabIndex = 2;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "设置参数";

            // ── imuSettingsHeaderLabel ──
            this.imuSettingsHeaderLabel.AutoSize = true;
            this.imuSettingsHeaderLabel.Location = new System.Drawing.Point(10, 20);
            this.imuSettingsHeaderLabel.Name = "imuSettingsHeaderLabel";
            this.imuSettingsHeaderLabel.Size = new System.Drawing.Size(79, 13);
            this.imuSettingsHeaderLabel.TabIndex = 0;
            this.imuSettingsHeaderLabel.Text = "IMU设置";

            // ── returnRateLabel ──
            this.returnRateLabel.AutoSize = true;
            this.returnRateLabel.Location = new System.Drawing.Point(10, 40);
            this.returnRateLabel.Name = "returnRateLabel";
            this.returnRateLabel.Size = new System.Drawing.Size(79, 13);
            this.returnRateLabel.TabIndex = 1;
            this.returnRateLabel.Text = "回传速率(Hz)";

            // ── returnRateComboBox (选择即设置) ──
            this.returnRateComboBox.FormattingEnabled = true;
            this.returnRateComboBox.Items.AddRange(new object[] { "50", "10" });
            this.returnRateComboBox.Location = new System.Drawing.Point(90, 37);
            this.returnRateComboBox.Name = "returnRateComboBox";
            this.returnRateComboBox.Size = new System.Drawing.Size(120, 21);
            this.returnRateComboBox.TabIndex = 2;
            this.returnRateComboBox.Text = "50";
            this.returnRateComboBox.SelectionChangeCommitted += new System.EventHandler(this.returnRateComboBox_SelectionChangeCommitted);

            // ── bandWidthLabel ──
            this.bandWidthLabel.AutoSize = true;
            this.bandWidthLabel.Location = new System.Drawing.Point(10, 65);
            this.bandWidthLabel.Name = "bandWidthLabel";
            this.bandWidthLabel.Size = new System.Drawing.Size(55, 13);
            this.bandWidthLabel.TabIndex = 4;
            this.bandWidthLabel.Text = "带宽(Hz)";

            // ── bandWidthComboBox (选择即设置) ──
            this.bandWidthComboBox.FormattingEnabled = true;
            this.bandWidthComboBox.Items.AddRange(new object[] { "20", "256" });
            this.bandWidthComboBox.Location = new System.Drawing.Point(90, 62);
            this.bandWidthComboBox.Name = "bandWidthComboBox";
            this.bandWidthComboBox.Size = new System.Drawing.Size(120, 21);
            this.bandWidthComboBox.TabIndex = 5;
            this.bandWidthComboBox.Text = "20";
            this.bandWidthComboBox.SelectionChangeCommitted += new System.EventHandler(this.bandWidthComboBox_SelectionChangeCommitted);

            // ── cameraSettingsHeaderLabel ──
            this.cameraSettingsHeaderLabel.AutoSize = true;
            this.cameraSettingsHeaderLabel.Location = new System.Drawing.Point(10, 95);
            this.cameraSettingsHeaderLabel.Name = "cameraSettingsHeaderLabel";
            this.cameraSettingsHeaderLabel.Size = new System.Drawing.Size(79, 13);
            this.cameraSettingsHeaderLabel.TabIndex = 9;
            this.cameraSettingsHeaderLabel.Text = "相机设置";

            // ── captureIntervalLabel ──
            this.captureIntervalLabel.AutoSize = true;
            this.captureIntervalLabel.Location = new System.Drawing.Point(10, 115);
            this.captureIntervalLabel.Name = "captureIntervalLabel";
            this.captureIntervalLabel.Size = new System.Drawing.Size(67, 13);
            this.captureIntervalLabel.TabIndex = 13;
            this.captureIntervalLabel.Text = "拍照间隔(秒)";

            // ── captureIntervalTextBox ──
            this.captureIntervalTextBox.Location = new System.Drawing.Point(100, 112);
            this.captureIntervalTextBox.Name = "captureIntervalTextBox";
            this.captureIntervalTextBox.Size = new System.Drawing.Size(60, 20);
            this.captureIntervalTextBox.TabIndex = 14;
            this.captureIntervalTextBox.Text = "5";

            // ── saveDirectoryLabel ──
            this.saveDirectoryLabel.AutoSize = true;
            this.saveDirectoryLabel.Location = new System.Drawing.Point(10, 140);
            this.saveDirectoryLabel.Name = "saveDirectoryLabel";
            this.saveDirectoryLabel.Size = new System.Drawing.Size(55, 13);
            this.saveDirectoryLabel.TabIndex = 15;
            this.saveDirectoryLabel.Text = "保存目录";

            // ── saveDirectoryTextBox ──
            this.saveDirectoryTextBox.Location = new System.Drawing.Point(10, 157);
            this.saveDirectoryTextBox.Name = "saveDirectoryTextBox";
            this.saveDirectoryTextBox.Size = new System.Drawing.Size(155, 20);
            this.saveDirectoryTextBox.TabIndex = 16;

            // ── browseSaveDirButton ──
            this.browseSaveDirButton.Location = new System.Drawing.Point(170, 155);
            this.browseSaveDirButton.Name = "browseSaveDirButton";
            this.browseSaveDirButton.Size = new System.Drawing.Size(40, 24);
            this.browseSaveDirButton.TabIndex = 17;
            this.browseSaveDirButton.Text = "...";
            this.browseSaveDirButton.UseVisualStyleBackColor = true;
            this.browseSaveDirButton.Click += new System.EventHandler(this.browseSaveDirButton_Click);

            // ── baseFileNameLabel ──
            this.baseFileNameLabel.AutoSize = true;
            this.baseFileNameLabel.Location = new System.Drawing.Point(10, 184);
            this.baseFileNameLabel.Name = "baseFileNameLabel";
            this.baseFileNameLabel.Size = new System.Drawing.Size(67, 13);
            this.baseFileNameLabel.TabIndex = 18;
            this.baseFileNameLabel.Text = "基准文件名";

            // ── baseFileNameTextBox ──
            this.baseFileNameTextBox.Location = new System.Drawing.Point(90, 181);
            this.baseFileNameTextBox.Name = "baseFileNameTextBox";
            this.baseFileNameTextBox.Size = new System.Drawing.Size(120, 20);
            this.baseFileNameTextBox.TabIndex = 19;
            this.baseFileNameTextBox.Text = "photo";

            // ── showPreviewButton ──
            this.showPreviewButton.Location = new System.Drawing.Point(10, 210);
            this.showPreviewButton.Name = "showPreviewButton";
            this.showPreviewButton.Size = new System.Drawing.Size(200, 25);
            this.showPreviewButton.TabIndex = 20;
            this.showPreviewButton.Text = "相机预览";
            this.showPreviewButton.UseVisualStyleBackColor = true;
            this.showPreviewButton.Click += new System.EventHandler(this.showPreviewButton_Click);

            // ================================================================
            // groupBoxCalibration (Height=120, 包含三个校准按钮)
            // ================================================================
            this.groupBoxCalibration.Controls.Add(this.magCalibrationButton);
            this.groupBoxCalibration.Controls.Add(this.chipTimeCalibrationButton);
            this.groupBoxCalibration.Controls.Add(this.appliedCalibrationButton);
            this.groupBoxCalibration.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBoxCalibration.Location = new System.Drawing.Point(0, 440);
            this.groupBoxCalibration.Name = "groupBoxCalibration";
            this.groupBoxCalibration.Size = new System.Drawing.Size(260, 120);
            this.groupBoxCalibration.TabIndex = 3;
            this.groupBoxCalibration.TabStop = false;
            this.groupBoxCalibration.Text = "校准";

            // ── magCalibrationButton ──
            this.magCalibrationButton.Location = new System.Drawing.Point(10, 22);
            this.magCalibrationButton.Name = "magCalibrationButton";
            this.magCalibrationButton.Size = new System.Drawing.Size(220, 28);
            this.magCalibrationButton.TabIndex = 0;
            this.magCalibrationButton.Text = "磁场校准";
            this.magCalibrationButton.UseVisualStyleBackColor = false;
            this.magCalibrationButton.Click += new System.EventHandler(this.magCalibrationButton_Click);

            // ── chipTimeCalibrationButton ──
            this.chipTimeCalibrationButton.Location = new System.Drawing.Point(10, 55);
            this.chipTimeCalibrationButton.Name = "chipTimeCalibrationButton";
            this.chipTimeCalibrationButton.Size = new System.Drawing.Size(220, 25);
            this.chipTimeCalibrationButton.TabIndex = 1;
            this.chipTimeCalibrationButton.Text = "ChipTime 校准";
            this.chipTimeCalibrationButton.UseVisualStyleBackColor = true;
            this.chipTimeCalibrationButton.Click += new System.EventHandler(this.chipTimeCalibrationButton_Click);

            // ── appliedCalibrationButton ──
            this.appliedCalibrationButton.Location = new System.Drawing.Point(10, 85);
            this.appliedCalibrationButton.Name = "appliedCalibrationButton";
            this.appliedCalibrationButton.Size = new System.Drawing.Size(220, 25);
            this.appliedCalibrationButton.TabIndex = 2;
            this.appliedCalibrationButton.Text = "加计校准";
            this.appliedCalibrationButton.UseVisualStyleBackColor = true;
            this.appliedCalibrationButton.Click += new System.EventHandler(this.appliedCalibrationButton_Click);

            // ================================================================
            // spacerPanel (填充GroupBox设置下方的剩余空间)
            // ================================================================
            this.spacerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spacerPanel.Location = new System.Drawing.Point(0, 460);
            this.spacerPanel.Name = "spacerPanel";
            this.spacerPanel.Size = new System.Drawing.Size(260, 0);
            this.spacerPanel.TabIndex = 4;

            // ================================================================
            // GroupBox 边框加粗美化（Paint事件绘制）
            // ================================================================
            this.groupBoxConnection.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);
            this.groupBoxSampling.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);
            this.groupBoxSettings.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);
            this.groupBoxCalibration.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);

            // ================================================================
            // Form1
            // ================================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.mainSplitContainer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "调平对中数字调节仪";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);

            // ── 恢复布局 ──
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.groupBoxSensorData.ResumeLayout(false);
            this.groupBoxCameraLog.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.groupBoxConnection.ResumeLayout(false);
            this.groupBoxConnection.PerformLayout();
            this.groupBoxSampling.ResumeLayout(false);
            this.groupBoxSampling.PerformLayout();
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.groupBoxCalibration.ResumeLayout(false);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.GroupBox groupBoxSensorData;
        private System.Windows.Forms.RichTextBox dataRichTextBox;
        private System.Windows.Forms.GroupBox groupBoxCameraLog;
        private System.Windows.Forms.RichTextBox cameraLogRichTextBox;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.Label sensorLabel;
        private System.Windows.Forms.Panel sensorStatusLight;
        private System.Windows.Forms.TextBox sensorNameTextBox;
        private System.Windows.Forms.Button sensorConnectButton;
        private System.Windows.Forms.Label cameraLabel;
        private System.Windows.Forms.Panel cameraStatusLight;
        private System.Windows.Forms.TextBox cameraIpTextBox;
        private System.Windows.Forms.Button cameraConnectButton;
        private System.Windows.Forms.GroupBox groupBoxSampling;
        private System.Windows.Forms.Button imuSamplingButton;
        private System.Windows.Forms.Button cameraSamplingButton;
        private System.Windows.Forms.Label logStatusLabel;
        private System.Windows.Forms.Label logCountLabel;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.Label imuSettingsHeaderLabel;
        private System.Windows.Forms.Label returnRateLabel;
        private System.Windows.Forms.ComboBox returnRateComboBox;
        private System.Windows.Forms.Label bandWidthLabel;
        private System.Windows.Forms.ComboBox bandWidthComboBox;
        private System.Windows.Forms.Button chipTimeCalibrationButton;
        private System.Windows.Forms.Button appliedCalibrationButton;
        private System.Windows.Forms.Label cameraSettingsHeaderLabel;
        private System.Windows.Forms.Label captureIntervalLabel;
        private System.Windows.Forms.TextBox captureIntervalTextBox;
        private System.Windows.Forms.Label saveDirectoryLabel;
        private System.Windows.Forms.TextBox saveDirectoryTextBox;
        private System.Windows.Forms.Button browseSaveDirButton;
        private System.Windows.Forms.Label baseFileNameLabel;
        private System.Windows.Forms.TextBox baseFileNameTextBox;
        private System.Windows.Forms.Button showPreviewButton;
        private System.Windows.Forms.GroupBox groupBoxCalibration;
        private System.Windows.Forms.Button magCalibrationButton;
        private System.Windows.Forms.Panel spacerPanel;
    }
}
