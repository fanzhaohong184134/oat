using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wit.SDK.Modular.Sensor.Modular.DataProcessor.Constant;
using Wit.SDK.Modular.WitSensorApi.Modular.BWT901BLE;
using Wit.SDK.Device.Device.Device.DKey;
using Wit.Bluetooth.WinBlue.Utils;
using Wit.Bluetooth.WinBlue.Interface;
using Wit.Example_BWT901BLE.Sampling;
using Wit.Example_BWT901BLE.Camera;

namespace Wit.Example_BWT901BLE
{   
    /// <summary>
    /// 程序主窗口
    /// 说明：
    /// 1.本程序是维特智能开发的BWT901BLE九轴传感器示例程序
    /// 2.适用示例程序前请咨询技术支持,询问本示例程序是否支持您的传感器
    /// 3.使用前请了解传感器的通信协议
    /// 4.本程序只有一个窗口,所有逻辑都在这里
    /// 
    /// Program Main Window
    /// Explanation:
    /// 1. This program is an example program for the BWT901BLE nine axis sensor developed by Weite Intelligence
    /// 2. Before applying the sample program, please consult technical support and ask if this sample program supports your sensor
    /// 3. Please understand the communication protocol of the sensor before use
    /// 4. This program only has one window, all logic is here
    /// </summary>
    public partial class Form1 : Form
    {

        /// <summary>
        /// 蓝牙管理器
        /// Bluetooth manager
        /// </summary>
        private IWinBlueManager WitBluetoothManager = WinBlueFactory.GetInstance();

        /// <summary>
        /// 找到的设备
        /// Found device
        /// </summary>
        private Dictionary<string, Bwt901ble> FoundDeviceDict = new Dictionary<string, Bwt901ble>();

        /// <summary>
        /// 控制自动刷新数据线程是否工作
        /// Control whether the automatic refresh data thread works
        /// </summary>
        public bool EnableRefreshDataTh { get; private set; }

        /// <summary>
        /// 采样日志管理器
        /// Sampling log manager
        /// </summary>
        private SamplingLogger _samplingLogger;

        /// <summary>
        /// 是否正在记录采样日志
        /// </summary>
        private bool _isLogging = false;

        /// <summary>
        /// 记录每个设备的MAC地址（用于日志记录）
        /// </summary>
        private Dictionary<string, string> _deviceMacMap = new Dictionary<string, string>();

        /// <summary>
        /// 相机管理器
        /// </summary>
        private CameraManager _cameraManager;

        /// <summary>
        /// 浮动预览窗口
        /// </summary>
        private FloatingPreviewForm _previewForm;

        /// <summary>
        /// 是否正在IMU采样
        /// </summary>
        private bool _isImuSampling = false;

        /// <summary>
        /// 是否正在相机采样
        /// </summary>
        private bool _isCameraSampling = false;

        /// <summary>
        /// 是否正在磁场校准
        /// </summary>
        private bool _isMagCalibrating = false;

        /// <summary>
        /// 蓝牙名称过滤关键字（从输入框读取）
        /// </summary>
        private string _bluetoothFilterName = "WT901BLE68";

        /// <summary>
        /// 传感器是否已连接
        /// </summary>
        private bool _isSensorConnected = false;

        /// <summary>
        /// OnRecord回调计数（用于诊断数据推送是否正常）
        /// </summary>
        private long _recordCount = 0;

        /// <summary>
        /// 各GroupBox内控件的初始Y比例（用于垂直等比缩放）
        /// </summary>
        private Dictionary<Control, float> _controlYRatios = new Dictionary<Control, float>();
        private Dictionary<GroupBox, int> _origGroupBoxHeights = new Dictionary<GroupBox, int>();

        /// <summary>
        /// 构造
        /// Structure
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗体加载时
        /// When the form is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // 版本号
            string ver = string.IsNullOrEmpty(GitVersion.CommitHash) ? "V1.0" : "V1.0-" + GitVersion.CommitHash;
            versionLabel.Text = ver;

            // 初始化相机管理器
            _cameraManager = new CameraManager();
            _cameraManager.OnCaptureLog += _cameraManager_OnCaptureLog;
            _cameraManager.OnPreviewImage += _cameraManager_OnPreviewImage;
            _cameraManager.OnStatusChanged += _cameraManager_OnStatusChanged;

            // 初始化浮动预览窗口
            _previewForm = new FloatingPreviewForm();

            // 设置默认保存目录
            saveDirectoryTextBox.Text = _cameraManager.SaveDirectory;
            cameraIpTextBox.Text = _cameraManager.CameraIp;
            captureIntervalTextBox.Text = _cameraManager.Interval.ToString();

