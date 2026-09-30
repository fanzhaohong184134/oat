using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using dsat.DataProcessing.Calibration;

namespace dsat.CalibrationPanels
{
    public class HeadingCalibrationPanelForm : CalibrationWizardBaseForm
    {
        public delegate bool TryGetImuAnglesDelegate(out double angleX, out double angleY, out double angleZ);

        private readonly CalibrationPathService _pathService;
        private readonly TryGetImuAnglesDelegate _imuProvider;
        private Timer _imuTimer;

        // Mode
        private RadioButton _modeReference;
        private RadioButton _modeMagnetic;
        private bool UseReferenceMode => _modeReference != null && _modeReference.Checked;

        // Step 3 inputs
        private TextBox _azimuthBox;
        private TextBox _magDecBox;

        // Step 4 image + line
        private PictureBox _previewBox;
        private Label _lineDetectStatus;
        private PointF? _clickP1, _clickP2;
        private PointF _detectedP1, _detectedP2;
        private bool _lineReady;
        private Bitmap _capturedImage;
        private Label _imuAngleZLabel;
        private double _capturedAngleZ;

        // Step 5 result
        private TextBox _resultBox;
        private double _computedPsiOffset;
        private bool _resultReady;

        public HeadingCalibrationPanelForm(string baseDirectory, TryGetImuAnglesDelegate imuProvider)
            : base("航向现场校核 (Step 0C · 现场)")
        {
            _pathService = new CalibrationPathService(baseDirectory);
            _imuProvider = imuProvider;

            CalibrationConfig cfg = LoadConfigSafe(_pathService.ConfigPath);

            BuildStep1_Prereq(cfg);
            BuildStep2_Mode();
            BuildStep3_Setup(cfg);
            BuildStep4_CaptureAndDetect();
            BuildStep5_Result(cfg);

            StartWizard();
        }

        private void BuildStep1_Prereq(CalibrationConfig cfg)
        {
            bool hasCamera = cfg.Fx > 0 && cfg.Fy > 0;
            bool hasMount = cfg.DeltaPitch != 0 || cfg.DeltaRoll != 0;

            var panel = BuildKvPanel(
                Tuple.Create("相机标定 (0A):", (Control)CreateInfoLabel(hasCamera ? "已完成" : "未完成")),
                Tuple.Create("安装角标定 (0B):", (Control)CreateInfoLabel(hasMount ? "已完成" : "未标定(默认0)")),
                Tuple.Create("当前 psi_offset:", (Control)CreateInfoLabel(cfg.PsiOffset.ToString("F4", CultureInfo.InvariantCulture) + "°")),
                Tuple.Create("磁偏角 D:", (Control)CreateInfoLabel(cfg.MagneticDeclination.ToString("F4", CultureInfo.InvariantCulture) + "°"))
            );

            AddStep(new WizardStep
            {
                Title = "前置检查",
                Instruction = "【现场校核】本页在现场校核/更新导入的出厂 ψ_offset，不依赖出厂校准台。\n" +
                    "需先『导入出厂校准』(含内参/安装角/ψ_offset)。有已知方向参考线时用模式A校核；无参考则沿用出厂值。",
                Content = panel
            });
        }

        private void BuildStep2_Mode()
        {
            _modeReference = new RadioButton
            {
                Text = "模式A: 已知方向参考线（高精度推荐）",
                AutoSize = true,
                Checked = true,
                ForeColor = ThemeText,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point)
            };
            _modeMagnetic = new RadioButton
            {
                Text = "模式B: 仅磁场定位（无参考线，精度受损）",
                AutoSize = true,
                ForeColor = ThemeText,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point)
            };

            var desc = CreateInfoLabel(
                "模式A: 在相机视野内放置一条已知精确方向的线（如沿真北的线），\n" +
                "系统将自动识别该线并计算航向偏移。精度高。\n\n" +
                "模式B: 仅使用磁场传感器（AngleZ + 磁偏角），无需参考线。\n" +
                "适用于无已知方向参考的场景，但精度下降。");
            desc.Padding = new Padding(20, 8, 0, 0);

