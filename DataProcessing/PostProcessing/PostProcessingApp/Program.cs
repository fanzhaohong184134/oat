using System;
using System.IO;
using dsat.DataProcessing.Calibration;
using dsat.DataProcessing.PostProcessing;

namespace dsat.PostProcessingApp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("=== 对中偏移后处理工具 ===");

            try
            {
                string imuCsvPath;
                string cameraCsvPath;
                string configPath;

                if (args.Length >= 3)
                {
                    imuCsvPath = args[0];
                    cameraCsvPath = args[1];
                    configPath = args[2];
                }
                else
                {
                    Console.WriteLine("请输入文件路径：");
                    imuCsvPath = ReadPath("IMU CSV 文件路径");
                    cameraCsvPath = ReadPath("Camera CSV 文件路径");
                    configPath = ReadPath("CalibrationConfig JSON 路径");
                }

                if (!File.Exists(imuCsvPath))
                {
                    Console.Error.WriteLine($"错误: IMU CSV 文件不存在: {imuCsvPath}");
                    return 1;
                }
                if (!File.Exists(cameraCsvPath))
                {
                    Console.Error.WriteLine($"错误: Camera CSV 文件不存在: {cameraCsvPath}");
                    return 1;
                }
                if (!File.Exists(configPath))
                {
                    Console.Error.WriteLine($"错误: 配置文件不存在: {configPath}");
                    return 1;
                }

                Console.WriteLine();
                Console.WriteLine($"IMU CSV:    {imuCsvPath}");
                Console.WriteLine($"Camera CSV: {cameraCsvPath}");
                Console.WriteLine($"Config:     {configPath}");
                Console.WriteLine();

                var config = CalibrationConfig.Load(configPath);
                var processor = new PostProcessor();
                var report = processor.Run(imuCsvPath, cameraCsvPath, config);

                Console.WriteLine("=== 处理报告 ===");
                Console.WriteLine(report.ToSummary());

                string reportDir = Path.GetDirectoryName(Path.GetFullPath(imuCsvPath));
                string reportFileName = string.Format("processing_report_{0:yyyyMMdd_HHmmss}.txt",
                    DateTime.Now);
                string reportPath = Path.Combine(reportDir, reportFileName);
                report.SaveToFile(reportPath);
                Console.WriteLine($"完整报告已保存到: {reportPath}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"错误: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static string ReadPath(string prompt)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim().Trim('"');
                Console.WriteLine("    路径不能为空。");
            }
        }
    }
}

