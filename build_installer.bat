@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0installer\BuildInstaller.ps1" %*
endlocal
