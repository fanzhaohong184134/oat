# Installer Packaging

This folder contains the installer packaging pipeline for `dsat`.

## What it does

- Builds the full solution in `Release`.
- Collects the main app + child executables + dependent libraries into a staging folder.
- Creates an installable EXE using Inno Setup.
- Installer supports custom install directory selection.
- Runtime-generated data (for example, `device_info` and sample records) is cleaned from staging before packaging.
- Installer payload includes runtime folders:
  - `IMU_sample/record`
  - `camera_captures/record`
  - `camera_captures/log`
  - `camera_captures/preview_stream`

## Files

- `BuildInstaller.ps1`: one-click build + package script.
- `Setup.iss`: Inno Setup script.

## Usage

Run from repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\BuildInstaller.ps1
```

Optional flags:

```powershell
# Only prepare staging files, skip installer EXE compile
powershell -ExecutionPolicy Bypass -File .\installer\BuildInstaller.ps1 -SkipIscc

# Skip project build, package from existing Release output
powershell -ExecutionPolicy Bypass -File .\installer\BuildInstaller.ps1 -SkipBuild
```

## Output

- Staging payload: `installer/_staging/app`
- Installer EXE: `installer/output`

## Requirement

- Inno Setup 6 (`ISCC.exe`) is required to compile installer EXE.
- If ISCC is missing, the script still prepares complete staging payload and prints manual compile command.
