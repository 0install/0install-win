;Version numbers
#define Version "1.5"

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

#include "scripts\modpath.iss"

[CustomMessages]
win2000sp4_title=Windows 2000 Service Pack 4
winxpsp2_title=Windows XP Service Pack 2
en.compile_netfx=Pre-compiling .NET assemblies for faster application startup...
de.compile_netfx=.NET Assemblies zum schnelleren Anwendugsstart vorkompilieren...

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
OutputBaseFilename=zero-install

;General settings
ShowLanguageDialog=auto
MinVersion=0,5.0
DefaultDirName={pf}\Zero Install
AppName=Zero Install
AppVersion={#Version}
AppVerName=Zero Install for Windows v{#Version}
AppCopyright=Copyright 2010-2012 0install.de
AppPublisher=0install.de
AppPublisherURL=http://0install.de/
AppID=Zero Install
AppMutex=Zero Install
VersionInfoDescription=Zero Install Setup
VersionInfoTextVersion=Zero Install for Windows v{#Version} Setup
VersionInfoVersion={#Version}
VersionInfoCompany=0install.de
DefaultGroupName=Zero Install
DisableProgramGroupPage=true
ArchitecturesInstallIn64BitMode=x64 ia64
PrivilegesRequired=admin
ChangesAssociations=true
ChangesEnvironment=yes
UninstallDisplayIcon={app}\ZeroInstall.exe
UninstallDisplayName=Zero Install
SetupIconFile=Setup.ico
WizardImageFile=WizModernImage.bmp
WizardSmallImageFile=WizModernSmallImage.bmp
Compression=lzma/ultra
SolidCompression=true

[Languages]
#ifdef Update
  Name: de; MessagesFile: Update_de.isl; InfoBeforeFile: Update_de.rtf
  Name: en; MessagesFile: Update_en.isl; InfoBeforeFile: Update_en.rtf
#else
  Name: de; MessagesFile: compiler:Languages\German.isl; LicenseFile: License_de.rtf
  Name: en; MessagesFile: compiler:Default.isl; LicenseFile: License_en.rtf
#endif

[InstallDelete]
;Remove NanoGrid shortcuts
Name: {commondesktop}\Zero Install.url; Type: files
Name: {commonprograms}\Zero Install\Zero Install.url; Type: files

;Remove obsolete files from previous versions
Name: {app}\Python; Type: filesandordirs
Name: {app}\*.xml; Type: files
Name: {app}\ZeroInstall.MyApps.dll; Type: files
Name: {app}\de\ZeroInstall.MyApps.resources.dll; Type: files

[Files]
Source: ..\lgpl.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\3rd party code.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\build\Frontend\Release\*; Excludes: *.log,*.pdb,*.mdb,*.vshost.exe,Test.*,nunit.*,*.xml; DestDir: {app}; Flags: ignoreversion recursesubdirs
Source: ..\bundled\*; DestDir: {app}; Flags: ignoreversion recursesubdirs

[Registry]
Root: HKLM32; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKLM64; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty; Check: IsWin64

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}
Name: modifypath; Description: {cm:AddToPath}
[Icons]
Name: {group}\{cm:UninstallProgram,Zero Install}; Filename: {uninstallexe}
Name: {group}\Website; Filename: http://0install.de/
Name: {group}\Zero Install; Filename: {app}\ZeroInstall.exe
Name: {group}\{cm:CacheManagement}; Filename: {app}\0store-win.exe; IconFilename: {app}\0store-win.exe
Name: {commondesktop}\Zero Install; Filename: {app}\ZeroInstall.exe; Tasks: desktopicon

;Post-installations tasks
[Run]
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: install ZeroInstall.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: install 0install.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: install 0install-win.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: install 0launch.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: install 0store.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: install 0store-win.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: install StoreService.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {app}\ZeroInstall.exe; Description: {cm:LaunchProgram,Zero Install}; Flags: nowait postinstall runasoriginaluser skipifsilent

[UninstallDelete]
;Remove files added by post-installation updates
Name: {app}\de; Type: filesandordirs
Name: {app}\Solver; Type: filesandordirs
Name: {app}\GnuPG; Type: filesandordirs
Name: {app}\ZeroInstall.*; Type: files
Name: {app}\.manifest; Type: files
Name: {app}; Type: dirifempty

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
			//if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
			//	kb835732();
			//	dotnetfx20sp1();
			//end else begin
				dotnetfx20();
			//end;
		end;
	end;

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
