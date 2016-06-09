; Script generated for the MonoGame Ruge Project Deploy Tool
; Don't run this script in Inno Setup; use the Ruge Deploy Tool

#define MyAppName "Ruge Deploy Test"
#define MyAppVersion "1.0"
#define MyAppPublisher "MetaSmug"
#define MyAppExeName "Ruge.DeployTest.exe"
#define MyReleaseDir "C:\Projects\Ruge\Deploy Tool\Example\Source\Ruge.DeployTest\bin\DesktopGL\x86\Release"
#define MyDeployDir "C:\Projects\Ruge\Deploy Tool\Example\Deploy"
#define MyNamespace "Ruge.DeployTest"
#define MyGuid "22f5c872-5382-45f4-8f6a-c4c7c9327b50"
#define MyIcon "C:\Projects\Ruge\Deploy Tool\Example\Source\icon.ico"

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

