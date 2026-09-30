using System;
using System.Collections.Generic;
using CalibrationEngine.Models;

namespace CalibrationEngine
{
    /// <summary>四个标定步骤的纯数学实现。PnP/角点检测由采集端提供，此处只做解算/判据/统计。</summary>
    public static class Steps
    {
        private const double Deg2Rad = Math.PI / 180.0;

        // ---------- 角度工具 ----------
        public static double NormDeg(double a)
        {
            while (a > 180.0) a -= 360.0;
            while (a < -180.0) a += 360.0;
            return a;
        }

        /// <summary>角度圆均值(°)。</summary>
        public static double CircularMean(IEnumerable<double> anglesDeg)
        {
            double sx = 0, sy = 0; int n = 0;
            foreach (var a in anglesDeg) { sx += Math.Cos(a * Deg2Rad); sy += Math.Sin(a * Deg2Rad); n++; }
            if (n == 0) return 0;
            return Math.Atan2(sy, sx) / Deg2Rad;
        }

        // ---------- Step 0A ----------
        public static Step0AOutput Run0A(Step0AInput inp)
        {
            var o = new Step0AOutput { Intrinsics = inp.Intrinsics, ReprojRms = inp.ReprojRms };
            var k = inp.Intrinsics;
            if (k == null || k.Fx <= 0 || k.Fy <= 0 || k.ImageWidth <= 0 || k.ImageHeight <= 0)
            {
                o.Passed = false; o.Message = "内参非法(Fx/Fy/ImageWidth/ImageHeight 必须为正)。"; return o;
            }
            if (inp.ReprojRms >= 0 && inp.ReprojRms > inp.RmsLimitPx)
            {
                o.Passed = false; o.Message = string.Format("重投影 RMS {0:F3} px 超阈 {1:F3} px。", inp.ReprojRms, inp.RmsLimitPx); return o;
            }
            o.Passed = true;
            o.Message = inp.ReprojRms >= 0
                ? string.Format("内参合格，RMS={0:F3} px。", inp.ReprojRms)
                : "内参已接受(未提供 RMS)。";
            return o;
        }

        // ---------- Step 0B ----------
        public static Step0BOutput Run0B(Step0BInput inp)
        {
            var o = new Step0BOutput();
            if (inp.Samples == null || inp.Samples.Count == 0)
            {
                o.Passed = false; o.Message = "无安装角样本。"; return o;
            }
            var dp = new List<double>(); var dr = new List<double>();
            foreach (var s in inp.Samples)
            {
                dp.Add(s.PitchCamDeg - s.ImuAngleYDeg);
                dr.Add(s.RollCamDeg - s.ImuAngleXDeg);
            }
            double mp = Mean(dp), mr = Mean(dr);
            double maxDev = 0;
            for (int i = 0; i < dp.Count; i++)
            {
                maxDev = Math.Max(maxDev, Math.Abs(dp[i] - mp));
                maxDev = Math.Max(maxDev, Math.Abs(dr[i] - mr));
            }
            o.DeltaPitch = mp; o.DeltaRoll = mr; o.MaxDeviationDeg = maxDev;
            o.SampleCount = inp.Samples.Count;
            o.Passed = maxDev <= inp.ToleranceDeg;
            o.Message = string.Format("δ_pitch={0:F4}°, δ_roll={1:F4}°, 最大偏差={2:F4}° ({3})",
                mp, mr, maxDev, o.Passed ? "合格" : "超阈 " + inp.ToleranceDeg.ToString("F3") + "°");
            return o;
        }

        // ---------- Step 0C ----------
        public static Step0COutput Run0C(Step0CInput inp)
        {
            var o = new Step0COutput { PerAzimuth = new List<HeadingPerAzimuth>() };
            if (inp.Samples == null || inp.Samples.Count == 0)
            {
                o.Passed = false; o.Message = "无航向样本。"; return o;
            }
            var psi = new List<double>();
            foreach (var s in inp.Samples)
            {
                double psiCam = inp.AlphaBoardDeg - s.ThetaImgBoardDeg;
                double psiImu = s.ImuAngleZDeg + inp.MagneticDeclinationDeg;
                psi.Add(NormDeg(psiCam - psiImu));
            }
            double mean = CircularMean(psi);
            double maxRes = 0;
            for (int i = 0; i < psi.Count; i++)
            {
                double res = NormDeg(psi[i] - mean);
                maxRes = Math.Max(maxRes, Math.Abs(res));
                o.PerAzimuth.Add(new HeadingPerAzimuth
                {
                    NominalAzimuthDeg = inp.Samples[i].NominalAzimuthDeg,
                    PsiOffsetDeg = psi[i],
                    ResidualDeg = res
                });
            }
            o.PsiOffsetDeg = NormDeg(mean);
            o.ResidualDeg = maxRes;
            o.Passed = maxRes <= inp.ToleranceDeg;
            o.Message = string.Format("ψ_offset={0:F4}°, 最大残差={1:F4}° ({2}), 方位数={3}",
                o.PsiOffsetDeg, maxRes, o.Passed ? "合格" : "超阈", inp.Samples.Count);
            return o;
        }

