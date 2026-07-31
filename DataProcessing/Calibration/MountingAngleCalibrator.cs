using System;

namespace dsat.DataProcessing.Calibration
{
    public class MountingAngleCalibrator
    {
        public void Calibrate(double imuAngleX, double imuAngleY, double uActual, double vActual,
            double fx, double fy, double cx, double cy,
            out double deltaPitch, out double deltaRoll)
        {
            double pitchFromImage = Math.Atan((uActual - cx) / fx) * 180.0 / Math.PI;
            double rollFromImage = Math.Atan((vActual - cy) / fy) * 180.0 / Math.PI;
            deltaPitch = pitchFromImage - imuAngleY;
            deltaRoll = rollFromImage - imuAngleX;
        }
    }
}

