;Set version number via command-line argument "/dVersion=X.Y"
#ifndef Version
  #define Version "0.1"
#endif

;Automatic dependency download and installation
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
;Used by downloader
appname=Zero Install
win2000sp4_title=Windows 2000 Service Pack 4
winxpsp2_title=Windows XP Service Pack 2

en.compile_netfx=Pre-compile .NET assemblies for faster application startup
en.DesktopIcon=Create desktop icon
en.StoreService=Install Store service (share app files between users)

de.DesktopIcon=Desktopsymbol erstellen
de.compile_netfx=.NET Assemblies zum schnelleren Anwendugsstart vorkompilieren
de.StoreService=Store Dienst installieren (Anwendungsdateien zwischen Benutzern teilen)

[Setup]
#ifdef PerUser
  PrivilegesRequired=lowest
  OutputBaseFilename=zero-install-per-user
  AppName=Zero Install (per-user)
  AppID=Zero Install (per-user)
  DefaultDirName={userappdata}\Programs\Zero Install
  UninstallDisplayName=Zero Install (per-user)
#else
  PrivilegesRequired=admin
  OutputBaseFilename=zero-install
  AppName=Zero Install
  AppID=Zero Install
  DefaultDirName={pf}\Zero Install
  UninstallDisplayName=Zero Install
#endif
OutputDir=..\build\Installer

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
ArchitecturesInstallIn64BitMode=x64 ia64
ChangesEnvironment=yes
SetupIconFile=Setup.ico
UninstallDisplayIcon={app}\ZeroInstall.exe
WizardImageFile=WizModernImage.bmp
WizardSmallImageFile=WizModernSmallImage.bmp
DisableWelcomePage=true
DisableProgramGroupPage=true
Compression=lzma/ultra
SolidCompression=true

[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: de; MessagesFile: compiler:Languages\German.isl

[Files]
Source: ..\COPYING.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\3rd party code.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\build\Release\Frontend\*; Excludes: *.log,*.pdb,*.mdb,*.vshost.exe,Test.*,nunit.*,*.xml; DestDir: {app}; Flags: ignoreversion
Source: ..\build\Release\Frontend\de\*; DestDir: {app}\de; Flags: ignoreversion
Source: ..\bundled\GnuPG\*; DestDir: {app}\GnuPG; Flags: ignoreversion recursesubdirs
Source: ..\bundled\Solver\*; DestDir: {app}\Solver; Flags: ignoreversion recursesubdirs

[InstallDelete]
;Deletes obsolete files
Name: {app}\C5.*; Type: files
Name: {app}\Common.dll; Type: files
Name: {app}\de\Common.resources.dll; Type: files
Name: {app}\Common.WinForms.dll; Type: files
Name: {app}\de\Common.WinForms.resources.dll; Type: files
Name: {app}\ZeroInstall.Backend.*; Type: files  
Name: {app}\de\ZeroInstall.Backend.resources.dll; Type: files  
Name: {app}\ZeroInstall.Fetchers.*; Type: files
Name: {app}\de\ZeroInstall.Fetchers.resources.dll; Type: files  
Name: {app}\ZeroInstall.Solvers.*; Type: files
Name: {app}\de\ZeroInstall.Solvers.resources.dll; Type: files  
Name: {app}\ZeroInstall.Injector.*; Type: files
Name: {app}\de\ZeroInstall.Injector.resources.dll; Type: files  
Name: {app}\ZeroInstall.Model.*; Type: files
Name: {app}\de\ZeroInstall.Model.resources.dll; Type: files

[Registry]
#ifdef PerUser
  Root: HKCU; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty
  Root: HKCU; Subkey: Software\Microsoft\PackageManagement; ValueType: string; ValueName: ZeroInstall; ValueData: {app}\ZeroInstall.OneGet.dll; Flags: uninsdeletevalue uninsdeletekeyifempty
#else
  Root: HKLM32; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty
  Root: HKLM64; Subkey: Software\Zero Install; ValueType: string; ValueName: InstallLocation; ValueData: {app}; Flags: uninsdeletevalue uninsdeletekeyifempty; Check: IsWin64
  Root: HKLM32; Subkey: Software\Microsoft\PackageManagement; ValueType: string; ValueName: ZeroInstall; ValueData: {app}\ZeroInstall.OneGet.dll; Flags: uninsdeletevalue uninsdeletekeyifempty
  Root: HKLM64; Subkey: Software\Microsoft\PackageManagement; ValueType: string; ValueName: ZeroInstall; ValueData: {app}\ZeroInstall.OneGet.dll; Flags: uninsdeletevalue uninsdeletekeyifempty; Check: IsWin64
#endif

[Tasks]
Name: desktopicon; Description: {cm:DesktopIcon}
#ifndef PerUser
  Name: ngen; Description: {cm:compile_netfx}
  Name: storeservice; Description: {cm:StoreService}
#endif

[Icons]
Name: {group}\Zero Install; Filename: {app}\ZeroInstall.exe
Name: {commondesktop}\Zero Install; Filename: {app}\ZeroInstall.exe; Tasks: desktopicon

[Run]
#ifndef PerUser
  ;Note: Can't use {dotnet40} because that shows an error message if we only have .NET 2.0 installed
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install ZeroInstall.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install 0install.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install 0install-win.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install 0launch.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install 0alias.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install 0store.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install 0store-service.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install ZeroInstall.Store.XmlSerializers.dll; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install ZeroInstall.DesktopIntegration.XmlSerializers.dll; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: install ZeroInstall.OneGet.dll; WorkingDir: {app}; Flags: runhidden skipifdoesntexist; Tasks: ngen; StatusMsg: {cm:compile_netfx}

  Filename: {app}\0store-service.exe; Parameters: install --silent; Tasks: storeservice
  Filename: {app}\0store-service.exe; Parameters: start --silent; Tasks: storeservice
#endif
Filename: {app}\ZeroInstall.exe; Description: {cm:LaunchProgram,Zero Install}; Flags: nowait postinstall runasoriginaluser skipifsilent

[UninstallRun]
Filename: {app}\0install-win.exe; Parameters: remove-all; RunOnceId: RemoveAllApps
Filename: {app}\0install-win.exe; Parameters: store purge; RunOnceId: PurgeCache
#ifndef PerUser
  Filename: {app}\0store-service.exe; Parameters: uninstall --silent; RunOnceId: UninstallService

  ;Note: Can't use {dotnet40} because that shows an error message if we only have .NET 2.0 installed
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall ZeroInstall.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall 0install.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall 0install-win.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall 0launch.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall 0alias.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall 0store.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall 0store-service.exe; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall ZeroInstall.Store.XmlSerializers.dll; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall ZeroInstall.DesktopIntegration.XmlSerializers.dll; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
  Filename: "{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot}\v4.0.30319\ngen.exe"; Parameters: uninstall ZeroInstall.OneGet.dll; WorkingDir: {app}; Flags: runhidden skipifdoesntexist
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
