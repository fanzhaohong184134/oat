using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using dsat.DataProcessing.Calibration;

namespace dsat.CalibrationPanels
{
    public class MountingCalibrationPanelForm : CalibrationWizardBaseForm
    {
        public delegate bool TryGetImuAnglesDelegate(out double angleX, out double angleY, out double angleZ);

        private readonly CalibrationPathService _pathService;
        private readonly TryGetImuAnglesDelegate _imuProvider;
        private Timer _imuTimer;
        private readonly double _fx, _fy, _cx, _cy;

        // Step 2 live display
        private Label _liveAngleX, _liveAngleY, _liveGyro, _liveAcc;
        private Label _stableIndicator;

        // Step 3 capture data
        private const int RequiredTrials = 3;
        private readonly List<double> _trialPitch = new List<double>();
        private readonly List<double> _trialRoll = new List<double>();
        private TextBox _captureLog;
        private Button _captureButton;

        // Step 4 result
        private TextBox _resultBox;

        public MountingCalibrationPanelForm(string baseDirectory, TryGetImuAnglesDelegate imuProvider)
            : base("安装角标定 (Step 0B)")
        {
            _pathService = new CalibrationPathService(baseDirectory);
            _imuProvider = imuProvider;
            CalibrationConfig cfg = LoadConfigSafe(_pathService.ConfigPath);

            _fx = cfg.Fx > 0 ? cfg.Fx : 500;
            _fy = cfg.Fy > 0 ? cfg.Fy : 500;
            _cx = cfg.Cx > 0 ? cfg.Cx : 320;
            _cy = cfg.Cy > 0 ? cfg.Cy : 240;

            BuildStep1_Prereq(cfg);
            BuildStep2_Setup();
            BuildStep3_Capture();
            BuildStep4_Result();

            StartWizard();
        }

        private void BuildStep1_Prereq(CalibrationConfig cfg)
        {
            bool hasIntrinsics = cfg.Fx > 0 && cfg.Fy > 0;
            string status = hasIntrinsics
                ? string.Format(CultureInfo.InvariantCulture, "✓ 已标定: fx={0:F1} fy={1:F1} cx={2:F1} cy={3:F1}", cfg.Fx, cfg.Fy, cfg.Cx, cfg.Cy)
                : "✗ 未完成，请先执行相机标定 (Step 0A)";

            var panel = BuildKvPanel(
                Tuple.Create("相机标定状态:", (Control)CreateInfoLabel(status)),
                Tuple.Create("设备编号:", (Control)CreateInfoLabel(_pathService.GetDefaultDeviceId()))
            );

            AddStep(new WizardStep
            {
                Title = "前置检查",
                Instruction = "安装角标定需要相机内参 (Step 0A) 已完成。\n请确认下方状态为\"已标定\"再继续。",
                Content = panel,
                OnValidate = () =>
                {
                    if (_fx <= 0 || _fy <= 0)
                    {
                        SetStatus("相机内参未标定，请先完成 Step 0A", false);
                        return false;
                    }
                    return true;
                }
            });
        }

        private void BuildStep2_Setup()
        {
            _liveAngleX = CreateInfoLabel("等待...");
            _liveAngleY = CreateInfoLabel("等待...");
            _liveGyro = CreateInfoLabel("等待...");
            _liveAcc = CreateInfoLabel("等待...");
            _stableIndicator = CreateInfoLabel("--");
            _stableIndicator.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);

            var panel = BuildKvPanel(
                Tuple.Create("AngleX (roll):", (Control)_liveAngleX),
                Tuple.Create("AngleY (pitch):", (Control)_liveAngleY),
                Tuple.Create("陀螺仪 (°/s):", (Control)_liveGyro),
                Tuple.Create("加速度 (g):", (Control)_liveAcc),
                Tuple.Create("稳态判定:", (Control)_stableIndicator)
            );

            AddStep(new WizardStep
            {
                Title = "悬挂设备并等待稳态",
                Instruction = "操作步骤:\n" +
                    "① 将设备竖直悬挂（镜头朝下）\n" +
                    "② 正下方放置可识别靶标（中心点）\n" +
                    "③ 保持静止，等待下方\"稳态判定\"显示 ✓\n" +
                    "④ 稳态后点击\"下一步\"进入采集",
                Content = panel,
                OnEnter = () =>
                {
                    if (_imuTimer == null)
                    {
                        _imuTimer = new Timer { Interval = 200 };
                        _imuTimer.Tick += (s, e) => RefreshImuLive();
                        _imuTimer.Start();
                        FormClosed += (s, e) => { _imuTimer.Stop(); _imuTimer.Dispose(); };
                    }
                },
                OnValidate = () =>
                {
                    double ax, ay, az;
                    if (_imuProvider == null || !_imuProvider(out ax, out ay, out az))
                    {
                        SetStatus("未连接传感器", false);
                        return false;
                    }
                    return true;
                }
            });
        }

