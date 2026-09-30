using System;
using System.Collections.Generic;
using System.IO;

namespace CalibrationBench.UI.Acquisition
{
    /// <summary>
    /// 手动挪位采样：无旋转台时，操作员人工把设备转/挪到各位置，逐站触发采集，累积后生成输入。
    /// 与自动 step-and-stare 产出同样的引擎输入契约(0B/0C/0D)。
    /// </summary>
    public sealed class ManualAcquisition
    {
        private readonly ISceneCapture _scene;
        private readonly Action<string> _log;

        public readonly Step0CInput C;
        public readonly Step0DInput D;
        public readonly Step0BInput B;

        public int StationCount { get; private set; }
        public int MountingFrameCount { get; private set; }

        public ManualAcquisition(ISceneCapture scene, AcquisitionSettings s, Action<string> log)
        {
            _scene = scene; _log = log;
            C = new Step0CInput
            {
                DeviceId = s.DeviceId, AlphaBoardDeg = s.SimAlphaBoardDeg,
                MagneticDeclinationDeg = s.SimDeclinationDeg, ToleranceDeg = 0.1,
                Samples = new List<HeadingSample>()
            };
            D = new Step0DInput
            {
                DeviceId = s.DeviceId, Intrinsics = s.SimIntrinsics(),
                DeltaPitch = s.SimDeltaPitchDeg, DeltaRoll = s.SimDeltaRollDeg,
                PsiOffsetDeg = s.SimPsiOffsetDeg, MagneticDeclinationDeg = s.SimDeclinationDeg,
                AlphaBoardDeg = s.SimAlphaBoardDeg, Frames = new List<VerifyFrame>(),
                SigmaRotLimit_mm = 0.3, ClosureLimit_mm = 0.5, ClosureMeanLimit_mm = 0.3
            };
            B = new Step0BInput { DeviceId = s.DeviceId, ToleranceDeg = 0.05, Samples = new List<MountingSample>() };
        }

        /// <summary>采集当前位置(一站)：用于 0C/0D。nominalAz 仅作分组标签。</summary>
        public void CaptureStation(double nominalAz, int frames)
        {
            double sumT = 0, sumZ = 0; int n = 0;
            for (int i = 0; i < frames; i++)
            {
                var f = _scene.Capture(nominalAz);
                sumT += f.ThetaImgBoardDeg; sumZ += f.ImuAngleZDeg; n++;
                D.Frames.Add(new VerifyFrame
                {
                    NominalAzimuthDeg = nominalAz, UCorrPx = f.UCorrPx, VCorrPx = f.VCorrPx, H_mm = f.H_mm,
                    ImuAngleXDeg = f.ImuAngleXDeg, ImuAngleYDeg = f.ImuAngleYDeg, ImuAngleZDeg = f.ImuAngleZDeg,
                    OffsetPnpE_mm = f.OffsetPnpE_mm, OffsetPnpN_mm = f.OffsetPnpN_mm
                });
            }
            C.Samples.Add(new HeadingSample { NominalAzimuthDeg = nominalAz, ThetaImgBoardDeg = sumT / n, ImuAngleZDeg = sumZ / n });
            StationCount++;
            _log(string.Format("  本站 {0:F0}° 采 {1} 帧；累计站数 {2}", nominalAz, n, StationCount));
        }

        /// <summary>采集当前静止(追加 0B 帧)。</summary>
        public void CaptureMounting(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                var f = _scene.Capture(0);
                B.Samples.Add(new MountingSample
                {
                    PitchCamDeg = f.PitchCamDeg, RollCamDeg = f.RollCamDeg,
                    ImuAngleYDeg = f.ImuAngleYDeg, ImuAngleXDeg = f.ImuAngleXDeg
                });
                MountingFrameCount++;
            }
            _log(string.Format("  0B 静止追加 {0} 帧；累计 {1}", frames, MountingFrameCount));
        }

        public void SaveRotation(string outDir, out string p0c, out string p0d)
        {
            Directory.CreateDirectory(outDir);
            p0c = Path.Combine(outDir, "0C_input.json");
            p0d = Path.Combine(outDir, "0D_input.json");
            JsonUtil.Write(p0c, C);
            JsonUtil.Write(p0d, D);
        }

        public string SaveMounting(string outDir)
        {
            Directory.CreateDirectory(outDir);
            string p = Path.Combine(outDir, "0B_input.json");
            JsonUtil.Write(p, B);
            return p;
        }
    }
}
