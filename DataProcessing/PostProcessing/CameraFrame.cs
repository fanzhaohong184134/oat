using System;

namespace Wit.Example_BWT901BLE.DataProcessing.PostProcessing
{
    public class CameraFrame
    {
        public DateTime Timestamp { get; set; }
        public double ElapsedMs { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public bool Success { get; set; }
    }
}
