using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using dsat.DataProcessing.Calibration;

namespace dsat.CalibrationPanels
{
    /// <summary>
    /// 导入出厂校准数据(CalibrationBench 产出的 calibration_config.json) + 现场参数录入(H、D)。
    /// 出厂标定(内参/安装角/航向)由校准台工装完成并导入；现场每次架站录入 H、磁偏角 D。
    /// </summary>
    public class CalibrationImportPanelForm : CalibrationWizardBaseForm
    {
        private readonly CalibrationPathService _pathService;
        private TextBox _srcBox;
        private TextBox _summaryBox;
        private TextBox _hBox;
        private TextBox _dBox;
        private bool _imported;

        public CalibrationImportPanelForm(string baseDirectory)
            : base("导入出厂校准 + 现场参数")
        {
            _pathService = new CalibrationPathService(baseDirectory);
            BuildStep1_Import();
            BuildStep2_FieldParams();
            StartWizard();
        }

        private void BuildStep1_Import()
        {
            var content = new Panel { Dock = DockStyle.Fill, BackColor = ThemePanelBackground };

            _srcBox = CreateReadOnlyBox("", 420);
            _srcBox.Left = 12; _srcBox.Top = 12;
            var browse = new Button { Text = "浏览...", Left = 440, Top = 10, Width = 70, Height = 26 };
            StyleButton(browse, false);
            browse.Click += (s, e) =>
            {
                using (var d = new OpenFileDialog { Filter = "校准配置|calibration_config.json;*.json", Title = "选择出厂校准 calibration_config.json" })
                    if (d.ShowDialog(this) == DialogResult.OK) _srcBox.Text = d.FileName;
            };
            var importBtn = new Button { Text = "导入并写入", Left = 520, Top = 10, Width = 100, Height = 26 };
            StyleButton(importBtn, true);
            importBtn.Click += (s, e) => DoImport();

            _summaryBox = CreateResultBox();
            _summaryBox.Dock = DockStyle.None;
            _summaryBox.Left = 12; _summaryBox.Top = 48; _summaryBox.Width = 608; _summaryBox.Height = 260;

            content.Controls.Add(_srcBox);
            content.Controls.Add(browse);
            content.Controls.Add(importBtn);
            content.Controls.Add(_summaryBox);

            AddStep(new WizardStep
            {
                Title = "导入出厂校准",
                Instruction = "选择校准台工装(CalibrationBench)产出的 calibration_config.json，导入内参/安装角/航向。\n" +
                              "0A 内参、0B 安装角、0C 航向为出厂标定项，现场不再执行，仅导入。",
                Content = content,
                OnValidate = () =>
                {
                    if (!_imported) { SetStatus("请先导入有效的校准配置", false); return false; }
                    return true;
                }
            });
        }