        // ---------- Step 0D ----------
        public static Step0DOutput Run0D(Step0DInput inp)
        {
            var o = new Step0DOutput { PerAzimuth = new List<VerifyPerAzimuth>() };
            if (inp.Frames == null || inp.Frames.Count == 0)
            {
                o.Passed = false; o.Message = "无验证帧。"; return o;
            }
            var k = inp.Intrinsics;
            // 按名义方位分组
            var groups = new Dictionary<double, List<VerifyFrame>>();
            foreach (var f in inp.Frames)
            {
                if (!groups.ContainsKey(f.NominalAzimuthDeg)) groups[f.NominalAzimuthDeg] = new List<VerifyFrame>();
                groups[f.NominalAzimuthDeg].Add(f);
            }
            var sysE = new List<double>(); var sysN = new List<double>();
            double closureMax = 0, closureSum = 0; int azCount = 0;
            foreach (var kv in groups)
            {
                double sumSE = 0, sumSN = 0, sumPE = 0, sumPN = 0, sumMagErr = 0; int n = 0;
                foreach (var f in kv.Value)
                {
                    ComputeSysOffset(f, k, inp.DeltaPitch, inp.DeltaRoll, inp.PsiOffsetDeg,
                        inp.MagneticDeclinationDeg, out double dE, out double dN);
                    sumSE += dE; sumSN += dN; sumPE += f.OffsetPnpE_mm; sumPN += f.OffsetPnpN_mm;
                    // 磁航向误差 = (AngleZ+D+ψ_offset) - 板真方位
                    double magHead = f.ImuAngleZDeg + inp.MagneticDeclinationDeg + inp.PsiOffsetDeg;
                    sumMagErr += NormDeg(magHead - inp.AlphaBoardDeg);
                    n++;
                }
                double mSE = sumSE / n, mSN = sumSN / n, mPE = sumPE / n, mPN = sumPN / n;
                double closure = Math.Sqrt((mSE - mPE) * (mSE - mPE) + (mSN - mPN) * (mSN - mPN));
                closureMax = Math.Max(closureMax, closure); closureSum += closure; azCount++;
                sysE.Add(mSE); sysN.Add(mSN);
                o.PerAzimuth.Add(new VerifyPerAzimuth
                {
                    NominalAzimuthDeg = kv.Key, FrameCount = n,
                    OffsetSysE_mm = mSE, OffsetSysN_mm = mSN,
                    OffsetPnpE_mm = mPE, OffsetPnpN_mm = mPN,
                    Closure_mm = closure, MagHeadingErrDeg = sumMagErr / n
                });
            }
            // 旋转一致性: 各方位 offset_sys 到质心的 RMS 距离
            double cE = Mean(sysE), cN = Mean(sysN), sq = 0;
            for (int i = 0; i < sysE.Count; i++)
                sq += (sysE[i] - cE) * (sysE[i] - cE) + (sysN[i] - cN) * (sysN[i] - cN);
            o.SigmaRot_mm = sysE.Count > 0 ? Math.Sqrt(sq / sysE.Count) : 0;
            o.ClosureMax_mm = closureMax;
            o.ClosureMean_mm = azCount > 0 ? closureSum / azCount : 0;
            // 预测最终误差: 闭合均值与 δψ=1°×|offset| 主项 RSS
            double offMag = Math.Sqrt(cE * cE + cN * cN);
            double aHeading = offMag * Math.Sin(1.0 * Deg2Rad);
            o.PredictedErr_mm = Math.Sqrt(o.ClosureMean_mm * o.ClosureMean_mm + aHeading * aHeading);
            o.Passed = o.SigmaRot_mm <= inp.SigmaRotLimit_mm
                       && o.ClosureMax_mm <= inp.ClosureLimit_mm
                       && o.ClosureMean_mm <= inp.ClosureMeanLimit_mm;
            o.Message = string.Format("σ_rot={0:F3}mm, 闭合max={1:F3}mm, 闭合均值={2:F3}mm, 预测={3:F3}mm ({4})",
                o.SigmaRot_mm, o.ClosureMax_mm, o.ClosureMean_mm, o.PredictedErr_mm, o.Passed ? "放行" : "未通过");
            return o;
        }

        /// <summary>现场简化算法(Step 4-6): 像素→相机系水平位移→倾斜修正→heading 旋转到 ENU。</summary>
        public static void ComputeSysOffset(VerifyFrame f, CameraIntrinsics k,
            double deltaPitch, double deltaRoll, double psiOffset, double magDecl,
            out double dE, out double dN)
        {
            double dx = (f.UCorrPx - k.Cx) / k.Fx * f.H_mm;
            double dy = (f.VCorrPx - k.Cy) / k.Fy * f.H_mm;
            dx += f.H_mm * Math.Tan((f.ImuAngleYDeg + deltaPitch) * Deg2Rad);
            dy += f.H_mm * Math.Tan((f.ImuAngleXDeg + deltaRoll) * Deg2Rad);
            double heading = (f.ImuAngleZDeg + magDecl + psiOffset) * Deg2Rad;
            dE = dx * Math.Cos(heading) + dy * Math.Sin(heading);
            dN = -dx * Math.Sin(heading) + dy * Math.Cos(heading);
        }

        private static double Mean(List<double> v)
        {
            if (v.Count == 0) return 0; double s = 0; foreach (var x in v) s += x; return s / v.Count;
        }
    }
}
