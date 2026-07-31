using System;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using dsat.DataProcessing.Calibration;

namespace dsat.CalibrationPanels
{
    public class HeadingCalibrationPanelForm : CalibrationPanelBaseForm
    {
        public delegate bool TryGetImuAnglesDelegate(out double angleX, out double angleY, out double angleZ);

        private readonly CalibrationPathService _pathService;
        private readonly TryGetImuAnglesDelegate _imuProvider;
        private readonly Label _deviceIdLabel;
        private readonly Label _imuAngleZLabel;
        private readonly Label _magDecLabel;
        private readonly Label _currentPsiOffsetLabel;
        private readonly Label _dependencyHint;
        private readonly Timer _imuTimer;

        public HeadingCalibrationPanelForm(string baseDirectory, TryGetImuAnglesDelegate imuProvider)
            : base("航向标定面板", "Step 0C 航向现场校核/必要时重标：自动采集IMU，默认磁场模式。")
        {
            _pathService = new CalibrationPathService(baseDirectory);
            _imuProvider = imuProvider;
            CalibrationConfig cfg = LoadConfigSafe(_pathService.ConfigPath);

            _deviceIdLabel = AddInfoRow("设备编号", _pathService.GetDefaultDeviceId());
            _imuAngleZLabel = AddInfoRow("IMU AngleZ", "等待传感器数据...");
            _magDecLabel = AddInfoRow("磁偏角 D", cfg.MagneticDeclination.ToString("F6", CultureInfo.InvariantCulture));
            _currentPsiOffsetLabel = AddInfoRow("当前 ψ_offset", cfg.PsiOffset.ToString("F6", CultureInfo.InvariantCulture));

            _dependencyHint = new Label
            {
                AutoSize = true,
                Text = string.Format(
                    "已读取依赖: fx={0:F2}, fy={1:F2}, cx={2:F2}, cy={3:F2}, δ_pitch={4:F3}, δ_roll={5:F3}",
                    cfg.Fx, cfg.Fy, cfg.Cx, cfg.Cy, cfg.DeltaPitch, cfg.DeltaRoll),
                Dock = DockStyle.Top
            };
            InputGrid.Controls.Add(_dependencyHint, 1, InputGrid.RowCount++);

            var note = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "说明: 本面板默认磁场模式，不需要手动输入点位与角度。",
                Dock = DockStyle.Top
            };
            InputGrid.Controls.Add(note, 1, InputGrid.RowCount++);

            _imuTimer = new Timer { Interval = 300 };
            _imuTimer.Tick += (s, e) => RefreshImuDisplay();
            _imuTimer.Start();
            FormClosed += (s, e) => _imuTimer.Dispose();

            RefreshImuDisplay();

            RunButton.Click += RunButton_Click;
        }

        private void RefreshImuDisplay()
        {
            double angleX, angleY, angleZ;
            if (_imuProvider != null && _imuProvider(out angleX, out angleY, out angleZ))
            {
                _imuAngleZLabel.Text = angleZ.ToString("F4", CultureInfo.InvariantCulture);
                SetStatus("已连接传感器，可执行标定。", true);
                RunButton.Enabled = true;
            }
            else
            {
                _imuAngleZLabel.Text = "未连接";
                SetStatus("未检测到传感器，请先连接后再执行。", false);
                RunButton.Enabled = false;
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            try
            {
                double angleX, angleY, angleZ;
                if (_imuProvider == null || !_imuProvider(out angleX, out angleY, out angleZ))
                    throw new InvalidOperationException("未检测到实时IMU数据，请先连接传感器");

                string deviceId = _pathService.EnsureAndPersistDeviceId(_pathService.GetDefaultDeviceId());
                _deviceIdLabel.Text = deviceId;
                string inputDir, outputDir;
                _pathService.EnsureCalibrationDirs(deviceId, "heading_calibration", out inputDir, out outputDir);
                CalibrationConfig cfg = LoadConfigSafe(_pathService.ConfigPath);
                _magDecLabel.Text = cfg.MagneticDeclination.ToString("F6", CultureInfo.InvariantCulture);
                _currentPsiOffsetLabel.Text = cfg.PsiOffset.ToString("F6", CultureInfo.InvariantCulture);

                var input = new HeadingCalibrationInput
                {
                    DeviceId = deviceId,
                    HasKnownReference = false,
                    ImuAngleZ = angleZ,
                    MagneticDeclination = cfg.MagneticDeclination,
                    CurrentPsiOffset = cfg.PsiOffset,
                    U1 = 0,
                    V1 = 0,
                    U2 = 0,
                    V2 = 0,
                    KnownAzimuth = 0,
                    DeltaPitch = cfg.DeltaPitch,
                    DeltaRoll = cfg.DeltaRoll,
                    Fx = cfg.Fx,
                    Fy = cfg.Fy,
                    Cx = cfg.Cx,
                    Cy = cfg.Cy
                };

                string inputPath = _pathService.CreateTimestampedFile(inputDir, "heading_input", "json");
                string outputPath = _pathService.CreateTimestampedFile(outputDir, "heading_output", "json");
                CalibrationJsonUtil.SaveToFile(input, inputPath);

                string exePath = CalibrationExecutableResolver.Resolve(
                    _pathService.BaseDirectory,
                    "InstrumentCalibration.exe",
                    Path.Combine("DataProcessing", "Calibration", "InstrumentCalibrationApp", "bin"));

                if (!File.Exists(exePath))
                    throw new FileNotFoundException("未找到 InstrumentCalibration.exe", exePath);

                string args = string.Format("--input \"{0}\" --output \"{1}\" --config \"{2}\"",
                    inputPath, outputPath, _pathService.ConfigPath);

                string stdout, stderr;
                int code = CalibrationProcessRunner.Run(exePath, args, out stdout, out stderr);
                if (code != 0)
                {
                    throw new InvalidOperationException("航向标定程序执行失败: " + stderr + "\n" + stdout);
                }

                HeadingCalibrationOutput output = CalibrationJsonUtil.LoadFromFile<HeadingCalibrationOutput>(outputPath);
                string summaryPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(outputPath) + "_summary.txt");
                ResultTextBox.Text = string.Format(
                    "执行成功: {0}\r\n模式: {1}\r\n旧 ψ_offset={2:F5}°, 新 ψ_offset={3:F5}°\r\n误差 e={4:F5}°\r\n预测方位={5:F5}°\r\n配置文件: {6}\r\n输入文件: {7}\r\n输出文件: {8}\r\n摘要文件: {9}",
                    output.Message, output.Mode,
                    output.CurrentPsiOffset, output.NewPsiOffset,
                    output.Error, output.PredictedAzimuth,
                    output.ConfigPath, inputPath, outputPath, summaryPath);

                ConfirmButton.Enabled = true;
                SetStatus("航向标定完成，确认后关闭。", true);
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = ex.ToString();
                SetStatus("执行失败: " + ex.Message, false);
                ConfirmButton.Enabled = false;
            }
        }

        private static CalibrationConfig LoadConfigSafe(string configPath)
        {
            try
            {
                if (File.Exists(configPath)) return CalibrationConfig.Load(configPath);
            }
            catch { }
            return new CalibrationConfig();
        }
    }
}

