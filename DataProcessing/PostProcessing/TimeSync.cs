using System;
using System.Collections.Generic;

namespace Wit.Example_BWT901BLE.DataProcessing.PostProcessing
{
    public static class TimeSync
    {
        public static List<SyncedFrame> Synchronize(List<ImuSample> imuData, List<CameraFrame> cameraFrames)
        {
            if (imuData == null || imuData.Count == 0)
                throw new ArgumentException("IMU data is empty.");
            if (cameraFrames == null || cameraFrames.Count == 0)
                throw new ArgumentException("Camera frames are empty.");

            var results = new List<SyncedFrame>();

            foreach (var cam in cameraFrames)
            {
                var interpolated = InterpolateImu(imuData, cam.Timestamp);
                results.Add(new SyncedFrame
                {
                    Camera = cam,
                    InterpolatedImu = interpolated,
                    IsStable = false
                });
            }

            return results;
        }

        private static ImuSample InterpolateImu(List<ImuSample> imuData, DateTime target)
        {
            long targetTicks = target.Ticks;

            if (targetTicks <= imuData[0].Timestamp.Ticks)
                return CloneSample(imuData[0], target);

            if (targetTicks >= imuData[imuData.Count - 1].Timestamp.Ticks)
                return CloneSample(imuData[imuData.Count - 1], target);

            int lo = 0, hi = imuData.Count - 1;
            while (lo < hi - 1)
            {
                int mid = (lo + hi) / 2;
                if (imuData[mid].Timestamp.Ticks <= targetTicks)
                    lo = mid;
                else
                    hi = mid;
            }

            var a = imuData[lo];
            var b = imuData[hi];
            double range = (b.Timestamp - a.Timestamp).TotalMilliseconds;
            double t = range > 0 ? (target - a.Timestamp).TotalMilliseconds / range : 0;

            return new ImuSample
            {
                Timestamp = target,
                AccX = Lerp(a.AccX, b.AccX, t),
                AccY = Lerp(a.AccY, b.AccY, t),
                AccZ = Lerp(a.AccZ, b.AccZ, t),
                GyroX = Lerp(a.GyroX, b.GyroX, t),
                GyroY = Lerp(a.GyroY, b.GyroY, t),
                GyroZ = Lerp(a.GyroZ, b.GyroZ, t),
                AngleX = Lerp(a.AngleX, b.AngleX, t),
                AngleY = Lerp(a.AngleY, b.AngleY, t),
                AngleZ = Lerp(a.AngleZ, b.AngleZ, t),
                MagX = Lerp(a.MagX, b.MagX, t),
                MagY = Lerp(a.MagY, b.MagY, t),
                MagZ = Lerp(a.MagZ, b.MagZ, t),
                Q0 = Lerp(a.Q0, b.Q0, t),
                Q1 = Lerp(a.Q1, b.Q1, t),
                Q2 = Lerp(a.Q2, b.Q2, t),
                Q3 = Lerp(a.Q3, b.Q3, t)
            };
        }

        private static ImuSample CloneSample(ImuSample src, DateTime timestamp)
        {
            return new ImuSample
            {
                Timestamp = timestamp,
                AccX = src.AccX, AccY = src.AccY, AccZ = src.AccZ,
                GyroX = src.GyroX, GyroY = src.GyroY, GyroZ = src.GyroZ,
                AngleX = src.AngleX, AngleY = src.AngleY, AngleZ = src.AngleZ,
                MagX = src.MagX, MagY = src.MagY, MagZ = src.MagZ,
                Q0 = src.Q0, Q1 = src.Q1, Q2 = src.Q2, Q3 = src.Q3
            };
        }

        private static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }
    }
}
