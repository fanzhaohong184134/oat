using System;

namespace CalibrationBench.UI.Acquisition
{
    /// <summary>
    /// 模拟数据源：按已知真值(ψ_offset/δ/α_board/offset/H)几何合成一帧，叠加噪声。
    /// 用于无硬件时跑通完整采集→解算链路。真实驱动实现 IRotaryStage/ISceneCapture 替换本类。
    /// </summary>
    public sealed class SimulatedBench : IRotaryStage, ISceneCapture
    {
        private const double D2R = Math.PI / 180.0;
        private readonly AcquisitionSettings _s;
        private readonly Random _rnd = new Random(12345);
        private double _az;

        public SimulatedBench(AcquisitionSettings s) { _s = s; }

        // ---- IRotaryStage ----
        public void MoveTo(double azimuthDeg) { _az = azimuthDeg; }
        public void Lock() { }
        public void Unlock() { }
        public bool IsSettled() { return true; }
        public double CurrentAzimuthDeg { get { return _az; } }

        private double N(double sigma) { return (_rnd.NextDouble() * 2 - 1) * sigma; }

        // ---- ISceneCapture ----
        public AcqFrame Capture(double nominalAzimuthDeg)
        {
            double fx = 2200, fy = 2200, cx = 1224, cy = 1024;
            double angleZ = nominalAzimuthDeg + N(_s.SimNoiseDeg);
            double angleX = 0 + N(_s.SimNoiseDeg);      // 近铅垂
            double angleY = 0 + N(_s.SimNoiseDeg);
            double heading = angleZ + _s.SimDeclinationDeg + _s.SimPsiOffsetDeg; // 相机 X 轴真方位
            double h = heading * D2R;

            // 物理 offset(ENU) 固定 → 相机系 [dx;dy]=R(heading)[E;N]
            double E = _s.SimOffsetE_mm, Nn = _s.SimOffsetN_mm;
            double dx = Math.Cos(h) * E - Math.Sin(h) * Nn;
            double dy = Math.Sin(h) * E + Math.Cos(h) * Nn;

            double u = cx + dx * fx / _s.SimH_mm + N(_s.SimNoisePx);
            double v = cy + dy * fy / _s.SimH_mm + N(_s.SimNoisePx);

            return new AcqFrame
            {
                ThetaImgBoardDeg = _s.SimAlphaBoardDeg - heading + N(_s.SimNoiseDeg),
                OffsetPnpE_mm = E + N(0.02),
                OffsetPnpN_mm = Nn + N(0.02),
                H_mm = _s.SimH_mm,
                PitchCamDeg = angleY + _s.SimDeltaPitchDeg + N(_s.SimNoiseDeg * 0.3),
                RollCamDeg = angleX + _s.SimDeltaRollDeg + N(_s.SimNoiseDeg * 0.3),
                UCorrPx = u,
                VCorrPx = v,
                ImuAngleXDeg = angleX,
                ImuAngleYDeg = angleY,
                ImuAngleZDeg = angleZ
            };
        }
    }
}