        private void BuildStep3_Capture()
        {
            _captureLog = CreateResultBox();
            _captureButton = new Button { Text = "采集一次 (0/3)", Width = 160, Height = 32, Dock = DockStyle.Bottom };
            StyleButton(_captureButton, true);
            _captureButton.Click += (s, e) => DoCapture();

            var panel = new Panel();
            panel.Controls.Add(_captureLog);
            panel.Controls.Add(_captureButton);

            AddStep(new WizardStep
            {
                Title = "采集数据（需重复3次）",
                Instruction = "每次采集时，设备必须保持静止（稳态）。\n" +
                    "靶标位于正下方，系统将自动读取 IMU pitch/roll 并以图像中心作为靶标像素坐标。\n" +
                    "完成 3 次采集后可进入下一步。",
                Content = panel,
                OnEnter = () =>
                {
                    _trialPitch.Clear();
                    _trialRoll.Clear();
                    _captureLog.Clear();
                    _captureButton.Text = "采集一次 (0/3)";
                    NextButton.Enabled = false;
                },
                OnValidate = () =>
                {
                    if (_trialPitch.Count < RequiredTrials)
                    {
                        SetStatus(string.Format("还需采集 {0} 次", RequiredTrials - _trialPitch.Count), false);
                        return false;
                    }
                    return true;
                }
            });
        }

        private void BuildStep4_Result()
        {
            _resultBox = CreateResultBox();
            var panel = new Panel();
            panel.Controls.Add(_resultBox);

            AddStep(new WizardStep
            {
                Title = "计算结果",
                Instruction = "以下为 3 次采集的平均安装角偏差。确认无误后点击\"完成\"保存到配置文件。",
                Content = panel,
                OnEnter = () => ComputeAndDisplay()
            });
        }

        private void DoCapture()
        {
            double angleX, angleY, angleZ;
            if (_imuProvider == null || !_imuProvider(out angleX, out angleY, out angleZ))
            {
                SetStatus("无法读取 IMU 数据，请检查传感器连接", false);
                return;
            }

            var cal = new MountingAngleCalibrator();
            double dp, dr;
            cal.Calibrate(angleX, angleY, _cx, _cy, _fx, _fy, _cx, _cy, out dp, out dr);

            _trialPitch.Add(dp);
            _trialRoll.Add(dr);

            int n = _trialPitch.Count;
            _captureLog.AppendText(string.Format(CultureInfo.InvariantCulture,
                "第 {0} 次:  δ_pitch = {1:F5}°,  δ_roll = {2:F5}°  (AngleX={3:F4} AngleY={4:F4})\r\n",
                n, dp, dr, angleX, angleY));

            _captureButton.Text = string.Format("采集一次 ({0}/{1})", n, RequiredTrials);
            SetStatus(string.Format("已完成 {0}/{1} 次采集", n, RequiredTrials), true);

            if (n >= RequiredTrials)
            {
                _captureButton.Enabled = false;
                NextButton.Enabled = true;
            }
        }

        private void ComputeAndDisplay()
        {
            double avgPitch = 0, avgRoll = 0;
            for (int i = 0; i < _trialPitch.Count; i++)
            {
                avgPitch += _trialPitch[i];
                avgRoll += _trialRoll[i];
            }
            avgPitch /= _trialPitch.Count;
            avgRoll /= _trialRoll.Count;

            double maxDevP = 0, maxDevR = 0;
            for (int i = 0; i < _trialPitch.Count; i++)
            {
                maxDevP = Math.Max(maxDevP, Math.Abs(_trialPitch[i] - avgPitch));
                maxDevR = Math.Max(maxDevR, Math.Abs(_trialRoll[i] - avgRoll));
            }

            string detail = string.Format(CultureInfo.InvariantCulture,
                "=== 安装角标定结果 ===\r\n\r\n" +
                "δ_pitch (平均) = {0:F5}°\r\nδ_roll  (平均) = {1:F5}°\r\n\r\n" +
                "最大偏差: pitch {2:F5}°, roll {3:F5}°\r\n" +
                "要求: 各次偏差 < 0.1°  →  {4}\r\n\r\n",
                avgPitch, avgRoll, maxDevP, maxDevR,
                (maxDevP < 0.1 && maxDevR < 0.1) ? "✓ 合格" : "✗ 偏差偏大，建议重新采集");

            for (int i = 0; i < _trialPitch.Count; i++)
            {
                detail += string.Format(CultureInfo.InvariantCulture,
                    "  第 {0} 次: δ_pitch={1:F5}° δ_roll={2:F5}°\r\n",
                    i + 1, _trialPitch[i], _trialRoll[i]);
            }

            _resultBox.Text = detail;
            SetStatus("结果已计算，点击\"完成\"保存", true);
        }

        protected override void OnWizardFinish()
        {
            try
            {
                double avgPitch = 0, avgRoll = 0;
                for (int i = 0; i < _trialPitch.Count; i++)
                {
                    avgPitch += _trialPitch[i];
                    avgRoll += _trialRoll[i];
                }
                avgPitch /= _trialPitch.Count;
                avgRoll /= _trialRoll.Count;

                var config = LoadConfigSafe(_pathService.ConfigPath);
                config.DeltaPitch = avgPitch;
                config.DeltaRoll = avgRoll;
                config.Save(_pathService.ConfigPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshImuLive()
        {
            double ax, ay, az;
            if (_imuProvider != null && _imuProvider(out ax, out ay, out az))
            {
                _liveAngleX.Text = ax.ToString("F4", CultureInfo.InvariantCulture) + "°";
                _liveAngleY.Text = ay.ToString("F4", CultureInfo.InvariantCulture) + "°";
                _liveGyro.Text = "实时读数中";
                _liveAcc.Text = "实时读数中";
                _stableIndicator.Text = "✓ 已连接 (请保持静止)";
                _stableIndicator.ForeColor = ThemeSuccess;
            }
            else
            {
                _liveAngleX.Text = "未连接";
                _liveAngleY.Text = "未连接";
                _stableIndicator.Text = "✗ 未连接";
                _stableIndicator.ForeColor = Color.DarkRed;
            }
        }
    }
}

