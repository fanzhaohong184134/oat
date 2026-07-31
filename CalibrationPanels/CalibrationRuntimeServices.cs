using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace dsat.CalibrationPanels
{
    public class CalibrationPathService
    {
        private readonly string _baseDirectory;

        public CalibrationPathService(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public string BaseDirectory => _baseDirectory;
        public string DeviceInfoRoot => Path.Combine(_baseDirectory, "device_info");
        public string DeviceIdFilePath => Path.Combine(DeviceInfoRoot, "device_id.txt");
        public string ConfigPath => Path.Combine(_baseDirectory, "calibration_config.json");

        public string EnsureAndPersistDeviceId(string preferredDeviceId)
        {
            if (!Directory.Exists(DeviceInfoRoot))
                Directory.CreateDirectory(DeviceInfoRoot);

            string deviceId = string.IsNullOrWhiteSpace(preferredDeviceId) ? "AT1" : preferredDeviceId.Trim();
            File.WriteAllText(DeviceIdFilePath, deviceId, Encoding.UTF8);
            return deviceId;
        }

        public string GetDefaultDeviceId()
        {
            if (!Directory.Exists(DeviceInfoRoot))
                Directory.CreateDirectory(DeviceInfoRoot);

            if (File.Exists(DeviceIdFilePath))
            {
                string id = File.ReadAllText(DeviceIdFilePath).Trim();
                if (!string.IsNullOrWhiteSpace(id))
                    return id;
            }

            File.WriteAllText(DeviceIdFilePath, "AT1", Encoding.UTF8);
            return "AT1";
        }

        public void EnsureCalibrationDirs(string deviceId, string calibrationKey, out string inputDir, out string outputDir)
        {
            string root = Path.Combine(DeviceInfoRoot, deviceId, calibrationKey);
            inputDir = Path.Combine(root, "input");
            outputDir = Path.Combine(root, "output");
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);
        }

        public string CreateTimestampedFile(string dir, string prefix, string extension)
        {
            string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(dir, string.Format("{0}_{1}.{2}", prefix, ts, extension.TrimStart('.')));
        }
    }

    public static class CalibrationExecutableResolver
    {
        public static string Resolve(string baseDirectory, string exeName, string relativeProjectOutput)
        {
            string direct = Path.Combine(baseDirectory, exeName);
            if (File.Exists(direct)) return direct;

            string repoRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", ".."));
            string releasePath = Path.Combine(repoRoot, relativeProjectOutput, "Release", exeName);
            if (File.Exists(releasePath)) return releasePath;

            string debugPath = Path.Combine(repoRoot, relativeProjectOutput, "Debug", exeName);
            if (File.Exists(debugPath)) return debugPath;

            return direct;
        }
    }

    public static class CalibrationProcessRunner
    {
        public static int Run(string exePath, string arguments, out string stdOut, out string stdErr)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                stdOut = process.StandardOutput.ReadToEnd();
                stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}

