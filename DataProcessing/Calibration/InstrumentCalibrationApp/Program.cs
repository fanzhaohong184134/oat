using System;
using System.IO;
using dsat.DataProcessing.Calibration;

namespace dsat.InstrumentCalibrationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = GetArg(args, "--input");
            string outputPath = GetArg(args, "--output");
            string configPathArg = GetArg(args, "--config");
            if (!string.IsNullOrWhiteSpace(inputPath) && !string.IsNullOrWhiteSpace(outputPath))
            {
                RunJsonMode(inputPath, outputPath, configPathArg);
                return;
            }

            Console.WriteLine("=== ψ_offset 航向现场校核工具 ===");
            Console.WriteLine("模式说明:");
            Console.WriteLine("  1) 有已知方向参考: 执行现场校核/必要时重标");
            Console.WriteLine("  2) 无已知方向参考: 跳过现场校核，依靠磁场定位(精度受损)");
            Console.WriteLine();

            int mode = ReadMode();
            if (mode == 1)
            {
                RunWithKnownReference();
                return;
            }

            RunMagneticOnlyFallback();
        }

        private static void RunJsonMode(string inputPath, string outputPath, string configPathArg)
        {
            var input = CalibrationJsonUtil.LoadFromFile<HeadingCalibrationInput>(inputPath);

            string configPath = string.IsNullOrWhiteSpace(configPathArg) ? "calibration_config.json" : configPathArg;
            CalibrationConfig config = File.Exists(configPath) ? CalibrationConfig.Load(configPath) : new CalibrationConfig();

            var calibrator = new InstrumentCalibrator();
            double currentOffset = input.CurrentPsiOffset;
            double newOffset = currentOffset;
            double predicted = 0.0;
            double error = 0.0;
            bool updated = false;
            string mode;
            string message;

            if (input.HasKnownReference)
            {
                mode = "KnownReference";
                predicted = calibrator.Verify(
                    input.ImuAngleZ,
                    input.MagneticDeclination,
                    currentOffset,
                    input.U1,
                    input.V1,
                    input.U2,
                    input.V2);

                error = NormalizeTo180(input.KnownAzimuth - predicted);
                if (Math.Abs(error) >= 1.0)
                {
                    newOffset = NormalizeTo180(currentOffset + error);
                    updated = true;
                    message = "误差超过阈值，已更新 ψ_offset";
                }
                else
                {
                    message = "误差在阈值内，保留当前 ψ_offset";
                }
            }
            else
            {
                mode = "MagneticOnly";
                message = "无已知方向参考，使用磁场定位模式（精度受损）";
            }

            config.PsiOffset = newOffset;
            config.MagneticDeclination = input.MagneticDeclination;
            config.Save(configPath);

            var output = new HeadingCalibrationOutput
            {
                Success = true,
                Mode = mode,
                Message = message,
                ConfigPath = configPath,
                CurrentPsiOffset = currentOffset,
                NewPsiOffset = newOffset,
                PredictedAzimuth = predicted,
                Error = error,
                Updated = updated
            };
            CalibrationJsonUtil.SaveToFile(output, outputPath);
            WriteSummary(outputPath, string.Format(
                "Heading Calibration Summary\r\nMode: {0}\r\nMessage: {1}\r\nConfig: {2}\r\nCurrentPsiOffset={3:F8}\r\nNewPsiOffset={4:F8}\r\nPredictedAzimuth={5:F8}\r\nError={6:F8}\r\nUpdated={7}",
                output.Mode,
                output.Message,
                output.ConfigPath,
                output.CurrentPsiOffset,
                output.NewPsiOffset,
                output.PredictedAzimuth,
                output.Error,
                output.Updated));
        }

        private static void WriteSummary(string outputPath, string content)
        {
            string dir = Path.GetDirectoryName(outputPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string file = Path.Combine(dir, Path.GetFileNameWithoutExtension(outputPath) + "_summary.txt");
            File.WriteAllText(file, content);
        }

        private static void RunWithKnownReference()
        {
            Console.WriteLine("[模式1] 已知方向参考校核");
            Console.WriteLine();

            double imuAngleZ = ReadDouble("IMU AngleZ (°)");
            double magneticDeclination = ReadDouble("磁偏角 D (°, 东偏为正)");
            double u1 = ReadDouble("P1 像素坐标 u1");
            double v1 = ReadDouble("P1 像素坐标 v1");
            double u2 = ReadDouble("P2 像素坐标 u2");
            double v2 = ReadDouble("P2 像素坐标 v2");
            double knownAzimuth = ReadDouble("P1→P2 已知真北方位角 (°)");

            CalibrationConfig config = LoadOrCreateConfig();
            var calibrator = new InstrumentCalibrator();

            double currentOffset = config.PsiOffset;
            double predicted = calibrator.Verify(
                imuAngleZ, magneticDeclination, currentOffset, u1, v1, u2, v2);
            double error = NormalizeTo180(knownAzimuth - predicted);

            Console.WriteLine();
            Console.WriteLine($"当前 ψ_offset = {currentOffset:F4}°");
            Console.WriteLine($"当前反算方位角 = {predicted:F4}°");
            Console.WriteLine($"与已知方位角误差 e = {error:F4}°");

            double newOffset = currentOffset;
            if (Math.Abs(error) >= 1.0)
            {
                newOffset = NormalizeTo180(currentOffset + error);
                Console.WriteLine($"|e| >= 1°，建议更新 ψ_offset -> {newOffset:F4}°");
            }
            else
            {
                Console.WriteLine("|e| < 1°，建议保留当前 ψ_offset。");
            }

            Console.Write("是否按建议值写入配置? (y/n) [y]: ");
            string saveAnswer = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(saveAnswer) || saveAnswer.Trim().ToLower() == "y")
            {
                config.PsiOffset = newOffset;
                config.MagneticDeclination = magneticDeclination;
                SaveConfig(config);
                Console.WriteLine($"已保存 ψ_offset={newOffset:F4}°, D={magneticDeclination:F4}°");
            }
        }

        private static void RunMagneticOnlyFallback()
        {
            Console.WriteLine("[模式2] 无已知方向参考，跳过现场校核");
            Console.WriteLine("说明: Step 0C 非必须。将沿用现有 ψ_offset，仅可更新磁偏角 D。\n");

            CalibrationConfig config = LoadOrCreateConfig();
            Console.WriteLine($"当前配置 ψ_offset = {config.PsiOffset:F4}°");
            Console.WriteLine("若不确定 D，可直接回车保留现值。\n");

            Console.Write($"磁偏角 D (当前 {config.MagneticDeclination:F4}°): ");
            string dInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dInput))
            {
                double d;
                if (double.TryParse(dInput, out d))
                {
                    config.MagneticDeclination = d;
                    Console.WriteLine($"已更新 D = {d:F4}°");
                }
                else
                {
                    Console.WriteLine("输入无效，保持原 D 不变。");
                }
            }

            Console.Write("是否保存当前配置? (y/n) [y]: ");
            string answer = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(answer) || answer.Trim().ToLower() == "y")
            {
                SaveConfig(config);
                Console.WriteLine("已保存。注意: 当前为磁场定位模式，绝对航向精度可能受损。");
            }
        }

        private static int ReadMode()
        {
            while (true)
            {
                Console.Write("请选择模式 [1/2] (默认1): ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return 1;
                if (input.Trim() == "1") return 1;
                if (input.Trim() == "2") return 2;
                Console.WriteLine("输入无效，请输入 1 或 2。\n");
            }
        }

        private static CalibrationConfig LoadOrCreateConfig()
        {
            string configPath = ReadConfigPath();
            CalibrationConfig config;
            if (File.Exists(configPath))
            {
                config = CalibrationConfig.Load(configPath);
                Console.WriteLine($"已加载现有配置: {configPath}");
            }
            else
            {
                config = new CalibrationConfig();
                Console.WriteLine("未找到现有配置，将创建新配置。");
            }

            _configPath = configPath;
            return config;
        }

        private static string ReadConfigPath()
        {
            Console.Write("CalibrationConfig路径 [calibration_config.json]: ");
            string configPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(configPath))
                configPath = "calibration_config.json";
            return configPath;
        }

        private static void SaveConfig(CalibrationConfig config)
        {
            config.Save(_configPath);
            Console.WriteLine($"配置已写入: {_configPath}");
        }

        private static double NormalizeTo180(double angle)
        {
            while (angle > 180.0) angle -= 360.0;
            while (angle <= -180.0) angle += 360.0;
            return angle;
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        private static string _configPath = "calibration_config.json";

        private static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double value))
                    return value;
                Console.WriteLine("    输入无效，请输入数字。");
            }
        }
    }
}

