using System;

namespace CalibrationBench.UI.Acquisition.Real
{
    /// <summary>
    /// 真机接线示例：把 相机 + IMU + PnP + 旋转台 组装成采集编排。
    /// 复制/参照本文件，按 TODO 替换为你的相机/IMU SDK 调用即可(其余零改动)。
    /// PnP 需定义 USE_OPENCV；旋转台为串口可用实现。
    /// </summary>
    public static class RealWiringSample
    {
        public static AcquisitionOrchestrator BuildOrchestrator(
            AcquisitionSettings settings, CameraIntrinsics intrinsics, string stagePort, Action<string> log)
        {
            // 1) 相机：用你的相机 SDK 抓帧(委托注入，UI 不依赖厂商程序集)
            var camera = new DelegatingCameraSource(
                grab: () => { throw new NotImplementedException("TODO: 相机 SDK 抓一帧 → RawImage(Data/Width/Height/PixelFormat)"); },
                open: () => { /* TODO: 打开相机, 设曝光/增益/全局快门/触发 */ });

            // 2) IMU：BWT901BLE。用回调缓存最新读数(或改用 Bwt901ImuSource + USE_WITSDK)
            ImuReading latest = null;
            var imu = new DelegatingImuSource(
                read: () => latest ?? throw new InvalidOperationException("尚无 IMU 数据(检查连接/稳态)"),
                open: () =>
                {
                    // TODO: 连接 BWT901BLE，OnRecord 回调里：
                    //   latest = new ImuReading { AngleXDeg=.., AngleYDeg=.., AngleZDeg=.., AccX=.., GyroX=.. };
                });

            // 3) PnP：OpenCvSharp(定义 USE_OPENCV)。按物理靶标板设置格数/尺寸/字典
            var pnp = new OpenCvCharucoPnpSolver(squaresX: 10, squaresY: 14, squareLenMm: 20f, markerLenMm: 15f);

            // 4) 场景 = 相机 + PnP + IMU（PnP 位姿→offset/H/姿态/方位 换算已内置）
            var scene = new RealSceneCapture(camera, pnp, imu, intrinsics);

            // 5) 旋转台（串口，按转台协议改 RealRotaryStage 命令帧）
            var stage = new RealRotaryStage(stagePort);

            // 6) 采集编排：AcquireRotation(8方位 step-and-stare) / AcquireMounting(静止)
            return new AcquisitionOrchestrator(stage, scene, log);
        }
    }
}
