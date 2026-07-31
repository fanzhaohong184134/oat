using System;
using System.Collections.Generic;

namespace dsat.DataProcessing.PostProcessing
{
    public static class StabilityFilter
    {
        public static List<SyncedFrame> Filter(List<SyncedFrame> frames, double gThreshold, double omegaThreshold, int minConsecutive)
        {
            foreach (var f in frames)
            {
                var imu = f.InterpolatedImu;
                double gMag = Math.Sqrt(imu.AccX * imu.AccX + imu.AccY * imu.AccY + imu.AccZ * imu.AccZ);
                bool accelOk = Math.Abs(gMag - 1.0) < gThreshold;
                bool gyroOk = Math.Abs(imu.GyroX) < omegaThreshold
                           && Math.Abs(imu.GyroY) < omegaThreshold
                           && Math.Abs(imu.GyroZ) < omegaThreshold;
                f.IsStable = accelOk && gyroOk;
            }

            var result = new List<SyncedFrame>();
            int runStart = -1;

            for (int i = 0; i <= frames.Count; i++)
            {
                bool stable = i < frames.Count && frames[i].IsStable;
                if (stable)
                {
                    if (runStart < 0) runStart = i;
                }
                else
                {
                    if (runStart >= 0)
                    {
                        int runLength = i - runStart;
                        if (runLength >= minConsecutive)
                        {
                            for (int j = runStart; j < i; j++)
                                result.Add(frames[j]);
                        }
                        runStart = -1;
                    }
                }
            }

            return result;
        }
    }
}

