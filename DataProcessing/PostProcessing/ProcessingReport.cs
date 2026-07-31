using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using dsat.DataProcessing.Calibration;

namespace dsat.DataProcessing.PostProcessing
{
    public class ProcessingReport
    {
        public double DeltaE { get; set; }
        public double DeltaN { get; set; }
        public double SigmaE { get; set; }
        public double SigmaN { get; set; }
        public double DeltaH { get; set; }
        public double Azimuth { get; set; }
        public int TotalFrames { get; set; }
        public int ValidFrames { get; set; }
        public int AnnotatedImageCount { get; set; }
        public int AnnotatedImageFailedCount { get; set; }
        public int CircleDetectedCount { get; set; }
        public int CircleFallbackCenterCount { get; set; }
        public bool MetadataOnlyMode { get; set; }
        public string AnnotatedImageDirectory { get; set; }
        public string AnnotatedImageManifestPath { get; set; }
        public List<double> PerFrameDeltaE { get; set; } = new List<double>();
        public List<double> PerFrameDeltaN { get; set; } = new List<double>();

        public static ProcessingReport Generate(List<SyncedFrame> stableFrames, CalibrationConfig config)
        {
            var report = new ProcessingReport();
            report.TotalFrames = stableFrames.Count;

            var rawE = new List<double>();
            var rawN = new List<double>();

            foreach (var frame in stableFrames)
            {
                double dE, dN;
                OffsetCalculator.Calculate(frame, config, out dE, out dN);
                rawE.Add(dE);
                rawN.Add(dN);
            }

            var maskE = MadOutlierMask(rawE);
            var maskN = MadOutlierMask(rawN);

            var filtE = new List<double>();
            var filtN = new List<double>();

            for (int i = 0; i < rawE.Count; i++)
            {
                if (!maskE[i] && !maskN[i])
                {
                    filtE.Add(rawE[i]);
                    filtN.Add(rawN[i]);
                }
            }

            report.PerFrameDeltaE = filtE;
            report.PerFrameDeltaN = filtN;
            report.ValidFrames = filtE.Count;

            if (filtE.Count > 0)
            {
                report.DeltaE = Mean(filtE);
                report.DeltaN = Mean(filtN);
                report.SigmaE = Std(filtE, report.DeltaE);
                report.SigmaN = Std(filtN, report.DeltaN);
                report.DeltaH = Math.Sqrt(report.DeltaE * report.DeltaE + report.DeltaN * report.DeltaN);
                report.Azimuth = Math.Atan2(report.DeltaE, report.DeltaN) * 180.0 / Math.PI;
                if (report.Azimuth < 0) report.Azimuth += 360.0;
            }

            return report;
        }

        public string ToSummary()
        {
            var ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.AppendLine("========== Processing Report ==========");
            sb.AppendLine(string.Format(ci, "Total Frames:  {0}", TotalFrames));
            sb.AppendLine(string.Format(ci, "Valid Frames:  {0}", ValidFrames));
            sb.AppendLine(string.Format(ci, "Delta E:       {0:F4} mm", DeltaE));
            sb.AppendLine(string.Format(ci, "Delta N:       {0:F4} mm", DeltaN));
            sb.AppendLine(string.Format(ci, "Sigma E:       {0:F4} mm", SigmaE));
            sb.AppendLine(string.Format(ci, "Sigma N:       {0:F4} mm", SigmaN));
            sb.AppendLine(string.Format(ci, "Delta H:       {0:F4} mm", DeltaH));
            sb.AppendLine(string.Format(ci, "Azimuth:       {0:F2} deg", Azimuth));
            if (!string.IsNullOrEmpty(AnnotatedImageDirectory))
            {
                sb.AppendLine(string.Format(ci, "North Overlay: {0} ok / {1} fail", AnnotatedImageCount, AnnotatedImageFailedCount));
                sb.AppendLine(string.Format(ci, "Image Mode:    {0}", MetadataOnlyMode ? "metadata-only" : "overlay+metadata"));
                sb.AppendLine(string.Format(ci, "Circle Origin: {0} detected / {1} fallback", CircleDetectedCount, CircleFallbackCenterCount));
                sb.AppendLine(string.Format(ci, "Image Out Dir: {0}", AnnotatedImageDirectory));
                if (!string.IsNullOrEmpty(AnnotatedImageManifestPath))
                    sb.AppendLine(string.Format(ci, "North CSV:    {0}", AnnotatedImageManifestPath));
            }
            sb.AppendLine("========================================");
            return sb.ToString();
        }

        public void SaveToFile(string filePath)
        {
            var ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append(ToSummary());
            sb.AppendLine();
            sb.AppendLine("Per-frame offsets (DeltaE, DeltaN):");

            for (int i = 0; i < PerFrameDeltaE.Count; i++)
            {
                sb.AppendLine(string.Format(ci, "  [{0}] E={1:F4}, N={2:F4}",
                    i, PerFrameDeltaE[i], PerFrameDeltaN[i]));
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        private static bool[] MadOutlierMask(List<double> values)
        {
            var mask = new bool[values.Count];
            if (values.Count == 0) return mask;

            double med = Median(values);
            var absDevs = new List<double>(values.Count);
            foreach (var v in values)
                absDevs.Add(Math.Abs(v - med));

            double mad = Median(absDevs) * 1.4826;

            if (mad < 1e-12)
                return mask;

            for (int i = 0; i < values.Count; i++)
                mask[i] = Math.Abs(values[i] - med) > 3.0 * mad;

            return mask;
        }

        private static double Median(List<double> values)
        {
            var sorted = new List<double>(values);
            sorted.Sort();
            int n = sorted.Count;
            if (n % 2 == 1)
                return sorted[n / 2];
            return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }

        private static double Mean(List<double> values)
        {
            double sum = 0;
            foreach (var v in values) sum += v;
            return sum / values.Count;
        }

        private static double Std(List<double> values, double mean)
        {
            double sum = 0;
            foreach (var v in values)
            {
                double d = v - mean;
                sum += d * d;
            }
            return Math.Sqrt(sum / values.Count);
        }
    }
}

