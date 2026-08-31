using System;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using dsat.DataProcessing.Calibration;

namespace dsat.CalibrationPanels
{
    public class CameraCalibrationPanelForm : CalibrationWizardBaseForm
    {
        public delegate bool IsCameraReadyDelegate();

        private readonly CalibrationPathService _pathService;
        private readonly IsCameraReadyDelegate _cameraReadyProvider;
        private Timer _cameraStateTimer;

        // Step 1 controls
        private Label _cameraStatusLabel;
        private Label _deviceIdValue;

        // Step 2 controls
        private TextBox _fxBox, _fyBox, _cxBox, _cyBox;
        private TextBox _k1Box, _k2Box, _p1Box, _p2Box;
        private TextBox _widthBox, _heightBox;

        // Step 3 controls
        private TextBox _resultBox;
        private bool _calibrationDone;

        public CameraCalibrationPanelForm(string baseDirectory, IsCameraReadyDelegate cameraReadyProvider)
            : base("相机内参标定 (Step 0A)")
        {
            _pathService = new CalibrationPathService(baseDirectory);
            _cameraReadyProvider = cameraReadyProvider;

            CalibrationConfig cfg = LoadConfigSafe(_pathService.ConfigPath);

            BuildStep1_Prepare(cfg);
            BuildStep2_Parameters(cfg);
            BuildStep3_Execute();
            BuildStep4_Confirm();

            StartWizard();
        }

        private void BuildStep1_Prepare(CalibrationConfig cfg)
        {
            _cameraStatusLabel = CreateInfoLabel("检测中...");
            _deviceIdValue = CreateInfoLabel(_pathService.GetDefaultDeviceId());

            var panel = BuildKvPanel(
                Tuple.Create("设备编号:", (Control)_deviceIdValue),
                Tuple.Create("相机状态:", (Control)_cameraStatusLabel),
                Tuple.Create("当前配置:",
                    (Control)CreateInfoLabel(cfg.Fx > 0
                        ? string.Format(CultureInfo.InvariantCulture, "fx={0:F1} fy={1:F1} cx={2:F1} cy={3:F1}", cfg.Fx, cfg.Fy, cfg.Cx, cfg.Cy)
                        : "尚未标定"))
            );

            AddStep(new WizardStep
            {
                Title = "准备工作",
                Instruction = "请确认相机已通过USB/网络连接。\n准备好 9×6 棋盘格标定板（格子边长已知），从不同角度和距离拍摄至少 15 张标定图像。",
                Content = panel,
                OnEnter = () =>
                {
                    if (_cameraStateTimer == null)
                    {
                        _cameraStateTimer = new Timer { Interval = 500 };
                        _cameraStateTimer.Tick += (s, e) => RefreshCameraState();
                        _cameraStateTimer.Start();
                        FormClosed += (s, e) => { _cameraStateTimer.Stop(); _cameraStateTimer.Dispose(); };
                    }
                    RefreshCameraState();
                }
            });
        }

