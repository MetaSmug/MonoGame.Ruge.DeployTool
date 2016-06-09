; Script generated for the MonoGame Ruge Project Deploy Tool
; Don't run this script in Inno Setup; use the Ruge Deploy Tool

#define MyAppName "{{displayName}}"
#define MyAppVersion "{{version}}"
#define MyAppPublisher "{{publisher}}"
#define MyAppExeName "{{namespace}}.exe"
#define MyReleaseDir "{{releaseDir}}"
#define MyDeployDir "{{deployDir}}"
#define MyNamespace "{{namespace}}"
#define MyGuid "{{installGuid}}"
#define MyIcon "{{iconFile}}"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{{#MyGuid}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
;LicenseFile={#MyReleaseDir}\eula.txt
;InfoBeforeFile={#MyReleaseDir}\changelog.txt
OutputDir={#MyDeployDir} 
OutputBaseFilename={#MyNamespace}.{#MyAppVersion}.Windows.Setup
Compression=lzma
SolidCompression=yes
SetupIconFile={#MyIcon}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyReleaseDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

