namespace Wit.Example_BWT901BLE.DataProcessing.PostProcessing
{
    public class SyncedFrame
    {
        public CameraFrame Camera { get; set; }
        public ImuSample InterpolatedImu { get; set; }
        public bool IsStable { get; set; }
    }
}
