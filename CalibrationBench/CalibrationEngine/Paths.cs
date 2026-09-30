using System;
using System.IO;

namespace CalibrationEngine
{
    /// <summary>device_info 目录与成果归档路径。</summary>
    public static class Paths
    {
        public static string StepDir(string step)
        {
            switch (step.ToUpperInvariant())
            {
                case "0A": return "camera_calibration";
                case "0B": return "mounting_calibration";
                case "0C": return "heading_calibration";
                case "0D": return "factory_verification";
                default: throw new ArgumentException("未知步骤: " + step);
            }
        }

        public static string Timestamp()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        /// <summary>归档一次输入/输出到 device_info/&lt;id&gt;/&lt;stepdir&gt;/{input,output}/。</summary>
        public static void Archive(string deviceRoot, string deviceId, string step,
            string inputJson, string outputJson, out string inPath, out string outPath)
        {
            var baseDir = Path.Combine(deviceRoot, "device_info", deviceId, StepDir(step));
            var ts = Timestamp();
            var inputDir = Path.Combine(baseDir, "input");
            var outputDir = Path.Combine(baseDir, "output");
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);
            inPath = Path.Combine(inputDir, step + "_input_" + ts + ".json");
            outPath = Path.Combine(outputDir, step + "_output_" + ts + ".json");
            if (inputJson != null) File.WriteAllText(inPath, inputJson);
            if (outputJson != null) File.WriteAllText(outPath, outputJson);
        }
    }
}
