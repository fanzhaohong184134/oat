# 维特智能 BWT901BLE 蓝牙IMU Windows C# 示例程序 分析文档

## 1. 项目概述

本项目是维特智能（WitMotion）开发的 **BWT901BLE 九轴蓝牙惯性测量单元（IMU）** 的 Windows C# 示例程序。该程序通过 Windows BLE（蓝牙低功耗）接口连接 BWT901BLE 传感器，实时读取并显示加速度、角速度、角度、磁场等九轴传感器数据，并支持传感器校准、参数配置和采样数据记录。

- **项目名称**: dsat
- **开发语言**: C# 8.0
- **目标框架**: .NET Framework 4.5
- **项目类型**: Windows Forms 桌面应用程序
- **GitHub 仓库**: https://github.com/WITMOTION/WitBluetooth_BWT901BLE5_0.git

---

## 2. 功能列表

### 2.1 核心功能

| 功能 | 说明 |
|------|------|
| **蓝牙设备扫描** | 扫描周围名称包含 "WT" 的蓝牙BLE设备 |
| **设备自动连接** | 找到设备后自动建立BLE连接 |
| **多设备支持** | 支持同时连接多个BWT901BLE传感器 |
| **实时数据显示** | 100ms间隔刷新显示所有已连接设备的传感器数据 |
| **数据采样记录** | 将传感器数据记录为CSV文件，支持带时间戳的精确记录 |

### 2.2 传感器数据项

| 数据类别 | 数据项 | 单位 |
|----------|--------|------|
| **加速度** | AccX, AccY, AccZ | g |
| **角速度** | GyroX, GyroY, GyroZ | °/s |
| **角度** | AngleX, AngleY, AngleZ | ° |
| **磁场** | MagX, MagY, MagZ, MagM | uT |
| **四元数** | Q0, Q1, Q2, Q3 | - |
| **温度** | Temperature | - |
| **电量** | PowerPercent | - |
| **芯片时间** | ChipTime | - |
| **版本号** | VersionNumber | - |
| **序列号** | SerialNumber | - |

### 2.3 传感器控制功能

| 功能 | 说明 |
|------|------|
| **加计校准** | 发送加速度校准命令，传感器需静止放置 |
| **磁场校准** | 分两步：开始校准（绕XYZ三轴各转一圈）→ 结束校准 |
| **设置回传速率** | 支持 10Hz（0x06）和 50Hz（0x08）两种速率 |
| **设置带宽** | 支持 20Hz（0x04）和 256Hz（0x00）两种带宽 |
| **读取寄存器** | 支持读取指定寄存器值（示例为03寄存器） |

### 2.4 数据记录功能（新增）

| 功能 | 说明 |
|------|------|
| **CSV采样记录** | 一键开始/停止记录，数据保存为CSV文件 |
| **毫秒级时间戳** | 每条记录包含精确到毫秒的采集时间戳 |
| **实时计数显示** | UI实时显示已记录的数据条数 |
| **自动文件管理** | 按日期时间自动命名文件，保存到 `logs/` 目录 |

---

## 3. 代码架构

### 3.1 目录结构

```
dsat/
├── Form1.cs                    # 主窗口逻辑（扫描、连接、数据显示、校准、记录）
├── Form1.Designer.cs           # 主窗口UI布局定义
├── Form1.resx                  # 中文资源文件
├── Form1.zh-CN.resx            # 中文（简体）资源文件
├── Program.cs                  # 程序入口点
├── App.config                  # 应用配置
│
├── Sampling/                   # 【新增】采样记录模块
│   ├── SamplingRecord.cs       # 采样数据结构体（一条完整的传感器读数）
│   └── SamplingLogger.cs       # 采样日志管理器（CSV文件写入、线程安全、自动刷新）
│
├── ble5/                       # BLE5.0 传感器设备层
│   ├── BWT901BLE.cs            # BWT901BLE设备连接类（封装Open/Close/SendData/校准/配置等API）
│   ├── Components/
│   │   ├── Bwt901bleResolver.cs # 协议解析器（将原始字节流解析为传感器数据）
│   │   └── Bwt901bleProcessor.cs # 数据处理器（将解析后的数据映射为具体物理量如加速度、角度等）
│   └── Data/
│       └── WitSensorKey.cs     # 传感器数据键值常量定义（AccX, AngleX等）
│
├── WitSdk/                     # SDK核心库（嵌入项目中，非外部引用）
│   ├── Device/                 # 设备抽象层
│   │   ├── Connector/          # 连接器（串口、TCP、UDP、BLE等连接方式）
│   │   ├── Device/             # 设备模型（DeviceModel、数据键值、设备事件等）
│   │   ├── Processor/          # 数据处理接口
│   │   └── Resolver/           # 协议解析接口
│   ├── Example/
│   │   └── IAttitudeSensorApi.cs # 姿态传感器API接口定义
│   ├── Tools/                  # 工具类（协议工具、字节转换、GPS工具等）
│   └── WinBlue/                # Windows BLE蓝牙管理模块
│       ├── Interface/
│       │   └── WinBlueManager.cs  # 蓝牙管理器接口（IWinBlueManager）
│       ├── WinBlueManagerImpl.cs  # 蓝牙管理器实现
│       ├── WinBlueClient.cs       # BLE客户端（GATT连接、特征值读写）
│       ├── WinBlueFinder.cs       # BLE设备扫描器
│       └── Utils/
│           ├── WinBlueFactory.cs  # 蓝牙管理器工厂
│           └── MacUtils.cs        # MAC地址工具
│
├── _DII/
│   └── Windows.winmd           # Windows Runtime元数据引用（BLE API需要）
│
└── Properties/                 # 程序集信息、资源、设置
    ├── AssemblyInfo.cs
    ├── Resources.Designer.cs
    └── Settings.Designer.cs
```

