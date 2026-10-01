using System;
using System.IO;
using System.Text;
using CalibrationEngine.Models;

namespace CalibrationEngine
{
    /// <summary>
    /// 无界面出厂校准引擎。用法:
    ///   CalibrationEngine.exe --step 0A|0B|0C|0D --input in.json --output out.json
    ///                         [--config calibration_config.json]
    ///                         [--device-root DIR] [--device-id ID]
    /// 退出码: 0=合格, 2=不合格(执行成功但未达判据), 1=错误。
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            try { Console.OutputEncoding = new UTF8Encoding(false); } catch { /* 重定向环境下忽略 */ }
            try
            {
                var a = ParseArgs(args);
                if (!a.ContainsKey("step") || !a.ContainsKey("input"))
                {
                    Console.Error.WriteLine("参数缺失: 需要 --step 与 --input。");
                    PrintUsage();
                    return 1;
                }
                string step = a["step"].ToUpperInvariant();
                string inputPath = a["input"];
                string outputPath = a.ContainsKey("output") ? a["output"] : null;
                string configPath = a.ContainsKey("config") ? a["config"] : null;
                string deviceRoot = a.ContainsKey("device-root") ? a["device-root"] : null;
                string deviceId = a.ContainsKey("device-id") ? a["device-id"] : null;
                string reportPath = a.ContainsKey("report") ? a["report"] : null;

                if (!File.Exists(inputPath)) { Console.Error.WriteLine("输入文件不存在: " + inputPath); return 1; }
                string inputJson = File.ReadAllText(inputPath);

                bool passed; string outputJson; string summary;
                switch (step)
                {
                    case "0A": passed = Do0A(inputPath, out outputJson, out summary, configPath, ref deviceId); break;
                    case "0B": passed = Do0B(inputPath, out outputJson, out summary, configPath, ref deviceId); break;
                    case "0C": passed = Do0C(inputPath, out outputJson, out summary, configPath, ref deviceId); break;
                    case "0D": passed = Do0D(inputPath, out outputJson, out summary, configPath, ref deviceId, reportPath, deviceRoot); break;
                    default: Console.Error.WriteLine("未知步骤: " + step); return 1;
                }

                if (outputPath != null) { EnsureDir(outputPath); File.WriteAllText(outputPath, outputJson); }

                if (deviceRoot != null && deviceId != null)
                {
                    Paths.Archive(deviceRoot, deviceId, step, inputJson, outputJson, out string inP, out string outP);
                    Console.WriteLine("归档输入: " + inP);
                    Console.WriteLine("归档输出: " + outP);
                }

                Console.WriteLine(summary);
                Console.WriteLine(passed ? "结果: 合格" : "结果: 未通过");
                return passed ? 0 : 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("执行异常: " + ex.Message);
                return 1;
            }
        }

        // ---------- 各步骤 ----------
        private static bool Do0A(string inputPath, out string outputJson, out string summary, string configPath, ref string deviceId)
        {
            var inp = Json.ReadFile<Step0AInput>(inputPath);
            if (deviceId == null) deviceId = inp.DeviceId;
            var o = Steps.Run0A(inp);
            outputJson = Json.ToJson(o); summary = o.Message;
            if (o.Passed && configPath != null)
                UpdateConfig(configPath, inp.DeviceId, cfg =>
                {
                    var k = o.Intrinsics;
                    cfg.Fx = k.Fx; cfg.Fy = k.Fy; cfg.Cx = k.Cx; cfg.Cy = k.Cy;
                    cfg.K1 = k.K1; cfg.K2 = k.K2; cfg.P1 = k.P1; cfg.P2 = k.P2;
                    cfg.ImageWidth = k.ImageWidth; cfg.ImageHeight = k.ImageHeight;
                });
            return o.Passed;
        }

        private static bool Do0B(string inputPath, out string outputJson, out string summary, string configPath, ref string deviceId)
        {
            var inp = Json.ReadFile<Step0BInput>(inputPath);
            if (deviceId == null) deviceId = inp.DeviceId;
            var o = Steps.Run0B(inp);
            outputJson = Json.ToJson(o); summary = o.Message;
            if (o.Passed && configPath != null)
                UpdateConfig(configPath, inp.DeviceId, cfg => { cfg.DeltaPitch = o.DeltaPitch; cfg.DeltaRoll = o.DeltaRoll; });
            return o.Passed;
        }

        private static bool Do0C(string inputPath, out string outputJson, out string summary, string configPath, ref string deviceId)
        {
            var inp = Json.ReadFile<Step0CInput>(inputPath);
            if (deviceId == null) deviceId = inp.DeviceId;
            var o = Steps.Run0C(inp);
            outputJson = Json.ToJson(o); summary = o.Message;
            if (o.Passed && configPath != null)
                UpdateConfig(configPath, inp.DeviceId, cfg =>
                {
                    cfg.PsiOffset = o.PsiOffsetDeg;
                    cfg.AlphaBoard = inp.AlphaBoardDeg;
                    cfg.MagneticDeclination = inp.MagneticDeclinationDeg;
                });
            return o.Passed;
        }

        private static bool Do0D(string inputPath, out string outputJson, out string summary, string configPath, ref string deviceId,
                                 string reportPath, string deviceRoot)
        {
            var inp = Json.ReadFile<Step0DInput>(inputPath);
            if (deviceId == null) deviceId = inp.DeviceId;
            var o = Steps.Run0D(inp);
            outputJson = Json.ToJson(o); summary = o.Message;

            // 自动生成出厂校准报告(Markdown)
            try
            {
                CalibrationConfig cfg = (configPath != null && File.Exists(configPath))
                    ? Json.ReadFile<CalibrationConfig>(configPath) : new CalibrationConfig { DeviceId = inp.DeviceId };
                string md = ReportWriter.Build(cfg, o);

                string target = reportPath;
                if (target == null && deviceRoot != null && deviceId != null)
                {
                    string dir = Path.Combine(deviceRoot, "device_info", deviceId, "factory_verification", "output");
                    Directory.CreateDirectory(dir);
                    target = Path.Combine(dir, "calibration_report_" + Paths.Timestamp() + ".md");
                }
                if (target != null)
                {
                    EnsureDir(target);
                    File.WriteAllText(target, md, new UTF8Encoding(false));
                    Console.WriteLine("出厂报告: " + target);
                }
            }
            catch (Exception ex) { Console.Error.WriteLine("报告生成警告: " + ex.Message); }

            return o.Passed;
        }

        // ---------- 配置读写 ----------
        private static void UpdateConfig(string configPath, string deviceId, Action<CalibrationConfig> mutate)
        {
            CalibrationConfig cfg;
            if (File.Exists(configPath))
            {
                try { cfg = Json.ReadFile<CalibrationConfig>(configPath); }
                catch { cfg = new CalibrationConfig(); }
            }
            else cfg = new CalibrationConfig();
            if (string.IsNullOrEmpty(cfg.DeviceId)) cfg.DeviceId = deviceId;
            mutate(cfg);
            cfg.CalibratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            Json.WriteFile(configPath, cfg);
            Console.WriteLine("已更新配置: " + configPath);
        }

        // ---------- 工具 ----------
        private static void EnsureDir(string path)
        {
            var d = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(d)) Directory.CreateDirectory(d);
        }

        private static System.Collections.Generic.Dictionary<string, string> ParseArgs(string[] args)
        {
            var d = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    string key = args[i].Substring(2);
                    string val = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : "true";
                    d[key] = val;
                }
            }
            return d;
        }

        private static void PrintUsage()
        {
            Console.Error.WriteLine("用法: CalibrationEngine.exe --step 0A|0B|0C|0D --input in.json --output out.json [--config cfg.json] [--device-root DIR] [--device-id ID] [--report report.md]");
        }
    }
}