        private void BuildStep2_FieldParams()
        {
            var content = new Panel { Dock = DockStyle.Fill, BackColor = ThemePanelBackground };

            var lbH = CreateInfoLabel("相机到地面高度 H (mm)：");
            lbH.Left = 12; lbH.Top = 20;
            _hBox = CreateEditBox("", 160); _hBox.Left = 220; _hBox.Top = 16;

            var lbD = CreateInfoLabel("当地磁偏角 D (°，东偏为正)：");
            lbD.Left = 12; lbD.Top = 60;
            _dBox = CreateEditBox("", 160); _dBox.Left = 220; _dBox.Top = 56;

            var note = CreateInfoLabel(
                "说明：H、D 为现场变量，每次架站录入（出厂已含 ψ_offset/D 等，H 需现场测量）。\n" +
                "H 用激光测距获取；未录入(H=0)时后处理偏移计算结果为 0。\n" +
                "点『完成』写入 calibration_config.json。");
            note.Left = 12; note.Top = 150;

            var checkBtn = new Button { Text = "一键校验完整性", Left = 12, Top = 104, Width = 150, Height = 28 };
            StyleButton(checkBtn, false);
            checkBtn.Click += (s, e) => ShowCompleteness();

            content.Controls.Add(lbH); content.Controls.Add(_hBox);
            content.Controls.Add(lbD); content.Controls.Add(_dBox);
            content.Controls.Add(checkBtn);
            content.Controls.Add(note);

            AddStep(new WizardStep
            {
                Title = "现场参数 (H / D)",
                Instruction = "录入本测站的相机高度 H 与磁偏角 D。",
                Content = content,
                OnEnter = () =>
                {
                    var cfg = LoadConfigSafe(_pathService.ConfigPath);
                    if (string.IsNullOrEmpty(_hBox.Text))
                        _hBox.Text = cfg.HeightH.ToString("F1", CultureInfo.InvariantCulture);
                    if (string.IsNullOrEmpty(_dBox.Text))
                        _dBox.Text = cfg.MagneticDeclination.ToString("F4", CultureInfo.InvariantCulture);
                },
                OnValidate = () =>
                {
                    double h, d;
                    if (!double.TryParse(_hBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out h) || h < 0)
                    { SetStatus("H 格式错误(应为非负数值，mm)", false); return false; }
                    if (!double.TryParse(_dBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                    { SetStatus("磁偏角 D 格式错误", false); return false; }
                    // 完成时自动校验完整性，并拦截 H 缺失
                    if (h <= 0)
                    {
                        var r = MessageBox.Show(
                            "H=0（未录入相机高度）。后处理偏移计算结果将为 0。\n\n是否仍要完成？(建议：先测量并录入 H)",
                            "完整性拦截", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (r != DialogResult.Yes) { SetStatus("已拦截：请录入现场高度 H", false); return false; }
                    }
                    return true;
                }
            });
        }

        private void DoImport()
        {
            string src = _srcBox.Text;
            if (string.IsNullOrWhiteSpace(src) || !File.Exists(src))
            { SetStatus("请选择存在的 calibration_config.json", false); return; }

            CalibrationConfig imp;
            try { imp = CalibrationConfig.Load(src); }
            catch (Exception ex) { SetStatus("解析失败：" + ex.Message, false); return; }

            if (!(imp.Fx > 0 && imp.Fy > 0))
            { SetStatus("无有效内参(Fx/Fy)，请确认是完成 0A 的配置", false); return; }

            // 合并：保留本机已测量的 H / D(若导入值为空)
            var existing = LoadConfigSafe(_pathService.ConfigPath);
            if (imp.HeightH <= 0 && existing.HeightH > 0) imp.HeightH = existing.HeightH;
            if (Math.Abs(imp.MagneticDeclination) < 1e-9 && Math.Abs(existing.MagneticDeclination) > 1e-9)
                imp.MagneticDeclination = existing.MagneticDeclination;

            try { imp.Save(_pathService.ConfigPath); }
            catch (Exception ex) { SetStatus("写入配置失败：" + ex.Message, false); return; }

            _imported = true;
            bool hWarn = imp.HeightH <= 0;

            _summaryBox.Text =
                "══ 已导入并写入 calibration_config.json ══\r\n" +
                string.Format(CultureInfo.InvariantCulture, "设备编号: {0}\r\n", string.IsNullOrEmpty(imp.DeviceId) ? "(无)" : imp.DeviceId) +
                string.Format(CultureInfo.InvariantCulture, "标定时间: {0}\r\n", string.IsNullOrEmpty(imp.CalibratedAtUtc) ? "(无)" : imp.CalibratedAtUtc) +
                "──────────────────────────\r\n" +
                string.Format(CultureInfo.InvariantCulture, "内参 fx={0:F2} fy={1:F2} cx={2:F2} cy={3:F2}\r\n", imp.Fx, imp.Fy, imp.Cx, imp.Cy) +
                string.Format(CultureInfo.InvariantCulture, "畸变 k1={0:F4} k2={1:F4} p1={2:F4} p2={3:F4}\r\n", imp.K1, imp.K2, imp.P1, imp.P2) +
                string.Format(CultureInfo.InvariantCulture, "图像 {0}x{1}\r\n", imp.ImageWidth, imp.ImageHeight) +
                string.Format(CultureInfo.InvariantCulture, "安装角 δ_pitch={0:F4}°  δ_roll={1:F4}°\r\n", imp.DeltaPitch, imp.DeltaRoll) +
                string.Format(CultureInfo.InvariantCulture, "航向 ψ_offset={0:F4}°  (板真方位 α={1:F2}°)\r\n", imp.PsiOffset, imp.AlphaBoard) +
                string.Format(CultureInfo.InvariantCulture, "磁偏角 D={0:F4}°\r\n", imp.MagneticDeclination) +
                string.Format(CultureInfo.InvariantCulture, "高度 H={0:F1} mm  {1}\r\n", imp.HeightH, hWarn ? "← 需在下一步录入" : "") +
                "──────────────────────────\r\n" +
                (TryLoadVerification(Path.GetDirectoryName(Path.GetFullPath(src)), imp.DeviceId)
                    ?? "0D 放行报告: 未找到(可选，见工装 device_info/<设备号>/factory_verification/)") + "\r\n" +
                (hWarn ? "⚠ H=0：请在『下一步』录入现场高度 H，否则后处理偏移为 0。" : "✓ 参数完整。");

            SetStatus(hWarn ? "导入成功(注意：H=0，下一步录入)" : "导入成功", !hWarn);
            _hBox.Text = imp.HeightH.ToString("F1", CultureInfo.InvariantCulture);
            _dBox.Text = imp.MagneticDeclination.ToString("F4", CultureInfo.InvariantCulture);
        }

        /// <summary>一键校验配置完整性（内参/δ/ψ/D/H 是否齐全）。</summary>
        private void ShowCompleteness()
        {
            var cfg = LoadConfigSafe(_pathService.ConfigPath);
            double h, d;
            double hv = double.TryParse(_hBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out h) ? h : cfg.HeightH;
            double dv = double.TryParse(_dBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out d) ? d : cfg.MagneticDeclination;

            bool okK = cfg.Fx > 0 && cfg.Fy > 0 && cfg.Cx > 0 && cfg.Cy > 0;
            bool okDelta = okK; // 内参存在即视为出厂配置已导入(δ 含其中，δ=0 亦为有效标定值)
            bool okPsi = Math.Abs(cfg.PsiOffset) > 1e-9 || Math.Abs(cfg.AlphaBoard) > 1e-9;
            bool okDdecl = Math.Abs(dv) > 1e-9;
            bool okH = hv > 0;

            string Mark(bool ok, bool warnOnly = false) => ok ? "✓" : (warnOnly ? "⚠" : "✗");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("══ 配置完整性校验 ══");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} 相机内参  fx={1:F1} fy={2:F1} cx={3:F1} cy={4:F1}", Mark(okK), cfg.Fx, cfg.Fy, cfg.Cx, cfg.Cy));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} 安装角 δ  pitch={1:F4}° roll={2:F4}°", Mark(okDelta), cfg.DeltaPitch, cfg.DeltaRoll));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} 航向 ψ_offset={1:F4}°{2}", Mark(okPsi, true), cfg.PsiOffset, okPsi ? "" : "  (为0，如未做0C可现场校核)"));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} 磁偏角 D={1:F4}°{2}", Mark(okDdecl, true), dv, okDdecl ? "" : "  (为0，请确认测站磁偏角)"));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} 高度 H={1:F1} mm{2}", Mark(okH), hv, okH ? "" : "  (必填！否则后处理偏移为0)"));
            sb.AppendLine("──────────────────────────");

            bool complete = okK && okH; // 阻断性: 内参 + H
            sb.AppendLine(complete
                ? (okPsi && okDdecl ? "结论: ✓ 配置完整，可进行测量与后处理。" : "结论: ✓ 关键项齐全(ψ/D 为可选警示)。")
                : "结论: ✗ 配置不完整，请补齐上面标 ✗ 的项。");

            MessageBox.Show(sb.ToString(), "配置完整性校验",
                MessageBoxButtons.OK, complete ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            SetStatus(complete ? "校验通过" : "校验未通过：存在缺失项", complete);
        }

        protected override void OnWizardFinish()
        {
            try
            {
                var cfg = LoadConfigSafe(_pathService.ConfigPath);
                double h, d;
                if (double.TryParse(_hBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out h)) cfg.HeightH = h;
                if (double.TryParse(_dBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) cfg.MagneticDeclination = d;
                cfg.Save(_pathService.ConfigPath);
            }
            catch { /* 保存失败不阻断关闭 */ }
        }

        /// <summary>在 config 同级 device_info/&lt;设备号&gt;/factory_verification/output 下查找最新 0D 报告并摘要。</summary>
        private static string TryLoadVerification(string configDir, string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(configDir) || string.IsNullOrEmpty(deviceId)) return null;
                string dir = Path.Combine(configDir, "device_info", deviceId, "factory_verification", "output");
                if (!Directory.Exists(dir)) return null;
                string[] files = Directory.GetFiles(dir, "*.json");
                if (files.Length == 0) return null;
                string latest = null; DateTime best = DateTime.MinValue;
                foreach (var f in files) { var t = File.GetLastWriteTime(f); if (t > best) { best = t; latest = f; } }
                using (var fs = File.OpenRead(latest))
                {
                    var ser = new DataContractJsonSerializer(typeof(VerificationReport));
                    var r = (VerificationReport)ser.ReadObject(fs);
                    return string.Format(CultureInfo.InvariantCulture,
                        "0D 放行: {0}   σ_rot={1:F3}mm  闭合max={2:F3}mm  均值={3:F3}mm  预测={4:F3}mm",
                        r.Passed ? "PASS ✓" : "FAIL ✗", r.SigmaRot_mm, r.ClosureMax_mm, r.ClosureMean_mm, r.PredictedErr_mm);
                }
            }
            catch { return null; }
        }

        [DataContract]
        private sealed class VerificationReport
        {
#pragma warning disable 0649
            [DataMember] public bool Passed;
            [DataMember] public double SigmaRot_mm;
            [DataMember] public double ClosureMax_mm;
            [DataMember] public double ClosureMean_mm;
            [DataMember] public double PredictedErr_mm;
#pragma warning restore 0649
        }
    }
}
