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

        public MainForm()
        {
            Text = "数字对中仪 出厂校准工装 V1.0";
            Width = 940; Height = 800;
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

            var acq = new GroupBox { Text = "采集编排(模拟数据源，预留真实相机/旋转台/IMU 驱动)", Left = 12, Top = y + 108, Width = 900, Height = 104 };
            AddNumeric(acq, "方位间隔°", _azStep, 45, 5, 90, 16, 26);
            AddNumeric(acq, "方位数", _azCount, 8, 3, 24, 176, 26);
            AddNumeric(acq, "每方位帧数", _framesPerAz, 25, 1, 200, 316, 26);
            AddNumeric(acq, "稳定秒", _settleSec, 1, 0, 60, 496, 26);
            var bMount = new Button { Text = "采集 0B(静止)", Left = 636, Top = 22, Width = 120, Height = 28 };
            bMount.Click += (s, e) => RunAcquireMounting();
            var bRot = new Button { Text = "采集 0C/0D(多方位)", Left = 766, Top = 22, Width = 130, Height = 28 };
            bRot.Click += (s, e) => RunAcquireRotation();
            acq.Controls.Add(bMount); acq.Controls.Add(bRot);
            var atip = new Label { Left = 16, Top = 66, Width = 870, Height = 30,
                Text = "step-and-stare：每方位下发旋转→悬停稳定→间隔采样多帧→PnP。产出输入 JSON 后用上方按钮执行(0B/0C/0D)。" };
            acq.Controls.Add(atip);
            Controls.Add(acq);

            _status.Left = 12; _status.Top = y + 216; _status.Width = 900; _status.Height = 22;
            _status.Text = "就绪"; _status.ForeColor = Color.DimGray;
            Controls.Add(_status);

            _log.Left = 12; _log.Top = y + 242; _log.Width = 900; _log.Height = 300;
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

        private void RunAcquireMounting()
        {
            try
            {
                var s = BuildSettings();
                var bench = new Acquisition.SimulatedBench(s);
                var orch = new Acquisition.AcquisitionOrchestrator(bench, bench, m => Log(m, Color.Black));
                Log("▶ 采集 0B(静止多帧) ...", Color.Black);
                string path = orch.AcquireMounting(s, AcqDir());
                SetStatus("0B 采集完成", Color.Green);
                Log("✔ 0B 输入已生成：" + path + "  → 点『Step 0B』选它执行", Color.Green);
                Log("", Color.Black);
            }
            catch (Exception ex) { SetStatus("采集异常: " + ex.Message, Color.Firebrick); Log("异常: " + ex.Message, Color.Firebrick); }
        }

        private void RunAcquireRotation()
        {
            try
            {
                var s = BuildSettings();
                var bench = new Acquisition.SimulatedBench(s);
                var orch = new Acquisition.AcquisitionOrchestrator(bench, bench, m => Log(m, Color.Black));
                Log("▶ 采集 0C/0D(多方位 step-and-stare) ...", Color.Black);
                string p0c, p0d;
                orch.AcquireRotation(s, AcqDir(), out p0c, out p0d);
                SetStatus("0C/0D 采集完成", Color.Green);
                Log("✔ 0C 输入：" + p0c, Color.Green);
                Log("✔ 0D 输入：" + p0d, Color.Green);
                Log("→ 依次点『Step 0C』『Step 0D』选对应文件执行", Color.Green);
                Log("", Color.Black);
            }
            catch (Exception ex) { SetStatus("采集异常: " + ex.Message, Color.Firebrick); Log("异常: " + ex.Message, Color.Firebrick); }
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
