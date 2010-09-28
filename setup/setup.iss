;Version numbers
#define Maj 0
#define Min 50
#define Rev 0

#ifndef Update
;Automatic dependency download and installation
#include "scripts\fileversion.iss"
#include "scripts\winversion.iss"
#include "scripts\products.iss"
#include "scripts\products\kb835732.iss"
#include "scripts\products\msi20.iss"
#include "scripts\products\msi31.iss"
#include "scripts\products\vcredist.iss"
#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#include "scripts\products\nanogrid.iss"
#endif

#include "scripts\modpath.iss"

[CustomMessages]
win2000sp4_title=Windows 2000 Service Pack 4
winxpsp2_title=Windows XP Service Pack 2

;Used by downloader
appname=Zero Install

en.AddToPath=Add to System &PATH (recommended)
de.AddToPath=Zum System &PATH hinzufügen (empfohlen)
en.CacheManagement=Cache management
de.CacheManagement=Cache Verwaltung
en.DeleteCache=Do you want to delete the Zero Install cache (installed applications)? These files can be re-downloaded again.
de.DeleteCache=Möchten Sie den Zero Install Cache (installierte Anwendungen) löschen? Diese Dateien können erneut heruntergeladen werden.

[Setup]
OutputDir=..\build\Setup
#ifndef Update
OutputBaseFilename=0install
#endif
#ifdef Update
OutputBaseFilename=0install_upd
#endif

;General settings
ShowLanguageDialog=auto
MinVersion=0,5.0
DefaultDirName={pf}\Zero Install
AppName=Zero Install
AppVerName=Zero Install for Windows v{#Maj}.{#Min}.{#Rev}
AppCopyright=Copyright 2010 0install.de
AppID=Zero Install
DefaultGroupName=Zero Install
AppPublisher=0install.de
AppVersion={#Maj}.{#Min}.{#Rev}
DisableProgramGroupPage=true
PrivilegesRequired=admin
ChangesAssociations=true
UninstallDisplayIcon={app}\ZeroInstall.exe
UninstallDisplayName=Zero Install
VersionInfoVersion={#Maj}.{#Min}.{#Rev}
VersionInfoCompany=0install.net
VersionInfoDescription=Zero Install
VersionInfoTextVersion=Zero Install {#Maj}.{#Min}.{#Rev}
AppPublisherURL=http://0install.net/
SetupIconFile=Setup.ico
WizardImageFile=compiler:WizModernImage-IS.bmp
WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp
Compression=lzma/ultra
SolidCompression=true
ChangesEnvironment=yes

[Languages]
#ifdef Update
  Name: de; MessagesFile: Update_de.isl; InfoBeforeFile: Update_de.rtf
  Name: en; MessagesFile: Update_en.isl; InfoBeforeFile: Update_en.rtf
#else
  Name: de; MessagesFile: compiler:Languages\German.isl; LicenseFile: License_de.rtf
  Name: en; MessagesFile: compiler:Default.isl; LicenseFile: License_en.rtf
#endif

[InstallDelete]
Name: {app}\Zero Install.exe; Type: files

[Files]
Source: ..\3rd party code.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\build\Frontend\Release\*; Excludes: *.log,*.pdb,*.vshost.exe,*.xml,Test.*,nunit.*,Mono.*; DestDir: {app}; Flags: ignoreversion recursesubdirs
#ifndef Update
;Distutils is required to install the Script into the portable Python distribution but is not needed on the end-user machine
Source: ..\build\Portable\*; Excludes: Python\Lib\distutils; DestDir: {app}; Flags: ignoreversion recursesubdirs
#endif
#ifdef Update
;Only update the Zero Install scripts and not the rest of Python
Source: ..\build\Portable\Python\Scripts\*; DestDir: {app}\Python\Scripts; Flags: ignoreversion recursesubdirs
Source: ..\build\Portable\Python\Lib\site-packages\zeroinstall\*; DestDir: {app}\Python\Lib\site-packages\zeroinstall; Flags: ignoreversion recursesubdirs
#endif

[Registry]
;These entries are required by the NanoGrid auto-update tool
Root: HKLM; Subkey: Software\Zero Install; Flags: deletekey
Root: HKLM; Subkey: Software\NanoByte\Zero Install; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; Flags: uninsdeletekey; Permissions: authusers-modify
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; ValueType: string; ValueName: Path; ValueData: {app}
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; ValueType: string; ValueName: Position; ValueData: {app}\ZeroInstall.exe
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; ValueType: string; ValueName: EditorPosition; ValueData: {app}\0publish-gui.exe
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; ValueType: string; ValueName: Uninstall; ValueData: {uninstallexe}
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; ValueType: string; ValueName: Major; ValueData: {#Maj}
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; ValueType: string; ValueName: Minor; ValueData: {#Min}
Root: HKLM; Subkey: Software\NanoByte\Zero Install\Info; ValueType: string; ValueName: Revision; ValueData: {#Rev}

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}
Name: modifypath; Description: {cm:AddToPath}
[Icons]
Name: {group}\{cm:UninstallProgram,Zero Install}; Filename: {uninstallexe}
Name: {group}\Website; Filename: http://0install.de/
Name: {group}\Zero Install; Filename: nanogrid:/launch/ZeroInstall /autoClose /anonLogin; IconFilename: {app}\ZeroInstall.exe
;Name: {group}\Feed Editor; Filename: nanogrid:/launch/ZeroInstall:/editor /autoClose /anonLogin; IconFilename: {app}\0publish-gui.exe
Name: {group}\{cm:CacheManagement}; Filename: {app}\0storew.exe; IconFilename: {app}\0storew.exe
Name: {commondesktop}\Zero Install; Filename: nanogrid:/launch/ZeroInstall /autoClose /anonLogin; IconFilename: {app}\ZeroInstall.exe; Tasks: desktopicon

;Post-installations tasks
[Run]
Filename: {app}\ZeroInstall.exe; Description: {cm:LaunchProgram,Zero Install}; Flags: nowait postinstall runasoriginaluser skipifsilent

[Code]
#ifndef Update
function InitializeSetup(): Boolean;
begin
	// Determine the exact Windows version, including Service pack
	initwinversion();

	// Check if vcredist and .NET 2.0 can be installed on this OS
	if not minwinspversion(5, 0, 4) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp4_title')]), mbError, MB_OK);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp2_title')]), mbError, MB_OK);
		exit;
	end;

  // Add all required products to the list
	msi20('2.0');
	msi31('3.0');
	vcredist();	
	if minwinversion(6, 0) then begin
		//starting with Windows Vista, .netfx 3.0 or newer is included
	end else begin
		if minwinversion(5, 1) then begin
			dotnetfx20sp2();
		end else begin
			if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
				kb835732();
				dotnetfx20sp1();
			end else begin
				dotnetfx20();
			end;
		end;
	end;
	nanogrid();

	Result := true;
end;
#endif

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
	appdir:			String;
	selectedTasks:	String;
begin
	appdir := ExpandConstant('{app}')
	if CurUninstallStep = usUninstall then begin
		if LoadStringFromFile(appdir + '\uninsTasks.txt', selectedTasks) then
			if Pos('modifypath', selectedTasks) > 0 then
				ModPath();
		DeleteFile(appdir + '\uninsTasks.txt')

		if MsgBox(CustomMessage('DeleteCache'), mbConfirmation, MB_YESNO) = IDYES
		then begin
			DelTree(GetShellFolder(False, sfLocalAppData) + '\0install.net', true, true, true);
		end;
	end;
end;
