using System;
using dsat.DataProcessing.Calibration;

namespace dsat.DataProcessing.PostProcessing
{
    public static class OffsetCalculator
    {
        private const double MinFocalAbs = 1e-9;

        public static void Calculate(SyncedFrame frame, CalibrationConfig config, out double deltaE, out double deltaN)
        {
            Calculate(frame, config, config.Cx, config.Cy, out deltaE, out deltaN);
        }

        public static void Calculate(SyncedFrame frame, CalibrationConfig config, double u, double v, out double deltaE, out double deltaN)
        {
            double fx = SafeFocal(config.Fx);
            double fy = SafeFocal(config.Fy);
            double cx = Safe(config.Cx);
            double cy = Safe(config.Cy);
            double H = Safe(config.HeightH);
            double inputU = Safe(u);
            double inputV = Safe(v);

            double dx = (inputU - cx) / fx * H;
            double dy = (inputV - cy) / fy * H;

            double pitchDeg = Safe(frame.InterpolatedImu.AngleX) + Safe(config.DeltaPitch);
            double rollDeg = Safe(frame.InterpolatedImu.AngleY) + Safe(config.DeltaRoll);
            double pitchRad = pitchDeg * Math.PI / 180.0;
            double rollRad = rollDeg * Math.PI / 180.0;
            dx += H * Math.Tan(pitchRad);
            dy += H * Math.Tan(rollRad);

            double psiTrue = Safe(frame.InterpolatedImu.AngleZ) + Safe(config.MagneticDeclination);
            double headingRad = (psiTrue + Safe(config.PsiOffset)) * Math.PI / 180.0;

            deltaE = dx * Math.Cos(headingRad) + dy * Math.Sin(headingRad);
            deltaN = -dx * Math.Sin(headingRad) + dy * Math.Cos(headingRad);

            deltaE = Safe(deltaE);
            deltaN = Safe(deltaN);
        }

        private static double Safe(double value)
        {
            return (double.IsNaN(value) || double.IsInfinity(value)) ? 0.0 : value;
        }

        private static double SafeFocal(double focal)
        {
            if (double.IsNaN(focal) || double.IsInfinity(focal) || Math.Abs(focal) < MinFocalAbs)
                return 1.0;
            return focal;
        }
    }
}

