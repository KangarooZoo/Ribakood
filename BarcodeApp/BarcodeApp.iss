; Inno Setup Script for BarcodeApp
; Creates a Windows installer with self-contained .NET 8.0 runtime

#define AppName "BarcodeApp"
#define AppVersion "1.0.0"
#define AppPublisher "Jan Alar"
#define AppURL ""
#define AppExeName "BarcodeApp.exe"
#define PublishDir "publish"

[Setup]
; App identification
AppId={{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}

; Installation settings
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir=installer
OutputBaseFilename=BarcodeApp-Setup

; Compression and architecture
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible

; UI settings
WizardStyle=modern
PrivilegesRequired=lowest

; Optional: Add license or info files
; LicenseFile=LICENSE.txt
; InfoBeforeFile=README.txt
; SetupIconFile=app-icon.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Include all published files (self-contained with .NET runtime)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

