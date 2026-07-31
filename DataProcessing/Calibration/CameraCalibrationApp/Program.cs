using System;
using System.IO;
using Wit.Example_BWT901BLE.DataProcessing.Calibration;

namespace Wit.Example_BWT901BLE.CameraCalibrationApp
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
            var input = CalibrationJsonUtil.LoadFromFile<CameraCalibrationInput>(inputPath);
            var config = CameraCalibrator.CreateManualConfig(
                input.Fx, input.Fy, input.Cx, input.Cy,
                input.K1, input.K2, input.P1, input.P2,
                input.ImageWidth, input.ImageHeight);

            string actualConfigPath = string.IsNullOrWhiteSpace(configPath)
                ? "calibration_config.json"
                : configPath;

            CalibrationConfig merged = File.Exists(actualConfigPath) ? CalibrationConfig.Load(actualConfigPath) : new CalibrationConfig();
            merged.Fx = config.Fx;
            merged.Fy = config.Fy;
            merged.Cx = config.Cx;
            merged.Cy = config.Cy;
            merged.K1 = config.K1;
            merged.K2 = config.K2;
            merged.P1 = config.P1;
            merged.P2 = config.P2;
            merged.ImageWidth = config.ImageWidth;
            merged.ImageHeight = config.ImageHeight;
            merged.Save(actualConfigPath);

            var output = new CameraCalibrationOutput
            {
                Success = true,
                Message = "相机标定完成",
                ConfigPath = actualConfigPath,
                Fx = merged.Fx,
                Fy = merged.Fy,
                Cx = merged.Cx,
                Cy = merged.Cy,
                K1 = merged.K1,
                K2 = merged.K2,
                P1 = merged.P1,
                P2 = merged.P2,
                ImageWidth = merged.ImageWidth,
                ImageHeight = merged.ImageHeight
            };
            CalibrationJsonUtil.SaveToFile(output, outputPath);
            WriteSummary(outputPath, string.Format(
                "Camera Calibration Summary\r\nConfig: {0}\r\nfx={1:F6}, fy={2:F6}, cx={3:F6}, cy={4:F6}\r\nk1={5:F8}, k2={6:F8}, p1={7:F8}, p2={8:F8}\r\nimage={9}x{10}",
                output.ConfigPath,
                output.Fx, output.Fy, output.Cx, output.Cy,
                output.K1, output.K2, output.P1, output.P2,
                output.ImageWidth, output.ImageHeight));
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
            Console.WriteLine("=== 相机内参标定工具 ===");
            Console.WriteLine("请输入相机内参参数（可从Python OpenCV标定结果获取）：");
            Console.WriteLine();

            double fx = ReadDouble("fx (焦距x, 像素)");
            double fy = ReadDouble("fy (焦距y, 像素)");
            double cx = ReadDouble("cx (主点x, 像素)");
            double cy = ReadDouble("cy (主点y, 像素)");
            double k1 = ReadDouble("k1 (径向畸变系数1)");
            double k2 = ReadDouble("k2 (径向畸变系数2)");
            double p1 = ReadDouble("p1 (切向畸变系数1)");
            double p2 = ReadDouble("p2 (切向畸变系数2)");
            int imageWidth = ReadInt("imageWidth (图像宽度, 像素)");
            int imageHeight = ReadInt("imageHeight (图像高度, 像素)");

            var config = CameraCalibrator.CreateManualConfig(
                fx, fy, cx, cy, k1, k2, p1, p2, imageWidth, imageHeight);

            Console.WriteLine();
            Console.Write("保存路径 [camera_intrinsics.json]: ");
            string path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
                path = "camera_intrinsics.json";

            config.Save(path);
            Console.WriteLine($"已保存相机内参配置到: {path}");
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
                Console.Write($"  {prompt}: ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double value))
                    return value;
                Console.WriteLine("    输入无效，请输入数字。");
            }
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int value))
                    return value;
                Console.WriteLine("    输入无效，请输入整数。");
            }
        }
    }
}
