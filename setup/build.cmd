@echo off
::Creates archives and and Inno Setup installer. Assumes "..\src\build.cmd Release" and "..\bundled\download-solver.ps1" have already been executed.
::Use command-line argument "+update" to additionally create updater archive.
if "%1"=="+updater" set UPDATER=TRUE
if "%2"=="+updater" set UPDATER=TRUE
if "%3"=="+updater" set UPDATER=TRUE
if "%4"=="+updater" set UPDATER=TRUE

rem Project settings
set TargetDir=%~dp0..\build\Setup
set GlobalVersion=1.8.1
set ToolsVersion=1.0.0
set UpdaterVersion=1.1.1


::Use bundled utility EXEs
path %~dp0utils;%path%

if not exist "%TargetDir%" mkdir "%TargetDir%"
del /q "%TargetDir%\*"

echo Building ZIP archive...
cd /d "%~dp0..\bundled"
zip -q -9 -r "%TargetDir%\zero-install.zip" GnuPG Solver
cd /d "%~dp0..\build\Frontend\Release"
zip -q -9 -r "%TargetDir%\zero-install.zip" . --exclude *.log *.mdb *.vshost.exe Test.* nunit.* Mono.* *.pdb *.xml
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install.zip" "%~dp0..\license.txt"
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install.zip" "%~dp0..\3rd party code.txt"
if errorlevel 1 pause
cd /d "%~dp0"

echo Building TAR.BZ2 archive...
bsdtar -cjf "%TargetDir%\zero-install-%GlobalVersion%.tar.bz2" --exclude=*.log --exclude=*.mdb --exclude=*.vshost.exe --exclude=Test.* --exclude=nunit.* --exclude=Mono.* --exclude=*.pdb --exclude=*.xml -C "%~dp0.." "license.txt" -C "%~dp0.." "3rd party code.txt" -C "%~dp0..\bundled" GnuPG Solver -C "%~dp0..\build\Frontend\Release" .
if errorlevel 1 pause

echo Building Tools archive...
bsdtar -cjf "%TargetDir%\zero-install-tools-%ToolsVersion%.tar.bz2" --exclude=*.log --exclude=*.mdb --exclude=*.vshost.exe --exclude=Test.* --exclude=nunit.* --exclude=Mono.* --exclude=*.pdb --exclude=*.xml -C "%~dp0.." "license.txt" -C "%~dp0.." "3rd party code.txt" -C "%~dp0..\bundled" GnuPG -C "%~dp0..\build\Tools\Release" .
if errorlevel 1 pause

echo Building Tools developer archive...
cd /d "%~dp0..\bundled"
zip -q -9 -r "%TargetDir%\zero-install-tools-dev.zip" GnuPG
cd /d "%~dp0..\build\Tools\Release"
zip -q -9 -r "%TargetDir%\zero-install-tools-dev.zip" . --exclude *.log *.mdb *.vshost.exe Test.* nunit.* Mono.*
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-tools-dev.zip" "%~dp0..\license.txt"
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-tools-dev.zip" "%~dp0..\3rd party code.txt"
if errorlevel 1 pause
cd /d "%~dp0"

echo Building Backend developer archive...
cd /d "%~dp0..\bundled"
zip -q -9 -r "%TargetDir%\zero-install-backend-dev.zip" GnuPG Solver
cd /d "%~dp0..\build\Backend\Release"
zip -q -9 -r "%TargetDir%\zero-install-backend-dev.zip" . --exclude *.log *.mdb *.vshost.exe Test.* nunit.* Mono.*
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-backend-dev.zip" "%~dp0..\license.txt"
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-backend-dev.zip" "%~dp0..\3rd party code.txt"
if errorlevel 1 pause
cd /d "%~dp0"

if "%BUILD_DOC%"=="TRUE" (
  echo Building Updater archive...
  bsdtar -cjf "%TargetDir%\zero-install-updater-%UpdaterVersion%.tar.bz2" --exclude=*.log --exclude=*.pdb --exclude=*.mdb --exclude=*.vshost.exe --exclude=Test.* --exclude=nunit.* --exclude=Mono.* --exclude=*.xml --exclude=SevenZip.* --exclude=C5.* -C "%~dp0..\build\Updater\Release" .
  if errorlevel 1 pause
)

rem Handle WOW
if %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles%
if not %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles(x86)%

rem Check for Inno Setup 5 installation (32-bit)
if not exist "%ProgramFiles_temp%\Inno Setup 5" (
  echo ERROR: No Inno Setup 5 installation found. >&2
  pause
  goto end
)

echo Building installer...
cd /d "%~dp0"
"%ProgramFiles_temp%\Inno Setup 5\iscc.exe" /q "/dVersion=%GlobalVersion%" "/o%TargetDir%" setup.iss
if errorlevel 1 pause

if "%1"=="+run" "%TargetDir%\zero-install.exe" /silent
if "%2"=="+run" "%TargetDir%\zero-install.exe" /silent
if "%3"=="+run" "%TargetDir%\zero-install.exe" /silent
if "%4"=="+run" "%TargetDir%\zero-install.exe" /silent

:end
