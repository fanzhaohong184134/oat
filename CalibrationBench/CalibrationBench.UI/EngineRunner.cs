using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CalibrationBench.UI
{
    /// <summary>通过启动无界面引擎 exe 执行校准，完全解耦(仅进程调用)。</summary>
    public sealed class EngineRunner
    {
        public string EnginePath { get; set; }

        public EngineRunner(string enginePath) { EnginePath = enginePath; }

        public sealed class RunResult
        {
            public int ExitCode;
            public string StdOut;
            public string StdErr;
            public bool Passed { get { return ExitCode == 0; } }
            public bool Executed { get { return ExitCode == 0 || ExitCode == 2; } }
        }

        public RunResult Run(string step, string inputPath, string outputPath,
                             string configPath, string deviceRoot, string deviceId)
        {
            if (!File.Exists(EnginePath))
                throw new FileNotFoundException("未找到校准引擎: " + EnginePath);

            var sb = new StringBuilder();
            sb.Append("--step ").Append(step);
            sb.Append(" --input ").Append(Q(inputPath));
            sb.Append(" --output ").Append(Q(outputPath));
            if (!string.IsNullOrEmpty(configPath)) sb.Append(" --config ").Append(Q(configPath));
            if (!string.IsNullOrEmpty(deviceRoot)) sb.Append(" --device-root ").Append(Q(deviceRoot));
            if (!string.IsNullOrEmpty(deviceId)) sb.Append(" --device-id ").Append(Q(deviceId));

            var psi = new ProcessStartInfo
            {
                FileName = EnginePath,
                Arguments = sb.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            using (var p = Process.Start(psi))
            {
                string so = p.StandardOutput.ReadToEnd();
                string se = p.StandardError.ReadToEnd();
                p.WaitForExit();
                return new RunResult { ExitCode = p.ExitCode, StdOut = so, StdErr = se };
            }
        }

        private static string Q(string s) { return "\"" + s + "\""; }
    }
}