            // GroupBox居中文字、无边框
            groupBoxConnection.Paint += GroupBoxHeader_Paint;
            groupBoxSampling.Paint += GroupBoxHeader_Paint;
            groupBoxSettings.Paint += GroupBoxHeader_Paint;
            groupBoxCalibration.Paint += GroupBoxHeader_Paint;
            baseFileNameTextBox.Text = _cameraManager.BaseFileName;

            // 记录各GroupBox内控件的初始Y位置比例，用于垂直等比缩放
            InitLayoutRatios(groupBoxConnection);
            InitLayoutRatios(groupBoxSampling);
            InitLayoutRatios(groupBoxSettings);
            InitLayoutRatios(groupBoxCalibration);
            groupBoxConnection.Resize += GroupBox_Resize;
            groupBoxSampling.Resize += GroupBox_Resize;
            groupBoxSettings.Resize += GroupBox_Resize;
            groupBoxCalibration.Resize += GroupBox_Resize;

            // 开启数据刷新线程
            // Enable data refresh thread
            Thread thread = new Thread(RefreshDataTh);
            thread.IsBackground = true;
            EnableRefreshDataTh = true;
            thread.Start();
        }

        private void InitLayoutRatios(GroupBox gb)
        {
            _origGroupBoxHeights[gb] = gb.Height;
            foreach (Control c in gb.Controls)
            {
                _controlYRatios[c] = c.Location.Y / (float)gb.Height;
            }
        }

        private void GroupBox_Resize(object sender, EventArgs e)
        {
            GroupBox gb = (GroupBox)sender;
            if (!_origGroupBoxHeights.ContainsKey(gb)) return;
            int origH = _origGroupBoxHeights[gb];
            if (origH <= 0) return;
            gb.SuspendLayout();
            foreach (Control c in gb.Controls)
            {
                if (!_controlYRatios.ContainsKey(c)) continue;
                float yRatio = _controlYRatios[c];
                int newY = (int)(yRatio * gb.Height);
                c.Location = new Point(c.Location.X, newY);
            }
            gb.ResumeLayout();
        }

        /// <summary>
        /// 窗体关闭时
        /// When the form is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭刷新数据线程
            // Close refresh data thread
            EnableRefreshDataTh = false;

            // 停止IMU采样
            if (_isLogging && _samplingLogger != null)
            {
                _samplingLogger.StopRecording();
                _isLogging = false;
            }

            // 停止相机采样
            if (_cameraManager != null)
            {
                _cameraManager.StopCapture();
                _cameraManager.Dispose();
            }

            // 关闭预览窗口
            if (_previewForm != null)
            {
                _previewForm.Dispose();
            }

