; Inno Setup 脚本 — 数字对中仪出厂校准工装(独立安装，和主应用/后处理程序解耦)
; 用 ISCC.exe 编译: ISCC.exe CalibrationBench.iss

#define AppName "数字对中仪出厂校准工装"
#define AppVer  "1.0.0"
#define Pub     "Calibration"

[Setup]
AppId={{7F3B2A10-9C4D-4E21-8B77-CAL1BRAT10N01}}
AppName={#AppName}
AppVersion={#AppVer}
AppPublisher={#Pub}
DefaultDirName={autopf}\CalibrationBench
DefaultGroupName=CalibrationBench
DisableProgramGroupPage=yes
OutputBaseFilename=CalibrationBench_Setup_{#AppVer}
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern

[Languages]
Name: "chs"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Files]
Source: "dist\CalibrationBench\CalibrationBench.UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\CalibrationBench\CalibrationEngine.exe";   DestDir: "{app}"; Flags: ignoreversion
Source: "dist\CalibrationBench\samples\*";               DestDir: "{app}\samples"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "dist\CalibrationBench\README.md";               DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
Name: "{group}\出厂校准工装"; Filename: "{app}\CalibrationBench.UI.exe"
Name: "{autodesktop}\出厂校准工装"; Filename: "{app}\CalibrationBench.UI.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加任务:"

[Run]
Filename: "{app}\CalibrationBench.UI.exe"; Description: "立即启动"; Flags: nowait postinstall skipifsilent
