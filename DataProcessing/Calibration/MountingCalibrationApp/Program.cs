using System;
using System.IO;
using Wit.Example_BWT901BLE.DataProcessing.Calibration;

namespace Wit.Example_BWT901BLE.MountingCalibrationApp
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                string inputPath = GetArg(args, "--input");
                string outputPath = GetArg(args, "--output");
                string configPath = GetArg(args, "--config");

                if (!string.IsNullOrWhiteSpace(inputPath) && !string.IsNullOrWhiteSpace(outputPath))
                {
                    return RunJsonMode(inputPath, outputPath, configPath);
                }

                RunInteractive();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static int RunJsonMode(string inputPath, string outputPath, string configPath)
        {
            var input = CalibrationJsonUtil.LoadFromFile<MountingCalibrationInput>(inputPath);
            var calibrator = new MountingAngleCalibrator();
            double deltaPitch, deltaRoll;
            calibrator.Calibrate(input.ImuAngleX, input.ImuAngleY, input.UActual, input.VActual,
                input.Fx, input.Fy, input.Cx, input.Cy, out deltaPitch, out deltaRoll);

            CalibrationConfig config = File.Exists(configPath) ? CalibrationConfig.Load(configPath) : new CalibrationConfig();
            config.DeltaPitch = deltaPitch;
            config.DeltaRoll = deltaRoll;
            config.Save(configPath);

            var output = new MountingCalibrationOutput
            {
                Success = true,
                Message = "安装角标定完成",
                ConfigPath = configPath,
                DeltaPitch = deltaPitch,
                DeltaRoll = deltaRoll
            };
            CalibrationJsonUtil.SaveToFile(output, outputPath);
            WriteSummary(outputPath, string.Format(
                "Mounting Calibration Summary\r\nConfig: {0}\r\nDeltaPitch={1:F8}\r\nDeltaRoll={2:F8}",
                output.ConfigPath,
                output.DeltaPitch,
                output.DeltaRoll));
            return 0;
        }

        private static void WriteSummary(string outputPath, string content)
        {
            string dir = Path.GetDirectoryName(outputPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string file = Path.Combine(dir, Path.GetFileNameWithoutExtension(outputPath) + "_summary.txt");
            File.WriteAllText(file, content);
        }

        private static void RunInteractive()
        {
            Console.WriteLine("=== 安装角标定工具 ===");
            double angleX = ReadDouble("IMU AngleX(roll)");
            double angleY = ReadDouble("IMU AngleY(pitch)");
            double u = ReadDouble("靶点像素 u");
            double v = ReadDouble("靶点像素 v");
            double fx = ReadDouble("fx");
            double fy = ReadDouble("fy");
            double cx = ReadDouble("cx");
            double cy = ReadDouble("cy");

            var calibrator = new MountingAngleCalibrator();
            double deltaPitch, deltaRoll;
            calibrator.Calibrate(angleX, angleY, u, v, fx, fy, cx, cy, out deltaPitch, out deltaRoll);

            Console.WriteLine("deltaPitch={0:F6}, deltaRoll={1:F6}", deltaPitch, deltaRoll);
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

        private static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + ": ");
                double v;
                if (double.TryParse(Console.ReadLine(), out v)) return v;
                Console.WriteLine("输入无效，请重试。");
            }
        }
    }
}
