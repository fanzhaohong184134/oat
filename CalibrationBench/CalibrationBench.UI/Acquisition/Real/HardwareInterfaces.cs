using System;

namespace CalibrationBench.UI.Acquisition.Real
{
    /// <summary>相机原始帧。</summary>
    public sealed class RawImage
    {
        public byte[] Data;
        public int Width, Height, Stride;
        public string PixelFormat;   // 如 "Mono8" / "BGR8"
        public DateTime Timestamp;
    }

    /// <summary>IMU 一次读数(映射 BWT901BLE)。</summary>
    public sealed class ImuReading
    {
        public double AngleXDeg, AngleYDeg, AngleZDeg; // roll,pitch,yaw
        public double AccX, AccY, AccZ;                // g
        public double GyroX, GyroY, GyroZ;             // °/s
        public DateTime Timestamp;
    }

    /// <summary>PnP 原始输出(靶标系→相机系) + 检测到的板原点像素。</summary>
    public sealed class PnpRaw
    {
        public double[] Rvec = new double[3];  // Rodrigues 旋转向量
        public double[] Tvec = new double[3];  // 平移(mm)
        public double OriginU, OriginV;        // 板原点(畸变校正后)像素
        public double ReprojRms;
        public bool Ok;
    }

    /// <summary>工业相机抓帧。真实实现对接相机 SDK(U3V/GigE Vision)。</summary>
    public interface ICameraSource : IDisposable
    {
        void Open();
        RawImage Grab();
    }

    /// <summary>ChArUco 检测 + PnP。真实实现用 OpenCvSharp。</summary>
    public interface ICharucoPnpSolver
    {
        PnpRaw Solve(RawImage image, CameraIntrinsics intrinsics);
    }

    /// <summary>IMU 读数源。真实实现对接 BWT901BLE。</summary>
    public interface IImuSource : IDisposable
    {
        void Open();
        ImuReading ReadLatest();
    }
}
