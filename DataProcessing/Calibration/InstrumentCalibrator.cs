using System;

namespace dsat.DataProcessing.Calibration
{
    public class InstrumentCalibrator
    {
        public double CalibrateHeadingOffset(
            double imuAngleZ, double magneticDeclination,
            double u1, double v1, double u2, double v2,
            double knownAzimuth)
        {
            double thetaImg = Math.Atan2(-(v2 - v1), (u2 - u1)) * 180.0 / Math.PI;
            double psiTrue = imuAngleZ + magneticDeclination;
            double psiOffset = knownAzimuth - psiTrue - thetaImg;
            return NormalizeTo180(psiOffset);
        }

        public double Verify(
            double imuAngleZ, double magneticDeclination, double psiOffset,
            double u1, double v1, double u2, double v2)
        {
            double thetaImg = Math.Atan2(-(v2 - v1), (u2 - u1)) * 180.0 / Math.PI;
            double psiTrue = imuAngleZ + magneticDeclination;
            double predictedAzimuth = psiTrue + psiOffset + thetaImg;
            return NormalizeTo180(predictedAzimuth);
        }

        private static double NormalizeTo180(double angle)
        {
            while (angle > 180.0) angle -= 360.0;
            while (angle <= -180.0) angle += 360.0;
            return angle;
        }
    }
}

