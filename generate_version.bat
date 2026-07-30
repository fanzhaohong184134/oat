@echo off
set GH=
for /f "delims=" %%i in ('git rev-parse --short HEAD 2^>nul') do set GH=%%i
> "%~dp0GitVersion.cs" (
echo // Auto-generated during build - do not edit
echo namespace Wit.Example_BWT901BLE
echo {
echo     static class GitVersion
echo     {
echo         public const string CommitHash = "%GH%";
echo     }
echo }
)
