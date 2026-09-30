using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CalibrationBench.UI
{
    /// <summary>出厂校准工装界面：设备信息 + 四步执行 + 日志。调用无界面引擎，独立解耦。</summary>
    public sealed class MainForm : Form
    {
        private readonly TextBox _deviceId = new TextBox();
        private readonly TextBox _deviceRoot = new TextBox();
        private readonly TextBox _enginePath = new TextBox();
        private readonly TextBox _configPath = new TextBox();
        private readonly RichTextBox _log = new RichTextBox();
        private readonly Label _status = new Label();
        private readonly NumericUpDown _azStep = new NumericUpDown();
        private readonly NumericUpDown _azCount = new NumericUpDown();
        private readonly NumericUpDown _framesPerAz = new NumericUpDown();
        private readonly NumericUpDown _settleSec = new NumericUpDown();
        private readonly ComboBox _dataSource = new ComboBox();
        private readonly TextBox _stagePort = new TextBox();
        private readonly NumericUpDown _manualAz = new NumericUpDown();
        private Acquisition.ManualAcquisition _manual;

        public MainForm()
        {
            Text = "数字对中仪 出厂校准工装 V1.0";
            Width = 940; Height = 900;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Microsoft YaHei UI", 9f);
            BuildUi();
            AutoDetectPaths();
        }

        private void BuildUi()
        {
            int y = 12;
            AddLabeled("设备编号", _deviceId, 12, ref y, "AT1");
            _deviceId.Text = "AT1";

            AddLabeled("设备根目录(device_info 上级)", _deviceRoot, 12, ref y, "");
            AddBrowseFolder(_deviceRoot, 12, y - 30);

            AddLabeled("校准引擎(CalibrationEngine.exe)", _enginePath, 12, ref y, "");
            AddBrowseFile(_enginePath, 12, y - 30, "可执行程序|*.exe");

            AddLabeled("配置文件(calibration_config.json)", _configPath, 12, ref y, "");
            AddBrowseFile(_configPath, 12, y - 30, "JSON|*.json");

            var grp = new GroupBox { Text = "校准步骤(选择输入 JSON 并执行)", Left = 12, Top = y + 4, Width = 900, Height = 96 };
            AddStepButton(grp, "Step 0A 相机内参", "0A", 16);
            AddStepButton(grp, "Step 0B 安装角", "0B", 236);
            AddStepButton(grp, "Step 0C 航向", "0C", 456);
            AddStepButton(grp, "Step 0D 综合验证", "0D", 676);
            var tip = new Label { Left = 16, Top = 58, Width = 860, Height = 30,
                Text = "手动模式：选择输入 JSON 执行。0A/0B/0C 合格后写入 calibration_config.json；0D 生成放行结果。" };
            grp.Controls.Add(tip);
            Controls.Add(grp);

            var acq = new GroupBox { Text = "采集编排(数据源可切换：模拟 / 真实驱动)", Left = 12, Top = y + 108, Width = 900, Height = 104 };
            AddNumeric(acq, "方位间隔°", _azStep, 45, 5, 90, 16, 26);
            AddNumeric(acq, "方位数", _azCount, 8, 3, 24, 176, 26);
            AddNumeric(acq, "每方位帧数", _framesPerAz, 25, 1, 200, 316, 26);
            AddNumeric(acq, "稳定秒", _settleSec, 1, 0, 60, 496, 26);
            var bMount = new Button { Text = "采集 0B(静止)", Left = 636, Top = 22, Width = 120, Height = 28 };
            bMount.Click += (s, e) => RunAcquireMounting();
            var bRot = new Button { Text = "采集 0C/0D(多方位)", Left = 766, Top = 22, Width = 130, Height = 28 };
            bRot.Click += (s, e) => RunAcquireRotation();
            acq.Controls.Add(bMount); acq.Controls.Add(bRot);
            var lbSrc = new Label { Left = 16, Top = 62, Width = 52, Text = "数据源" };
            _dataSource.Left = 68; _dataSource.Top = 59; _dataSource.Width = 130;
            _dataSource.DropDownStyle = ComboBoxStyle.DropDownList;
            _dataSource.Items.AddRange(new object[] { "模拟(无硬件)", "真实(硬件驱动)" });
            _dataSource.SelectedIndex = 0;
            var lbPort = new Label { Left = 210, Top = 62, Width = 84, Text = "旋转台串口" };
            _stagePort.Left = 296; _stagePort.Top = 59; _stagePort.Width = 80; _stagePort.Text = "COM3";
            var atip = new Label { Left = 392, Top = 62, Width = 380, Height = 30,
                Text = "真实模式需实现相机/PnP/IMU 驱动(见 Acquisition/Real)；旋转台为串口可用实现。" };
            var bSelfTest = new Button { Text = "连接自检", Left = 782, Top = 58, Width = 110, Height = 26 };
            bSelfTest.Click += (s, e) => RunSelfTest();
            acq.Controls.Add(lbSrc); acq.Controls.Add(_dataSource);
            acq.Controls.Add(lbPort); acq.Controls.Add(_stagePort); acq.Controls.Add(atip);
            acq.Controls.Add(bSelfTest);
            Controls.Add(acq);

            var man = new GroupBox { Text = "手动挪位采样(无旋转台：人工转/挪到各位置逐站触发)", Left = 12, Top = y + 216, Width = 900, Height = 92 };
            var lbMa = new Label { Left = 16, Top = 28, Width = 64, Text = "名义方位°" };
            _manualAz.Left = 84; _manualAz.Top = 25; _manualAz.Width = 60; _manualAz.Minimum = 0; _manualAz.Maximum = 360; _manualAz.Value = 0;
            var bStation = new Button { Text = "采集本站(0C/0D)", Left = 156, Top = 22, Width = 130, Height = 28 };
            bStation.Click += (s, e) => ManualStation();
            var bManMount = new Button { Text = "追加0B(静止)", Left = 292, Top = 22, Width = 110, Height = 28 };
            bManMount.Click += (s, e) => ManualMounting();
            var bGenCD = new Button { Text = "生成0C/0D", Left = 408, Top = 22, Width = 100, Height = 28 };
            bGenCD.Click += (s, e) => ManualSaveRotation();
            var bGenB = new Button { Text = "生成0B", Left = 514, Top = 22, Width = 80, Height = 28 };
            bGenB.Click += (s, e) => ManualSaveMounting();
            var bReset = new Button { Text = "重置会话", Left = 600, Top = 22, Width = 90, Height = 28 };
            bReset.Click += (s, e) => ManualReset();
            man.Controls.Add(lbMa); man.Controls.Add(_manualAz);
            man.Controls.Add(bStation); man.Controls.Add(bManMount);
            man.Controls.Add(bGenCD); man.Controls.Add(bGenB); man.Controls.Add(bReset);
            var mtip = new Label { Left = 16, Top = 58, Width = 870, Height = 26,
                Text = "每采一站站数+1(每站取『每方位帧数』帧)；转到下一方位改『名义方位』再采；采够后生成 JSON，用上方步骤按钮执行。" };
            man.Controls.Add(mtip);
            Controls.Add(man);

            _status.Left = 12; _status.Top = y + 314; _status.Width = 900; _status.Height = 22;
            _status.Text = "就绪"; _status.ForeColor = Color.DimGray;
            Controls.Add(_status);

            _log.Left = 12; _log.Top = y + 340; _log.Width = 900; _log.Height = 250;
            _log.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _log.ReadOnly = true; _log.BackColor = Color.White; _log.Font = new Font("Consolas", 9f);
            Controls.Add(_log);
        }

        private void AddLabeled(string caption, TextBox tb, int x, ref int y, string def)
        {
            var lb = new Label { Left = x, Top = y, Width = 260, Text = caption };
            Controls.Add(lb);
            tb.Left = x + 270; tb.Top = y - 3; tb.Width = 560; tb.Text = def;
            Controls.Add(tb);
            y += 34;
        }

        private void AddBrowseFolder(TextBox tb, int x, int top)
        {
            var b = new Button { Left = x + 838, Top = top, Width = 60, Height = 24, Text = "..." };
            b.Click += (s, e) => { using (var d = new FolderBrowserDialog()) { if (d.ShowDialog() == DialogResult.OK) { tb.Text = d.SelectedPath; AutoConfigFromRoot(); } } };
            Controls.Add(b);
        }

        private void AddBrowseFile(TextBox tb, int x, int top, string filter)
        {
            var b = new Button { Left = x + 838, Top = top, Width = 60, Height = 24, Text = "..." };
            b.Click += (s, e) => { using (var d = new OpenFileDialog { Filter = filter }) { if (d.ShowDialog() == DialogResult.OK) tb.Text = d.FileName; } };
            Controls.Add(b);
        }

        private void AddStepButton(GroupBox grp, string text, string step, int left)
        {
            var b = new Button { Text = text, Left = left, Top = 22, Width = 200, Height = 30 };
            b.Click += (s, e) => RunStep(step);
            grp.Controls.Add(b);
        }

        private void AutoDetectPaths()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string eng = Path.Combine(baseDir, "CalibrationEngine.exe");
            if (File.Exists(eng)) _enginePath.Text = eng;
            _deviceRoot.Text = baseDir.TrimEnd('\\');
            AutoConfigFromRoot();
        }

        private void AutoConfigFromRoot()
        {
            if (!string.IsNullOrEmpty(_deviceRoot.Text))
                _configPath.Text = Path.Combine(_deviceRoot.Text, "calibration_config.json");
        }

        private void RunStep(string step)
        {
            try
            {
                using (var d = new OpenFileDialog { Filter = "JSON|*.json", Title = "选择 " + step + " 输入 JSON" })
                {
                    if (d.ShowDialog() != DialogResult.OK) return;
                    string input = d.FileName;
                    string outDir = Path.Combine(Path.GetDirectoryName(input), "output");
                    Directory.CreateDirectory(outDir);
                    string output = Path.Combine(outDir, step + "_output.json");

                    var runner = new EngineRunner(_enginePath.Text);
                    Log("▶ 执行 " + step + " ...", Color.Black);
                    var r = runner.Run(step, input, output, _configPath.Text, _deviceRoot.Text, _deviceId.Text);

                    if (!string.IsNullOrWhiteSpace(r.StdOut)) Log(r.StdOut.TrimEnd(), Color.Black);
                    if (!string.IsNullOrWhiteSpace(r.StdErr)) Log(r.StdErr.TrimEnd(), Color.Firebrick);

                    if (r.ExitCode == 0) { SetStatus(step + " 合格", Color.Green); Log("✔ 合格，输出: " + output, Color.Green); }
                    else if (r.ExitCode == 2) { SetStatus(step + " 未通过判据", Color.DarkOrange); Log("✖ 未通过判据，输出: " + output, Color.DarkOrange); }
                    else { SetStatus(step + " 执行错误", Color.Firebrick); Log("✖ 执行错误(退出码 " + r.ExitCode + ")", Color.Firebrick); }
                    Log("", Color.Black);
                }
            }
            catch (Exception ex)
            {
                SetStatus("异常: " + ex.Message, Color.Firebrick);
                Log("异常: " + ex.Message, Color.Firebrick);
            }
        }

        private void AddNumeric(GroupBox g, string cap, NumericUpDown nud, decimal val, decimal min, decimal max, int left, int top)
        {
            var lb = new Label { Left = left, Top = top + 2, Width = 64, Text = cap };
            nud.Left = left + 66; nud.Top = top; nud.Width = 56;
            nud.Minimum = min; nud.Maximum = max; nud.Value = val; nud.DecimalPlaces = 0;
            g.Controls.Add(lb); g.Controls.Add(nud);
        }

        private Acquisition.AcquisitionSettings BuildSettings()
        {
            return new Acquisition.AcquisitionSettings
            {
                DeviceId = _deviceId.Text,
                AzimuthStepDeg = (double)_azStep.Value,
                AzimuthCount = (int)_azCount.Value,
                FramesPerAzimuth = (int)_framesPerAz.Value,
                SettleSeconds = (double)_settleSec.Value
            };
        }

        private string AcqDir()
        {
            string root = string.IsNullOrEmpty(_deviceRoot.Text) ? AppDomain.CurrentDomain.BaseDirectory : _deviceRoot.Text;
            string dir = Path.Combine(root, "acquired");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private Acquisition.ISceneCapture MakeScene(Acquisition.AcquisitionSettings s)
        {
            if (_dataSource.SelectedIndex == 1) // 真实驱动
            {
                var k = Acquisition.ConfigLoader.LoadIntrinsics(_configPath.Text);
                if (k != null) Log(string.Format("数据源=真实：内参取自 config (fx={0:F1}, cx={1:F1})", k.Fx, k.Cx), Color.DimGray);
                else { k = s.SimIntrinsics(); Log("警告：未从 config 读到内参，暂用占位内参，请先完成 Step 0A。", Color.DarkOrange); }
                Log("相机/PnP/IMU 需已实现 Acquisition/Real 驱动(或用 Delegating* 注入)。", Color.DimGray);
                return new Acquisition.Real.RealSceneCapture(
                    new Acquisition.Real.IndustrialCameraSource(),
                    new Acquisition.Real.OpenCvCharucoPnpSolver(),
                    new Acquisition.Real.Bwt901ImuSource(),
                    k);
            }
            return new Acquisition.SimulatedBench(s);
        }

        private Acquisition.IRotaryStage MakeStage(Acquisition.AcquisitionSettings s, Acquisition.ISceneCapture scene)
        {
            if (_dataSource.SelectedIndex == 1)
            {
                Log("旋转台串口 " + _stagePort.Text + "(可用实现)。", Color.DimGray);
                return new Acquisition.Real.RealRotaryStage(_stagePort.Text);
            }
            return (Acquisition.IRotaryStage)scene; // 模拟台同时实现两接口
        }

        private void MakeBench(Acquisition.AcquisitionSettings s, out Acquisition.IRotaryStage stage, out Acquisition.ISceneCapture scene)
        {
            scene = MakeScene(s);
            stage = MakeStage(s, scene);
        }

        // ---- 手动挪位采样 ----
        private void EnsureManual()
        {
            if (_manual == null)
            {
                var s = BuildSettings();
                _manual = new Acquisition.ManualAcquisition(MakeScene(s), s, m => Log(m, Color.Black));
                Log("▶ 新建手动采样会话。", Color.Black);
            }
        }

        private void ManualStation()
        {
            try { EnsureManual(); _manual.CaptureStation((double)_manualAz.Value, (int)_framesPerAz.Value); SetStatus("手动站数 " + _manual.StationCount, Color.Green); }
            catch (Exception ex) { SetStatus("采集异常: " + ex.Message, Color.Firebrick); Log("异常: " + ex.Message, Color.Firebrick); }
        }

        private void ManualMounting()
        {
            try { EnsureManual(); _manual.CaptureMounting((int)_framesPerAz.Value); SetStatus("0B 累计 " + _manual.MountingFrameCount + " 帧", Color.Green); }
            catch (Exception ex) { SetStatus("采集异常: " + ex.Message, Color.Firebrick); Log("异常: " + ex.Message, Color.Firebrick); }
        }

        private void ManualSaveRotation()
        {
            if (_manual == null || _manual.StationCount == 0) { SetStatus("尚无手动站点", Color.DarkOrange); Log("请先『采集本站(0C/0D)』至少 3 站。", Color.DarkOrange); return; }
            string p0c, p0d; _manual.SaveRotation(AcqDir(), out p0c, out p0d);
            SetStatus("已生成 0C/0D(" + _manual.StationCount + " 站)", Color.Green);
            Log("✔ 0C 输入：" + p0c, Color.Green);
            Log("✔ 0D 输入：" + p0d, Color.Green);
            Log("→ 用『Step 0C』『Step 0D』选对应文件执行", Color.Green); Log("", Color.Black);
        }

        private void ManualSaveMounting()
        {
            if (_manual == null || _manual.MountingFrameCount == 0) { SetStatus("尚无 0B 帧", Color.DarkOrange); Log("请先『追加0B(静止)』。", Color.DarkOrange); return; }
            string p = _manual.SaveMounting(AcqDir());
            SetStatus("已生成 0B(" + _manual.MountingFrameCount + " 帧)", Color.Green);
            Log("✔ 0B 输入：" + p + "  → 点『Step 0B』执行", Color.Green); Log("", Color.Black);
        }

        private void ManualReset()
        {
            _manual = null; SetStatus("手动会话已重置", Color.DimGray); Log("手动采样会话已重置。", Color.DimGray);
        }

        // ---- 连接自检 ----
        private void RunSelfTest()
        {
            Log("▶ 连接自检 ...", Color.Black);
            bool real = _dataSource.SelectedIndex == 1;

            // 1) 内参
            var k = Acquisition.ConfigLoader.LoadIntrinsics(_configPath.Text);
            if (k != null) Log(string.Format("✔ 内参已加载 (fx={0:F1}, cx={1:F1})", k.Fx, k.Cx), Color.Green);
            else Log("✖ 未找到内参(calibration_config.json)，请先完成 Step 0A。", Color.DarkOrange);

            // 2) 旋转台
            if (real)
            {
                try
                {
                    using (var st = new Acquisition.Real.RealRotaryStage(_stagePort.Text))
                        Log("✔ 旋转台串口 " + _stagePort.Text + " 打开成功，STAT=" + st.IsSettled(), Color.Green);
                }
                catch (Exception ex) { Log("✖ 旋转台 " + _stagePort.Text + "：" + ex.Message, Color.Firebrick); }
            }
            else Log("· 数据源=模拟：旋转台/相机/IMU 均为模拟。", Color.DimGray);

            // 3) 场景采集一帧(相机+PnP+IMU)
            try
            {
                var scene = MakeScene(BuildSettings());
                var f = scene.Capture(0);
                Log(string.Format("✔ 场景采集一帧成功：offsetPnp=({0:F2},{1:F2})mm, H={2:F1}mm, θ_img={3:F2}°",
                    f.OffsetPnpE_mm, f.OffsetPnpN_mm, f.H_mm, f.ThetaImgBoardDeg), Color.Green);
            }
            catch (Exception ex) { Log("✖ 场景采集：" + ex.Message, Color.Firebrick); }

            Log("自检完成。", Color.Black); Log("", Color.Black);
        }

        private void RunAcquireMounting()
        {
            try
            {
                var s = BuildSettings();
                Acquisition.IRotaryStage stage; Acquisition.ISceneCapture scene;
                MakeBench(s, out stage, out scene);
                var orch = new Acquisition.AcquisitionOrchestrator(stage, scene, m => Log(m, Color.Black));
                Log("▶ 采集 0B(静止多帧) ...", Color.Black);
                string path = orch.AcquireMounting(s, AcqDir());
                SetStatus("0B 采集完成", Color.Green);
                Log("✔ 0B 输入已生成：" + path + "  → 点『Step 0B』选它执行", Color.Green);
                Log("", Color.Black);
            }
            catch (Exception ex) { SetStatus("采集异常: " + ex.Message, Color.Firebrick); Log("异常: " + ex.Message, Color.Firebrick); Log("", Color.Black); }
        }

        private void RunAcquireRotation()
        {
            try
            {
                var s = BuildSettings();
                Acquisition.IRotaryStage stage; Acquisition.ISceneCapture scene;
                MakeBench(s, out stage, out scene);
                var orch = new Acquisition.AcquisitionOrchestrator(stage, scene, m => Log(m, Color.Black));
                Log("▶ 采集 0C/0D(多方位 step-and-stare) ...", Color.Black);
                string p0c, p0d;
                orch.AcquireRotation(s, AcqDir(), out p0c, out p0d);
                SetStatus("0C/0D 采集完成", Color.Green);
                Log("✔ 0C 输入：" + p0c, Color.Green);
                Log("✔ 0D 输入：" + p0d, Color.Green);
                Log("→ 依次点『Step 0C』『Step 0D』选对应文件执行", Color.Green);
                Log("", Color.Black);
            }
            catch (Exception ex) { SetStatus("采集异常: " + ex.Message, Color.Firebrick); Log("异常: " + ex.Message, Color.Firebrick); Log("", Color.Black); }
        }

        private void SetStatus(string text, Color c) { _status.Text = text; _status.ForeColor = c; }

        private void Log(string text, Color c)
        {
            _log.SelectionStart = _log.TextLength;
            _log.SelectionColor = c;
            _log.AppendText(text + Environment.NewLine);
            _log.SelectionColor = _log.ForeColor;
            _log.ScrollToCaret();
        }
    }
}