            var panel = new Panel();
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12)
            };
            flow.Controls.Add(_modeReference);
            flow.Controls.Add(_modeMagnetic);
            flow.Controls.Add(desc);
            panel.Controls.Add(flow);

            AddStep(new WizardStep
            {
                Title = "选择标定模式",
                Instruction = "请选择航向标定方式。如果现场有已知方向参考线，推荐选择模式A。",
                Content = panel
            });
        }

        private void BuildStep3_Setup(CalibrationConfig cfg)
        {
            _azimuthBox = CreateEditBox("0.000");
            _magDecBox = CreateEditBox(cfg.MagneticDeclination.ToString("F4", CultureInfo.InvariantCulture));

            var panel = BuildKvPanel(
                Tuple.Create("参考线真方位角 α (°):", (Control)_azimuthBox),
                Tuple.Create("当地磁偏角 D (°):", (Control)_magDecBox)
            );

            AddStep(new WizardStep
            {
                Title = "配置参数",
                Instruction = "模式A: 输入参考线的真方位角（从真北顺时针量度），并确认磁偏角。\n" +
                    "模式B: 仅需确认磁偏角，方位角将忽略。\n\n" +
                    "操作: 将设备安装到三脚架上，在相机拍照视野内放置一条已知精确方向的线。",
                Content = panel,
                OnEnter = () =>
                {
                    _azimuthBox.Enabled = UseReferenceMode;
                },
                OnValidate = () =>
                {
                    double d;
                    if (!double.TryParse(_magDecBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                    {
                        SetStatus("磁偏角格式错误", false);
                        return false;
                    }
                    if (UseReferenceMode)
                    {
                        double a;
                        if (!double.TryParse(_azimuthBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out a))
                        {
                            SetStatus("方位角格式错误", false);
                            return false;
                        }
                    }
                    return true;
                }
            });
        }

        private void BuildStep4_CaptureAndDetect()
        {
            _previewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 30, 30),
                Cursor = Cursors.Cross
            };
            _previewBox.MouseClick += PreviewBox_MouseClick;

            _lineDetectStatus = CreateInfoLabel("请先加载图像");
            _lineDetectStatus.Dock = DockStyle.Top;

            _imuAngleZLabel = CreateInfoLabel("AngleZ: 等待...");
            _imuAngleZLabel.Dock = DockStyle.Top;

            var loadBtn = new Button { Text = "加载照片", Width = 100, Height = 28 };
            StyleButton(loadBtn, false);
            loadBtn.Click += (s, e) => LoadImage();

            var detectBtn = new Button { Text = "自动检测参考线", Width = 140, Height = 28 };
            StyleButton(detectBtn, true);
            detectBtn.Click += (s, e) => AutoDetectLine();

            var clearBtn = new Button { Text = "清除标记", Width = 100, Height = 28 };
            StyleButton(clearBtn, false);
            clearBtn.Click += (s, e) => ClearLineMarks();

            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(4, 2, 4, 2)
            };
            topBar.Controls.Add(loadBtn);
            topBar.Controls.Add(detectBtn);
            topBar.Controls.Add(clearBtn);

            var panel = new Panel();
            panel.Controls.Add(_previewBox);
            panel.Controls.Add(_lineDetectStatus);
            panel.Controls.Add(_imuAngleZLabel);
            panel.Controls.Add(topBar);

            AddStep(new WizardStep
            {
                Title = "采集照片并识别参考线",
                Instruction = UseReferenceMode
                    ? "操作步骤:\n① 点击加载照片选择已拍摄的照片\n② 点击自动检测参考线尝试自动识别\n③ 如自动检测不准确，可在图像上点击两个端点手动标记参考线\n④ 确认线段正确后继续"
                    : "模式B: 无需图像。系统将直接使用当前 IMU AngleZ。",
                Content = panel,
                OnEnter = () =>
                {
                    _lineReady = false;
                    _clickP1 = null;
                    _clickP2 = null;
                    NextButton.Enabled = !UseReferenceMode;

                    if (_imuTimer == null)
                    {
                        _imuTimer = new Timer { Interval = 300 };
                        _imuTimer.Tick += (s, e) =>
                        {
                            double ax, ay, az;
                            if (_imuProvider != null && _imuProvider(out ax, out ay, out az))
                            {
                                _capturedAngleZ = az;
                                _imuAngleZLabel.Text = "AngleZ: " + az.ToString("F4", CultureInfo.InvariantCulture) + "°";
                            }
                        };
                        _imuTimer.Start();
                        FormClosed += (s, e) => { _imuTimer.Stop(); _imuTimer.Dispose(); };
                    }
                },
                OnValidate = () =>
                {
                    if (UseReferenceMode && !_lineReady)
                    {
                        SetStatus("请先检测或手动标记参考线", false);
                        return false;
                    }
                    return true;
                }
            });
        }

        private void BuildStep5_Result(CalibrationConfig cfg)
        {
            _resultBox = CreateResultBox();
            var panel = new Panel();
            panel.Controls.Add(_resultBox);

            AddStep(new WizardStep
            {
                Title = "计算结果",
                Instruction = "以下为航向现场校核结果。确认无误后点击完成，更新配置中的 ψ_offset 与磁偏角 D。",
                Content = panel,
                OnEnter = () => ComputeHeading(cfg)
            });
        }

        private void LoadImage()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "选择相机拍摄的照片";
                dlg.Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    if (_capturedImage != null) _capturedImage.Dispose();
                    _capturedImage = new Bitmap(dlg.FileName);
                    _previewBox.Image = _capturedImage;
                    _lineDetectStatus.Text = string.Format("已加载: {0}  ({1}x{2})", Path.GetFileName(dlg.FileName), _capturedImage.Width, _capturedImage.Height);
                    _clickP1 = null;
                    _clickP2 = null;
                    _lineReady = false;
                    SetStatusNeutral("照片已加载，请检测参考线");
                }
                catch (Exception ex)
                {
                    SetStatus("加载图像失败: " + ex.Message, false);
                }
            }
        }

        private void AutoDetectLine()
        {
            if (_capturedImage == null)
            {
                SetStatus("请先加载照片", false);
                return;
            }

            SetStatusNeutral("正在检测参考线...");
            Application.DoEvents();

            ReferenceLineDetector.DetectedLine line;
            if (ReferenceLineDetector.TryDetect(_capturedImage, out line))
            {
                _detectedP1 = line.P1;
                _detectedP2 = line.P2;
                _lineReady = true;

                var annotated = ReferenceLineDetector.DrawLine(_capturedImage, line.P1, line.P2);
                _previewBox.Image = annotated;

                _lineDetectStatus.Text = string.Format(CultureInfo.InvariantCulture,
                    "检测到参考线: P1({0:F1},{1:F1}) -> P2({2:F1},{3:F1})  票数={4}",
                    line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, line.Votes);
                NextButton.Enabled = true;
                SetStatus("参考线检测成功！如不准确可在图上点击两点手动标记", true);
            }
            else
            {
                SetStatus("未检测到明显直线，请在图像上点击两个端点手动标记参考线", false);
            }
        }

        private void ClearLineMarks()
        {
            _clickP1 = null;
            _clickP2 = null;
            _lineReady = false;
            if (_capturedImage != null)
                _previewBox.Image = _capturedImage;
            _lineDetectStatus.Text = "已清除标记";
            NextButton.Enabled = !UseReferenceMode;
        }

        private void PreviewBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_capturedImage == null || !UseReferenceMode) return;

            float scaleX = (float)_capturedImage.Width / _previewBox.ClientSize.Width;
            float scaleY = (float)_capturedImage.Height / _previewBox.ClientSize.Height;
            float scale = Math.Max(scaleX, scaleY);

            float offsetX = (_previewBox.ClientSize.Width - _capturedImage.Width / scale) / 2f;
            float offsetY = (_previewBox.ClientSize.Height - _capturedImage.Height / scale) / 2f;

            float imgX = (e.X - offsetX) * scale;
            float imgY = (e.Y - offsetY) * scale;

            if (imgX < 0 || imgX >= _capturedImage.Width || imgY < 0 || imgY >= _capturedImage.Height)
                return;

            if (_clickP1 == null)
            {
                _clickP1 = new PointF(imgX, imgY);
                _lineDetectStatus.Text = string.Format(CultureInfo.InvariantCulture,
                    "P1({0:F1},{1:F1}) - 请点击第二个端点", imgX, imgY);
            }
            else
            {
                _clickP2 = new PointF(imgX, imgY);
                _detectedP1 = _clickP1.Value;
                _detectedP2 = _clickP2.Value;
                _lineReady = true;

                var annotated = ReferenceLineDetector.DrawLine(_capturedImage, _detectedP1, _detectedP2);
                _previewBox.Image = annotated;

                _lineDetectStatus.Text = string.Format(CultureInfo.InvariantCulture,
                    "手动标记: P1({0:F1},{1:F1}) -> P2({2:F1},{3:F1})",
                    _detectedP1.X, _detectedP1.Y, _detectedP2.X, _detectedP2.Y);
                NextButton.Enabled = true;
                SetStatus("参考线已标记，可继续", true);

                _clickP1 = null;
                _clickP2 = null;
            }
        }

        private void ComputeHeading(CalibrationConfig cfg)
        {
            try
            {
                double magDec = double.Parse(_magDecBox.Text, CultureInfo.InvariantCulture);
                var calibrator = new InstrumentCalibrator();

                if (UseReferenceMode)
                {
                    double knownAzimuth = double.Parse(_azimuthBox.Text, CultureInfo.InvariantCulture);
                    _computedPsiOffset = calibrator.CalibrateHeadingOffset(
                        _capturedAngleZ, magDec,
                        _detectedP1.X, _detectedP1.Y, _detectedP2.X, _detectedP2.Y,
                        knownAzimuth);

                    double predicted = calibrator.Verify(
                        _capturedAngleZ, magDec, _computedPsiOffset,
                        _detectedP1.X, _detectedP1.Y, _detectedP2.X, _detectedP2.Y);
                    double error = knownAzimuth - predicted;

                    _resultBox.Text = string.Format(CultureInfo.InvariantCulture,
                        "=== 航向标定结果 (模式A: 已知方向参考) ===\r\n\r\n" +
                        "IMU AngleZ = {0:F4}°\r\n" +
                        "磁偏角 D = {1:F4}°\r\n" +
                        "参考线: P1({2:F1},{3:F1}) -> P2({4:F1},{5:F1})\r\n" +
                        "已知方位角 α = {6:F4}°\r\n\r\n" +
                        "计算得 psi_offset = {7:F4}°\r\n" +
                        "验证预测方位角 = {8:F4}°\r\n" +
                        "误差 = {9:F4}°\r\n\r\n" +
                        "旧值: {10:F4}°\r\n",
                        _capturedAngleZ, magDec,
                        _detectedP1.X, _detectedP1.Y, _detectedP2.X, _detectedP2.Y,
                        knownAzimuth, _computedPsiOffset, predicted, error,
                        cfg.PsiOffset);
                }
                else
                {
                    _computedPsiOffset = cfg.PsiOffset;

                    _resultBox.Text = string.Format(CultureInfo.InvariantCulture,
                        "=== 航向标定结果 (模式B: 仅磁场定位) ===\r\n\r\n" +
                        "IMU AngleZ = {0:F4}°\r\n" +
                        "磁偏角 D = {1:F4}°\r\n" +
                        "psi_true = AngleZ + D = {2:F4}°\r\n\r\n" +
                        "psi_offset 保持现有值: {3:F4}°\r\n" +
                        "（无已知方向参考，无法更新 psi_offset）\r\n",
                        _capturedAngleZ, magDec, _capturedAngleZ + magDec, cfg.PsiOffset);
                }

                _resultReady = true;
                SetStatus("计算完成，点击完成保存", true);
            }
            catch (Exception ex)
            {
                _resultBox.Text = "计算失败:\r\n" + ex.Message;
                _resultReady = false;
                SetStatus("计算失败: " + ex.Message, false);
            }
        }

        protected override void OnWizardFinish()
        {
            try
            {
                var config = LoadConfigSafe(_pathService.ConfigPath);
                double magDec;
                if (double.TryParse(_magDecBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out magDec))
                    config.MagneticDeclination = magDec;

                if (UseReferenceMode && _resultReady)
                    config.PsiOffset = _computedPsiOffset;

                config.Save(_pathService.ConfigPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (_capturedImage != null)
                {
                    _capturedImage.Dispose();
                    _capturedImage = null;
                }
            }
        }
    }
}