        private void BuildStep2_Parameters(CalibrationConfig cfg)
        {
            _fxBox = CreateEditBox(cfg.Fx > 0 ? cfg.Fx.ToString("F3", CultureInfo.InvariantCulture) : "500.000");
            _fyBox = CreateEditBox(cfg.Fy > 0 ? cfg.Fy.ToString("F3", CultureInfo.InvariantCulture) : "500.000");
            _cxBox = CreateEditBox(cfg.Cx > 0 ? cfg.Cx.ToString("F3", CultureInfo.InvariantCulture) : "320.000");
            _cyBox = CreateEditBox(cfg.Cy > 0 ? cfg.Cy.ToString("F3", CultureInfo.InvariantCulture) : "240.000");
            _k1Box = CreateEditBox(cfg.K1.ToString("F6", CultureInfo.InvariantCulture));
            _k2Box = CreateEditBox(cfg.K2.ToString("F6", CultureInfo.InvariantCulture));
            _p1Box = CreateEditBox(cfg.P1.ToString("F6", CultureInfo.InvariantCulture));
            _p2Box = CreateEditBox(cfg.P2.ToString("F6", CultureInfo.InvariantCulture));
            _widthBox = CreateEditBox(cfg.ImageWidth > 0 ? cfg.ImageWidth.ToString() : "640");
            _heightBox = CreateEditBox(cfg.ImageHeight > 0 ? cfg.ImageHeight.ToString() : "480");

            var panel = BuildKvPanel(
                Tuple.Create("fx (像素焦距X):", (Control)_fxBox),
                Tuple.Create("fy (像素焦距Y):", (Control)_fyBox),
                Tuple.Create("cx (主点X):", (Control)_cxBox),
                Tuple.Create("cy (主点Y):", (Control)_cyBox),
                Tuple.Create("k1 (径向畸变1):", (Control)_k1Box),
                Tuple.Create("k2 (径向畸变2):", (Control)_k2Box),
                Tuple.Create("p1 (切向畸变1):", (Control)_p1Box),
                Tuple.Create("p2 (切向畸变2):", (Control)_p2Box),
                Tuple.Create("图像宽度:", (Control)_widthBox),
                Tuple.Create("图像高度:", (Control)_heightBox)
            );

            AddStep(new WizardStep
            {
                Title = "确认/输入内参",
                Instruction = "确认或修改相机内参与畸变参数。如果已有标定结果则直接使用当前值，否则请输入标定得到的参数。",
                Content = panel,
                OnValidate = () =>
                {
                    double fx, fy;
                    if (!double.TryParse(_fxBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out fx) || fx <= 0)
                    { SetStatus("fx 必须为正数", false); return false; }
                    if (!double.TryParse(_fyBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out fy) || fy <= 0)
                    { SetStatus("fy 必须为正数", false); return false; }
                    return true;
                }
            });
        }

        private void BuildStep3_Execute()
        {
            _resultBox = CreateResultBox();
            var panel = new Panel();
            panel.Controls.Add(_resultBox);

            var execButton = new Button { Text = "执行标定写入", Width = 160, Height = 32, Dock = DockStyle.Bottom };
            StyleButton(execButton, true);
            execButton.Click += (s, e) => RunCalibration();
            panel.Controls.Add(execButton);

            AddStep(new WizardStep
            {
                Title = "执行标定",
                Instruction = "点击\"执行标定写入\"按钮，将参数写入 calibration_config.json。\n" +
                              "如已接入外部棋盘格标定程序，将自动调用 CameraCalibration.exe。",
                Content = panel,
                OnEnter = () => { _calibrationDone = false; NextButton.Enabled = false; },
                OnValidate = () =>
                {
                    if (!_calibrationDone) { SetStatus("请先执行标定", false); return false; }
                    return true;
                }
            });
        }

        private void BuildStep4_Confirm()
        {
            var lbl = CreateInfoLabel("标定完成后此处显示最终参数。");
            var panel = new Panel();
            panel.Controls.Add(lbl);

            AddStep(new WizardStep
            {
                Title = "确认结果",
                Instruction = "请确认标定结果正确，点击\"完成\"保存并退出。",
                Content = panel,
                OnEnter = () =>
                {
                    CalibrationConfig final = LoadConfigSafe(_pathService.ConfigPath);
                    lbl.Text = string.Format(CultureInfo.InvariantCulture,
                        "标定结果已保存到: {0}\n\nfx = {1:F3}\nfy = {2:F3}\ncx = {3:F3}\ncy = {4:F3}\n" +
                        "k1 = {5:F6}\nk2 = {6:F6}\np1 = {7:F6}\np2 = {8:F6}\n图像: {9}×{10}",
                        _pathService.ConfigPath,
                        final.Fx, final.Fy, final.Cx, final.Cy,
                        final.K1, final.K2, final.P1, final.P2,
                        final.ImageWidth, final.ImageHeight);
                    SetStatus("标定完成，可关闭面板。", true);
                }
            });
        }

