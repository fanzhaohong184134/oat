using System;
using System.Globalization;
using System.IO;
using System.Text;
using CalibrationEngine.Models;

namespace CalibrationEngine
{
    /// <summary>由 config + Step 0D 输出生成填好的出厂校准报告(Markdown)。</summary>
    public static class ReportWriter
    {
        public static string Build(CalibrationConfig cfg, Step0DOutput d)
        {
            var ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            string Mark(bool ok) => ok ? "✓ 合格" : "✗ 不合格";

            sb.AppendLine("# 数字对中仪 — 出厂校准报告");
            sb.AppendLine();
            sb.AppendLine("> 本报告由 CalibrationEngine 自动生成。判据见 校准与测量流程.md。");
            sb.AppendLine();

            sb.AppendLine("## 1. 设备与环境信息");
            sb.AppendLine();
            sb.AppendLine("| 项 | 内容 |");
            sb.AppendLine("|----|------|");
            sb.AppendLine(string.Format(ci, "| 设备编号 DeviceId | {0} |", NZ(cfg.DeviceId)));
            sb.AppendLine(string.Format(ci, "| 校准时间 (UTC) | {0} |", NZ(cfg.CalibratedAtUtc)));
            sb.AppendLine("| 型号 / 序列号 | ________ |");
            sb.AppendLine("| 操作员 | ________ |");
            sb.AppendLine("| 环境温度 / 湿度 | ______℃ / ______% |");
            sb.AppendLine();

            sb.AppendLine("## 2. 相机内参 (Step 0A)");
            sb.AppendLine();
            sb.AppendLine("| 参数 | 值 |");
            sb.AppendLine("|------|----|");
            sb.AppendLine(string.Format(ci, "| fx / fy | {0:F2} / {1:F2} |", cfg.Fx, cfg.Fy));
            sb.AppendLine(string.Format(ci, "| cx / cy | {0:F2} / {1:F2} |", cfg.Cx, cfg.Cy));
            sb.AppendLine(string.Format(ci, "| k1 / k2 / p1 / p2 | {0:F5} / {1:F5} / {2:F5} / {3:F5} |", cfg.K1, cfg.K2, cfg.P1, cfg.P2));
            sb.AppendLine(string.Format(ci, "| 图像宽×高 | {0} × {1} |", cfg.ImageWidth, cfg.ImageHeight));
            sb.AppendLine();

            sb.AppendLine("## 3. 安装角 (Step 0B)");
            sb.AppendLine();
            sb.AppendLine("| 参数 | 值 | 判据 |");
            sb.AppendLine("|------|----|------|");
            sb.AppendLine(string.Format(ci, "| δ_pitch | {0:F4}° | 各次偏差 ≤0.05° |", cfg.DeltaPitch));
            sb.AppendLine(string.Format(ci, "| δ_roll | {0:F4}° | 各次偏差 ≤0.05° |", cfg.DeltaRoll));
            sb.AppendLine();

            sb.AppendLine("## 4. 航向 ψ_offset (Step 0C)");
            sb.AppendLine();
            sb.AppendLine("| 参数 | 值 | 判据 |");
            sb.AppendLine("|------|----|------|");
            sb.AppendLine(string.Format(ci, "| α_board（板真方位） | {0:F2}° | — |", cfg.AlphaBoard));
            sb.AppendLine(string.Format(ci, "| 磁偏角 D | {0:F4}° | 东偏为正 |", cfg.MagneticDeclination));
            sb.AppendLine(string.Format(ci, "| ψ_offset | {0:F4}° | 残差 <0.1° |", cfg.PsiOffset));
            sb.AppendLine();

            sb.AppendLine("## 5. 综合验证 (Step 0D)");
            sb.AppendLine();
            sb.AppendLine("| 指标 | 值 | 判据 | 结果 |");
            sb.AppendLine("|------|----|------|------|");
            if (d != null)
            {
                sb.AppendLine(string.Format(ci, "| 旋转一致性 σ_rot | {0:F3} mm | <0.3 mm | {1} |", d.SigmaRot_mm, Mark(d.SigmaRot_mm < 0.3)));
                sb.AppendLine(string.Format(ci, "| 闭合 max / 均值 | {0:F3} / {1:F3} mm | <0.5 / <0.3 mm | {2} |",
                    d.ClosureMax_mm, d.ClosureMean_mm, Mark(d.ClosureMax_mm < 0.5 && d.ClosureMean_mm < 0.3)));
                sb.AppendLine(string.Format(ci, "| 预测最终误差 | {0:F3} mm | <1 mm | {1} |", d.PredictedErr_mm, Mark(d.PredictedErr_mm < 1.0)));
                sb.AppendLine(string.Format(ci, "| **放行** | | 以上全通过 | **{0}** |", d.Passed ? "PASS ✓" : "FAIL ✗"));
                sb.AppendLine();
                if (d.PerAzimuth != null && d.PerAzimuth.Count > 0)
                {
                    sb.AppendLine("### 各方位明细");
                    sb.AppendLine();
                    sb.AppendLine("| 方位° | 帧数 | offset_sys(E,N) mm | offset_pnp(E,N) mm | 闭合 mm | 磁航向误差° |");
                    sb.AppendLine("|-------|------|--------------------|--------------------|---------|-------------|");
                    foreach (var p in d.PerAzimuth)
                        sb.AppendLine(string.Format(ci, "| {0:F0} | {1} | ({2:F2},{3:F2}) | ({4:F2},{5:F2}) | {6:F3} | {7:F2} |",
                            p.NominalAzimuthDeg, p.FrameCount, p.OffsetSysE_mm, p.OffsetSysN_mm,
                            p.OffsetPnpE_mm, p.OffsetPnpN_mm, p.Closure_mm, p.MagHeadingErrDeg));
                    sb.AppendLine();
                }
            }
            else sb.AppendLine("| (无 0D 结果) | | | |");
            sb.AppendLine();

            sb.AppendLine("## 6. 配置校验");
            sb.AppendLine();
            sb.AppendLine(string.Format(ci, "- 内参齐全: {0}", cfg.Fx > 0 && cfg.Fy > 0 ? "✓" : "✗"));
            sb.AppendLine(string.Format(ci, "- 安装角/航向/磁偏角: δ={0:F4}/{1:F4}°, ψ={2:F4}°, D={3:F4}°", cfg.DeltaPitch, cfg.DeltaRoll, cfg.PsiOffset, cfg.MagneticDeclination));
            sb.AppendLine(string.Format(ci, "- HeightH = {0:F1} mm {1}", cfg.HeightH, cfg.HeightH > 0 ? "" : "（出厂为 0，现场测量后由 dsat 导入面板录入）"));
            sb.AppendLine();

            sb.AppendLine("## 7. 结论与签署");
            sb.AppendLine();
            sb.AppendLine(string.Format(ci, "综合结论：{0}", d != null && d.Passed ? "□ 合格放行（0D PASS）" : "□ 合格放行　□ 不合格"));
            sb.AppendLine();
            sb.AppendLine("| 角色 | 签名 | 日期 |");
            sb.AppendLine("|------|------|------|");
            sb.AppendLine("| 校准员 | | |");
            sb.AppendLine("| 复核 | | |");
            sb.AppendLine("| 质量 | | |");
            sb.AppendLine();
            sb.AppendLine("## 8. 现场使用须知");
            sb.AppendLine();
            sb.AppendLine("1. dsat『导入出厂校准 / 现场参数(H·D)』导入本配置。");
            sb.AppendLine("2. **每次架站必须现场测量并录入 H**，否则后处理偏移为 0。");
            sb.AppendLine("3. 按测站确认磁偏角 D；磁扰现场可执行 0C 航向现场校核（模式 A）。");
            sb.AppendLine("4. 0A 内参 / 0B 安装角为出厂只读项，重标须返厂。");

            return sb.ToString();
        }

        private static string NZ(string s) => string.IsNullOrEmpty(s) ? "(无)" : s;
    }
}
