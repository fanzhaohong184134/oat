using System;
using dsat.DataProcessing.Calibration;

namespace dsat.DataProcessing.PostProcessing
{
    public static class OffsetCalculator
    {
        public static void Calculate(SyncedFrame frame, CalibrationConfig config, out double deltaE, out double deltaN)
        {
            Calculate(frame, config, config.Cx, config.Cy, out deltaE, out deltaN);
        }

        public static void Calculate(SyncedFrame frame, CalibrationConfig config, double u, double v, out double deltaE, out double deltaN)
        {
            double H = config.HeightH;
            double dx = (u - config.Cx) / config.Fx * H;
            double dy = (v - config.Cy) / config.Fy * H;

            double pitchRad = (frame.InterpolatedImu.AngleX + config.DeltaPitch) * Math.PI / 180.0;
            double rollRad = (frame.InterpolatedImu.AngleY + config.DeltaRoll) * Math.PI / 180.0;
            dx += H * Math.Tan(pitchRad);
            dy += H * Math.Tan(rollRad);

            double psiTrue = frame.InterpolatedImu.AngleZ + config.MagneticDeclination;
            double headingRad = (psiTrue + config.PsiOffset) * Math.PI / 180.0;

            deltaE = dx * Math.Cos(headingRad) + dy * Math.Sin(headingRad);
            deltaN = -dx * Math.Sin(headingRad) + dy * Math.Cos(headingRad);
        }
    }
}

