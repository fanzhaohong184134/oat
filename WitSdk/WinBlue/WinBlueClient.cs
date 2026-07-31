using System;
using System.Diagnostics;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Wit.Bluetooth.Utils;
using Wit.Bluetooth.WinBlue.Entity;
using Wit.Bluetooth.WinBlue.Enums;
using Wit.Bluetooth.WinBlue.Interface;
using Wit.Bluetooth.WinBlue.Utils;

namespace Wit.Bluetooth.WinBlue
{
    /// <summary>
    /// 蓝牙连接器
    /// </summary>
    public class WinBlueClient
    {
        // 蓝牙管理器
        public IWinBlueManager bluetoothManager = null;

        // 存储检测到的主服务。
        public GattDeviceService CurrentService { get; set; }

        // 蓝牙设备。
        public BluetoothLEDevice BluetoothDevice { get; set; }

        // 存储检测到的写特征对象。
        public GattCharacteristic CurrentWriteCharacteristic { get; set; }

        // 存储检测到的通知特征对象。
        public GattCharacteristic CurrentNotifyCharacteristic { get; set; }

        // 定义一个委托
        public delegate void ReceiveDataDelegate(BluetoothEvent type, string mac, byte[] data = null);

        // 定义一个事件
        public event ReceiveDataDelegate OnReceive;

        /// <summary>
        /// 是否连接蓝牙
        /// </summary>
        public bool IsConnect = false;

        // 配置
        private WinBleOption Config;

        // 特性通知类型通知启用
        private const GattClientCharacteristicConfigurationDescriptorValue CHARACTERISTIC_NOTIFICATION_TYPE =
            GattClientCharacteristicConfigurationDescriptorValue.Notify;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="config"></param>
        public WinBlueClient(WinBleOption config)
        {
            Config = config;
            bluetoothManager = WinBlueFactory.GetInstance();
        }

        /// <summary>
        /// 按MAC地址直接组装设备ID查找设备
        /// </summary>
        public void Connect()
        {
            BluetoothDevice = bluetoothManager.GetDevice(Config.Mac);
            if (BluetoothDevice == null)
            {
                OnReceive?.Invoke(BluetoothEvent.Disconnected, Config.Mac);
                return;
            }

            // 连接状态改变事件
            BluetoothDevice.ConnectionStatusChanged -= CurrentDevice_ConnectionStatusChanged;
            BluetoothDevice.ConnectionStatusChanged += CurrentDevice_ConnectionStatusChanged;
            Guid guid = new Guid(Config.ServiceGuid);

            // 连接中
            OnReceive?.Invoke(BluetoothEvent.Connecting, Config.Mac);

            BluetoothDevice.GetGattServicesForUuidAsync(guid).Completed = (asyncInfo, asyncStatus) =>
            {
                if (asyncStatus == AsyncStatus.Completed)
                {
                    try
                    {
                        GattDeviceServicesResult result = asyncInfo.GetResults();
                        if (result.Services.Count > 0)
                        {
                            CurrentService = result.Services[0];
                            if (CurrentService != null)
                            {
                                GetCurrentWriteCharacteristic();
                                GetCurrentNotifyCharacteristic();
                            }
                            IsConnect = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine(e.StackTrace);
                    }
                }
            };
        }

        /// <summary>
        /// 连接状态改变事件
        /// </summary>
        private void CurrentDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                OnReceive?.Invoke(BluetoothEvent.Connected, Config.Mac);
            }
            else
            {
                OnReceive?.Invoke(BluetoothEvent.Disconnected, Config.Mac);
            }
        }

        /// <summary>
        /// 设置写特征对象。
        /// </summary>
        public void GetCurrentWriteCharacteristic()
        {
            if (CurrentService == null) return;
            Guid guid = new Guid(Config.WriteGuid);

            CurrentService.GetCharacteristicsForUuidAsync(guid).Completed = (asyncInfo, asyncStatus) =>
            {
                if (asyncStatus == AsyncStatus.Completed)
                {
                    GattCharacteristicsResult result = asyncInfo.GetResults();
                    if (result.Characteristics.Count > 0)
                    {
                        CurrentWriteCharacteristic = result.Characteristics[0];
                    }
                    else
                    {
                        Thread.Sleep(10);
                        GetCurrentWriteCharacteristic();
                    }
                }
            };
        }

        /// <summary>
        /// 设置通知特征对象。
        /// </summary>
        public void GetCurrentNotifyCharacteristic()
        {
            if (CurrentService == null) return;
            Guid guid = new Guid(Config.NotifyGuid);
            CurrentService.GetCharacteristicsForUuidAsync(guid).Completed = (asyncInfo, asyncStatus) =>
            {
                if (asyncStatus == AsyncStatus.Completed)
                {
                    GattCharacteristicsResult result = asyncInfo.GetResults();
                    if (result.Characteristics.Count > 0)
                    {
                        CurrentNotifyCharacteristic = result.Characteristics[0];
                        CurrentNotifyCharacteristic.ProtectionLevel = GattProtectionLevel.Plain;
                        CurrentNotifyCharacteristic.ValueChanged += Characteristic_ValueChanged;
                        EnableNotifications(CurrentNotifyCharacteristic);
                    }
                    else
                    {
                        OnReceive?.Invoke(BluetoothEvent.Connecting, Config.Mac);
                        Thread.Sleep(10);
                        GetCurrentNotifyCharacteristic();
                    }
                }
            };
        }

        /// <summary>
        /// 特征值改变事件
        /// </summary>
        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (!IsConnect) return;
            byte[] data;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
            if (sender != null)
            {
                string mac = MacUtils.DeviceIdToMac(sender.Service.Device.DeviceId);
                OnReceive?.Invoke(BluetoothEvent.Data, mac, data);
            }
        }

        /// <summary>
        /// 设置特征对象为接收通知对象
        /// </summary>
        public void EnableNotifications(GattCharacteristic characteristic)
        {
            characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(CHARACTERISTIC_NOTIFICATION_TYPE).Completed = (asyncInfo, asyncStatus) =>
            {
                if (asyncStatus == AsyncStatus.Completed)
                {
                    GattCommunicationStatus status = asyncInfo.GetResults();
                    if (status == GattCommunicationStatus.Unreachable)
                    {
                        OnReceive?.Invoke(BluetoothEvent.Connecting, Config.Mac);
                        if (CurrentNotifyCharacteristic != null)
                        {
                            EnableNotifications(CurrentNotifyCharacteristic);
                        }
                    }
                    OnReceive?.Invoke(BluetoothEvent.Connecting, Config.Mac);
                }
            };
        }

        /// <summary>
        /// 写出数据
        /// </summary>
        public void Write(byte[] data)
        {
            if (CurrentWriteCharacteristic != null)
            {
                _ = CurrentWriteCharacteristic.WriteValueAsync(
                    CryptographicBuffer.CreateFromByteArray(data),
                    GattWriteOption.WriteWithResponse);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            IsConnect = false;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            IsConnect = false;

            if (CurrentNotifyCharacteristic != null)
            {
                CurrentNotifyCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                CurrentNotifyCharacteristic = null;
            }

            CurrentService?.Dispose();
            BluetoothDevice?.Dispose();
            BluetoothDevice = null;
            CurrentService = null;
            CurrentWriteCharacteristic = null;
            CurrentNotifyCharacteristic = null;
        }

        /// <summary>
        /// 程序关闭时
        /// </summary>
        ~WinBlueClient()
        {
            Dispose();
        }
    }
}
