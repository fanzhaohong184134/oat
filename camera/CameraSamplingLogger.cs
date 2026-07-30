using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Wit.Example_BWT901BLE.Camera
{
    /// <summary>
    /// 相机采样日志管理器
    /// 负责将每次拍照记录写入CSV文件，格式与IMU采样日志一致
    /// </summary>
    public class CameraSamplingLogger
    {
        private StreamWriter _writer;
        private int _sampleNo = 0;
        private DateTime _startTime;
        private Stopwatch _stopwatch;
        private bool _isRecording = false;
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
        /// 已记录的拍照条数
        /// </summary>
        public int SampleCount { get { return _sampleNo; } }

        /// <summary>
        /// 开始记录
        /// </summary>
        /// <param name="logDirectory">日志保存目录</param>
        /// <param name="cameraInfo">相机信息（用于文件名）</param>
        public void StartRecording(string logDirectory, string cameraInfo)
        {
            lock (_lock)
            {
                if (_isRecording)
                {
                    return;
                }

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string safeName = GetSafeFileName(cameraInfo);
                string fileName = string.Format("Camera_Log_{0}_{1}.csv", safeName, timestamp);
                CurrentLogPath = Path.Combine(logDirectory, fileName);

                _writer = new StreamWriter(CurrentLogPath, false, Encoding.UTF8);
                _writer.AutoFlush = false;

                _writer.WriteLine("# 相机拍照采样日志");
                _writer.WriteLine("# 开始时间: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                _writer.WriteLine("# 相机信息: {0}", cameraInfo);
                _writer.WriteLine("# ============================================");

                _writer.WriteLine(CameraLogRecord.GetCsvHeader());

                _sampleNo = 0;
                _startTime = DateTime.Now;
                _stopwatch = Stopwatch.StartNew();
                _isRecording = true;
            }
        }

        /// <summary>
        /// 写入一条拍照记录
        /// </summary>
        public void WriteRecord(CameraLogRecord record)
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

                record.CaptureNo = _sampleNo;
                record.ElapsedMs = _stopwatch.Elapsed.TotalMilliseconds;

                _writer.WriteLine(record.ToCsvLine());
                _sampleNo++;

                if (_sampleNo % 10 == 0)
                {
                    _writer.Flush();
                }
            }
        }

        /// <summary>
        /// 停止记录
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

                if (_writer != null)
                {
                    _writer.WriteLine("# ============================================");
                    _writer.WriteLine("# 结束时间: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    _writer.WriteLine("# 总拍照数: {0}", _sampleNo);
                    _writer.Flush();
                    _writer.Close();
                    _writer.Dispose();
                    _writer = null;
                }

                return CurrentLogPath;
            }
        }

        private static string GetSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Camera";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            if (name.Length > 30)
            {
                name = name.Substring(0, 30);
            }
            return name;
        }
    }
}
