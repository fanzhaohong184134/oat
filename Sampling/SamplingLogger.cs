using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace dsat.Sampling
{
    /// <summary>
    /// 采样日志管理器
    /// Sampling log manager
    /// 负责管理采样编号、时间计算、CSV文件写入
    /// </summary>
    public class SamplingLogger
    {
        /// <summary>
        /// CSV写入器
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        /// 采样编号计数器
        /// </summary>
        private int _sampleNo = 0;

        /// <summary>
        /// 采样开始时间
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// 高精度计时器（用于计算相对时间）
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// 是否正在记录
        /// </summary>
        private bool _isRecording = false;

        /// <summary>
        /// 线程锁
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// 当前日志文件路径
        /// </summary>
        public string CurrentLogPath { get; private set; }

        /// <summary>
        /// 是否正在记录
        /// </summary>
        public bool IsRecording { get { return _isRecording; } }

        /// <summary>
        /// 已记录的采样条数
        /// </summary>
        public int SampleCount { get { return _sampleNo; } }

        /// <summary>
        /// 开始记录
        /// Start recording
        /// </summary>
        /// <param name="logDirectory">日志保存目录</param>
        /// <param name="deviceInfo">设备信息（用于文件名）</param>
        public void StartRecording(string logDirectory, string deviceInfo)
        {
            lock (_lock)
            {
                if (_isRecording)
                {
                    return;
                }

                // 确保日志目录存在
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 生成文件名
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string safeDeviceName = GetSafeFileName(deviceInfo);
                string fileName = string.Format("BWT901BLE_Log_{0}_{1}.csv", safeDeviceName, timestamp);
                CurrentLogPath = Path.Combine(logDirectory, fileName);

                // 创建文件写入器（UTF-8 with BOM，兼容Excel）
                _writer = new StreamWriter(CurrentLogPath, false, Encoding.UTF8);
                _writer.AutoFlush = false;

                // 写入文件头注释
                _writer.WriteLine("# BWT901BLE 采样日志");
                _writer.WriteLine("# 开始时间: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                _writer.WriteLine("# 设备信息: {0}", deviceInfo);
                _writer.WriteLine("# ============================================");

                // 写入CSV表头
                _writer.WriteLine(SamplingRecord.GetCsvHeader());

                // 初始化计数器和计时器
                _sampleNo = 0;
                _startTime = DateTime.Now;
                _stopwatch = Stopwatch.StartNew();
                _isRecording = true;
            }
        }

        /// <summary>
        /// 写入一条采样记录
        /// Write one sampling record
        /// </summary>
        /// <param name="record">采样记录</param>
        public void WriteRecord(SamplingRecord record)
        {
            if (!_isRecording)
            {
                return;
            }

            lock (_lock)
            {
                if (!_isRecording || _writer == null)
                {
                    return;
                }

                // 填充编号和时间信息
                record.SampleNo = _sampleNo;
                record.Timestamp = DateTime.Now;
                record.ElapsedMs = _stopwatch.Elapsed.TotalMilliseconds;

                // 写入CSV行
                _writer.WriteLine(record.ToCsvLine());
                _sampleNo++;

                // 每100条刷新一次磁盘
                if (_sampleNo % 100 == 0)
                {
                    _writer.Flush();
                }
            }
        }

        /// <summary>
        /// 停止记录
        /// Stop recording
        /// </summary>
        /// <returns>日志文件路径</returns>
        public string StopRecording()
        {
            lock (_lock)
            {
                if (!_isRecording)
                {
                    return null;
                }

                _isRecording = false;
                _stopwatch.Stop();

                // 写入结束注释
                if (_writer != null)
                {
                    _writer.WriteLine("# ============================================");
                    _writer.WriteLine("# 结束时间: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    _writer.WriteLine("# 总采样数: {0}", _sampleNo);
                    _writer.Flush();
                    _writer.Close();
                    _writer.Dispose();
                    _writer = null;
                }

                string path = CurrentLogPath;
                return path;
            }
        }

        /// <summary>
        /// 获取安全的文件名（移除非法字符）
        /// </summary>
        private static string GetSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unknown";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            // 截断过长的名称
            if (name.Length > 30)
            {
                name = name.Substring(0, 30);
            }
            return name;
        }
    }
}