### 3.2 架构分层图

```
┌─────────────────────────────────────────────────────────┐
│                    UI 层 (WinForms)                       │
│  Form1.cs — 主窗口：扫描/连接/显示/校准/配置/采样记录     │
│  Sampling/ — CSV采样记录模块                               │
├─────────────────────────────────────────────────────────┤
│                    设备API层                               │
│  BWT901BLE.cs — 封装传感器操作API（校准/配置/数据读取）    │
│  IAttitudeSensorApi — 姿态传感器接口                       │
├─────────────────────────────────────────────────────────┤
│                  SDK设备抽象层                              │
│  DeviceModel — 设备模型（生命周期管理、数据存储）          │
│  IConnector / WinBleConnector — 连接器抽象                 │
│  IProtocolResolver / Bwt901bleResolver — 协议解析          │
│  IDataProcessor / Bwt901bleProcessor — 数据处理            │
├─────────────────────────────────────────────────────────┤
│                通信层（Windows BLE）                       │
│  IWinBlueManager → WinBlueManagerImpl — 蓝牙管理器        │
│  WinBlueClient — BLE GATT读写                             │
│  WinBlueFinder — 设备扫描                                 │
├─────────────────────────────────────────────────────────┤
│              Windows Runtime (Windows.winmd)              │
│  Windows.Devices.Bluetooth — 系统BLE API                  │
└─────────────────────────────────────────────────────────┘
```

### 3.3 核心数据流

```
BWT901BLE传感器 (BLE广播)
    │
    ▼
WinBlueFinder (扫描发现设备，回调 OnDeviceFound)
    │
    ▼
WinBlueClient (建立GATT连接，订阅特征值通知)
    │  收到原始字节数据
    ▼
Bwt901bleResolver (协议解析：帧头检测、校验和验证、数据提取)
    │  解析为原始 short 值
    ▼
Bwt901bleProcessor (数据处理：转换为物理量，如 short/32768*180 = 角度)
    │  存储到 DeviceModel 的数据字典
    ▼
DeviceModel.OnListenKeyUpdate → BWT901BLE.OnRecord 事件
    │
    ▼
Form1.BWT901BLE_OnRecord (更新UI显示 / 写入CSV采样日志)
```

### 3.4 通信协议

- **协议格式**: 维特智能自定义协议（5字节帧格式）
- **帧结构**: `0xFF 0xAA [寄存器地址] [数据低字节] [数据高字节]`
- **解锁命令**: `0xFF 0xAA 0x69 0x88 0xB5`
- **BLE通信**: 使用 GATT 服务的 Notify 特征值接收传感器数据

---

## 4. 如何编译

### 4.1 环境要求

| 要求 | 说明 |
|------|------|
| **操作系统** | Windows 10（需支持BLE） |
| **开发工具** | Visual Studio 2019 或更高版本 |
| **目标框架** | .NET Framework 4.5 |
| **外部依赖** | 无NuGet包依赖（所有SDK代码嵌入项目） |
| **特殊引用** | `_DII/Windows.winmd`（Windows Runtime BLE API） |

### 4.2 使用 Visual Studio 编译

