using System;

namespace CalibrationBench.UI.Acquisition.Real
{
    /// <summary>
    /// 委托桥接相机：把任意相机 SDK 以委托注入，UI 无需引用厂商程序集。
    /// 用法：new DelegatingCameraSource(grab: () => 抓一帧转 RawImage, open: () => 初始化)
    /// </summary>
    public sealed class DelegatingCameraSource : ICameraSource
    {
        private readonly Func<RawImage> _grab;
        private readonly Action _open;
        public DelegatingCameraSource(Func<RawImage> grab, Action open = null) { _grab = grab; _open = open; }
        public void Open() { _open?.Invoke(); }
        public RawImage Grab() { return _grab(); }
        public void Dispose() { }
    }

    /// <summary>委托桥接 IMU：把任意 IMU SDK(如 BWT901BLE)以委托注入。</summary>
    public sealed class DelegatingImuSource : IImuSource
    {
        private readonly Func<ImuReading> _read;
        private readonly Action _open;
        public DelegatingImuSource(Func<ImuReading> read, Action open = null) { _read = read; _open = open; }
        public void Open() { _open?.Invoke(); }
        public ImuReading ReadLatest() { return _read(); }
        public void Dispose() { }
    }
}
