using System;
using System.Collections.Generic;
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
        private FileSystemWatcher _fileWatcher;
        private readonly HashSet<string> _processedFiles = new HashSet<string>();
        private readonly object _processedFilesLock = new object();

        // 预览相关成员
        private Process _previewProcess;
        private Process _ffmpegProcess;
        private string _previewOutputDir;
        private FileSystemWatcher _previewFileWatcher;
        private bool _isPreviewRunning;
        private int _previewFrameCount;
        private string _lastPreviewFramePath;

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
                    // 异步读取stdout，设置10秒超时
                    var readTask = proc.StandardOutput.ReadToEndAsync();
                    int timeoutMs = 10000;

                    if (!readTask.Wait(timeoutMs))
                    {
                        try { proc.Kill(); } catch { }
                        OnStatusChanged?.Invoke("连接超时，请检查IP地址和网络");
                        return false;
                    }

                    string output = readTask.Result;

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
            lock (_processedFilesLock) { _processedFiles.Clear(); }

            // 启动CSV日志
            string logDir = Path.Combine(_saveDirectory, "logs");
            _csvLogger = new CameraSamplingLogger();
            _csvLogger.StartRecording(logDir, _cameraIp);

            // 启动FileSystemWatcher监控新图片（主检测方式，不依赖stdout缓冲）
            StartFileWatcher();

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

                // 异步读取stdout（作为备用，当exe修复后也能工作）
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
                StopFileWatcher();
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

            StopFileWatcher();

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
                string fileName = m.Groups[2].Value;

                // 去重：FileSystemWatcher可能已记录过此文件
                lock (_processedFilesLock)
                {
                    if (_processedFiles.Contains(fileName))
                        return;
                    _processedFiles.Add(fileName);
                }

                int captureNo = int.Parse(m.Groups[1].Value);
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

        /// <summary>
        /// 启动FileSystemWatcher监控保存目录中的新jpeg文件
        /// </summary>
        private void StartFileWatcher()
        {
            try
            {
                _fileWatcher = new FileSystemWatcher(_saveDirectory, "*.jpeg");
                _fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
                _fileWatcher.Created += FileWatcher_Created;
                _fileWatcher.EnableRaisingEvents = true;
            }
            catch { }
        }

        /// <summary>
        /// 停止FileSystemWatcher
        /// </summary>
        private void StopFileWatcher()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Created -= FileWatcher_Created;
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }
        }

        /// <summary>
        /// FileSystemWatcher检测到新图片文件时触发
        /// </summary>
        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (_stopRequested || !_isCapturing)
                return;

            try
            {
                // 等待文件写入完成
                System.Threading.Thread.Sleep(200);

                string fileName = Path.GetFileName(e.FullPath);

                // 去重：OutputDataReceived可能已记录过此文件
                lock (_processedFilesLock)
                {
                    if (_processedFiles.Contains(fileName))
                        return;
                    _processedFiles.Add(fileName);
                }

                long fileSize = 0;
                if (File.Exists(e.FullPath))
                {
                    FileInfo fi = new FileInfo(e.FullPath);
                    fileSize = fi.Length;
                }

                DateTime now = DateTime.Now;
                _captureCount++;

                CameraLogRecord record = new CameraLogRecord
                {
                    CaptureNo = _captureCount,
                    Timestamp = now,
                    ElapsedMs = (now - _captureStartTime).TotalMilliseconds,
                    FileName = fileName,
                    FilePath = e.FullPath,
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
                    if (File.Exists(e.FullPath))
                    {
                        byte[] imgBytes = File.ReadAllBytes(e.FullPath);
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
            catch { }
        }

        public void Dispose()
        {
            StopCapture();
        }

        #region 实时预览功能

        /// <summary>
        /// 是否正在预览
        /// </summary>
        public bool IsPreviewRunning => _isPreviewRunning;

        /// <summary>
        /// 获取预览进程输出目录
        /// </summary>
        public string PreviewOutputDir => _previewOutputDir;

        /// <summary>
        /// 获取预览exe路径
        /// </summary>
        private string GetPreviewExePath()
        {
            // 1. 优先查找输出目录
            string appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo_h264_app.exe");
            if (File.Exists(appDir))
                return appDir;

            // 2. 查找camera子目录
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "camera", "dlls", "x64", "Release", "demo_h264_app.exe");
            if (File.Exists(localPath))
                return localPath;

            return appDir;
        }

        private string GetFfmpegExePath()
        {
            string[] candidates = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg-8.1.2-essentials_build", "ffmpeg-8.1.2-essentials_build", "bin", "ffmpeg.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "ffmpeg-8.1.2-essentials_build", "ffmpeg-8.1.2-essentials_build", "bin", "ffmpeg.exe")
            };

            foreach (string path in candidates)
            {
                string fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                    return fullPath;
            }

            return candidates[0];
        }

        /// <summary>
        /// 启动实时预览
        /// </summary>
        public void StartPreview(int fps = 15)
        {
            if (_isPreviewRunning)
                return;

            string exePath = GetPreviewExePath();
            if (!File.Exists(exePath))
            {
                OnStatusChanged?.Invoke("demo_h264_app.exe 未找到: " + exePath);
                return;
            }

            string ffmpegExe = GetFfmpegExePath();
            if (!File.Exists(ffmpegExe))
            {
                OnStatusChanged?.Invoke("ffmpeg.exe 未找到: " + ffmpegExe);
                return;
            }

            _previewOutputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preview_stream");
            if (!Directory.Exists(_previewOutputDir))
                Directory.CreateDirectory(_previewOutputDir);

            _isPreviewRunning = true;
            _previewFrameCount = 0;
            _lastPreviewFramePath = null;

            try
            {
                ProcessStartInfo previewPsi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = string.Format("-i {0}", _cameraIp),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _previewOutputDir
                };

                _previewProcess = Process.Start(previewPsi);
                if (_previewProcess == null)
                {
                    _isPreviewRunning = false;
                    OnStatusChanged?.Invoke("启动预览失败: demo_h264_app.exe 启动失败");
                    return;
                }

                _previewProcess.ErrorDataReceived += PreviewProcess_ErrorDataReceived;
                _previewProcess.BeginErrorReadLine();

                Thread ffmpegStarter = new Thread(() => StartFfmpegPreview(ffmpegExe, _previewProcess.StandardOutput.BaseStream, fps));
                ffmpegStarter.IsBackground = true;
                ffmpegStarter.Start();

                OnStatusChanged?.Invoke(string.Format("预览已启动: IP={0}, FPS={1}", _cameraIp, fps));
            }
            catch (Exception ex)
            {
                _isPreviewRunning = false;
                OnStatusChanged?.Invoke("启动预览失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 停止实时预览
        /// </summary>
        public void StopPreview()
        {
            if (!_isPreviewRunning)
                return;

            _isPreviewRunning = false;

            // 停止文件监控
            StopPreviewFileWatcher();

            if (_ffmpegProcess != null)
            {
                try
                {
                    if (!_ffmpegProcess.HasExited)
                    {
                        _ffmpegProcess.Kill();
                    }
                    _ffmpegProcess.Dispose();
                }
                catch { }
                _ffmpegProcess = null;
            }

            // 停止进程
            if (_previewProcess != null)
            {
                try
                {
                    if (!_previewProcess.HasExited)
                    {
                        _previewProcess.Kill();
                    }
                    _previewProcess.Dispose();
                }
                catch { }
                _previewProcess = null;
            }

            OnStatusChanged?.Invoke(string.Format("预览已停止，共 {0} 帧", _previewFrameCount));
        }

        /// <summary>
        /// 启动预览帧文件监控
        /// </summary>
        private void StartPreviewFileWatcher()
        {
        }

        /// <summary>
        /// 停止预览帧文件监控
        /// </summary>
        private void StopPreviewFileWatcher()
        {
        }

        private void StartFfmpegPreview(string ffmpegExe, Stream h264Stream, int fps)
        {
            try
            {
                if (!_isPreviewRunning)
                    return;

                ProcessStartInfo ffmpegPsi = new ProcessStartInfo
                {
                    FileName = ffmpegExe,
                    Arguments = string.Format("-fflags nobuffer -flags low_delay -f h264 -r {0} -i pipe:0 -vf fps={0},scale=960:-1 -f image2pipe -vcodec mjpeg -q:v 5 pipe:1", fps),
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _previewOutputDir
                };

                _ffmpegProcess = Process.Start(ffmpegPsi);
                if (_ffmpegProcess == null)
                {
                    OnStatusChanged?.Invoke("[预览] FFmpeg 启动失败");
                    return;
                }

                _ffmpegProcess.ErrorDataReceived += PreviewProcess_ErrorDataReceived;
                _ffmpegProcess.BeginErrorReadLine();

                Thread pipeThread = new Thread(() => PipeH264ToFfmpeg(h264Stream, _ffmpegProcess.StandardInput.BaseStream));
                pipeThread.IsBackground = true;
                pipeThread.Start();

                ReadMjpegFrames(_ffmpegProcess.StandardOutput.BaseStream);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("[预览] FFmpeg异常: " + ex.Message);
            }
        }

        private void PipeH264ToFfmpeg(Stream input, Stream output)
        {
            byte[] buffer = new byte[64 * 1024];
            try
            {
                while (_isPreviewRunning)
                {
                    int read = input.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;

                    output.Write(buffer, 0, read);
                    output.Flush();
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("[预览] H264 管道失败: " + ex.Message);
            }
            finally
            {
                try { output.Close(); } catch { }
            }
        }

        private void ReadMjpegFrames(Stream stream)
        {
            MemoryStream frameBuffer = new MemoryStream();
            int prev = -1;
            bool started = false;

            try
            {
                while (_isPreviewRunning)
                {
                    int b = stream.ReadByte();
                    if (b < 0)
                        break;

                    if (!started)
                    {
                        if (prev == 0xFF && b == 0xD8)
                        {
                            frameBuffer.SetLength(0);
                            frameBuffer.WriteByte(0xFF);
                            frameBuffer.WriteByte(0xD8);
                            started = true;
                        }
                    }
                    else
                    {
                        frameBuffer.WriteByte((byte)b);
                        if (prev == 0xFF && b == 0xD9)
                        {
                            byte[] jpeg = frameBuffer.ToArray();
                            using (MemoryStream ms = new MemoryStream(jpeg))
                            using (Image img = Image.FromStream(ms))
                            {
                                _previewFrameCount++;
                                OnPreviewImage?.Invoke(new Bitmap(img));
                            }
                            started = false;
                            frameBuffer.SetLength(0);
                        }
                    }

                    prev = b;
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke("[预览] 读取视频帧失败: " + ex.Message);
            }
            finally
            {
                frameBuffer.Dispose();
            }
        }

        /// <summary>
        /// 处理预览进程输出
        /// </summary>
        private void PreviewProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            string line = e.Data;

            if (line.StartsWith("STATUS "))
            {
                string status = line.Substring(7);
                OnStatusChanged?.Invoke("[预览] " + status);
            }
        }

        /// <summary>
        /// 处理预览进程错误输出
        /// </summary>
        private void PreviewProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            string line = e.Data.Trim();
            if (line.Length == 0)
                return;

            if (line.StartsWith("STATUS "))
            {
                OnStatusChanged?.Invoke("[预览] " + line.Substring(7));
                return;
            }

            if (IsPreviewInfoLog(line))
            {
                OnStatusChanged?.Invoke("[预览日志] " + line);
                return;
            }

            OnStatusChanged?.Invoke("[预览错误] " + line);
        }

        private bool IsPreviewInfoLog(string line)
        {
            if (Regex.IsMatch(line, @"^\[\d{4}/\d{2}/\d{2} .*\] [NWU]:"))
                return true;

            if (line.StartsWith("ffmpeg version ")
                || line.StartsWith("built with ")
                || line.StartsWith("configuration: ")
                || line.StartsWith("libavutil")
                || line.StartsWith("libavcodec")
                || line.StartsWith("libavformat")
                || line.StartsWith("libavdevice")
                || line.StartsWith("libavfilter")
                || line.StartsWith("libswscale")
                || line.StartsWith("libswresample")
                || line.StartsWith("Input #")
                || line.StartsWith("Output #")
                || line.StartsWith("Stream mapping:")
                || line.StartsWith("Duration:")
                || line.StartsWith("Metadata:")
                || line.StartsWith("Side data:")
                || line.StartsWith("frame=")
                || line.StartsWith("encoder         :")
                || line.StartsWith("CPB properties:"))
                return true;

            if (Regex.IsMatch(line, @"^Stream #\d+:\d+"))
                return true;

            if (Regex.IsMatch(line, @"^\[[^\]]+\] "))
                return true;

            return false;
        }

        #endregion
    }
}
