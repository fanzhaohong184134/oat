# CalibrationBench — 数字对中仪出厂校准工装

独立的出厂校准程序：**无界面引擎 exe** + **C# WinForms 界面**，与 dsat 主应用/后处理程序**完全解耦**（仅进程调用），可独立安装。

技术栈：.NET Framework 4.7.2 / C# / WinForms，零第三方依赖（JSON 用 `DataContractJsonSerializer`）。

## 组成

| 项目 | 产物 | 说明 |
|------|------|------|
| `CalibrationEngine` | `CalibrationEngine.exe` | 无界面引擎：0A/0B/0C/0D 解算+判据+写配置+归档 |
| `CalibrationBench.UI` | `CalibrationBench.UI.exe` | 界面：采集输入、调用引擎、展示结果 |
| `installer` | zip / Inno Setup | 独立打包安装 |
| `samples` | *_input.json | 可运行样例 |

## 构建

```powershell
# 需 Visual Studio 2019 (MSBuild) + .NET Framework 4.7.2 targeting pack
& "<VS>\MSBuild\Current\Bin\MSBuild.exe" CalibrationBench.sln /t:Rebuild /p:Configuration=Release
```

## 打包（独立安装）

```powershell
powershell -ExecutionPolicy Bypass -File installer\package.ps1
# 产出: installer\CalibrationBench_Portable_1.0.0.zip（绿色版）
#       若装了 Inno Setup(ISCC.exe) → 生成安装包
```

## 使用

- 界面：运行 `CalibrationBench.UI.exe`，填设备编号/设备根目录，点各步骤按钮选输入 JSON 执行；引擎自动写 `calibration_config.json` 并归档。
- 命令行：

```
CalibrationEngine.exe --step 0D --input samples\0D_input.json --output out.json --config calibration_config.json --device-root . --device-id AT1
```

## 校准流程与判据

| 步骤 | 内容 | 放行判据 |
|------|------|----------|
| 0A | 相机内参 | 重投影 RMS < 0.5 px |
| 0B | 安装角 δ_pitch/δ_roll | 各次偏差 ≤ 0.05° |
| 0C | 航向 ψ_offset（≥8 方位） | 残差 < 0.1° |
| 0D | 综合验证（PnP 闭合 + 旋转不变性） | σ_rot<0.3mm 且 闭合<0.5mm |

## 采集编排（旋转采集只 0C/0D 需要，且共用一次采集）

- 旋转台仅 **0C 与 0D** 需要，二者**共用一次 8/9 方位 step-and-stare 采集**；0A（棋盘格多姿态静态）、0B（铅垂静止多帧）不转台。
- 界面「采集编排」分组：设方位间隔°/方位数/每方位帧数/稳定秒 → 一键采集：
  - `采集 0B(静止)` → 生成 `acquired\0B_input.json`
  - `采集 0C/0D(多方位)` → 生成 `acquired\0C_input.json`、`acquired\0D_input.json`
  - 之后用步骤按钮选对应文件执行。
- 当前数据源为**模拟**（`Acquisition/Sim/SimulatedBench`），硬件访问抽象为 HAL 接口（`IRotaryStage`/`ISceneCapture`）；接入真实相机/旋转台/IMU 只需实现同接口替换模拟类。

## 数据源切换与真实驱动骨架

界面「采集编排」有**数据源下拉（模拟 / 真实）**与旋转台串口输入：

- **模拟**：无硬件，按已知真值合成，端到端可跑通。
- **真实**：使用 `Acquisition/Real` 下的驱动：
  - `RealRotaryStage`（`System.IO.Ports` 串口，**可用实现**；协议 `MOVE/LOCK/STAT?`，按转台改命令帧即可）
  - `RealSceneCapture`（**完整实现** PnP 位姿→offset/H/姿态/方位 换算，含 Rodrigues）
  - 三个 IO 驱动：`IndustrialCameraSource`（相机 SDK，骨架）、`Bwt901ImuSource`（对接 `Bwt901ble`，骨架）、`OpenCvCharucoPnpSolver`（**含 OpenCvSharp 参考实现**，见下）。
  - 真实模式内参**自动从 `calibration_config.json` 读取**（`ConfigLoader`），未找到时用占位内参并提示先做 0A。

### 启用 OpenCvSharp 真实 PnP

`OpenCvCharucoPnpSolver` 默认走骨架（零依赖、可离线编译）；启用真实 PnP：

1. NuGet 安装 `OpenCvSharp4` + `OpenCvSharp4.runtime.win`；
2. 在 `CalibrationBench.UI.csproj` 的 `DefineConstants` 追加 `USE_OPENCV`；
3. 按物理靶标板修改 `OpenCvCharucoPnpSolver` 的 `CharucoBoard` 参数（格数/尺寸/字典）。

参考实现（`#if USE_OPENCV` 分支）：`CvAruco.DetectMarkers → InterpolateCornersCharuco → EstimatePoseCharucoBoard`，并用零畸变 `ProjectPoints` 得校正后板原点像素。

接入真实硬件只需实现相机/IMU 两个骨架类的方法体（PnP 参考实现已给出），采集编排/引擎/界面/契约均无需改动。

输入/输出每项字段与存储位置见 [数据字典与接口说明.md](数据字典与接口说明.md)。

方案与台架设计见 [../校准台研制详细方案设计.md](../校准台研制详细方案设计.md)、[../校准方案（靶标板自定位）.md](../校准方案（靶标板自定位）.md)、[../校准台设计指南与步骤.md](../校准台设计指南与步骤.md)。

## 说明

- PnP/角点检测由采集端（相机 SDK + OpenCV）完成，结果以 JSON 喂入引擎；引擎只做解算/判据/统计/存储，故零 native 依赖、易测试、可独立部署。
- 现场简化算法（Step 0D 内 `ComputeSysOffset`）与 [../校准与测量流程.md](../校准与测量流程.md) Step 4–6 完全一致。
