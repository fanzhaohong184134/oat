using System;

namespace CalibrationBench.UI.Acquisition.Real
{
    // ============ 待接入的硬件驱动骨架 ============
    // 以下三个类是"接口 + 对接指引"的骨架。接入真实硬件时按注释实现方法体，
    // 其余(采集编排、PnP→AcqFrame 换算、引擎解算、界面)均无需改动。

    /// <summary>工业相机骨架(U3V/GigE Vision SDK)。</summary>
    public sealed class IndustrialCameraSource : ICameraSource
    {
        // TODO: 用相机厂商 SDK 打开设备、设参数(曝光/增益/触发)、启动取流。
        public void Open()
        {
            throw new NotSupportedException(
                "相机驱动未实现。请用相机 SDK 实现 IndustrialCameraSource：\n" +
                "1) Open(): 枚举/打开设备, 设置曝光/增益/全局快门, 取硬件时间戳;\n" +
                "2) Grab(): 抓一帧, 填 RawImage(Data/Width/Height/PixelFormat/Timestamp)。");
        }

        public RawImage Grab() { throw new NotSupportedException("相机 Grab() 未实现。"); }
        public void Dispose() { }
    }

    /// <summary>ChArUco+PnP 骨架(OpenCvSharp)。</summary>
    public sealed class OpenCvCharucoPnpSolver : ICharucoPnpSolver
    {
        // TODO: 引入 NuGet: OpenCvSharp4 + OpenCvSharp4.runtime.win，实现：
        //  1) 由 RawImage 构造 Mat；
        //  2) CvAruco.DetectMarkers + InterpolateCornersCharuco 得角点(charucoCorners/Ids);
        //  3) objPoints(靶标系,mm) 与 imgPoints;
        //  4) Cv2.SolvePnP(objPoints, imgPoints, K, distCoeffs, out rvec, out tvec);
        //  5) 用 Cv2.UndistortPoints 得畸变校正后的板原点像素 → OriginU/OriginV;
        //  6) 计算重投影 RMS。
        public PnpRaw Solve(RawImage image, CameraIntrinsics intrinsics)
        {
            throw new NotSupportedException(
                "PnP 未实现。请用 OpenCvSharp 实现 OpenCvCharucoPnpSolver：\n" +
                "CvAruco.DetectMarkers → InterpolateCornersCharuco → Cv2.SolvePnP(objPts,imgPts,K,D,out rvec,out tvec)。");
        }
    }

    /// <summary>BWT901BLE 骨架。对接现有 Wit SDK 的 Bwt901ble(见 dsat 项目 ble5/BWT901BLE.cs)。</summary>
    public sealed class Bwt901ImuSource : IImuSource
    {
        // TODO: 复用 Wit SDK：连接 BWT901BLE，订阅 OnRecord，缓存最新一帧；
        //  ReadLatest() 读 WitSensorKey.AngleX/AngleY/AngleZ/AccX.../AsX... 填 ImuReading。
        //  为保持工装与 dsat 解耦，建议把 Wit SDK 适配代码单独放一个适配程序集。
        public void Open()
        {
            throw new NotSupportedException(
                "IMU 驱动未实现。请对接 BWT901BLE：连接后订阅 OnRecord，\n" +
                "ReadLatest() 用 GetDeviceData(WitSensorKey.AngleX/Y/Z, AccX.., AsX..) 填 ImuReading。");
        }

        public ImuReading ReadLatest() { throw new NotSupportedException("IMU ReadLatest() 未实现。"); }
        public void Dispose() { }
    }
}
