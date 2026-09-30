#if USE_OPENCV
using OpenCvSharp;
using OpenCvSharp.Aruco;
#endif
#if USE_WITSDK
using Wit.SDK.Modular.WitSensorApi.Modular.BWT901BLE; // 命名空间按实际 Wit SDK 调整
#endif
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

    /// <summary>ChArUco 检测 + PnP。默认骨架；定义 USE_OPENCV 后为 OpenCvSharp 参考实现。</summary>
    public sealed class OpenCvCharucoPnpSolver : ICharucoPnpSolver
    {
        // 物理靶标板参数(按实际板修改)
        private readonly int _squaresX;
        private readonly int _squaresY;
        private readonly float _squareLenMm;
        private readonly float _markerLenMm;

        public OpenCvCharucoPnpSolver(int squaresX = 10, int squaresY = 14,
                                      float squareLenMm = 20f, float markerLenMm = 15f)
        {
            _squaresX = squaresX; _squaresY = squaresY;
            _squareLenMm = squareLenMm; _markerLenMm = markerLenMm;
        }

#if USE_OPENCV
        // 参考实现：DetectMarkers → InterpolateCornersCharuco → EstimatePoseCharucoBoard。
        // OpenCvSharp4 各版本 API 可能略有差异，按实际版本微调。
        public PnpRaw Solve(RawImage image, CameraIntrinsics intr)
        {
            using (var mat = ToMat(image))
            using (var gray = ToGray(mat))
            {
                var dict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict5X5_1000);
                using (var board = CharucoBoard.Create(_squaresX, _squaresY, _squareLenMm, _markerLenMm, dict))
                using (var dp = DetectorParameters.Create())
                {
                    Point2f[][] corners; int[] ids; Point2f[][] rejected;
                    CvAruco.DetectMarkers(gray, dict, out corners, out ids, dp, out rejected);
                    if (ids == null || ids.Length == 0) return new PnpRaw { Ok = false };

                    Point2f[] chCorners; int[] chIds;
                    CvAruco.InterpolateCornersCharuco(corners, ids, gray, board, out chCorners, out chIds);
                    if (chIds == null || chIds.Length < 6) return new PnpRaw { Ok = false };

                    using (var K = Mat.FromArray(new double[,] {
                               { intr.Fx, 0, intr.Cx }, { 0, intr.Fy, intr.Cy }, { 0, 0, 1 } }))
                    using (var dist = Mat.FromArray(new double[] { intr.K1, intr.K2, intr.P1, intr.P2, 0 }))
                    using (var rvec = new Mat())
                    using (var tvec = new Mat())
                    {
                        bool ok = CvAruco.EstimatePoseCharucoBoard(
                            InputArray.Create(chCorners), InputArray.Create(chIds),
                            board, K, dist, rvec, tvec);
                        if (!ok) return new PnpRaw { Ok = false };

                        double[] rv = { rvec.At<double>(0), rvec.At<double>(1), rvec.At<double>(2) };
                        double[] tv = { tvec.At<double>(0), tvec.At<double>(1), tvec.At<double>(2) };

                        // 板原点(0,0,0)的无畸变(pinhole)投影 = 校正后像素(与引擎 UCorr/VCorr 对齐)
                        using (var zeroDist = Mat.Zeros(1, 5, MatType.CV_64F))
                        {
                            Point2f[] proj; double[,] jac;
                            Cv2.ProjectPoints(new[] { new Point3f(0, 0, 0) }, rvec, tvec, K, zeroDist, out proj, out jac);
                            return new PnpRaw
                            {
                                Rvec = rv, Tvec = tv,
                                OriginU = proj[0].X, OriginV = proj[0].Y,
                                ReprojRms = 0, Ok = true
                            };
                        }
                    }
                }
            }
        }

        private static Mat ToMat(RawImage img)
        {
            var type = img.PixelFormat == "BGR8" ? MatType.CV_8UC3 : MatType.CV_8UC1;
            var m = new Mat(img.Height, img.Width, type);
            System.Runtime.InteropServices.Marshal.Copy(img.Data, 0, m.Data, img.Data.Length);
            return m;
        }

        private static Mat ToGray(Mat m)
        {
            if (m.Channels() == 1) return m.Clone();
            var g = new Mat();
            Cv2.CvtColor(m, g, ColorConversionCodes.BGR2GRAY);
            return g;
        }
#else
        public PnpRaw Solve(RawImage image, CameraIntrinsics intrinsics)
        {
            throw new NotSupportedException(
                "PnP 未实现(默认骨架)。启用真实 PnP：\n" +
                "1) NuGet 安装 OpenCvSharp4 + OpenCvSharp4.runtime.win；\n" +
                "2) csproj 的 DefineConstants 追加 USE_OPENCV；\n" +
                "3) 按物理板调整 CharucoBoard 参数(格数/尺寸/字典)。\n" +
                "参考实现见本文件 #if USE_OPENCV 分支。");
        }
#endif
    }

    /// <summary>BWT901BLE 骨架。对接现有 Wit SDK 的 Bwt901ble(见 dsat 项目 ble5/BWT901BLE.cs)。</summary>
    /// <summary>BWT901BLE。默认骨架；定义 USE_WITSDK 后为 Wit SDK 参考实现。
    /// 也可直接用 DelegatingImuSource 注入现成读数，无需改本类。</summary>
#if USE_WITSDK
    public sealed class Bwt901ImuSource : IImuSource
    {
        private readonly Bwt901ble _dev;
        private volatile ImuReading _latest;

        public Bwt901ImuSource(Bwt901ble device) { _dev = device; }

        public void Open()
        {
            _dev.OnRecord += d =>
            {
                _latest = new ImuReading
                {
                    AngleXDeg = P(d, WitSensorKey.AngleX),
                    AngleYDeg = P(d, WitSensorKey.AngleY),
                    AngleZDeg = P(d, WitSensorKey.AngleZ),
                    AccX = P(d, WitSensorKey.AccX), AccY = P(d, WitSensorKey.AccY), AccZ = P(d, WitSensorKey.AccZ),
                    GyroX = P(d, WitSensorKey.AsX), GyroY = P(d, WitSensorKey.AsY), GyroZ = P(d, WitSensorKey.AsZ),
                    Timestamp = DateTime.Now
                };
            };
        }

        public ImuReading ReadLatest()
        {
            var l = _latest;
            if (l == null) throw new InvalidOperationException("尚未收到 IMU 数据(检查连接/稳态)。");
            return l;
        }

        public void Dispose() { }

        private static double P(Bwt901ble d, string key)
        {
            double v; return double.TryParse(d.GetDeviceData(key), out v) ? v : 0;
        }
    }
#else
    public sealed class Bwt901ImuSource : IImuSource
    {
        public void Open()
        {
            throw new NotSupportedException(
                "IMU 驱动未实现(默认骨架)。二选一：\n" +
                "A) 用 DelegatingImuSource 注入现成读数(推荐, 无需改本类)；\n" +
                "B) 定义 USE_WITSDK 并引用 Wit SDK，用 Bwt901ImuSource(Bwt901ble) 参考实现\n" +
                "   (OnRecord → GetDeviceData(WitSensorKey.AngleX/Y/Z, AccX.., AsX..))。");
        }
        public ImuReading ReadLatest() { throw new NotSupportedException("IMU ReadLatest() 未实现。"); }
        public void Dispose() { }
    }
#endif
}
