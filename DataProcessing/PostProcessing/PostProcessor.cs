using System;
using System.IO;
using System.Linq;
using dsat.DataProcessing.Calibration;

namespace dsat.DataProcessing.PostProcessing
{
    public class PostProcessor
    {
        public ProcessingReport Run(
            string imuCsvPath,
            string cameraCsvPath,
            CalibrationConfig config,
            bool drawDetectedCircleOutline = true,
            bool metadataOnlyMode = false)
        {
            if (!File.Exists(imuCsvPath))
                throw new FileNotFoundException("IMU CSV file not found.", imuCsvPath);
            if (!File.Exists(cameraCsvPath))
                throw new FileNotFoundException("Camera CSV file not found.", cameraCsvPath);
            if (config == null)
                throw new ArgumentNullException("config");

            var imuData = CsvParser.ParseImuCsv(imuCsvPath);
            if (imuData.Count == 0)
                throw new InvalidOperationException("No valid IMU samples parsed from: " + imuCsvPath);

            var cameraFrames = CsvParser.ParseCameraCsv(cameraCsvPath);
            if (cameraFrames.Count == 0)
                throw new InvalidOperationException("No valid camera frames parsed from: " + cameraCsvPath);

            var synced = TimeSync.Synchronize(imuData, cameraFrames);

            var stable = StabilityFilter.Filter(synced, config.GThreshold, config.OmegaThreshold, config.MinStableFrames);
            if (stable.Count == 0)
                throw new InvalidOperationException("No stable frames found after filtering.");

            var report = ProcessingReport.Generate(stable, config);

            string cameraCsvDir = Path.GetDirectoryName(Path.GetFullPath(cameraCsvPath));
            string outDir = Path.Combine(cameraCsvDir, "north_annotated");
            var anno = NorthDirectionImageAnnotator.AnnotateFrames(stable, config, outDir, drawDetectedCircleOutline, metadataOnlyMode);
            report.AnnotatedImageCount = anno.SuccessCount;
            report.AnnotatedImageFailedCount = anno.FailedCount;
            report.AnnotatedImageDirectory = anno.OutputDirectory;
            report.AnnotatedImageManifestPath = anno.ManifestCsvPath;
            report.MetadataOnlyMode = metadataOnlyMode;

            if (!string.IsNullOrEmpty(anno.ManifestCsvPath) && File.Exists(anno.ManifestCsvPath))
            {
                var lines = File.ReadAllLines(anno.ManifestCsvPath).Skip(1);
                int detected = 0;
                int fallback = 0;
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.Contains(",OK,") == false) continue;
                    if (line.Contains(",Y,OK,")) detected++;
                    else if (line.Contains(",N,OK,")) fallback++;
                }
                report.CircleDetectedCount = detected;
                report.CircleFallbackCenterCount = fallback;
            }

            return report;
        }
    }
}

