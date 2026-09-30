using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CalibrationBench.UI.Acquisition
{
    /// <summary>
    /// 采集编排：step-and-stare 多方位采集(0C/0D 共用) + 0B 静止采集。
    /// 产出符合引擎输入契约的 JSON；执行仍交由引擎 exe。
    /// </summary>
    public sealed class AcquisitionOrchestrator
    {
        private readonly IRotaryStage _stage;
        private readonly ISceneCapture _scene;
        private readonly Action<string> _log;

        public AcquisitionOrchestrator(IRotaryStage stage, ISceneCapture scene, Action<string> log)
        {
            _stage = stage; _scene = scene; _log = log;
        }

        /// <summary>0B 安装角：设备铅垂静止，采集多帧 → Step0BInput。</summary>
        public string AcquireMounting(AcquisitionSettings s, string outDir)
        {
            Directory.CreateDirectory(outDir);
            _log("▶ 0B 静止采集：等待稳态 ...");
            _stage.MoveTo(0); _stage.Lock();
            var input = new Step0BInput { DeviceId = s.DeviceId, ToleranceDeg = 0.05, Samples = new List<MountingSample>() };
            for (int i = 0; i < s.MountingFrames; i++)
            {
                var f = _scene.Capture(0);
                input.Samples.Add(new MountingSample
                {
                    PitchCamDeg = f.PitchCamDeg, RollCamDeg = f.RollCamDeg,
                    ImuAngleYDeg = f.ImuAngleYDeg, ImuAngleXDeg = f.ImuAngleXDeg
                });
            }
            _stage.Unlock();
            string path = Path.Combine(outDir, "0B_input.json");
            WriteJson(path, input);
            _log("  采集 " + s.MountingFrames + " 帧 → " + path);
            return path;
        }

        /// <summary>多方位 step-and-stare 采集：一次产出 0C 与 0D 输入。</summary>
        public void AcquireRotation(AcquisitionSettings s, string outDir,
                                    out string path0C, out string path0D)
        {
            Directory.CreateDirectory(outDir);
            var c = new Step0CInput
            {
                DeviceId = s.DeviceId, AlphaBoardDeg = s.SimAlphaBoardDeg,
                MagneticDeclinationDeg = s.SimDeclinationDeg, ToleranceDeg = 0.1,
                Samples = new List<HeadingSample>()
            };
            var d = new Step0DInput
            {
                DeviceId = s.DeviceId, Intrinsics = s.SimIntrinsics(),
                DeltaPitch = s.SimDeltaPitchDeg, DeltaRoll = s.SimDeltaRollDeg,
                PsiOffsetDeg = s.SimPsiOffsetDeg, MagneticDeclinationDeg = s.SimDeclinationDeg,
                AlphaBoardDeg = s.SimAlphaBoardDeg, Frames = new List<VerifyFrame>(),
                SigmaRotLimit_mm = 0.3, ClosureLimit_mm = 0.5, ClosureMeanLimit_mm = 0.3
            };

            for (int k = 0; k < s.AzimuthCount; k++)
            {
                double phi = k * s.AzimuthStepDeg;
                _log(string.Format("▶ 方位 {0}/{1}: 转到 {2:F0}° ...", k + 1, s.AzimuthCount, phi));
                _stage.MoveTo(phi); _stage.Lock();
                WaitSettle(s);

                double sumTheta = 0, sumZ = 0; int n = 0;
                for (int i = 0; i < s.FramesPerAzimuth; i++)
                {
                    var f = _scene.Capture(phi);
                    sumTheta += f.ThetaImgBoardDeg; sumZ += f.ImuAngleZDeg; n++;
                    d.Frames.Add(new VerifyFrame
                    {
                        NominalAzimuthDeg = phi, UCorrPx = f.UCorrPx, VCorrPx = f.VCorrPx, H_mm = f.H_mm,
                        ImuAngleXDeg = f.ImuAngleXDeg, ImuAngleYDeg = f.ImuAngleYDeg, ImuAngleZDeg = f.ImuAngleZDeg,
                        OffsetPnpE_mm = f.OffsetPnpE_mm, OffsetPnpN_mm = f.OffsetPnpN_mm
                    });
                }
                _stage.Unlock();
                c.Samples.Add(new HeadingSample
                {
                    NominalAzimuthDeg = phi, ThetaImgBoardDeg = sumTheta / n, ImuAngleZDeg = sumZ / n
                });
                _log(string.Format("  采集 {0} 帧, θ_img均值={1:F3}°", n, sumTheta / n));
            }

            path0C = Path.Combine(outDir, "0C_input.json");
            path0D = Path.Combine(outDir, "0D_input.json");
            WriteJson(path0C, c);
            WriteJson(path0D, d);
            _log("0C 输入 → " + path0C);
            _log("0D 输入 → " + path0D + "  (共 " + d.Frames.Count + " 帧)");
        }

        private void WaitSettle(AcquisitionSettings s)
        {
            int waited = 0, target = (int)(s.SettleSeconds * 1000);
            while (waited < target) { if (_stage.IsSettled()) { } System.Threading.Thread.Sleep(50); waited += 50; }
        }

        private static void WriteJson<T>(string path, T obj)
        {
            using (var ms = new MemoryStream())
            {
                var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
                var ser = new DataContractJsonSerializer(typeof(T), settings);
                ser.WriteObject(ms, obj);
                File.WriteAllText(path, Encoding.UTF8.GetString(ms.ToArray()), new UTF8Encoding(false));
            }
        }
    }
}
