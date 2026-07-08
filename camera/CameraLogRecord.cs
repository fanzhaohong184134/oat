using System;

namespace Wit.Example_BWT901BLE.Camera
{
    /// <summary>
    /// 相机拍照日志记录
    /// </summary>
    public class CameraLogRecord
    {
        /// <summary>
        /// 拍照序号
        /// </summary>
        public int CaptureNo { get; set; }

        /// <summary>
        /// 拍照绝对时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 距首次拍照的相对时间(ms)
        /// </summary>
        public double ElapsedMs { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 完整保存路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 获取CSV表头
        /// </summary>
        public static string GetCsvHeader()
        {
            return "CaptureNo,Timestamp,ElapsedMs,FileName,FilePath,FileSize,Status";
        }

        /// <summary>
        /// 转换为CSV行
        /// </summary>
        public string ToCsvLine()
        {
            return string.Join(",",
                CaptureNo,
                Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                ElapsedMs.ToString("F3"),
                EscapeCsv(FileName ?? ""),
                EscapeCsv(FilePath ?? ""),
                FileSize,
                Success ? "OK" : "FAIL"
            );
        }

        public override string ToString()
        {
            string status = Success ? "OK" : "FAIL";
            return string.Format("[{0}] {1} | {2} | {3} ({4} bytes) [{5}]",
                CaptureNo,
                Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                FileName,
                FilePath,
                FileSize,
                status);
        }

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
