@echo off
setlocal EnableDelayedExpansion
REM ============================================================
REM build_camera.bat - 编译 demo_jpeg_app.exe 和 demo_h264_app.exe
REM 由 C# 项目的 PreBuildEvent 自动调用
REM 所有情况都返回 exit /b 0，不阻塞C#编译
REM 注意: 路径可能含括号，所有path检查用goto代替if块
REM ============================================================

set "SCRIPT_DIR=%~dp0"
set "SRC_DIR=%SCRIPT_DIR%src"
set "INCLUDE_DIR=%SCRIPT_DIR%include"
set "LIB_DIR=%SCRIPT_DIR%libs\x64\Release"
set "OUTPUT_DIR=%SCRIPT_DIR%dlls\x64\Release"
set "CL_FLAGS=/nologo /W3 /O2 /EHsc /D_CRT_SECURE_NO_WARNINGS"
set "LINK_LIBS=libsvplayer.lib websockets.lib libcurl.lib ws2_32.lib wldap32.lib advapi32.lib crypt32.lib user32.lib"

REM 检查源文件
if exist "%SRC_DIR%\demo_jpeg_app.c" goto :src_ok
echo [build_camera] src\demo_jpeg_app.c not found, skip.
exit /b 0
:src_ok

REM 检查是否需要编译（两个exe都存在则跳过）
set "NEED_BUILD=0"
if not exist "%OUTPUT_DIR%\demo_jpeg_app.exe" set "NEED_BUILD=1"
if not exist "%OUTPUT_DIR%\demo_h264_app.exe" set "NEED_BUILD=1"
if "%NEED_BUILD%"=="0" (
    echo [build_camera] Both exe exist, skip.
    exit /b 0
)

REM 检查cl.exe，如果不在PATH中则尝试通过vswhere自动加载VS环境
where cl.exe >nul 2>&1
if not errorlevel 1 goto :cl_ok
echo [build_camera] cl.exe not in PATH, trying to locate Visual Studio...

set "VSWHERE=C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
if exist "!VSWHERE!" goto :vswhere_ok
echo [build_camera] vswhere.exe not found, skip C compilation.
exit /b 0
:vswhere_ok

for /f "usebackq tokens=*" %%i in (`"!VSWHERE!" -latest -property installationPath`) do set "VS_PATH=%%i"
if defined VS_PATH goto :vs_found
echo [build_camera] Visual Studio not found, skip C compilation.
exit /b 0
:vs_found

set "VCVARSALL=!VS_PATH!\VC\Auxiliary\Build\vcvarsall.bat"
if exist "!VCVARSALL!" goto :vcvars_ok
echo [build_camera] vcvarsall.bat not found at !VCVARSALL!, skip.
exit /b 0
:vcvars_ok

echo [build_camera] Loading VS environment from: !VCVARSALL!
call "!VCVARSALL!" x64 >nul 2>&1

where cl.exe >nul 2>&1
if not errorlevel 1 goto :cl_ok
echo [build_camera] cl.exe still not available after loading VS env, skip.
exit /b 0
:cl_ok

REM 检查链接库目录
if exist "%LIB_DIR%\" goto :lib_ok
echo [build_camera] Library directory not found: %LIB_DIR%
echo [build_camera] Please place libsvplayer.lib, websockets.lib, libcurl.lib in this folder.
exit /b 0
:lib_ok

REM 检查必需的库文件
if exist "%LIB_DIR%\libsvplayer.lib" goto :libsv_ok
echo [build_camera] libsvplayer.lib not found in %LIB_DIR%, skip.
exit /b 0
:libsv_ok

REM 创建输出目录
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

REM 编译 demo_jpeg_app.exe
if exist "%OUTPUT_DIR%\demo_jpeg_app.exe" goto :skip_jpeg
echo [build_camera] Compiling demo_jpeg_app.exe ...
cl.exe %CL_FLAGS% /I"%INCLUDE_DIR%" "%SRC_DIR%\demo_jpeg_app.c" /link /LIBPATH:"%LIB_DIR%" %LINK_LIBS% /OUT:"%OUTPUT_DIR%\demo_jpeg_app.exe"
if errorlevel 1 (
    echo [build_camera] demo_jpeg_app.exe compile failed.
) else (
    echo [build_camera] OK: demo_jpeg_app.exe built.
)
:skip_jpeg

REM 编译 demo_h264_app.exe
if exist "%OUTPUT_DIR%\demo_h264_app.exe" goto :skip_h264
if not exist "%SRC_DIR%\demo_h264_app.c" goto :skip_h264
echo [build_camera] Compiling demo_h264_app.exe ...
cl.exe %CL_FLAGS% /I"%INCLUDE_DIR%" "%SRC_DIR%\demo_h264_app.c" /link /LIBPATH:"%LIB_DIR%" %LINK_LIBS% /OUT:"%OUTPUT_DIR%\demo_h264_app.exe"
if errorlevel 1 (
    echo [build_camera] demo_h264_app.exe compile failed.
) else (
    echo [build_camera] OK: demo_h264_app.exe built.
)
:skip_h264

REM 清理obj
if exist "%SRC_DIR%\demo_jpeg_app.obj" del /q "%SRC_DIR%\demo_jpeg_app.obj"
if exist "%SRC_DIR%\demo_h264_app.obj" del /q "%SRC_DIR%\demo_h264_app.obj"

exit /b 0
