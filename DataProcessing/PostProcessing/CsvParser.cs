using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Wit.Example_BWT901BLE.DataProcessing.PostProcessing
{
    public static class CsvParser
    {
        private static double ParseDouble(string s)
        {
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double val);
            return val;
        }

        private static long ParseLong(string s)
        {
            long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out long val);
            return val;
        }

        // Strip Excel ="..." formula wrapper if present
        private static string StripExcelQuote(string s)
        {
            s = s.Trim();
            if (s.StartsWith("=\"") && s.EndsWith("\""))
                return s.Substring(2, s.Length - 3);
            return s.Trim('"');
        }

        private static readonly string[] TimestampFormats = new string[]
        {
            "yyyy/MM/dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss.fff"
        };

        private static bool TryParseTimestamp(string raw, out DateTime result)
        {
            string clean = StripExcelQuote(raw);
            return DateTime.TryParseExact(clean, TimestampFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        public static List<ImuSample> ParseImuCsv(string filePath)
        {
            var results = new List<ImuSample>();
            var lines = File.ReadAllLines(filePath);
            bool headerSkipped = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                if (!headerSkipped)
                {
                    headerSkipped = true;
                    continue;
                }

                var parts = line.Split(',');
                if (parts.Length < 21)
                    continue;

                DateTime timestamp;
                if (!TryParseTimestamp(parts[1], out timestamp))
                    continue;

                results.Add(new ImuSample
                {
                    Timestamp = timestamp,
                    AccX = ParseDouble(parts[4]),
                    AccY = ParseDouble(parts[5]),
                    AccZ = ParseDouble(parts[6]),
                    GyroX = ParseDouble(parts[7]),
                    GyroY = ParseDouble(parts[8]),
                    GyroZ = ParseDouble(parts[9]),
                    AngleX = ParseDouble(parts[10]),
                    AngleY = ParseDouble(parts[11]),
                    AngleZ = ParseDouble(parts[12]),
                    MagX = ParseDouble(parts[13]),
                    MagY = ParseDouble(parts[14]),
                    MagZ = ParseDouble(parts[15]),
                    Q0 = ParseDouble(parts[17]),
                    Q1 = ParseDouble(parts[18]),
                    Q2 = ParseDouble(parts[19]),
                    Q3 = ParseDouble(parts[20])
                });
            }

            return results;
        }

        public static List<CameraFrame> ParseCameraCsv(string filePath)
        {
            var results = new List<CameraFrame>();
            var lines = File.ReadAllLines(filePath);
            bool headerSkipped = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                if (!headerSkipped)
                {
                    headerSkipped = true;
                    continue;
                }

                var parts = line.Split(',');
                if (parts.Length < 7)
                    continue;

                if (parts[6].Trim() != "OK")
                    continue;

                DateTime timestamp;
                if (!TryParseTimestamp(parts[1], out timestamp))
                    continue;

                results.Add(new CameraFrame
                {
                    Timestamp = timestamp,
                    ElapsedMs = ParseDouble(parts[2]),
                    FileName = parts[3].Trim(),
                    FilePath = parts[4].Trim(),
                    FileSize = ParseLong(parts[5]),
                    Success = true
                });
            }

            return results;
        }
    }
}