        private void RunCalibration()
        {
            try
            {
                double fx = double.Parse(_fxBox.Text, CultureInfo.InvariantCulture);
                double fy = double.Parse(_fyBox.Text, CultureInfo.InvariantCulture);
                double cx = double.Parse(_cxBox.Text, CultureInfo.InvariantCulture);
                double cy = double.Parse(_cyBox.Text, CultureInfo.InvariantCulture);
                double k1 = double.Parse(_k1Box.Text, CultureInfo.InvariantCulture);
                double k2 = double.Parse(_k2Box.Text, CultureInfo.InvariantCulture);
                double p1 = double.Parse(_p1Box.Text, CultureInfo.InvariantCulture);
                double p2 = double.Parse(_p2Box.Text, CultureInfo.InvariantCulture);
                int w = int.Parse(_widthBox.Text);
                int h = int.Parse(_heightBox.Text);

                string deviceId = _pathService.EnsureAndPersistDeviceId(_pathService.GetDefaultDeviceId());
                string inputDir, outputDir;
                _pathService.EnsureCalibrationDirs(deviceId, "camera_calibration", out inputDir, out outputDir);

                var input = new CameraCalibrationInput
                {
                    DeviceId = deviceId,
                    Fx = fx, Fy = fy, Cx = cx, Cy = cy,
                    K1 = k1, K2 = k2, P1 = p1, P2 = p2,
                    ImageWidth = w, ImageHeight = h
                };

                string inputPath = _pathService.CreateTimestampedFile(inputDir, "camera_input", "json");
                string outputPath = _pathService.CreateTimestampedFile(outputDir, "camera_output", "json");
                CalibrationJsonUtil.SaveToFile(input, inputPath);

                string exePath = CalibrationExecutableResolver.Resolve(
                    _pathService.BaseDirectory, "CameraCalibration.exe",
                    Path.Combine("DataProcessing", "Calibration", "CameraCalibrationApp", "bin"));

                if (File.Exists(exePath))
                {
                    string args = string.Format("--input \"{0}\" --output \"{1}\" --config \"{2}\"",
                        inputPath, outputPath, _pathService.ConfigPath);
                    string stdout, stderr;
                    int code = CalibrationProcessRunner.Run(exePath, args, out stdout, out stderr);

                    CameraCalibrationOutput output = CalibrationJsonUtil.LoadFromFile<CameraCalibrationOutput>(outputPath);
                    _resultBox.Text = string.Format("执行成功: {0}\r\nfx={1:F3}, fy={2:F3}\r\ncx={3:F3}, cy={4:F3}\r\n配置已保存: {5}",
                        output.Message, output.Fx, output.Fy, output.Cx, output.Cy, output.ConfigPath);
                }
                else
                {
                    var config = LoadConfigSafe(_pathService.ConfigPath);
                    config.Fx = fx; config.Fy = fy; config.Cx = cx; config.Cy = cy;
                    config.K1 = k1; config.K2 = k2; config.P1 = p1; config.P2 = p2;
                    config.ImageWidth = w; config.ImageHeight = h;
                    config.Save(_pathService.ConfigPath);

                    _resultBox.Text = string.Format(
                        "已直接写入 calibration_config.json:\r\nfx={0:F3}, fy={1:F3}, cx={2:F3}, cy={3:F3}\r\n" +
                        "k1={4:F6}, k2={5:F6}, p1={6:F6}, p2={7:F6}\r\n图像: {8}×{9}",
                        fx, fy, cx, cy, k1, k2, p1, p2, w, h);
                }

                _calibrationDone = true;
                NextButton.Enabled = true;
                SetStatus("标定执行成功！", true);
            }
            catch (Exception ex)
            {
                _resultBox.Text = "执行失败:\r\n" + ex.Message;
                SetStatus("执行失败: " + ex.Message, false);
            }
        }

        private void RefreshCameraState()
        {
            bool ready = _cameraReadyProvider != null && _cameraReadyProvider();
            _cameraStatusLabel.Text = ready ? "✓ 已连接" : "✗ 未连接";
            _cameraStatusLabel.ForeColor = ready ? Color.DarkGreen : Color.DarkRed;
        }
    }
}

