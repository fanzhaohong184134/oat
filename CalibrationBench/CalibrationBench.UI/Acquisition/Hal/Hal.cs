namespace CalibrationBench.UI.Acquisition
{
    /// <summary>一帧采集结果：相机拍照→ChArUco→PnP→读 IMU 后的综合量。</summary>
    public sealed class AcqFrame
    {
        public double ThetaImgBoardDeg;   // 图像中板 X_B 相对相机 X 轴方位
        public double OffsetPnpE_mm;      // PnP 真值 E
        public double OffsetPnpN_mm;      // PnP 真值 N
        public double H_mm;               // 相机到板面高度(PnP)
        public double PitchCamDeg;        // 相机光轴 pitch(PnP)
        public double RollCamDeg;         // 相机光轴 roll(PnP)
        public double UCorrPx, VCorrPx;   // 畸变校正后靶心像素
        public double ImuAngleXDeg;       // IMU roll
        public double ImuAngleYDeg;       // IMU pitch
        public double ImuAngleZDeg;       // IMU yaw(磁北)
    }

    /// <summary>旋转台抽象：下发方位、锁定、到位稳态判定。真实驱动实现同接口。</summary>
    public interface IRotaryStage
    {
        void MoveTo(double azimuthDeg);
        void Lock();
        void Unlock();
        bool IsSettled();
        double CurrentAzimuthDeg { get; }
    }

    /// <summary>场景采集抽象：拍照+靶标检测+PnP+读 IMU，返回一帧综合量。</summary>
    public interface ISceneCapture
    {
        AcqFrame Capture(double nominalAzimuthDeg);
    }
}
