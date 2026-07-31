# dsat

数字对中仪桌面程序（Digital Shaft Alignment Tool），基于 WinForms 与 .NET Framework 4.6.1。

## 项目概览

- 主程序工程：`dsat.csproj`
- 解决方案：`dsat.sln`
- 主程序输出：`bin/Release/dsat.exe`
- 安装包输出：`installer/output/dsat_Setup_1.0.0.exe`

核心能力：

- BLE 传感器连接与姿态数据采集
- 相机采样与预览日志记录
- 安装角/航向角/相机标定流程
- 采样记录、日志分层展示与后处理

## 构建要求

- Visual Studio 2019（MSBuild 16.x）
- .NET Framework 4.6.1 开发环境
- Inno Setup 6（用于生成安装包）

## 本地构建

仅构建（不打安装包）：

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\BuildInstaller.ps1 -SkipIscc
```

完整构建并生成安装包：

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\BuildInstaller.ps1
```

也可使用：

```bat
build_installer.bat
```

## 目录说明

- `CalibrationPanels/`：标定面板与弹窗
- `DataProcessing/`：标定与后处理逻辑
- `camera/`：相机采样与预览相关能力
- `Sampling/`：IMU 采样记录与日志
- `installer/`：安装包脚本与 Inno Setup 配置

## 文档

- 使用说明书：`数字对中仪（Digital Shaft Alignment Tool）使用说明书.md`
- 安装打包说明：`installer/README.md`
- 分析文档：`dsat_分析文档.md`

## 说明

- 工程名、可执行文件名、安装包名统一为 `dsat`。
- 安装脚本内保留了旧名称清理逻辑，用于防止历史构建产物混入新包。
