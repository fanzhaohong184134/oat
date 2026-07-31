using System;
using System.IO;
using Wit.Example_BWT901BLE.DataProcessing.Calibration;

namespace Wit.Example_BWT901BLE.DataProcessing.PostProcessing
{
    public class PostProcessor
    {
        public ProcessingReport Run(string imuCsvPath, string cameraCsvPath, CalibrationConfig config)
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

            return ProcessingReport.Generate(stable, config);
        }
    }
}
