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

输入/输出每项字段与存储位置见 [数据字典与接口说明.md](数据字典与接口说明.md)。

方案与台架设计见 [../校准台研制详细方案设计.md](../校准台研制详细方案设计.md)、[../校准方案（靶标板自定位）.md](../校准方案（靶标板自定位）.md)、[../校准台设计指南与步骤.md](../校准台设计指南与步骤.md)。

## 说明

- PnP/角点检测由采集端（相机 SDK + OpenCV）完成，结果以 JSON 喂入引擎；引擎只做解算/判据/统计/存储，故零 native 依赖、易测试、可独立部署。
- 现场简化算法（Step 0D 内 `ComputeSysOffset`）与 [../校准与测量流程.md](../校准与测量流程.md) Step 4–6 完全一致。
