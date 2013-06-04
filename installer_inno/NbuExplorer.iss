#define MyAppName "NbuExplorer"
#define MyAppPublisher "Petr Vilem"
#define MyAppURL "http://sourceforge.net/projects/nbuexplorer"
#define MyAppExeName "NbuExplorer.exe"
#define LicenseFile "..\bin\Release\license.rtf"
#define DbShellDir "..\bin\Release\dbshell"

#dim Version[4]
#expr ParseVersion("..\bin\Release\" + MyAppExeName, Version[0], Version[1], Version[2], Version[3])
#define MyAppVersionFull Str(Version[0]) + "." + Str(Version[1]) + "." + Str(Version[2]) + "." + Str(Version[3])
#define MyAppVersion Str(Version[0]) + "." + Str(Version[1])
#define MyAppVersionFile Str(Version[0]) + "_" + Str(Version[1])

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6C58B3E8-0822-490B-BC94-40CC02A6B37F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile={#LicenseFile}
OutputDir=.\output
OutputBaseFilename={#MyAppName}_{#MyAppVersionFile}_Setup
Compression=lzma
SolidCompression=yes
AppCopyright={#MyAppPublisher}
VersionInfoVersion={#MyAppVersionFull}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
WizardImageFile=compiler:WizModernImage-IS.bmp
WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1
Name: "associateFiles"; Description: "Associate with Nokia backup files (*.nbu, *.nbf, *.nfb, *.nfc)"; Flags: unchecked

[Files]
Source: "..\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Components: main
Source: "..\bin\Release\changelog.txt"; DestDir: "{app}"; Flags: ignoreversion; Components: main
Source: "{#LicenseFile}"; DestDir: "{app}"; Flags: ignoreversion; Components: main
Source: "..\bin\Release\readme.txt"; DestDir: "{app}"; Flags: ignoreversion; Components: main
#if DirExists(DbShellDir)
Source: "{#DbShellDir}\*"; DestDir: "{app}\dbshell"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: dbshell
#endif

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Readme"; Filename: "{app}\readme.txt"
Name: "{group}\Donate"; Filename: "http://sourceforge.net/p/nbuexplorer/donate"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Components]
Name: "main"; Description: "Main application"; Types: compact full custom; Flags: fixed
Name: "dbshell"; Description: "DbShell"; Types: full custom

#define use_msi31
#define use_dotnetfx20

#include "scripts\products.iss"
#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"

#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#include "scripts\products\dotnetfx20sp1lp.iss"
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2

[Registry]
Root: "HKCR"; Subkey: ".nbu"; ValueType: string; ValueData: "NbuFile"; Flags: uninsdeletevalue; Tasks: associateFiles
Root: "HKCR"; Subkey: "NbuFile"; ValueType: string; ValueData: "Nokia backup"; Flags: uninsdeletekey; Tasks: associateFiles
Root: "HKCR"; Subkey: "NbuFile\DefaultIcon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associateFiles
Root: "HKCR"; Subkey: "NbuFile\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associateFiles

Root: "HKCR"; Subkey: ".nbf"; ValueType: string; ValueData: "NbfFile"; Flags: uninsdeletevalue; Tasks: associateFiles
Root: "HKCR"; Subkey: "NbfFile"; ValueType: string; ValueData: "Nokia backup"; Flags: uninsdeletekey; Tasks: associateFiles
Root: "HKCR"; Subkey: "NbfFile\DefaultIcon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associateFiles
Root: "HKCR"; Subkey: "NbfFile\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associateFiles

Root: "HKCR"; Subkey: ".nfb"; ValueType: string; ValueData: "NfbFile"; Flags: uninsdeletevalue; Tasks: associateFiles
Root: "HKCR"; Subkey: "NfbFile"; ValueType: string; ValueData: "Nokia backup"; Flags: uninsdeletekey; Tasks: associateFiles
Root: "HKCR"; Subkey: "NfbFile\DefaultIcon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associateFiles
Root: "HKCR"; Subkey: "NfbFile\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associateFiles

Root: "HKCR"; Subkey: ".nfc"; ValueType: string; ValueData: "NfcFile"; Flags: uninsdeletevalue; Tasks: associateFiles
Root: "HKCR"; Subkey: "NfcFile"; ValueType: string; ValueData: "Nokia backup"; Flags: uninsdeletekey; Tasks: associateFiles
Root: "HKCR"; Subkey: "NfcFile\DefaultIcon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associateFiles
Root: "HKCR"; Subkey: "NfcFile\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: associateFiles

[Code]
function InitializeSetup(): boolean;
begin
	//init windows version
	initwinversion();

#ifdef use_msi31
	msi31('3.1');
#endif

	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	//check if .netfx 2.0 can be installed on this OS
	if not minwinspversion(5, 0, 3) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['2000', '3'])]), mberror, mb_ok);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['XP', '2'])]), mberror, mb_ok);
		exit;
	end;

	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif

	Result := true;
end;

/////////////////////////////////////////////////////////////////////
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;


/////////////////////////////////////////////////////////////////////
function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;


/////////////////////////////////////////////////////////////////////
function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  // get the uninstall string of the old app
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

/////////////////////////////////////////////////////////////////////
procedure CurStepChanged(CurStep: TSetupStep);
var
  iResultCode: Integer;
begin
  if (CurStep=ssInstall) then
  begin
    Exec('MsiExec.exe', '/X{3B3CA39D-E0E8-41F1-A0F8-D7505306A715} /quiet','', SW_HIDE, ewWaitUntilTerminated, iResultCode);
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;
end;

/////////////////////////////////////////////////////////////////////
function ShouldSkipPage(PageID: Integer): Boolean;
begin
#if DirExists(DbShellDir)
#else
  if (PageID=wpSelectComponents) then Result := true;
#endif
end;