1. **打开解决方案**
   ```
   双击 dsat.sln 用 Visual Studio 打开
   ```

2. **选择编译配置**
   - 配置: `Debug` 或 `Release`
   - 平台: `Any CPU`

3. **编译**
   - 菜单: `生成` → `生成解决方案`
   - 快捷键: `Ctrl + Shift + B`

4. **输出文件**
   ```
   bin\Debug\dsat.exe
   bin\Debug\Windows.winmd
    bin\Debug\zh-CN\dsat.resources.dll
   ```

### 4.3 使用 MSBuild 命令行编译

```cmd
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" dsat.sln /p:Configuration=Release "/p:Platform=Any CPU" /t:Rebuild
```

> **注意**: 不能使用 `dotnet build` 编译，因为本项目是 .NET Framework 4.5 项目，需要使用 Visual Studio 自带的 MSBuild。

---

## 5. 如何使用

### 5.1 前置条件

1. **硬件要求**
   - BWT901BLE 九轴蓝牙传感器
   - 支持 BLE 4.0+ 的蓝牙适配器（内置或USB外置）
   - Windows 10 电脑

2. **软件要求**
   - Windows 10 版本 1607 或更高（BLE API 需要）
   - 确保蓝牙已开启

### 5.2 操作步骤

#### 步骤1：启动程序
运行 `bin\Debug\dsat.exe`

#### 步骤2：扫描设备
点击左侧 **【开始扫描】** 按钮，程序将自动扫描周围蓝牙设备。

#### 步骤3：连接设备
程序自动过滤名称包含 "WT" 的设备并建立连接，连接成功后右侧数据区域开始显示传感器数据。

#### 步骤4：查看实时数据
数据区域每 100ms 刷新一次，显示内容包括：
- **采样时间** (yyyy-MM-dd HH:mm:ss.fff 毫秒精度)
- **采样编号** (记录模式下显示)
- **设备名称** (设备名(MAC地址))
- **加速度** (AccX/AccY/AccZ, 单位: g)
- **角速度** (GyroX/GyroY/GyroZ, 单位: °/s)
- **角度** (AngleX/AngleY/AngleZ, 单位: °)
- **磁场** (MagX/MagY/MagZ, 单位: uT)
- **版本号**

#### 步骤5：采样记录（新功能）
1. 连接设备后，点击左侧 **【开始记录】** 按钮
2. 程序开始将传感器数据记录到 CSV 文件
3. 实时显示已记录的数据条数
4. 点击 **【停止记录】** 按钮结束记录
5. 弹窗提示保存路径和记录条数
6. 日志文件保存在程序目录下的 `logs/` 文件夹中

#### 步骤6：传感器校准

**加计校准**：将传感器静止水平放置，点击 **【加计校准】** 按钮

**磁场校准**：
1. 点击 **【开始磁场校准】** 按钮
2. 分别绕传感器 X、Y、Z 三轴各转一圈
3. 点击 **【结束磁场校准】** 按钮

#### 步骤7：参数配置

| 操作 | 说明 |
|------|------|
| **【回传10HZ】** | 设置传感器数据回传速率为 10Hz |
| **【回传50HZ】** | 设置传感器数据回传速率为 50Hz |
| **【带宽20HZ】** | 设置传感器滤波带宽为 20Hz |
| **【带宽256HZ】** | 设置传感器滤波带宽为 256Hz |
| **【读03寄存器】** | 读取传感器 03 号寄存器的值 |

#### 步骤8：停止扫描
点击 **【停止扫描】** 按钮停止设备扫描。

### 5.3 采样日志文件格式

日志保存为 CSV 格式，文件名格式：`设备名_yyyyMMdd_HHmmssfff.csv`

**CSV 列定义：**

| 列名 | 类型 | 说明 |
|------|------|------|
| SampleTime | string | 采样时间 (yyyy-MM-dd HH:mm:ss.fff) |
| DeviceMAC | string | 设备蓝牙MAC地址 |
| DeviceName | string | 设备名称 |
| AccX/AccY/AccZ | double? | 三轴加速度 (g) |
| GyroX/GyroY/GyroZ | double? | 三轴角速度 (°/s) |
| AngleX/AngleY/AngleZ | double? | 三轴角度 (°) |
| MagX/MagY/MagZ/MagM | double? | 三轴磁场及磁场强度 (uT) |
| Q0/Q1/Q2/Q3 | double? | 四元数 |
| Temperature | double? | 温度 |
| PowerPercent | double? | 电量百分比 |
| ChipTime | string | 芯片时间 |
| VersionNumber | string | 版本号 |
| SerialNumber | string | 序列号 |

