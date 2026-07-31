using System;

namespace dsat.Sampling
{
    /// <summary>
    /// 采样记录数据结构
    /// Sampling record data structure
    /// </summary>
    public class SamplingRecord
    {
        /// <summary>
        /// 采样编号，从0开始递增
        /// Sample number, starting from 0
        /// </summary>
        public int SampleNo { get; set; }

        /// <summary>
        /// 绝对时间戳
        /// Absolute timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 距首次采样的相对时间(ms)
        /// Elapsed time since first sample (ms)
        /// </summary>
        public double ElapsedMs { get; set; }

        /// <summary>
        /// 设备蓝牙MAC地址
        /// Device Bluetooth MAC address
        /// </summary>
        public string DeviceMAC { get; set; }

        /// <summary>
        /// 设备名称
        /// Device name
        /// </summary>
        public string DeviceName { get; set; }

        // 加速度 X/Y/Z (g)
        public double? AccX { get; set; }
        public double? AccY { get; set; }
        public double? AccZ { get; set; }

        // 角速度 X/Y/Z (°/s)
        public double? GyroX { get; set; }
        public double? GyroY { get; set; }
        public double? GyroZ { get; set; }

        // 角度 X/Y/Z (°)
        public double? AngleX { get; set; }
        public double? AngleY { get; set; }
        public double? AngleZ { get; set; }

        // 磁场 X/Y/Z (uT)
        public double? MagX { get; set; }
        public double? MagY { get; set; }
        public double? MagZ { get; set; }

        // 磁场矢量和 (uT)
        public double? MagM { get; set; }

        // 四元数 Q0-Q3
        public double? Q0 { get; set; }
        public double? Q1 { get; set; }
        public double? Q2 { get; set; }
        public double? Q3 { get; set; }

        // 温度 (°C)
        public double? Temperature { get; set; }

        // 电量百分比 (%)
        public double? PowerPercent { get; set; }

        // 芯片时间
        public string ChipTime { get; set; }

        // 固件版本号
        public string VersionNumber { get; set; }

        // 设备序列号
        public string SerialNumber { get; set; }

        /// <summary>
        /// 获取CSV表头
        /// Get CSV header
        /// </summary>
        public static string GetCsvHeader()
        {
            return "SampleNo,Timestamp,ElapsedMs,ChipTime," +
                   "AccX,AccY,AccZ,GyroX,GyroY,GyroZ," +
                   "AngleX,AngleY,AngleZ," +
                   "MagX,MagY,MagZ,MagM," +
                   "Q0,Q1,Q2,Q3," +
                   "Temperature,PowerPercent," +
                   "VersionNumber,SerialNumber";
        }

        /// <summary>
        /// 转换为CSV行
        /// Convert to CSV line
        /// </summary>
        public string ToCsvLine()
        {
            return string.Join(",",
                SampleNo,
                "=\"" + Timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff") + "\"",
                ElapsedMs.ToString("F3"),
                EscapeCsv(ChipTime ?? ""),
                FormatNullable(AccX),
                FormatNullable(AccY),
                FormatNullable(AccZ),
                FormatNullable(GyroX),
                FormatNullable(GyroY),
                FormatNullable(GyroZ),
                FormatNullable(AngleX),
                FormatNullable(AngleY),
                FormatNullable(AngleZ),
                FormatNullable(MagX),
                FormatNullable(MagY),
                FormatNullable(MagZ),
                FormatNullable(MagM),
                FormatNullable(Q0),
                FormatNullable(Q1),
                FormatNullable(Q2),
                FormatNullable(Q3),
                FormatNullable(Temperature),
                FormatNullable(PowerPercent),
                EscapeCsv(VersionNumber ?? ""),
                EscapeCsv(SerialNumber ?? "")
            );
        }

        /// <summary>
        /// 格式化可空数值
        /// Format nullable value
        /// </summary>
        private static string FormatNullable(double? value)
        {
            return value.HasValue ? value.Value.ToString("F6") : "";
        }

        /// <summary>
        /// CSV字段转义（包含逗号、引号、换行时需要引号包裹）
        /// CSV field escaping
        /// </summary>
        private static string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }
    }
}
