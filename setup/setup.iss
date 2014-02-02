;Set version number via command-line argument "/dVersion=X.Y"
#ifndef Version
  #define Version "0.1"
#endif

#include "scripts\fileversion.iss"
#include "scripts\winversion.iss"
#include "scripts\products.iss"
#include "scripts\products\kb835732.iss"
#include "scripts\products\msi20.iss"
#include "scripts\products\msi31.iss"
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

en.DesktopIcon=Create desktop icon
de.DesktopIcon=Desktopsymbol erstellen
en.StoreService=Install Store service (share app files between users)
de.StoreService=Store Dienst installieren (Anwendungsdateien zwischen Benutzern teilen)

[Setup]
#ifdef PerUser
  PrivilegesRequired=lowest
  OutputBaseFilename=zero-install-per-user
  AppName=Zero Install (per-user)
  AppID=Zero Install (per-user)
  UninstallDisplayName=Zero Install (per-user)
  ;DefaultDirName={userpf}\Zero Install
  DefaultDirName={userappdata}\Programs\Zero Install
#else
  PrivilegesRequired=admin
  OutputBaseFilename=zero-install
  AppName=Zero Install
  AppID=Zero Install
  UninstallDisplayName=Zero Install
  DefaultDirName={pf}\Zero Install
#endif
OutputDir=..\build\Setup

ShowLanguageDialog=auto
MinVersion=0,5.0
AppVersion={#Version}
AppVerName=Zero Install for Windows v{#Version}
AppCopyright=Copyright 2010-2013 Bastian Eicher et al
AppPublisher=0install.de
AppPublisherURL=http://0install.de/
AppMutex=Zero Install
VersionInfoDescription=Zero Install Setup
VersionInfoTextVersion=Zero Install for Windows v{#Version} Setup
VersionInfoVersion={#Version}
VersionInfoCompany=0install.de
DefaultGroupName=Zero Install
DisableWelcomePage=true
DisableProgramGroupPage=true
ArchitecturesInstallIn64BitMode=x64 ia64
ChangesEnvironment=yes
UninstallDisplayIcon={app}\ZeroInstall.exe
SetupIconFile=Setup.ico
WizardImageFile=WizModernImage.bmp
WizardSmallImageFile=WizModernSmallImage.bmp
Compression=lzma/ultra
SolidCompression=true

[Languages]
Name: de; MessagesFile: compiler:Languages\German.isl
Name: en; MessagesFile: compiler:Default.isl

[Files]
Source: ..\COPYING.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\3rd party code.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\build\Frontend\Release\*; Excludes: *.log,*.pdb,*.mdb,*.vshost.exe,Test.*,nunit.*,*.xml; DestDir: {app}; Flags: ignoreversion recursesubdirs
Source: ..\bundled\GnuPG\*; DestDir: {app}\GnuPG; Flags: ignoreversion recursesubdirs
Source: ..\bundled\Solver\*; DestDir: {app}\Solver; Flags: ignoreversion recursesubdirs

[InstallDelete]
;Deletes obsolete files
Name: {app}\C5.*; Type: files
Name: {app}\ZeroInstall.Backend.*; Type: files  
Name: {app}\de\ZeroInstall.Backend.dll; Type: files  
Name: {app}\ZeroInstall.Fetchers.*; Type: files
Name: {app}\de\ZeroInstall.Fetchers.dll; Type: files  
Name: {app}\ZeroInstall.Solvers.*; Type: files
Name: {app}\de\ZeroInstall.Solvers.dll; Type: files  
Name: {app}\ZeroInstall.Injector.*; Type: files
Name: {app}\de\ZeroInstall.Injector.dll; Type: files  
Name: {app}\ZeroInstall.Model.*; Type: files
Name: {app}\de\ZeroInstall.Model.dll; Type: files

[Registry]
#ifdef PerUser
  Root: HKCU32; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty
  Root: HKCU64; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty; Check: IsWin64
#else
  Root: HKLM32; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty
  Root: HKLM64; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty; Check: IsWin64
#endif

[Tasks]
Name: desktopicon; Description: {cm:DesktopIcon}
#ifndef PerUser
  Name: storeservice; Description: {cm:StoreService}
#endif

[Icons]
Name: {group}\Zero Install; Filename: {app}\ZeroInstall.exe
Name: {commondesktop}\Zero Install; Filename: {app}\ZeroInstall.exe; Tasks: desktopicon

[Run]
#ifndef PerUser
  Filename: {app}\0store-service.exe; Parameters: install --silent; Tasks: storeservice
  Filename: {app}\0store-service.exe; Parameters: start --silent
#endif
Filename: {app}\ZeroInstall.exe; Description: {cm:LaunchProgram,Zero Install}; Flags: nowait postinstall runasoriginaluser skipifsilent

[UninstallRun]
Filename: {app}\0install-win.exe; Parameters: remove-all; RunOnceId: RemoveAllApps
Filename: {app}\0install-win.exe; Parameters: store purge; RunOnceId: PurgeCache
#ifndef PerUser
  Filename: {app}\0store-service.exe; Parameters: uninstall --silent; RunOnceId: UninstallService
#endif

[UninstallDelete]
;Remove files added by post-installation updates
Name: {app}\de; Type: filesandordirs
Name: {app}\Solver; Type: filesandordirs
Name: {app}\GnuPG; Type: filesandordirs
Name: {app}\ZeroInstall.*; Type: files
Name: {app}\.manifest; Type: files
Name: {app}\.xbit; Type: files
Name: {app}\.symlink; Type: files
Name: {app}\*.InstallLog; Type: files
Name: {app}\*.pdb; Type: files
Name: {app}; Type: dirifempty

[Code]
function InitializeSetup(): Boolean;
begin
	// Determine the exact Windows version, including Service pack
	initwinversion();

	// Check if .NET 2.0 can be installed on this OS
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

procedure CurStepChanged(CurStep: TSetupStep);
var
	ResultCode: Integer;
begin
	if CurStep = ssInstall then begin
		// Stop the Zero Install Store Service if it is running
		Exec(ExpandConstant('{app}\0store-service.exe'), 'stop --silent', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
	end else if CurStep = ssPostInstall then begin
		// Add Zero Install to PATH
		ModPath();
	end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
	if CurUninstallStep = usUninstall then begin
		// Remove Zero Install from PATH
		ModPath();
	end;
end;
