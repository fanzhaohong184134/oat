namespace CalibrationBench.UI.Acquisition
{
    /// <summary>采集参数设置(界面可配) + 模拟数据源真值(真实驱动时忽略真值段)。</summary>
    public sealed class AcquisitionSettings
    {
        // ---- 采集编排参数 ----
        public string DeviceId = "AT1";
        public double AzimuthStepDeg = 45;   // 方位间隔角(如 40/45)
        public int AzimuthCount = 8;         // 方位数
        public int FramesPerAzimuth = 25;    // 每方位帧数(间隔采样累计)
        public int SampleIntervalMs = 200;   // 间隔采样周期
        public double SettleSeconds = 1.0;   // 每方位稳定等待(模拟压缩)
        public int MountingFrames = 20;      // 0B 静止采集帧数

        // 稳态阈值(真实驱动判定用)
        public double AccTolG = 0.005;
        public double GyroTolDps = 0.3;

        // ---- 模拟真值(仅模拟数据源) ----
        public bool SimulationMode = true;
        public double SimPsiOffsetDeg = 12.34;
        public double SimDeltaPitchDeg = 0.02;
        public double SimDeltaRollDeg = -0.015;
        public double SimAlphaBoardDeg = 30.0;
        public double SimDeclinationDeg = -6.5;
        public double SimOffsetE_mm = 10.0;
        public double SimOffsetN_mm = 5.0;
        public double SimH_mm = 400.0;
        public double SimNoisePx = 0.10;     // 像素噪声
        public double SimNoiseDeg = 0.02;    // 角度噪声

        public CameraIntrinsics SimIntrinsics()
        {
            return new CameraIntrinsics
            {
                Fx = 2200, Fy = 2200, Cx = 1224, Cy = 1024,
                K1 = 0, K2 = 0, P1 = 0, P2 = 0,
                ImageWidth = 2448, ImageHeight = 2048
            };
        }
    }
}
