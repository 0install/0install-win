;Version numbers
#define Version "1.7.1"

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
Name: de; MessagesFile: compiler:Languages\German.isl; LicenseFile: License_de.rtf
Name: en; MessagesFile: compiler:Default.isl; LicenseFile: License_en.rtf

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

[Dirs]
;ToDo: Suppress inherited permissions from {commonappdata}
;Name: {commonappdata}\0install.net; Flags: uninsneveruninstall; Permissions: admins-full

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

[Run]
;Pre-compile .NET executables and their dependencies
Filename: {dotnet20}\ngen.exe; Parameters: install ZeroInstall.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install 0install.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install 0install-win.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install 0launch.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install 0store.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install 0store-win.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install StoreService.exe /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}

;Pre-compile XML serialization assemblies (are not explicit dependencies, therefore need to be listed separately)
Filename: {dotnet20}\ngen.exe; Parameters: install ZeroInstall.Model.XmlSerializers.dll /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install ZeroInstall.Injector.XmlSerializers.dll /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}
Filename: {dotnet20}\ngen.exe; Parameters: install ZeroInstall.DesktopIntegration.XmlSerializers.dll /queue; WorkingDir: {app}; Flags: runhidden; StatusMsg: {cm:compile_netfx}

Filename: {app}\ZeroInstall.exe; Description: {cm:LaunchProgram,Zero Install}; Flags: nowait postinstall runasoriginaluser skipifsilent

[UninstallRun]
;Remove pre-compiled .NET files
Filename: {dotnet20}\ngen.exe; Parameters: uninstall ZeroInstall.exe; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall 0install.exe; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall 0install-win.exe; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall 0launch.exe; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall 0store.exe; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall 0store-win.exe; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall StoreService.exe; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall ZeroInstall.Model.XmlSerializers.dll; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall ZeroInstall.Injector.XmlSerializers.dll; WorkingDir: {app}; Flags: runhidden
Filename: {dotnet20}\ngen.exe; Parameters: uninstall ZeroInstall.DesktopIntegration.XmlSerializers.dll WorkingDir: {app}; Flags: runhidden

[UninstallDelete]
;Remove files added by post-installation updates
Name: {app}\de; Type: filesandordirs
Name: {app}\Solver; Type: filesandordirs
Name: {app}\GnuPG; Type: filesandordirs
Name: {app}\ZeroInstall.*; Type: files
Name: {app}\.manifest; Type: files
Name: {app}\.xbit; Type: files
Name: {app}\.symlink; Type: files
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

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
	appdir:			String;
	selectedTasks:	String;
begin
	if CurUninstallStep = usUninstall then begin
		// Remove Zero Install from PATH
		if LoadStringFromFile(ExpandConstant('{app}\uninsTasks.txt'), selectedTasks) then
			if Pos('modifypath', selectedTasks) > 0 then
				ModPath();
		DeleteFile(ExpandConstant('{app}\\uninsTasks.txt'))

		// Clean implementation cache
		if MsgBox(CustomMessage('DeleteCache'), mbConfirmation, MB_YESNO) = IDYES
		then begin
			DelTree(ExpandConstant('{localappdata}\0install.net'), true, true, true);
			DelTree(ExpandConstant('{commonappdata}\0install.net\implementations'), true, true, true);
		end;
	end;
end;
