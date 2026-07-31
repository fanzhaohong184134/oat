using System;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Wit.Example_BWT901BLE.DataProcessing.Calibration;

namespace Wit.Example_BWT901BLE.CalibrationPanels
{
    public class CameraCalibrationPanelForm : CalibrationPanelBaseForm
    {
        public delegate bool IsCameraReadyDelegate();

        private readonly CalibrationPathService _pathService;
        private readonly IsCameraReadyDelegate _cameraReadyProvider;
        private readonly Label _deviceIdLabel;
        private readonly double _fx;
        private readonly double _fy;
        private readonly double _cx;
        private readonly double _cy;
        private readonly double _k1;
        private readonly double _k2;
        private readonly double _p1;
        private readonly double _p2;
        private readonly int _width;
        private readonly int _height;
        private readonly Timer _cameraStateTimer;

        public CameraCalibrationPanelForm(string baseDirectory, IsCameraReadyDelegate cameraReadyProvider)
            : base("相机标定面板", "Step 0A 相机内参与畸变参数标定：自动读取当前配置参数。")
        {
            _pathService = new CalibrationPathService(baseDirectory);
            _cameraReadyProvider = cameraReadyProvider;

            string defaultDeviceId = _pathService.GetDefaultDeviceId();
            CalibrationConfig cfg = LoadConfigSafe(_pathService.ConfigPath);

            _fx = cfg.Fx > 0 ? cfg.Fx : 500;
            _fy = cfg.Fy > 0 ? cfg.Fy : 500;
            _cx = cfg.Cx > 0 ? cfg.Cx : 320;
            _cy = cfg.Cy > 0 ? cfg.Cy : 240;
            _k1 = cfg.K1;
            _k2 = cfg.K2;
            _p1 = cfg.P1;
            _p2 = cfg.P2;
            _width = cfg.ImageWidth > 0 ? cfg.ImageWidth : 640;
            _height = cfg.ImageHeight > 0 ? cfg.ImageHeight : 480;

            _deviceIdLabel = AddInfoRow("设备编号", defaultDeviceId);
            AddInfoRow("fx/fy/cx/cy", string.Format(CultureInfo.InvariantCulture, "{0:F3} / {1:F3} / {2:F3} / {3:F3}", _fx, _fy, _cx, _cy));
            AddInfoRow("k1/k2/p1/p2", string.Format(CultureInfo.InvariantCulture, "{0:F6} / {1:F6} / {2:F6} / {3:F6}", _k1, _k2, _p1, _p2));
            AddInfoRow("图像宽高", string.Format(CultureInfo.InvariantCulture, "{0} x {1}", _width, _height));

            var note = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "说明: 此面板不需要手动输入参数，执行时自动使用 calibration_config.json 当前值。",
                Dock = DockStyle.Top
            };
            InputGrid.Controls.Add(note, 1, InputGrid.RowCount++);

            _cameraStateTimer = new Timer { Interval = 300 };
            _cameraStateTimer.Tick += (s, e) => RefreshCameraState();
            _cameraStateTimer.Start();
            FormClosed += (s, e) => _cameraStateTimer.Dispose();

            RefreshCameraState();

            RunButton.Click += RunButton_Click;
        }

        private void RefreshCameraState()
        {
            bool cameraReady = _cameraReadyProvider != null && _cameraReadyProvider();
            if (cameraReady)
            {
                SetStatus("相机已连接，可执行标定。", true);
                RunButton.Enabled = true;
            }
            else
            {
                SetStatus("未检测到传感器，请先连接后执行。", false);
                RunButton.Enabled = false;
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool cameraReady = _cameraReadyProvider != null && _cameraReadyProvider();
                if (!cameraReady)
                    throw new InvalidOperationException("未检测到传感器，请先连接后执行。");

                string deviceId = _pathService.EnsureAndPersistDeviceId(_pathService.GetDefaultDeviceId());
                _deviceIdLabel.Text = deviceId;
                string inputDir, outputDir;
                _pathService.EnsureCalibrationDirs(deviceId, "camera_calibration", out inputDir, out outputDir);

                var input = new CameraCalibrationInput
                {
                    DeviceId = deviceId,
                    Fx = _fx,
                    Fy = _fy,
                    Cx = _cx,
                    Cy = _cy,
                    K1 = _k1,
                    K2 = _k2,
                    P1 = _p1,
                    P2 = _p2,
                    ImageWidth = _width,
                    ImageHeight = _height
                };

                if (input.Fx <= 0 || input.Fy <= 0)
                    throw new InvalidOperationException("fx/fy 必须大于 0");
                if (input.ImageWidth <= 0 || input.ImageHeight <= 0)
                    throw new InvalidOperationException("图像宽高必须大于 0");

                string inputPath = _pathService.CreateTimestampedFile(inputDir, "camera_input", "json");
                string outputPath = _pathService.CreateTimestampedFile(outputDir, "camera_output", "json");
                CalibrationJsonUtil.SaveToFile(input, inputPath);

                string exePath = CalibrationExecutableResolver.Resolve(
                    _pathService.BaseDirectory,
                    "CameraCalibration.exe",
                    Path.Combine("DataProcessing", "Calibration", "CameraCalibrationApp", "bin"));

                if (!File.Exists(exePath))
                    throw new FileNotFoundException("未找到 CameraCalibration.exe", exePath);

                string args = string.Format("--input \"{0}\" --output \"{1}\" --config \"{2}\"",
                    inputPath, outputPath, _pathService.ConfigPath);

                string stdout, stderr;
                int code = CalibrationProcessRunner.Run(exePath, args, out stdout, out stderr);
                if (code != 0)
                {
                    throw new InvalidOperationException("相机标定程序执行失败: " + stderr + "\n" + stdout);
                }

                CameraCalibrationOutput output = CalibrationJsonUtil.LoadFromFile<CameraCalibrationOutput>(outputPath);
                string summaryPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(outputPath) + "_summary.txt");
                ResultTextBox.Text = string.Format(
                    "执行成功: {0}\r\nfx={1:F3}, fy={2:F3}, cx={3:F3}, cy={4:F3}\r\nK1={5:F6}, K2={6:F6}, P1={7:F6}, P2={8:F6}\r\n配置文件: {9}\r\n输入文件: {10}\r\n输出文件: {11}\r\n摘要文件: {12}",
                    output.Message,
                    output.Fx, output.Fy, output.Cx, output.Cy,
                    output.K1, output.K2, output.P1, output.P2,
                    output.ConfigPath, inputPath, outputPath, summaryPath);

                ConfirmButton.Enabled = true;
                SetStatus("相机标定完成，确认后关闭。", true);
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
