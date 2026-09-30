using System;

namespace CalibrationBench.UI.Acquisition.Real
{
    /// <summary>
    /// 真实场景采集：组合 相机抓帧 + ChArUco/PnP + IMU 读数 → AcqFrame。
    /// 本类逻辑(PnP 位姿 → offset/H/姿态/方位 换算)为完整实现；
    /// 底层三个 IO 由 ICameraSource/ICharucoPnpSolver/IImuSource 提供。
    /// </summary>
    public sealed class RealSceneCapture : ISceneCapture, IDisposable
    {
        private const double R2D = 180.0 / Math.PI;
        private readonly ICameraSource _camera;
        private readonly ICharucoPnpSolver _pnp;
        private readonly IImuSource _imu;
        private readonly CameraIntrinsics _k;
        private bool _opened;

        public RealSceneCapture(ICameraSource camera, ICharucoPnpSolver pnp, IImuSource imu, CameraIntrinsics intrinsics)
        {
            _camera = camera; _pnp = pnp; _imu = imu; _k = intrinsics;
        }

        private void EnsureOpen()
        {
            if (_opened) return;
            _camera.Open(); _imu.Open(); _opened = true;
        }

        public AcqFrame Capture(double nominalAzimuthDeg)
        {
            EnsureOpen();
            var img = _camera.Grab();
            var p = _pnp.Solve(img, _k);
            if (!p.Ok) throw new InvalidOperationException("PnP 失败(角点不足/离焦/板出视场)。");
            var imu = _imu.ReadLatest();

            // R: 靶标系→相机系 (solvePnP: p_cam = R*p_board + t)
            double[,] R = Rodrigues(p.Rvec);
            double[] t = p.Tvec;

            // 相机光心在靶标系: C_B = -R^T * t
            double[] Cb = MulT(R, new[] { -t[0], -t[1], -t[2] });
            double offsetE = Cb[0], offsetN = Cb[1], Hmm = Cb[2];

            // 板 X 轴(1,0,0)在相机系 = R 第一列；图像方位(v 向下取负→数学系)
            double vx0 = R[0, 0], vx1 = R[1, 0];
            double thetaImg = Math.Atan2(-vx1, vx0) * R2D;

            // 相机光轴(0,0,1)在靶标系(世界铅垂) = R^T*(0,0,1) = R 第三行
            double zx = R[2, 0], zy = R[2, 1], zz = R[2, 2];
            // 近铅垂小倾角(符号约定需在真机上核验，见方案文档 Step 6 注)
            double pitchCam = Math.Atan2(zx, -zz) * R2D;
            double rollCam = Math.Atan2(zy, -zz) * R2D;

            return new AcqFrame
            {
                ThetaImgBoardDeg = thetaImg,
                OffsetPnpE_mm = offsetE,
                OffsetPnpN_mm = offsetN,
                H_mm = Hmm,
                PitchCamDeg = pitchCam,
                RollCamDeg = rollCam,
                UCorrPx = p.OriginU,
                VCorrPx = p.OriginV,
                ImuAngleXDeg = imu.AngleXDeg,
                ImuAngleYDeg = imu.AngleYDeg,
                ImuAngleZDeg = imu.AngleZDeg
            };
        }

        // Rodrigues: 旋转向量 → 3x3 旋转矩阵
        private static double[,] Rodrigues(double[] r)
        {
            double x = r[0], y = r[1], z = r[2];
            double th = Math.Sqrt(x * x + y * y + z * z);
            var R = new double[3, 3];
            if (th < 1e-12) { R[0, 0] = R[1, 1] = R[2, 2] = 1; return R; }
            double kx = x / th, ky = y / th, kz = z / th;
            double c = Math.Cos(th), s = Math.Sin(th), v = 1 - c;
            R[0, 0] = c + kx * kx * v; R[0, 1] = kx * ky * v - kz * s; R[0, 2] = kx * kz * v + ky * s;
            R[1, 0] = ky * kx * v + kz * s; R[1, 1] = c + ky * ky * v; R[1, 2] = ky * kz * v - kx * s;
            R[2, 0] = kz * kx * v - ky * s; R[2, 1] = kz * ky * v + kx * s; R[2, 2] = c + kz * kz * v;
            return R;
        }

        // R^T * v
        private static double[] MulT(double[,] R, double[] v)
        {
            return new[]
            {
                R[0,0]*v[0] + R[1,0]*v[1] + R[2,0]*v[2],
                R[0,1]*v[0] + R[1,1]*v[1] + R[2,1]*v[2],
                R[0,2]*v[0] + R[1,2]*v[1] + R[2,2]*v[2]
            };
        }

        public void Dispose() { _camera?.Dispose(); _imu?.Dispose(); }
    }
}
