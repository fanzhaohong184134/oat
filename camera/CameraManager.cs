using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Wit.Example_BWT901BLE.Camera
{
    /// <summary>
    /// 相机管理器 - 通过demo_jpeg_app.exe控制相机拍照
    /// 命令行格式: demo_jpeg_app.exe -i ip -d interval -f prefix [-n count]
    /// </summary>
    public class CameraManager
    {
        public delegate void CaptureLogHandler(CameraLogRecord record);
        public event CaptureLogHandler OnCaptureLog;

        public delegate void PreviewImageHandler(Image image);
        public event PreviewImageHandler OnPreviewImage;

        public delegate void StatusChangedHandler(string status);
        public event StatusChangedHandler OnStatusChanged;

        private Process _captureProcess;
        private bool _isCapturing;
        private int _captureCount;
        private string _cameraIp = "192.168.0.38";
        private int _interval = 5;
        private string _baseFileName = "photo";
        private string _saveDirectory;
        private bool _stopRequested;
        private CameraSamplingLogger _csvLogger;
        private DateTime _captureStartTime;

        public bool IsCapturing => _isCapturing;
        public int CaptureCount => _captureCount;

        public string CameraIp
        {
            get => _cameraIp;
            set => _cameraIp = value;
        }

        public int Interval
        {
            get => _interval;
            set { if (value > 0) _interval = value; }
        }

        public string BaseFileName
        {
            get => _baseFileName;
            set => _baseFileName = value;
        }

        public string SaveDirectory
        {
            get => _saveDirectory;
            set => _saveDirectory = value;
        }

        public string CsvLogPath
        {
            get { return _csvLogger != null ? _csvLogger.CurrentLogPath : null; }
        }

        public CameraManager()
        {
            _saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "camera_captures");
        }

        /// <summary>
        /// 获取demo_jpeg_app.exe的路径
        /// 优先查找输出目录（由PostBuildEvent复制），其次查找camera子目录
        /// </summary>
        private string GetExePath()
        {
            // 1. 优先查找exe所在目录（由PostBuildEvent复制到输出目录）
            string appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo_jpeg_app.exe");
            if (File.Exists(appDir))
                return appDir;

            // 2. 查找camera子目录（源码编译输出）
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "camera", "dlls", "x64", "Release", "demo_jpeg_app.exe");
            if (File.Exists(localPath))
                return localPath;

            return appDir;
        }

        /// <summary>
        /// 测试相机连接 - 拍一张照片验证
        /// 命令: demo_jpeg_app.exe -i {ip} -n 1 -d 1
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                string exePath = GetExePath();
                if (!File.Exists(exePath))
                {
                    OnStatusChanged?.Invoke("demo_jpeg_app.exe 未找到: " + exePath);
                    return false;
                }

                if (!Directory.Exists(_saveDirectory))
                    Directory.CreateDirectory(_saveDirectory);

                OnStatusChanged?.Invoke("正在连接相机 " + _cameraIp + "...");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = string.Format("-i {0} -n 1 -d 1", _cameraIp),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _saveDirectory
                };

                using (Process proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(15000);

                    if (output.Contains("Saved"))
                    {
                        // 解析拍照结果用于日志
                        Match m = Regex.Match(output, @"\[1\] Saved: (\S+) \((\d+) bytes\)");
                        if (m.Success)
                        {
                            string fileName = m.Groups[1].Value;
                            int bytes = int.Parse(m.Groups[2].Value);
                            string filePath = Path.Combine(_saveDirectory, fileName);

                            OnStatusChanged?.Invoke(string.Format("连接成功! 已拍摄测试照片: {0} ({1} bytes)", fileName, bytes));
                        }
                        else
                        {
                            OnStatusChanged?.Invoke("连接成功!");
                        }
                        return true;
                    }
                    else
                    {
                        OnStatusChanged?.Invoke("连接失败: " + output.Trim());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("连接异常: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 开始连续拍照
        /// 命令: demo_jpeg_app.exe -i {ip} -d {interval} -f {prefix}
        /// 通过解析stdout实时获取拍照日志
        /// </summary>
        public void StartCapture()
        {
            if (_isCapturing)
                return;

            string exePath = GetExePath();
            if (!File.Exists(exePath))
            {
                OnStatusChanged?.Invoke("demo_jpeg_app.exe 未找到");
                return;
            }

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            _stopRequested = false;
            _isCapturing = true;
            _captureCount = 0;
            _captureStartTime = DateTime.Now;

            // 启动CSV日志
            string logDir = Path.Combine(_saveDirectory, "logs");
            _csvLogger = new CameraSamplingLogger();
            _csvLogger.StartRecording(logDir, _cameraIp);

            // 启动demo_jpeg_app.exe进程 (不限次数, 持续运行)
            // 命令: demo_jpeg_app.exe -i {ip} -d {interval} -f {prefix}
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = string.Format("-i {0} -d {1} -f {2}", _cameraIp, _interval, _baseFileName),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _saveDirectory
            };

            try
            {
                _captureProcess = Process.Start(psi);

                // 异步读取stdout，实时解析拍照日志
                _captureProcess.OutputDataReceived += CaptureProcess_OutputDataReceived;
                _captureProcess.BeginOutputReadLine();

                // 异步读取stderr
                _captureProcess.ErrorDataReceived += CaptureProcess_ErrorDataReceived;
                _captureProcess.BeginErrorReadLine();

                // 监控进程退出
                _captureProcess.EnableRaisingEvents = true;
                _captureProcess.Exited += CaptureProcess_Exited;

                OnStatusChanged?.Invoke(string.Format("开始拍照: IP={0}, 间隔={1}s, 前缀={2}",
                    _cameraIp, _interval, _baseFileName));
            }
            catch (Exception ex)
            {
                _isCapturing = false;
                OnStatusChanged?.Invoke("启动拍照失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 停止拍照
        /// </summary>
        public void StopCapture()
        {
            if (!_isCapturing)
                return;

            _stopRequested = true;
            _isCapturing = false;

            if (_captureProcess != null)
            {
                try
                {
                    if (!_captureProcess.HasExited)
                    {
                        _captureProcess.Kill();
                    }
                    _captureProcess.Dispose();
                }
                catch { }
                _captureProcess = null;
            }

            string csvPath = null;
            if (_csvLogger != null && _csvLogger.IsRecording)
            {
                csvPath = _csvLogger.StopRecording();
            }

            OnStatusChanged?.Invoke(string.Format("拍照停止，共拍摄 {0} 张。日志: {1}",
                _captureCount, csvPath ?? "无"));
        }

        /// <summary>
        /// 解析stdout输出 - 匹配 [count] Saved: filename (bytes)
        /// </summary>
        private void CaptureProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data) || _stopRequested)
                return;

            string line = e.Data;

            // 匹配: [1] Saved: photo_1234567890.jpeg (52480 bytes)
            Match m = Regex.Match(line, @"\[(\d+)\] Saved: (\S+) \((\d+) bytes\)");
            if (m.Success)
            {
                int captureNo = int.Parse(m.Groups[1].Value);
                string fileName = m.Groups[2].Value;
                int fileSize = int.Parse(m.Groups[3].Value);
                string filePath = Path.Combine(_saveDirectory, fileName);
                DateTime now = DateTime.Now;

                _captureCount = captureNo;

                CameraLogRecord record = new CameraLogRecord
                {
                    CaptureNo = captureNo,
                    Timestamp = now,
                    ElapsedMs = (now - _captureStartTime).TotalMilliseconds,
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = fileSize,
                    Success = true
                };

                // 写入CSV
                if (_csvLogger != null && _csvLogger.IsRecording)
                {
                    _csvLogger.WriteRecord(record);
                }

                // 加载预览图
                try
                {
                    if (File.Exists(filePath))
                    {
                        byte[] imgBytes = File.ReadAllBytes(filePath);
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            Image img = Image.FromStream(ms);
                            OnPreviewImage?.Invoke(img);
                        }
                    }
                }
                catch { }

                OnCaptureLog?.Invoke(record);
            }
            // 匹配失败信息
            else if (line.Contains("Failed to get jpeg"))
            {
                DateTime now = DateTime.Now;
                _captureCount++;

                CameraLogRecord record = new CameraLogRecord
                {
                    CaptureNo = _captureCount,
                    Timestamp = now,
                    ElapsedMs = (now - _captureStartTime).TotalMilliseconds,
                    FileName = "N/A",
                    FilePath = "N/A",
                    FileSize = 0,
                    Success = false
                };

                if (_csvLogger != null && _csvLogger.IsRecording)
                {
                    _csvLogger.WriteRecord(record);
                }

                OnCaptureLog?.Invoke(record);
            }
        }

        private void CaptureProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                OnStatusChanged?.Invoke("STDERR: " + e.Data);
            }
        }

        private void CaptureProcess_Exited(object sender, EventArgs e)
        {
            if (_isCapturing && !_stopRequested)
            {
                _isCapturing = false;
                OnStatusChanged?.Invoke("拍照进程意外退出");

                string csvPath = null;
                if (_csvLogger != null && _csvLogger.IsRecording)
                {
                    csvPath = _csvLogger.StopRecording();
                }
            }
        }

        public void Dispose()
        {
            StopCapture();
        }
    }
}
