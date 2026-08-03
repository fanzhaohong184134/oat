; Inno Setup script for dsat
; Build with: ISCC.exe /DSourceDir="..." /DOutDir="..." installer\Setup.iss

#define AppName "dsat"
#define AppVersion "1.0.0"
#define AppPublisher "Wit"
#define AppExeName "dsat.exe"

#ifndef SourceDir
  #error SourceDir is not defined. Pass /DSourceDir="absolute staging path"
#endif

#ifndef OutDir
  #define OutDir "."
#endif

[Setup]
AppId={{A1F4A3E9-6F5B-4B90-8F45-6E6A5132F4F5}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\Wit\{#AppName}
DisableDirPage=no
UsePreviousAppDir=no
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#OutDir}
OutputBaseFilename={#AppName}_Setup_{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Dirs]
Name: "{app}\IMU_sample\record"
Name: "{app}\camera_captures\record"
Name: "{app}\camera_captures\log"
Name: "{app}\camera_captures\preview_stream"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
