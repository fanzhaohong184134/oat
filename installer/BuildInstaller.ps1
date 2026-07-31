param(
    [string]$Configuration = "Release",
    [string]$Platform = "",
    [switch]$SkipBuild,
    [switch]$SkipIscc
)

$ErrorActionPreference = "Stop"

function Resolve-MSBuild {
    $candidates = @(
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($p in $candidates) {
        if (Test-Path $p) { return $p }
    }

    $cmd = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    throw "MSBuild not found. Install Visual Studio Build Tools or set PATH."
}

function Resolve-ISCC {
    $candidates = @(
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"),
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )

    foreach ($p in $candidates) {
        if (Test-Path $p) { return $p }
    }

    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    return $null
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$sln = Join-Path $repoRoot "dsat.sln"
$mainOut = Join-Path $repoRoot "bin\$Configuration"
$stageRoot = Join-Path $repoRoot "installer\_staging"
$stageApp = Join-Path $stageRoot "app"
$outDir = Join-Path $repoRoot "installer\output"
$issPath = Join-Path $PSScriptRoot "Setup.iss"

if (-not (Test-Path $sln)) {
    throw "Solution not found: $sln"
}

if (-not $SkipBuild) {
    $msbuild = Resolve-MSBuild
    Write-Host "[1/4] Building solution..."
    $buildArgs = @(
        $sln,
        "/t:Build",
        "/p:Configuration=$Configuration",
        "/verbosity:minimal"
    )

    if (-not [string]::IsNullOrWhiteSpace($Platform)) {
        $buildArgs += "/p:Platform=$Platform"
    }

    & $msbuild @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
}

Write-Host "[2/4] Preparing staging directory..."
if (Test-Path $stageRoot) {
    Remove-Item $stageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $stageApp -Force | Out-Null

if (-not (Test-Path $mainOut)) {
    throw "Main output folder not found: $mainOut"
}

Copy-Item (Join-Path $mainOut "*") $stageApp -Recurse -Force

# Ensure calibration and post-processing child apps (and their dependencies) are included.
$childOutputDirs = @(
    (Join-Path $repoRoot "DataProcessing\Calibration\CameraCalibrationApp\bin\$Configuration"),
    (Join-Path $repoRoot "DataProcessing\Calibration\InstrumentCalibrationApp\bin\$Configuration"),
    (Join-Path $repoRoot "DataProcessing\Calibration\MountingCalibrationApp\bin\$Configuration"),
    (Join-Path $repoRoot "DataProcessing\PostProcessing\PostProcessingApp\bin\$Configuration")
)

foreach ($childOut in $childOutputDirs) {
    if (-not (Test-Path $childOut)) {
        Write-Warning "Missing child output folder: $childOut"
        continue
    }

    Get-ChildItem -Path $childOut -File | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $stageApp $_.Name) -Force
    }
}

# Create runtime folders expected by the app.
$runtimeDirs = @(
    "IMU_sample\record",
    "camera_captures\record",
    "camera_captures\log",
    "camera_captures\preview_stream"
)
foreach ($rel in $runtimeDirs) {
    New-Item -ItemType Directory -Path (Join-Path $stageApp $rel) -Force | Out-Null
}

# Remove legacy folders that were replaced by new paths.
$legacyDirs = @(
    "logs",
    "camera_captures\logs"
)
foreach ($rel in $legacyDirs) {
    $legacyPath = Join-Path $stageApp $rel
    if (Test-Path $legacyPath) {
        Remove-Item $legacyPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Remove runtime-generated data files from camera_captures.
$cameraCapturesPath = Join-Path $stageApp "camera_captures"
if (Test-Path $cameraCapturesPath) {
    Get-ChildItem -Path $cameraCapturesPath -Recurse -File | Remove-Item -Force -ErrorAction SilentlyContinue
}

# Remove legacy app binary names after product rename to dsat.
$legacyFiles = @(
    "Wit.Example_BWT901BLE.exe",
    "Wit.Example_BWT901BLE.exe.config"
)
foreach ($name in $legacyFiles) {
    $legacyFile = Join-Path $stageApp $name
    if (Test-Path $legacyFile) {
        Remove-Item $legacyFile -Force -ErrorAction SilentlyContinue
    }
}

$legacyResource = Join-Path $stageApp "zh-CN\Wit.Example_BWT901BLE.resources.dll"
if (Test-Path $legacyResource) {
    Remove-Item $legacyResource -Force -ErrorAction SilentlyContinue
}

# Remove files that should not go to installer payload.
Get-ChildItem -Path $stageApp -Recurse -Include *.pdb,*.xml | Remove-Item -Force -ErrorAction SilentlyContinue

Write-Host "[3/4] Staging completed: $stageApp"

if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

if ($SkipIscc) {
    Write-Host "[4/4] Skip installer compile (--SkipIscc)."
    Write-Host "You can run ISCC manually with:"
    Write-Host ('ISCC.exe /DSourceDir="{0}" /DOutDir="{1}" "{2}"' -f $stageApp, $outDir, $issPath)
    exit 0
}

$iscc = Resolve-ISCC
if (-not $iscc) {
    Write-Warning "Inno Setup ISCC.exe not found. Staging is ready, but installer EXE was not generated."
    Write-Host "Install Inno Setup 6 and run:"
    Write-Host ('ISCC.exe /DSourceDir="{0}" /DOutDir="{1}" "{2}"' -f $stageApp, $outDir, $issPath)
    exit 0
}

Write-Host "[4/4] Building installer with Inno Setup..."
& $iscc "/DSourceDir=$stageApp" "/DOutDir=$outDir" $issPath
if ($LASTEXITCODE -ne 0) {
    throw "ISCC failed with exit code $LASTEXITCODE"
}

Write-Host "Installer generated in: $outDir"