### 5.4 UI 布局说明

```
┌──────────────────────────────────────────────────────────────┐
│  ┌──────────────┐  ┌────────────────────────────────────────┐│
│  │  蓝牙操作     │  │                                        ││
│  │ [开始扫描]    │  │                                        ││
│  │ [停止扫描]    │  │                                        ││
│  ├──────────────┤  │          传感器数据展示区                 ││
│  │  传感器操作   │  │          (RichTextBox)                  ││
│  │ [加计校准]    │  │                                        ││
│  │ [开始磁场校准]│  │  采样时间: 2026-07-08 09:27:07.123     ││
│  │ [结束磁场校准]│  │  采样编号: #100                        ││
│  │ [读03寄存器]  │  │  WT901BLE(XX:XX:XX:XX:XX:XX)           ││
│  │ [回传10HZ]    │  │  AccX:0.01g  AccY:0.02g  AccZ:1.00g   ││
│  │ [回传50HZ]    │  │  GyroX:0.1°/s GyroY:0.2°/s GyroZ:...  ││
│  │ [带宽20HZ]    │  │  AngleX:1.2°  AngleY:0.5°  AngleZ:...  ││
│  │ [带宽256HZ]   │  │  ...                                   ││
│  ├──────────────┤  │                                        ││
│  │  采样记录     │  │                                        ││
│  │ [开始记录]    │  │                                        ││
│  │ [停止记录]    │  │                                        ││
│  │ 状态: 未记录  │  │                                        ││
│  │ 已记录: 0 条  │  │                                        ││
│  └──────────────┘  └────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────────┘
```

---

## 6. 关键类说明

| 类名 | 文件 | 职责 |
|------|------|------|
| `Form1` | Form1.cs | 主窗口，包含所有业务逻辑 |
| `Bwt901ble` | ble5/BWT901BLE.cs | BWT901BLE设备封装类，提供传感器操作API |
| `Bwt901bleResolver` | ble5/Components/Bwt901bleResolver.cs | 维特协议解析器 |
| `Bwt901bleProcessor` | ble5/Components/Bwt901bleProcessor.cs | 传感器数据处理器 |
| `WitSensorKey` | ble5/Data/WitSensorKey.cs | 传感器数据键值常量 |
| `SamplingRecord` | Sampling/SamplingRecord.cs | 采样数据结构体 |
| `SamplingLogger` | Sampling/SamplingLogger.cs | CSV采样日志管理器（线程安全） |
| `DeviceModel` | WitSdk/Device/Device/DeviceModel.cs | 设备模型（核心SDK类） |
| `IWinBlueManager` | WitSdk/WinBlue/Interface/WinBlueManager.cs | 蓝牙管理器接口 |
| `WinBlueClient` | WitSdk/WinBlue/WinBlueClient.cs | BLE GATT客户端 |
| `WinBlueFinder` | WitSdk/WinBlue/WinBlueFinder.cs | BLE设备扫描器 |

---

## 7. 扩展开发指南

### 7.1 添加新的传感器数据

1. 在 `WitSensorKey.cs` 中添加新的数据键常量
2. 在 `Bwt901bleProcessor.cs` 中添加数据处理逻辑
3. 在 `Form1.cs` 的 `GetDeviceData()` 和 `BuildSamplingRecord()` 方法中添加显示/记录逻辑
4. 在 `SamplingRecord.cs` 中添加对应的属性字段

### 7.2 添加新的传感器命令

在 `Bwt901BLE.cs` 中添加新的方法，通过 `SendProtocolData()` 发送 5 字节协议命令：
```csharp
public void YourNewCommand(byte param)
{
    SendProtocolData(new byte[] { 0xff, 0xaa, 寄存器地址, param, 0x00 });
}
```

### 7.3 改造为其他传感器

本项目架构支持扩展到维特智能的其他传感器型号：
1. 创建新的设备类（类似 `Bwt901ble`）
2. 创建新的协议解析器和数据处理器
3. 实现 `IAttitudeSensorApi` 接口

---

## 8. 已知限制

1. **仅支持 Windows 10+**: BLE API 依赖 Windows Runtime
2. **仅支持维特智能传感器**: 设备名过滤条件为 "WT"
3. **单窗口设计**: 所有逻辑集中在 Form1 中
4. **无持久化配置**: 参数（如回传速率、带宽）重启后不保留
5. **.NET Framework 4.5**: 不支持 .NET Core / .NET 5+