            // 关闭蓝牙连接
            // Close Bluetooth connection
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                keyValue.Value.Close();
            }
            WitBluetoothManager.OnDeviceFound -= this.WitBluetoothManager_OnDeviceFound;
            WitBluetoothManager.StopScan();
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 连接/断开传感器（切换模式）
        /// Connect/Disconnect sensor (toggle mode)
        /// 流程：开启BLE广播监听 → 收到广播包 → 解析MAC+设备名 → 过滤名称 → 自动Open建立GATT连接 → 推送数据
        /// </summary>
        private void sensorConnectButton_Click(object sender, EventArgs e)
        {
            if (!_isSensorConnected)
            {
                // ── 连接流程 ──
                string filterName = sensorNameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(filterName))
                {
                    MessageBox.Show("请输入蓝牙名称关键字", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _bluetoothFilterName = filterName;

                // 清除找到的设备
                FoundDeviceDict.Clear();
                _deviceMacMap.Clear();
                System.Threading.Interlocked.Exchange(ref _recordCount, 0);

                sensorConnectButton.Enabled = false;
                sensorConnectButton.Text = "搜索中...";
                sensorStatusLight.BackColor = Color.Yellow;

                // 先取消订阅再订阅，避免重复注册
                WitBluetoothManager.OnDeviceFound -= this.WitBluetoothManager_OnDeviceFound;
                WitBluetoothManager.OnDeviceFound += this.WitBluetoothManager_OnDeviceFound;
                WitBluetoothManager.StartScan();

                // 5秒后检查是否找到设备
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 5000;
                timer.Tick += (s, ev) =>
                {
                    if (FoundDeviceDict.Count > 0)
                    {
                        _isSensorConnected = true;
                        sensorConnectButton.Text = "断开";
                        sensorConnectButton.Enabled = true;
                        sensorStatusLight.BackColor = Color.Green;
                    }
                    else
                    {
                        sensorConnectButton.Text = "连接";
                        sensorConnectButton.Enabled = true;
                        sensorStatusLight.BackColor = Color.Gray;
                        MessageBox.Show("未搜索到设备，请检查蓝牙是否开启", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            else
            {
                // ── 断开流程 ──
                for (int i = 0; i < FoundDeviceDict.Count; i++)
                {
                    var keyValue = FoundDeviceDict.ElementAt(i);
                    Bwt901ble bWT901BLE = keyValue.Value;
                    bWT901BLE.Close();
                }
                FoundDeviceDict.Clear();
                _deviceMacMap.Clear();

                WitBluetoothManager.StopScan();

                _isSensorConnected = false;
                sensorConnectButton.Text = "连接";
                sensorStatusLight.BackColor = Color.Gray;
            }
        }

        /// <summary>
        /// 当搜索到蓝牙设备时会回调这个方法
        /// Call back this method when Bluetooth devices are found
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="deviceName"></param>
        private void WitBluetoothManager_OnDeviceFound(string mac, string deviceName)
        {
            // 名称过滤（使用输入框中的关键字）
            // Name filtering (using keyword from input box)
            if (deviceName != null && deviceName.Contains(_bluetoothFilterName))
            {
                if (!FoundDeviceDict.ContainsKey(mac))
                {
                    Bwt901ble bWT901BLE = new Bwt901ble(mac,deviceName);
                    FoundDeviceDict.Add(mac, bWT901BLE);
                    // 记录MAC地址映射
                    _deviceMacMap[bWT901BLE.GetDeviceName()] = mac;
                    // 打开这个设备
                    // Open this device
                    bWT901BLE.Open();
                    bWT901BLE.OnRecord += BWT901BLE_OnRecord;

                    // 更新传感器状态灯（线程安全）
                    try
                    {
                        sensorStatusLight.Invoke(new Action(() =>
                        {
                            sensorStatusLight.BackColor = Color.Green;
                        }));
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 当传感器数据刷新时会调用这里，您可以在这里记录数据
        /// This will be called when the sensor data is refreshed, where you can record the data
        /// </summary>
        /// <param name="BWT901BLE"></param>
        private void BWT901BLE_OnRecord(Bwt901ble BWT901BLE)
        {
            System.Threading.Interlocked.Increment(ref _recordCount);
            string text = GetDeviceData(BWT901BLE);
            Debug.WriteLine(text);

            // 如果正在记录，写入采样日志
            // If recording, write sampling log
            if (_isLogging && _samplingLogger != null && _samplingLogger.IsRecording)
            {
                SamplingRecord record = BuildSamplingRecord(BWT901BLE);
                _samplingLogger.WriteRecord(record);
            }
        }

        /// <summary>
        /// 设备状态发生时会调这个方法
        /// This method will be called when the device status occurs
        /// </summary>
        /// <param name="macAddr"></param>
        /// <param name="mType"></param>
        /// <param name="sMsg"></param>
        private void OnDeviceStatu(string macAddr, int mType, string sMsg)
        {
            if (mType == 20)
            {
                // 断开连接
                // Disconnect
                Debug.WriteLine(macAddr + "Disconnect");
            }

            if (mType == 11)
            {
                // 连接失败
                // Connect failed
                Debug.WriteLine(macAddr + "Connect failed");
            }

            if (mType == 10)
            {
                // 连接成功
                // Successfully connected
                Debug.WriteLine(macAddr + "Successfully connected");
            }
        }

        /// <summary>
        /// 构建采样记录
        /// Build sampling record from device data
        /// </summary>
        private SamplingRecord BuildSamplingRecord(Bwt901ble device)
        {
            string deviceName = device.GetDeviceName();
            string mac = _deviceMacMap.ContainsKey(deviceName) ? _deviceMacMap[deviceName] : "";

            return new SamplingRecord
            {
                DeviceMAC = mac,
                DeviceName = deviceName,
                AccX = device.GetDeviceData(WitSensorKey.AccX),
                AccY = device.GetDeviceData(WitSensorKey.AccY),
                AccZ = device.GetDeviceData(WitSensorKey.AccZ),
                GyroX = device.GetDeviceData(WitSensorKey.AsX),
                GyroY = device.GetDeviceData(WitSensorKey.AsY),
                GyroZ = device.GetDeviceData(WitSensorKey.AsZ),
                AngleX = device.GetDeviceData(WitSensorKey.AngleX),
                AngleY = device.GetDeviceData(WitSensorKey.AngleY),
                AngleZ = device.GetDeviceData(WitSensorKey.AngleZ),
                MagX = device.GetDeviceData(WitSensorKey.HX),
                MagY = device.GetDeviceData(WitSensorKey.HY),
                MagZ = device.GetDeviceData(WitSensorKey.HZ),
                MagM = device.GetDeviceData(WitSensorKey.HM),
                Q0 = device.GetDeviceData(WitSensorKey.Q0),
                Q1 = device.GetDeviceData(WitSensorKey.Q1),
                Q2 = device.GetDeviceData(WitSensorKey.Q2),
                Q3 = device.GetDeviceData(WitSensorKey.Q3),
                Temperature = device.GetDeviceData(WitSensorKey.T),
                PowerPercent = device.GetDeviceData(WitSensorKey.PowerPercent),
                ChipTime = device.GetDeviceData(WitSensorKey.ChipTime) ?? "",
                VersionNumber = device.GetDeviceData(WitSensorKey.VersionNumber) ?? "",
                SerialNumber = device.GetDeviceData(WitSensorKey.SerialNumber) ?? ""
            };
        }

        /// <summary>
        /// 获得设备的数据（带时间戳显示）
        /// Obtaining device data (with timestamp display)
        /// </summary>
        private string GetDeviceData(Bwt901ble BWT901BLE)
        {
            StringBuilder builder = new StringBuilder();
            // 显示采样时间戳
            builder.Append("采样时间: ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("\n");
            if (_isLogging && _samplingLogger != null)
            {
                builder.Append("采样编号: #").Append(_samplingLogger.SampleCount).Append("\n");
            }
            builder.Append(BWT901BLE.GetDeviceName()).Append("\n");
            // 加速度
            // Acc
            builder.Append("AccX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AccX)).Append("g \t");
            builder.Append("AccY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AccY)).Append("g \t");
            builder.Append("AccZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AccZ)).Append("g \n");
            // 角速度
            // Gyro
            builder.Append("GyroX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AsX)).Append("°/s \t");
            builder.Append("GyroY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AsY)).Append("°/s \t");
            builder.Append("GyroZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AsZ)).Append("°/s \n");
            // 角度
            // Angle
            builder.Append("AngleX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleX)).Append("° \t");
            builder.Append("AngleY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleY)).Append("° \t");
            builder.Append("AngleZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.AngleZ)).Append("° \n");
            // 磁场
            // Mag
            builder.Append("MagX").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.HX)).Append("uT \t");
            builder.Append("MagY").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.HY)).Append("uT \t");
            builder.Append("MagZ").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.HZ)).Append("uT \n");
            // 版本号
            // VersionNumber
            builder.Append("VersionNumber").Append(":").Append(BWT901BLE.GetDeviceData(WitSensorKey.VersionNumber)).Append("\n");
            return builder.ToString();
        }

        /// <summary>
        /// 加计校准
        /// Acceleration calibration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void appliedCalibrationButton_Click(object sender, EventArgs e)
        {
            // 所有连接的蓝牙设备都加计校准
            // All connected Bluetooth devices are calibrated
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    return;
                }

                try
                {
                    // 解锁寄存器并发送命令
                    // Unlock register and send command
                    bWT901BLE.UnlockReg();
                    bWT901BLE.AppliedCalibration();

                    // 下面两行与上面等价,推荐使用上面的
                    // The following two lines are equivalent to the above, and it is recommended to use the above one
                    //bWT901BLE.SendProtocolData(new byte[] { 0xff, 0xaa, 0x69, 0x88, 0xb5 });
                    //bWT901BLE.SendProtocolData(new byte[] { 0xff, 0xaa, 0x01, 0x01, 0x00 });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        /// <summary>
        /// 读取03寄存器
        /// Read 03 register
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void readReg03Button_Click(object sender, EventArgs e)
        {
            string reg03Value = "";
            // 读取所有连接的蓝牙设备的03寄存器
            // Read the 03 register of all connected Bluetooth devices
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    return;
                }
                try
                {
                    // 等待时长
                    // Waiting time
                    int waitTime = 3000;
                    // 发送读取命令，并且等待传感器返回数据，如果没读上来可以将 waitTime 延长，或者多读几次
                    // Send a read command and wait for the sensor to return data. If it is not read, the waitTime can be extended or read several more times
                    bWT901BLE.SendReadReg(0x03, waitTime);

                    // 下面这行和上面等价推荐使用上面的
                    // The following two lines are equivalent to the above, and it is recommended to use the above one
                    //bWT901BLE.SendProtocolData(new byte[] { 0xff, 0xaa, 0x27, 0x03, 0x00 }, waitTime);

                    // 拿到所有连接的蓝牙设备的值
                    // Get the values of all connected Bluetooth devices
                    reg03Value += bWT901BLE.GetDeviceName() + "的寄存器03值为 :" + bWT901BLE.GetDeviceData(new ShortKey("03")) + "\r\n";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            MessageBox.Show(reg03Value);
        }

        /// <summary>
        /// 回传速率 Hz 到寄存器值的映射
        /// Return rate Hz to register value mapping
        /// </summary>
        private static readonly Dictionary<double, byte> ReturnRateMap = new Dictionary<double, byte>
        {
            { 0.1, 0x01 }, { 0.5, 0x02 }, { 1, 0x03 }, { 2, 0x04 },
            { 5, 0x05 }, { 10, 0x06 }, { 20, 0x07 }, { 50, 0x08 }, { 100, 0x09 }
        };

        /// <summary>
        /// 带宽 Hz 到寄存器值的映射
        /// Bandwidth Hz to register value mapping
        /// </summary>
        private static readonly Dictionary<int, byte> BandWidthMap = new Dictionary<int, byte>
        {
            { 256, 0x00 }, { 188, 0x01 }, { 98, 0x02 }, { 42, 0x03 }, { 20, 0x04 }, { 10, 0x05 }
        };

        /// <summary>
        /// ChipTime校准 - 将PC当前时间同步到传感器
        /// ChipTime calibration - sync PC current time to sensor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chipTimeCalibrationButton_Click(object sender, EventArgs e)
        {
            if (FoundDeviceDict.Count == 0)
            {
                MessageBox.Show("没有已连接的设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime now = DateTime.Now;
            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;

                if (bWT901BLE.IsOpen() == false)
                {
                    continue;
                }
                try
                {
                    bWT901BLE.UnlockReg();
                    bWT901BLE.SetChipTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            MessageBox.Show("ChipTime 校准完成，已同步为: " + now.ToString("yyyy-MM-dd HH:mm:ss.fff"), "校准成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 回传速率下拉框选择变化时自动设置
        /// Auto set return rate when combobox selection changes
        /// </summary>
        private void returnRateComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string input = returnRateComboBox.Text.Trim();
            if (string.IsNullOrEmpty(input) || !double.TryParse(input, out double hzValue))
                return;

            if (hzValue < 10 || hzValue > 100)
            {
                MessageBox.Show("回传速率范围为 10 ~ 100 Hz", "超出范围", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte regValue = FindClosestReturnRate(hzValue);

            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;
                if (bWT901BLE.IsOpen() == false) return;
                try
                {
                    bWT901BLE.UnlockReg();
                    bWT901BLE.SetReturnRate(regValue);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 带宽下拉框选择变化时自动设置
        /// Auto set bandwidth when combobox selection changes
        /// </summary>
        private void bandWidthComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string input = bandWidthComboBox.Text.Trim();
            if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int hzValue))
                return;

            if (hzValue < 10 || hzValue > 512)
            {
                MessageBox.Show("带宽范围为 10 ~ 512 Hz", "超出范围", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte regValue = FindClosestBandWidth(hzValue);

            for (int i = 0; i < FoundDeviceDict.Count; i++)
            {
                var keyValue = FoundDeviceDict.ElementAt(i);
                Bwt901ble bWT901BLE = keyValue.Value;
                if (bWT901BLE.IsOpen() == false) return;
                try
                {
                    bWT901BLE.UnlockReg();
                    bWT901BLE.SetBandWidth(regValue);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 查找最接近的回传速率寄存器值
        /// </summary>
        private byte FindClosestReturnRate(double hzValue)
        {
            double closestHz = ReturnRateMap.Keys.OrderBy(k => Math.Abs(k - hzValue)).First();
            return ReturnRateMap[closestHz];
        }

        /// <summary>
        /// 查找最接近的带宽寄存器值
        /// </summary>
        private byte FindClosestBandWidth(int hzValue)
        {
            int closestHz = BandWidthMap.Keys.OrderBy(k => Math.Abs(k - hzValue)).First();
            return BandWidthMap[closestHz];
        }

        /// <summary>
        /// 磁场校准（切换按钮：点击开始，再次点击停止）
        /// Magnetic field calibration (toggle button: click to start, click again to stop)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void magCalibrationButton_Click(object sender, EventArgs e)
        {
            if (!_isMagCalibrating)
            {
                // 开始磁场校准
                for (int i = 0; i < FoundDeviceDict.Count; i++)
                {
                    var keyValue = FoundDeviceDict.ElementAt(i);
                    Bwt901ble bWT901BLE = keyValue.Value;

                    if (bWT901BLE.IsOpen() == false)
                    {
                        return;
                    }
                    try
                    {
                        bWT901BLE.UnlockReg();
                        bWT901BLE.StartFieldCalibration();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }

                _isMagCalibrating = true;
                magCalibrationButton.BackColor = Color.Green;
                magCalibrationButton.Text = "磁场校准中... (再次点击停止)";
                MessageBox.Show("开始磁场校准,请绕传感器XYZ三轴各转一圈,转完以后再次点击按钮结束校准");
            }
            else
            {
                // 结束磁场校准
                for (int i = 0; i < FoundDeviceDict.Count; i++)
                {
                    var keyValue = FoundDeviceDict.ElementAt(i);
                    Bwt901ble bWT901BLE = keyValue.Value;

                    if (bWT901BLE.IsOpen() == false)
                    {
                        return;
                    }
                    try
                    {
                        bWT901BLE.UnlockReg();
                        bWT901BLE.EndFieldCalibration();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }

                _isMagCalibrating = false;
                magCalibrationButton.BackColor = SystemColors.Control;
                magCalibrationButton.Text = "磁场校准";
            }
        }

        /// <summary>
        /// 连接相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void cameraConnectButton_Click(object sender, EventArgs e)
        {
            string ip = cameraIpTextBox.Text.Trim();
            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("请输入相机IP地址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cameraManager.CameraIp = ip;
            cameraConnectButton.Enabled = false;
            cameraConnectButton.Text = "连接中...";

            try
            {
                bool result = await Task.Run(() => _cameraManager.TestConnection());

                if (result)
                {
                    cameraStatusLight.BackColor = Color.Green;
                }
                else
                {
                    cameraStatusLight.BackColor = Color.Gray;
                    MessageBox.Show("连接相机失败，请检查IP地址和网络", "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                cameraStatusLight.BackColor = Color.Gray;
                MessageBox.Show("连接相机异常: " + ex.Message, "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cameraConnectButton.Enabled = true;
                cameraConnectButton.Text = "连接";
            }
        }

        /// <summary>
        /// IMU采样（切换按钮）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imuSamplingButton_Click(object sender, EventArgs e)
        {
            if (!_isImuSampling)
            {
                // 开始IMU采样
                try
                {
                    _samplingLogger = new SamplingLogger();
                    string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                    string deviceInfo = "BWT901BLE";
                    if (FoundDeviceDict.Count > 0)
                    {
                        var firstDevice = FoundDeviceDict.ElementAt(0).Value;
                        deviceInfo = firstDevice.GetDeviceName();
                    }

                    _samplingLogger.StartRecording(logDir, deviceInfo);
                    _isLogging = true;
                    _isImuSampling = true;

                    imuSamplingButton.BackColor = Color.Green;
                    imuSamplingButton.Text = "IMU采样中...";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("开始IMU采样失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // 停止IMU采样
                try
                {
                    if (_samplingLogger != null)
                    {
                        string savedPath = _samplingLogger.StopRecording();
                        if (savedPath != null)
                        {
                            int count = _samplingLogger.SampleCount;
                            MessageBox.Show(
                                string.Format("IMU采样记录已保存!\n\n共记录 {0} 条数据\n保存路径:\n{1}", count, savedPath),
                                "记录完成",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                    _isLogging = false;
                    _isImuSampling = false;

                    imuSamplingButton.BackColor = SystemColors.Control;
                    imuSamplingButton.Text = "IMU采样";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("停止IMU采样失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 相机采样（切换按钮）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cameraSamplingButton_Click(object sender, EventArgs e)
        {
            if (!_isCameraSampling)
            {
                // 读取UI设置
                string ip = cameraIpTextBox.Text.Trim();
                string saveDir = saveDirectoryTextBox.Text.Trim();
                string baseName = baseFileNameTextBox.Text.Trim();
                string intervalStr = captureIntervalTextBox.Text.Trim();

                if (string.IsNullOrEmpty(ip))
                {
                    MessageBox.Show("请先设置相机IP地址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(saveDir))
                {
                    MessageBox.Show("请设置保存目录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(intervalStr, out int interval) || interval <= 0)
                {
                    MessageBox.Show("请输入有效的拍照间隔（正整数，单位秒）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 配置相机管理器
                _cameraManager.CameraIp = ip;
                _cameraManager.SaveDirectory = saveDir;
                _cameraManager.BaseFileName = string.IsNullOrEmpty(baseName) ? "photo" : baseName;
                _cameraManager.Interval = interval;

                // 开始拍照
                _cameraManager.StartCapture();
                _isCameraSampling = true;

                cameraSamplingButton.BackColor = Color.Green;
                cameraSamplingButton.Text = "相机采样中...";
            }
            else
            {
                // 停止拍照
                _cameraManager.StopCapture();
                _isCameraSampling = false;

                cameraSamplingButton.BackColor = SystemColors.Control;
                cameraSamplingButton.Text = "相机采样";
            }
        }

        /// <summary>
        /// 浏览保存目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseSaveDirButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择相机照片保存目录";
                if (!string.IsNullOrEmpty(saveDirectoryTextBox.Text))
                {
                    dialog.SelectedPath = saveDirectoryTextBox.Text;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    saveDirectoryTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// 显示预览窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showPreviewButton_Click(object sender, EventArgs e)
        {
            if (_cameraManager == null || _previewForm == null)
                return;

            if (_cameraManager.IsPreviewRunning)
            {
                _cameraManager.StopPreview();
                showPreviewButton.Text = "相机预览";
                showPreviewButton.BackColor = SystemColors.Control;
                return;
            }

            if (!_previewForm.Visible)
            {
                _previewForm.Show();
            }
            _previewForm.BringToFront();

            _cameraManager.StartPreview(15);
            showPreviewButton.Text = "停止预览";
            showPreviewButton.BackColor = Color.LightGreen;
        }

        /// <summary>
        /// GroupBox自定义绘制：保留边框线，文字居中、加粗黑色
        /// </summary>
        private void GroupBoxHeader_Paint(object sender, PaintEventArgs e)
        {
            GroupBox gb = (GroupBox)sender;
            e.Graphics.Clear(gb.BackColor);

            Size textSize = TextRenderer.MeasureText(gb.Text, gb.Font);
            int textLeft = (gb.Width - textSize.Width) / 2;
            int textTop = (20 - textSize.Height) / 2;

            // 绘制边框线（上方中间断开放文字）
            using (Pen pen = new Pen(SystemColors.ControlDark))
            {
                // 左边线段
                e.Graphics.DrawLine(pen, 0, 7, textLeft - 4, 7);
                // 右边线段
                e.Graphics.DrawLine(pen, textLeft + textSize.Width + 4, 7, gb.Width, 7);
                // 左竖线
                e.Graphics.DrawLine(pen, 0, 7, 0, gb.Height - 1);
                // 右竖线
                e.Graphics.DrawLine(pen, gb.Width - 1, 7, gb.Width - 1, gb.Height - 1);
                // 底边线
                e.Graphics.DrawLine(pen, 0, gb.Height - 1, gb.Width - 1, gb.Height - 1);
            }

            // 居中绘制加粗黑色标题
            using (Font boldFont = new Font(gb.Font, FontStyle.Bold))
            {
                TextRenderer.DrawText(e.Graphics, gb.Text, boldFont,
                    new Rectangle(textLeft, textTop, textSize.Width, textSize.Height),
                    Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        /// <summary>
        /// 相机拍照日志回调
        /// </summary>
        /// <param name="record"></param>
        private void _cameraManager_OnCaptureLog(CameraLogRecord record)
        {
            try
            {
                if (cameraLogRichTextBox.InvokeRequired)
                {
                    cameraLogRichTextBox.Invoke(new Action(() =>
                    {
                        cameraLogRichTextBox.AppendText(record.ToString() + "\n");
                        cameraLogRichTextBox.ScrollToCaret();
                    }));
                }


                else
                {
                    cameraLogRichTextBox.AppendText(record.ToString() + "\n");
                    cameraLogRichTextBox.ScrollToCaret();
                }
            }
            catch { }
        }

        /// <summary>
        /// 相机预览图像回调
        /// </summary>
        /// <param name="image"></param>
        private void _cameraManager_OnPreviewImage(Image image)
        {
            try
            {
                if (_previewForm != null)
                {
                    _previewForm.UpdateImage(image);
                }
            }
            catch { }
        }

        /// <summary>
        /// 相机状态变化回调
        /// </summary>
        /// <param name="status"></param>
        private void _cameraManager_OnStatusChanged(string status)
        {
            try
            {
                if (cameraLogRichTextBox.InvokeRequired)
                {
                    cameraLogRichTextBox.Invoke(new Action(() =>
                    {
                        cameraLogRichTextBox.AppendText("[状态] " + status + "\n");
                        cameraLogRichTextBox.ScrollToCaret();
                    }));
                }
                else
                {
                    cameraLogRichTextBox.AppendText("[状态] " + status + "\n");
                    cameraLogRichTextBox.ScrollToCaret();
                }

                if (_previewForm != null)
                {
                    _previewForm.UpdateStatus(status);
                }

                if ((status.Contains("启动预览失败") || status.Contains("未找到") || status.Contains("[预览错误]"))
                    && showPreviewButton.Text == "停止预览")
                {
                    showPreviewButton.Text = "相机预览";
                    showPreviewButton.BackColor = SystemColors.Control;
                }

                if (!_cameraManager.IsPreviewRunning && showPreviewButton.Text == "停止预览")
                {
                    showPreviewButton.Text = "相机预览";
                    showPreviewButton.BackColor = SystemColors.Control;
                }
            }
            catch { }
        }

        /// <summary>
        /// GroupBox边框加粗美化（浅色，标题处断开）
        /// </summary>
        private void GroupBoxBorderPaint(object sender, PaintEventArgs e)
        {
            GroupBox box = (GroupBox)sender;
            using (Pen pen = new Pen(Color.FromArgb(180, 180, 180), 2))
            {
                int top = box.Font.Height / 2 + 1;
                int left = 1;
                int right = box.Width - 3;
                int bottom = box.Height - 2;

                // 测量标题文字宽度，在文字处断开
                SizeF textSize = e.Graphics.MeasureString(box.Text, box.Font);
                int textLeft = 12;
                int textRight = (int)(textLeft + textSize.Width + 4);

                // 上边线 - 文字左侧
                e.Graphics.DrawLine(pen, left, top, textLeft, top);
                // 上边线 - 文字右侧
                e.Graphics.DrawLine(pen, textRight, top, right, top);
                // 左边线
                e.Graphics.DrawLine(pen, left, top, left, bottom);
                // 右边线
                e.Graphics.DrawLine(pen, right, top, right, bottom);
                // 下边线
                e.Graphics.DrawLine(pen, left, bottom, right, bottom);
            }
        }

        /// <summary>
        /// 刷新数据线程
        /// Refresh Data Thread
        /// </summary>
        private void RefreshDataTh()
        {
            while (EnableRefreshDataTh)
            {
                try
                {
                    // 多设备的展示数据
                    string DeviceData = "";
                    Thread.Sleep(100);

                    bool hasOpenDevice = false;

                    // 刷新所有连接设备的数据
                    long currentRecordCount = System.Threading.Interlocked.Read(ref _recordCount);
                    for (int i = 0; i < FoundDeviceDict.Count; i++)
                    {
                        var keyValue = FoundDeviceDict.ElementAt(i);
                        Bwt901ble bWT901BLE = keyValue.Value;
                        if (bWT901BLE.IsOpen())
                        {
                            hasOpenDevice = true;
                            DeviceData += "═══════════════════════════════════════════\n";
                            DeviceData += "设备: " + bWT901BLE.GetDeviceName() + "  MAC: " + keyValue.Key + "\n";
                            DeviceData += "推送计数: " + currentRecordCount + " 条\n";
                            DeviceData += "───────────────────────────────────────────\n";
                            DeviceData += GetDeviceData(bWT901BLE) + "\r\n";
                        }
                        else
                        {
                            // 设备已找到但GATT未打开，显示诊断信息
                            DeviceData += "═══════════════════════════════════════════\n";
                            DeviceData += "设备: " + bWT901BLE.GetDeviceName() + "  MAC: " + keyValue.Key + "\n";
                            DeviceData += "状态: GATT连接中... (IsOpen=False)\n";
                        }
                    }

                    // 如果没有找到任何设备，显示提示
                    if (FoundDeviceDict.Count == 0 && _isSensorConnected)
                    {
                        DeviceData = "正在搜索传感器...\n";
                    }

                    // 更新传感器状态灯颜色
                    try
                    {
                        sensorStatusLight.Invoke(new Action(() =>
                        {
                            sensorStatusLight.BackColor = hasOpenDevice ? Color.Green : (_isSensorConnected ? Color.Yellow : Color.Gray);
                        }));
                    }
                    catch { }

                    try
                    {
                        dataRichTextBox.Invoke(new Action(() =>
                        {
                            dataRichTextBox.Text = DeviceData;
                            // 更新日志计数显示
                            if (_isLogging && _samplingLogger != null)
                            {
                                logCountLabel.Text = string.Format("已记录: {0} 条", _samplingLogger.SampleCount);
                            }
                        }));
                    }
                    catch { }
                }
                catch
                {
                    // 防止线程因异常退出
                    Thread.Sleep(500);
                }
            }
        }
    }
}
