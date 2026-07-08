@echo off
REM ============================================================
REM build_camera.bat - 编译 demo_jpeg_app.exe
REM 由 C# 项目的 PreBuildEvent 自动调用
REM 所有情况都返回 exit /b 0，不阻塞C#编译
REM ============================================================

set SCRIPT_DIR=%~dp0
set SRC_DIR=%SCRIPT_DIR%src
set INCLUDE_DIR=%SCRIPT_DIR%include
set LIB_DIR=%SCRIPT_DIR%libs\x64\Release
set OUTPUT_DIR=%SCRIPT_DIR%dlls\x64\Release

REM 检查源文件
if not exist "%SRC_DIR%\demo_jpeg_app.c" (
    echo [build_camera] src\demo_jpeg_app.c not found, skip.
    exit /b 0
)

REM exe已存在则跳过
if exist "%OUTPUT_DIR%\demo_jpeg_app.exe" (
    echo [build_camera] demo_jpeg_app.exe exists, skip.
    exit /b 0
)

REM 检查cl.exe
where cl.exe >nul 2>&1
if errorlevel 1 (
    echo [build_camera] cl.exe not found, skip C compilation.
    exit /b 0
)

echo [build_camera] Compiling demo_jpeg_app.exe ...

cl.exe /nologo /W3 /O2 /EHsc /I"%INCLUDE_DIR%" "%SRC_DIR%\demo_jpeg_app.c" /link /LIBPATH:"%LIB_DIR%" libsvplayer.lib websockets.lib libcurl.lib ws2_32.lib wldap32.lib advapi32.lib crypt32.lib user32.lib /OUT:"%OUTPUT_DIR%\demo_jpeg_app.exe"

if errorlevel 1 (
    echo [build_camera] Compile failed, skip.
    exit /b 0
)

REM 清理obj
if exist "%OUTPUT_DIR%\demo_jpeg_app.obj" del /q "%OUTPUT_DIR%\demo_jpeg_app.obj"

echo [build_camera] OK: demo_jpeg_app.exe built.
exit /b 0
