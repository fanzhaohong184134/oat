
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
            this.cameraCalibButton = new System.Windows.Forms.Button();
            this.instrumentCalibButton = new System.Windows.Forms.Button();
            this.mountingCalibButton = new System.Windows.Forms.Button();
            this.groupBoxDataProcessing = new System.Windows.Forms.GroupBox();
            this.imuCsvLabel = new System.Windows.Forms.Label();
            this.imuCsvTextBox = new System.Windows.Forms.TextBox();
            this.browseImuCsvButton = new System.Windows.Forms.Button();
            this.cameraCsvLabel = new System.Windows.Forms.Label();
            this.cameraCsvTextBox = new System.Windows.Forms.TextBox();
            this.browseCameraCsvButton = new System.Windows.Forms.Button();
            this.processButton = new System.Windows.Forms.Button();
            this.reportLabel = new System.Windows.Forms.Label();
            this.leftTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.versionLabel = new System.Windows.Forms.Label();

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
            this.leftTableLayout.SuspendLayout();
            this.groupBoxConnection.SuspendLayout();
            this.groupBoxSampling.SuspendLayout();
            this.groupBoxSettings.SuspendLayout();
            this.groupBoxCalibration.SuspendLayout();
            this.groupBoxDataProcessing.SuspendLayout();
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
            // leftTableLayout (按比例分配4个GroupBox的垂直空间)
            // ================================================================
            this.leftTableLayout.ColumnCount = 1;
            this.leftTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.leftTableLayout.RowCount = 5;
            this.leftTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16F));
            this.leftTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11F));
            this.leftTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.leftTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.leftTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.leftTableLayout.Controls.Add(this.groupBoxConnection, 0, 0);
            this.leftTableLayout.Controls.Add(this.groupBoxSampling, 0, 1);
            this.leftTableLayout.Controls.Add(this.groupBoxSettings, 0, 2);
            this.leftTableLayout.Controls.Add(this.groupBoxCalibration, 0, 3);
            this.leftTableLayout.Controls.Add(this.groupBoxDataProcessing, 0, 4);
            this.leftTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftTableLayout.Location = new System.Drawing.Point(0, 0);
            this.leftTableLayout.Name = "leftTableLayout";
            this.leftTableLayout.Size = new System.Drawing.Size(260, 600);
            this.leftTableLayout.TabIndex = 0;

            // ================================================================
            // leftPanel (Dock=Fill, 在mainSplitContainer.Panel1内)
            // ================================================================
            this.leftPanel.Controls.Add(this.leftTableLayout);
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
            this.groupBoxConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxConnection.Location = new System.Drawing.Point(0, 0);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(260, 120);
            this.groupBoxConnection.TabIndex = 0;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "连接";

            // ── sensorLabel ──
            this.sensorLabel.AutoSize = true;
            this.sensorLabel.Location = new System.Drawing.Point(10, 20);
            this.sensorLabel.Name = "sensorLabel";
            this.sensorLabel.Size = new System.Drawing.Size(44, 13);
            this.sensorLabel.TabIndex = 0;
            this.sensorLabel.Text = "传感器";

            // ── sensorStatusLight ──
            this.sensorStatusLight.BackColor = System.Drawing.Color.Gray;
            this.sensorStatusLight.Location = new System.Drawing.Point(75, 20);
            this.sensorStatusLight.Name = "sensorStatusLight";
            this.sensorStatusLight.Size = new System.Drawing.Size(16, 16);
            this.sensorStatusLight.TabIndex = 1;

            // ── sensorNameTextBox (蓝牙名称输入框，与相机IP对齐) ──
            this.sensorNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.sensorNameTextBox.Location = new System.Drawing.Point(10, 44);
            this.sensorNameTextBox.Name = "sensorNameTextBox";
            this.sensorNameTextBox.Size = new System.Drawing.Size(120, 20);
            this.sensorNameTextBox.TabIndex = 2;
            this.sensorNameTextBox.Text = "WT901BLE68";

            // ── sensorConnectButton (连接按钮，与相机连接对齐) ──
            this.sensorConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sensorConnectButton.Location = new System.Drawing.Point(135, 43);
            this.sensorConnectButton.Name = "sensorConnectButton";
            this.sensorConnectButton.Size = new System.Drawing.Size(70, 24);
            this.sensorConnectButton.TabIndex = 3;
            this.sensorConnectButton.Text = "连接";
            this.sensorConnectButton.UseVisualStyleBackColor = true;
            this.sensorConnectButton.Click += new System.EventHandler(this.sensorConnectButton_Click);

            // ── cameraLabel ──
            this.cameraLabel.AutoSize = true;
            this.cameraLabel.Location = new System.Drawing.Point(10, 74);
            this.cameraLabel.Name = "cameraLabel";
            this.cameraLabel.Size = new System.Drawing.Size(32, 13);
            this.cameraLabel.TabIndex = 4;
            this.cameraLabel.Text = "相机";

            // ── cameraStatusLight ──
            this.cameraStatusLight.BackColor = System.Drawing.Color.Gray;
            this.cameraStatusLight.Location = new System.Drawing.Point(75, 74);
            this.cameraStatusLight.Name = "cameraStatusLight";
            this.cameraStatusLight.Size = new System.Drawing.Size(16, 16);
            this.cameraStatusLight.TabIndex = 5;

            // ── cameraIpTextBox ──
            this.cameraIpTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraIpTextBox.Location = new System.Drawing.Point(10, 98);
            this.cameraIpTextBox.Name = "cameraIpTextBox";
            this.cameraIpTextBox.Size = new System.Drawing.Size(120, 20);
            this.cameraIpTextBox.TabIndex = 6;
            this.cameraIpTextBox.Text = "192.168.0.38";

            // ── cameraConnectButton ──
            this.cameraConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraConnectButton.Location = new System.Drawing.Point(135, 97);
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
            this.groupBoxSampling.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSampling.Location = new System.Drawing.Point(0, 120);
            this.groupBoxSampling.Name = "groupBoxSampling";
            this.groupBoxSampling.Size = new System.Drawing.Size(260, 90);
            this.groupBoxSampling.TabIndex = 1;
            this.groupBoxSampling.TabStop = false;
            this.groupBoxSampling.Text = "采样记录";

            // ── imuSamplingButton ──
            this.imuSamplingButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.imuSamplingButton.Location = new System.Drawing.Point(10, 24);
            this.imuSamplingButton.Name = "imuSamplingButton";
            this.imuSamplingButton.Size = new System.Drawing.Size(100, 28);
            this.imuSamplingButton.TabIndex = 0;
            this.imuSamplingButton.Text = "IMU采样";
            this.imuSamplingButton.UseVisualStyleBackColor = false;
            this.imuSamplingButton.Click += new System.EventHandler(this.imuSamplingButton_Click);

            // ── cameraSamplingButton ──
            this.cameraSamplingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraSamplingButton.Location = new System.Drawing.Point(115, 24);
            this.cameraSamplingButton.Name = "cameraSamplingButton";
            this.cameraSamplingButton.Size = new System.Drawing.Size(100, 28);
            this.cameraSamplingButton.TabIndex = 1;
            this.cameraSamplingButton.Text = "相机采样";
            this.cameraSamplingButton.UseVisualStyleBackColor = false;
            this.cameraSamplingButton.Click += new System.EventHandler(this.cameraSamplingButton_Click);

            // ── logStatusLabel ──
            this.logStatusLabel.AutoSize = true;
            this.logStatusLabel.Location = new System.Drawing.Point(10, 60);
            this.logStatusLabel.Name = "logStatusLabel";
            this.logStatusLabel.Size = new System.Drawing.Size(79, 13);
            this.logStatusLabel.TabIndex = 2;
            this.logStatusLabel.Text = "状态: 未记录";

            // ── logCountLabel ──
            this.logCountLabel.AutoSize = true;
            this.logCountLabel.Location = new System.Drawing.Point(130, 60);
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
            this.groupBoxSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxSettings.Location = new System.Drawing.Point(0, 210);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(260, 240);
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
            this.returnRateLabel.Location = new System.Drawing.Point(10, 42);
            this.returnRateLabel.Name = "returnRateLabel";
            this.returnRateLabel.Size = new System.Drawing.Size(79, 13);
            this.returnRateLabel.TabIndex = 1;
            this.returnRateLabel.Text = "回传速率(Hz)";

            // ── returnRateComboBox (选择即设置) ──
            this.returnRateComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.returnRateComboBox.FormattingEnabled = true;
            this.returnRateComboBox.Items.AddRange(new object[] { "50", "10" });
            this.returnRateComboBox.Location = new System.Drawing.Point(90, 40);
            this.returnRateComboBox.Name = "returnRateComboBox";
            this.returnRateComboBox.Size = new System.Drawing.Size(120, 21);
            this.returnRateComboBox.TabIndex = 2;
            this.returnRateComboBox.Text = "50";
            this.returnRateComboBox.SelectionChangeCommitted += new System.EventHandler(this.returnRateComboBox_SelectionChangeCommitted);

            // ── bandWidthLabel ──
            this.bandWidthLabel.AutoSize = true;
            this.bandWidthLabel.Location = new System.Drawing.Point(10, 66);
            this.bandWidthLabel.Name = "bandWidthLabel";
            this.bandWidthLabel.Size = new System.Drawing.Size(55, 13);
            this.bandWidthLabel.TabIndex = 4;
            this.bandWidthLabel.Text = "带宽(Hz)";

            // ── bandWidthComboBox (选择即设置) ──
            this.bandWidthComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.bandWidthComboBox.FormattingEnabled = true;
            this.bandWidthComboBox.Items.AddRange(new object[] { "20", "256" });
            this.bandWidthComboBox.Location = new System.Drawing.Point(90, 64);
            this.bandWidthComboBox.Name = "bandWidthComboBox";
            this.bandWidthComboBox.Size = new System.Drawing.Size(120, 21);
            this.bandWidthComboBox.TabIndex = 5;
            this.bandWidthComboBox.Text = "20";
            this.bandWidthComboBox.SelectionChangeCommitted += new System.EventHandler(this.bandWidthComboBox_SelectionChangeCommitted);

            // ── cameraSettingsHeaderLabel ──
            this.cameraSettingsHeaderLabel.AutoSize = true;
            this.cameraSettingsHeaderLabel.Location = new System.Drawing.Point(10, 94);
            this.cameraSettingsHeaderLabel.Name = "cameraSettingsHeaderLabel";
            this.cameraSettingsHeaderLabel.Size = new System.Drawing.Size(79, 13);
            this.cameraSettingsHeaderLabel.TabIndex = 9;
            this.cameraSettingsHeaderLabel.Text = "相机设置";

            // ── captureIntervalLabel ──
            this.captureIntervalLabel.AutoSize = true;
            this.captureIntervalLabel.Location = new System.Drawing.Point(10, 116);
            this.captureIntervalLabel.Name = "captureIntervalLabel";
            this.captureIntervalLabel.Size = new System.Drawing.Size(67, 13);
            this.captureIntervalLabel.TabIndex = 13;
            this.captureIntervalLabel.Text = "拍照间隔(秒)";

            // ── captureIntervalTextBox ──
            this.captureIntervalTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.captureIntervalTextBox.Location = new System.Drawing.Point(100, 114);
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
            this.saveDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.saveDirectoryTextBox.Location = new System.Drawing.Point(10, 158);
            this.saveDirectoryTextBox.Name = "saveDirectoryTextBox";
            this.saveDirectoryTextBox.Size = new System.Drawing.Size(155, 20);
            this.saveDirectoryTextBox.TabIndex = 16;

            // ── browseSaveDirButton ──
            this.browseSaveDirButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseSaveDirButton.Location = new System.Drawing.Point(170, 156);
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
            this.baseFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.baseFileNameTextBox.Location = new System.Drawing.Point(90, 182);
            this.baseFileNameTextBox.Name = "baseFileNameTextBox";
            this.baseFileNameTextBox.Size = new System.Drawing.Size(120, 20);
            this.baseFileNameTextBox.TabIndex = 19;
            this.baseFileNameTextBox.Text = "photo";

            // ── showPreviewButton ──
            this.showPreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.showPreviewButton.Location = new System.Drawing.Point(10, 208);
            this.showPreviewButton.Name = "showPreviewButton";
            this.showPreviewButton.Size = new System.Drawing.Size(200, 25);
            this.showPreviewButton.TabIndex = 20;
            this.showPreviewButton.Text = "相机预览";
            this.showPreviewButton.UseVisualStyleBackColor = true;
            this.showPreviewButton.Click += new System.EventHandler(this.showPreviewButton_Click);

            // ================================================================
            // groupBoxCalibration (包含校准按钮)
            // ================================================================
            this.groupBoxCalibration.Controls.Add(this.magCalibrationButton);
            this.groupBoxCalibration.Controls.Add(this.chipTimeCalibrationButton);
            this.groupBoxCalibration.Controls.Add(this.appliedCalibrationButton);
            this.groupBoxCalibration.Controls.Add(this.cameraCalibButton);
            this.groupBoxCalibration.Controls.Add(this.instrumentCalibButton);
            this.groupBoxCalibration.Controls.Add(this.mountingCalibButton);
            this.groupBoxCalibration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCalibration.Location = new System.Drawing.Point(0, 440);
            this.groupBoxCalibration.Name = "groupBoxCalibration";
            this.groupBoxCalibration.Size = new System.Drawing.Size(260, 120);
            this.groupBoxCalibration.TabIndex = 3;
            this.groupBoxCalibration.TabStop = false;
            this.groupBoxCalibration.Text = "校准";

            // ── magCalibrationButton ──
            this.magCalibrationButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.magCalibrationButton.Location = new System.Drawing.Point(10, 20);
            this.magCalibrationButton.Name = "magCalibrationButton";
            this.magCalibrationButton.Size = new System.Drawing.Size(105, 25);
            this.magCalibrationButton.TabIndex = 0;
            this.magCalibrationButton.Text = "磁场校准";
            this.magCalibrationButton.UseVisualStyleBackColor = false;
            this.magCalibrationButton.Click += new System.EventHandler(this.magCalibrationButton_Click);

            // ── chipTimeCalibrationButton ──
            this.chipTimeCalibrationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chipTimeCalibrationButton.Location = new System.Drawing.Point(120, 20);
            this.chipTimeCalibrationButton.Name = "chipTimeCalibrationButton";
            this.chipTimeCalibrationButton.Size = new System.Drawing.Size(105, 25);
            this.chipTimeCalibrationButton.TabIndex = 1;
            this.chipTimeCalibrationButton.Text = "ChipTime校准";
            this.chipTimeCalibrationButton.UseVisualStyleBackColor = true;
            this.chipTimeCalibrationButton.Click += new System.EventHandler(this.chipTimeCalibrationButton_Click);

            // ── appliedCalibrationButton ──
            this.appliedCalibrationButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.appliedCalibrationButton.Location = new System.Drawing.Point(10, 50);
            this.appliedCalibrationButton.Name = "appliedCalibrationButton";
            this.appliedCalibrationButton.Size = new System.Drawing.Size(105, 25);
            this.appliedCalibrationButton.TabIndex = 2;
            this.appliedCalibrationButton.Text = "加计校准";
            this.appliedCalibrationButton.UseVisualStyleBackColor = true;
            this.appliedCalibrationButton.Click += new System.EventHandler(this.appliedCalibrationButton_Click);

            // ── cameraCalibButton ──
            this.cameraCalibButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraCalibButton.Location = new System.Drawing.Point(120, 50);
            this.cameraCalibButton.Name = "cameraCalibButton";
            this.cameraCalibButton.Size = new System.Drawing.Size(105, 25);
            this.cameraCalibButton.TabIndex = 3;
            this.cameraCalibButton.Text = "相机标定";
            this.cameraCalibButton.UseVisualStyleBackColor = true;
            this.cameraCalibButton.Click += new System.EventHandler(this.cameraCalibButton_Click);

            // ── instrumentCalibButton ──
            this.instrumentCalibButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.instrumentCalibButton.Location = new System.Drawing.Point(10, 80);
            this.instrumentCalibButton.Name = "instrumentCalibButton";
            this.instrumentCalibButton.Size = new System.Drawing.Size(105, 25);
            this.instrumentCalibButton.TabIndex = 4;
            this.instrumentCalibButton.Text = "仪器标定";
            this.instrumentCalibButton.UseVisualStyleBackColor = true;
            this.instrumentCalibButton.Click += new System.EventHandler(this.instrumentCalibButton_Click);

            // ── mountingCalibButton ──
            this.mountingCalibButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mountingCalibButton.Location = new System.Drawing.Point(120, 80);
            this.mountingCalibButton.Name = "mountingCalibButton";
            this.mountingCalibButton.Size = new System.Drawing.Size(105, 25);
            this.mountingCalibButton.TabIndex = 5;
            this.mountingCalibButton.Text = "安装角标定";
            this.mountingCalibButton.UseVisualStyleBackColor = true;
            this.mountingCalibButton.Click += new System.EventHandler(this.mountingCalibButton_Click);

            // ================================================================
            // groupBoxDataProcessing (数据处理)
            // ================================================================
            this.groupBoxDataProcessing.Controls.Add(this.imuCsvLabel);
            this.groupBoxDataProcessing.Controls.Add(this.imuCsvTextBox);
            this.groupBoxDataProcessing.Controls.Add(this.browseImuCsvButton);
            this.groupBoxDataProcessing.Controls.Add(this.cameraCsvLabel);
            this.groupBoxDataProcessing.Controls.Add(this.cameraCsvTextBox);
            this.groupBoxDataProcessing.Controls.Add(this.browseCameraCsvButton);
            this.groupBoxDataProcessing.Controls.Add(this.processButton);
            this.groupBoxDataProcessing.Controls.Add(this.reportLabel);
            this.groupBoxDataProcessing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDataProcessing.Location = new System.Drawing.Point(0, 560);
            this.groupBoxDataProcessing.Name = "groupBoxDataProcessing";
            this.groupBoxDataProcessing.Size = new System.Drawing.Size(260, 140);
            this.groupBoxDataProcessing.TabIndex = 4;
            this.groupBoxDataProcessing.TabStop = false;
            this.groupBoxDataProcessing.Text = "数据处理";

            // ── imuCsvLabel ──
            this.imuCsvLabel.AutoSize = true;
            this.imuCsvLabel.Location = new System.Drawing.Point(10, 20);
            this.imuCsvLabel.Name = "imuCsvLabel";
            this.imuCsvLabel.Size = new System.Drawing.Size(55, 13);
            this.imuCsvLabel.TabIndex = 0;
            this.imuCsvLabel.Text = "IMU CSV";

            // ── imuCsvTextBox ──
            this.imuCsvTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.imuCsvTextBox.Location = new System.Drawing.Point(70, 18);
            this.imuCsvTextBox.Name = "imuCsvTextBox";
            this.imuCsvTextBox.Size = new System.Drawing.Size(120, 20);
            this.imuCsvTextBox.TabIndex = 1;

            // ── browseImuCsvButton ──
            this.browseImuCsvButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseImuCsvButton.Location = new System.Drawing.Point(195, 17);
            this.browseImuCsvButton.Name = "browseImuCsvButton";
            this.browseImuCsvButton.Size = new System.Drawing.Size(30, 22);
            this.browseImuCsvButton.TabIndex = 2;
            this.browseImuCsvButton.Text = "...";
            this.browseImuCsvButton.UseVisualStyleBackColor = true;
            this.browseImuCsvButton.Click += new System.EventHandler(this.browseImuCsvButton_Click);

            // ── cameraCsvLabel ──
            this.cameraCsvLabel.AutoSize = true;
            this.cameraCsvLabel.Location = new System.Drawing.Point(10, 44);
            this.cameraCsvLabel.Name = "cameraCsvLabel";
            this.cameraCsvLabel.Size = new System.Drawing.Size(55, 13);
            this.cameraCsvLabel.TabIndex = 3;
            this.cameraCsvLabel.Text = "相机 CSV";

            // ── cameraCsvTextBox ──
            this.cameraCsvTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraCsvTextBox.Location = new System.Drawing.Point(70, 42);
            this.cameraCsvTextBox.Name = "cameraCsvTextBox";
            this.cameraCsvTextBox.Size = new System.Drawing.Size(120, 20);
            this.cameraCsvTextBox.TabIndex = 4;

            // ── browseCameraCsvButton ──
            this.browseCameraCsvButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseCameraCsvButton.Location = new System.Drawing.Point(195, 41);
            this.browseCameraCsvButton.Name = "browseCameraCsvButton";
            this.browseCameraCsvButton.Size = new System.Drawing.Size(30, 22);
            this.browseCameraCsvButton.TabIndex = 5;
            this.browseCameraCsvButton.Text = "...";
            this.browseCameraCsvButton.UseVisualStyleBackColor = true;
            this.browseCameraCsvButton.Click += new System.EventHandler(this.browseCameraCsvButton_Click);

            // ── processButton ──
            this.processButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.processButton.Location = new System.Drawing.Point(10, 68);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(215, 25);
            this.processButton.TabIndex = 6;
            this.processButton.Text = "开始处理";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.processButton_Click);

            // ── reportLabel ──
            this.reportLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.reportLabel.Location = new System.Drawing.Point(10, 96);
            this.reportLabel.Name = "reportLabel";
            this.reportLabel.Size = new System.Drawing.Size(215, 40);
            this.reportLabel.TabIndex = 7;
            this.reportLabel.Text = "等待处理...";
            this.reportLabel.ForeColor = System.Drawing.Color.Gray;

            // ================================================================
            // GroupBox 边框加粗美化（Paint事件绘制）
            // ================================================================
            this.groupBoxConnection.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);
            this.groupBoxSampling.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);
            this.groupBoxSettings.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);
            this.groupBoxCalibration.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);
            this.groupBoxDataProcessing.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupBoxBorderPaint);

            // ================================================================
            // versionLabel (右下角版本号)
            // ================================================================
            this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.versionLabel.AutoSize = true;
            this.versionLabel.ForeColor = System.Drawing.Color.Gray;
            this.versionLabel.Location = new System.Drawing.Point(940, 764);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(80, 13);
            this.versionLabel.TabIndex = 3;
            this.versionLabel.Text = "";

            // ================================================================
            // Form1
            // ================================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 780);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.mainSplitContainer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(800, 680);
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
            this.leftTableLayout.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.groupBoxConnection.ResumeLayout(false);
            this.groupBoxConnection.PerformLayout();
            this.groupBoxSampling.ResumeLayout(false);
            this.groupBoxSampling.PerformLayout();
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.groupBoxCalibration.ResumeLayout(false);
            this.groupBoxDataProcessing.ResumeLayout(false);
            this.groupBoxDataProcessing.PerformLayout();
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
        private System.Windows.Forms.Button cameraCalibButton;
        private System.Windows.Forms.Button instrumentCalibButton;
        private System.Windows.Forms.Button mountingCalibButton;
        private System.Windows.Forms.GroupBox groupBoxDataProcessing;
        private System.Windows.Forms.Label imuCsvLabel;
        private System.Windows.Forms.TextBox imuCsvTextBox;
        private System.Windows.Forms.Button browseImuCsvButton;
        private System.Windows.Forms.Label cameraCsvLabel;
        private System.Windows.Forms.TextBox cameraCsvTextBox;
        private System.Windows.Forms.Button browseCameraCsvButton;
        private System.Windows.Forms.Button processButton;
        private System.Windows.Forms.Label reportLabel;
        private System.Windows.Forms.TableLayoutPanel leftTableLayout;
        private System.Windows.Forms.Label versionLabel;
    }
}
