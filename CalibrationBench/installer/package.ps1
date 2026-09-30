# 构建并打包 出厂校准工装(独立安装，和 dsat 主应用解耦)
# 用法: powershell -ExecutionPolicy Bypass -File installer\package.ps1
$ErrorActionPreference = "Stop"
$root  = Split-Path -Parent $PSScriptRoot          # CalibrationBench\
$sln   = Join-Path $root "CalibrationBench.sln"
$dist  = Join-Path $PSScriptRoot "dist\CalibrationBench"

# 1. 定位 MSBuild(VS2019)
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vsPath  = & $vswhere -latest -products * -property installationPath
$msb     = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msb)) { throw "未找到 MSBuild: $msb" }

# 2. 编译 Release
& $msb $sln /t:Rebuild /p:Configuration=Release /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "编译失败" }

# 3. 暂存
if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item -ItemType Directory -Force -Path $dist | Out-Null
New-Item -ItemType Directory -Force -Path "$dist\samples" | Out-Null
Copy-Item "$root\CalibrationBench.UI\bin\Release\CalibrationBench.UI.exe" $dist -Force
Copy-Item "$root\CalibrationEngine\bin\Release\CalibrationEngine.exe"     $dist -Force
Copy-Item "$root\samples\*_input.json" "$dist\samples" -Force
Copy-Item "$root\README.md" $dist -Force -ErrorAction SilentlyContinue

# 4. 生成绿色版 zip
$zip = Join-Path $PSScriptRoot "CalibrationBench_Portable_1.0.0.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path "$dist\*" -DestinationPath $zip
Write-Output "绿色版: $zip"

# 5. 若安装了 Inno Setup 则编译安装包
$iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
if ($iscc) {
    & $iscc.Source (Join-Path $PSScriptRoot "CalibrationBench.iss")
    Write-Output "安装包已生成(见 installer 输出目录)"
} else {
    Write-Output "未检测到 Inno Setup(ISCC.exe)，已跳过安装包编译；绿色版 zip 可直接分发。"
}
