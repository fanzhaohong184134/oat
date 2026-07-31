using System;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Wit.Example_BWT901BLE.DataProcessing.Calibration;

namespace Wit.Example_BWT901BLE.CalibrationPanels
{
    public class MountingCalibrationPanelForm : CalibrationPanelBaseForm
    {
        public delegate bool TryGetImuAnglesDelegate(out double angleX, out double angleY, out double angleZ);

        private readonly CalibrationPathService _pathService;
        private readonly TryGetImuAnglesDelegate _imuProvider;
        private readonly Label _deviceIdLabel;
        private readonly Label _angleXLabel;
        private readonly Label _angleYLabel;
        private readonly Label _pixelLabel;
        private readonly Label _cameraParamLabel;
        private readonly Timer _imuTimer;
        private readonly double _u;
        private readonly double _v;
        private readonly double _fx;
        private readonly double _fy;
        private readonly double _cx;
        private readonly double _cy;

        public MountingCalibrationPanelForm(string baseDirectory, TryGetImuAnglesDelegate imuProvider)
            : base("安装角标定面板", "Step 0B 安装角偏差标定：自动采集实时IMU，像素点与内参自动读取。")
        {
            _pathService = new CalibrationPathService(baseDirectory);
            _imuProvider = imuProvider;
            CalibrationConfig cfg = LoadConfigSafe(_pathService.ConfigPath);

            _fx = cfg.Fx > 0 ? cfg.Fx : 500;
            _fy = cfg.Fy > 0 ? cfg.Fy : 500;
            _cx = cfg.Cx > 0 ? cfg.Cx : 320;
            _cy = cfg.Cy > 0 ? cfg.Cy : 240;
            _u = _cx;
            _v = _cy;

            _deviceIdLabel = AddInfoRow("设备编号", _pathService.GetDefaultDeviceId());
            _angleXLabel = AddInfoRow("IMU AngleX(roll)", "等待传感器数据...");
            _angleYLabel = AddInfoRow("IMU AngleY(pitch)", "等待传感器数据...");
            _pixelLabel = AddInfoRow("靶点像素 (u,v)", string.Format(CultureInfo.InvariantCulture, "({0:F2}, {1:F2})", _u, _v));
            _cameraParamLabel = AddInfoRow("相机内参 (fx,fy,cx,cy)", string.Format(CultureInfo.InvariantCulture, "({0:F2}, {1:F2}, {2:F2}, {3:F2})", _fx, _fy, _cx, _cy));

            var note = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "说明: 本面板不需要手动输入参数，执行时自动读取当前IMU与配置参数。",
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
                _angleXLabel.Text = angleX.ToString("F4", CultureInfo.InvariantCulture);
                _angleYLabel.Text = angleY.ToString("F4", CultureInfo.InvariantCulture);
                SetStatus("已连接传感器，可执行标定。", true);
                RunButton.Enabled = true;
            }
            else
            {
                _angleXLabel.Text = "未连接";
                _angleYLabel.Text = "未连接";
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
                _pathService.EnsureCalibrationDirs(deviceId, "mounting_calibration", out inputDir, out outputDir);

                var input = new MountingCalibrationInput
                {
                    DeviceId = deviceId,
                    ImuAngleX = angleX,
                    ImuAngleY = angleY,
                    UActual = _u,
                    VActual = _v,
                    Fx = _fx,
                    Fy = _fy,
                    Cx = _cx,
                    Cy = _cy
                };

                if (input.Fx <= 0 || input.Fy <= 0)
                    throw new InvalidOperationException("fx/fy 必须大于 0");

                string inputPath = _pathService.CreateTimestampedFile(inputDir, "mounting_input", "json");
                string outputPath = _pathService.CreateTimestampedFile(outputDir, "mounting_output", "json");
                CalibrationJsonUtil.SaveToFile(input, inputPath);

                string exePath = CalibrationExecutableResolver.Resolve(
                    _pathService.BaseDirectory,
                    "MountingCalibration.exe",
                    Path.Combine("DataProcessing", "Calibration", "MountingCalibrationApp", "bin"));

                if (!File.Exists(exePath))
                    throw new FileNotFoundException("未找到 MountingCalibration.exe", exePath);

                string args = string.Format("--input \"{0}\" --output \"{1}\" --config \"{2}\"",
                    inputPath, outputPath, _pathService.ConfigPath);

                string stdout, stderr;
                int code = CalibrationProcessRunner.Run(exePath, args, out stdout, out stderr);
                if (code != 0)
                {
                    throw new InvalidOperationException("安装角标定程序执行失败: " + stderr + "\n" + stdout);
                }

                MountingCalibrationOutput output = CalibrationJsonUtil.LoadFromFile<MountingCalibrationOutput>(outputPath);
                string summaryPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(outputPath) + "_summary.txt");
                ResultTextBox.Text = string.Format(
                    "执行成功: {0}\r\nδ_pitch={1:F5}°\r\nδ_roll={2:F5}°\r\n配置文件: {3}\r\n输入文件: {4}\r\n输出文件: {5}\r\n摘要文件: {6}",
                    output.Message, output.DeltaPitch, output.DeltaRoll,
                    output.ConfigPath, inputPath, outputPath, summaryPath);

                ConfirmButton.Enabled = true;
                SetStatus("安装角标定完成，确认后关闭。", true);
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
