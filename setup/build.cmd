@echo off
::Creates an Inno Setup installer. Assumes "..\src\build.cmd Release" has already been executed.
cd /d "%~dp0"

rem Project settings
set TargetDir=%~dp0..\build\Setup
set SetupEXE=zero-install.exe


echo Building Backend archive...
if exist "%TargetDir%\zero-install-backend.zip" del "%TargetDir%\zero-install-backend.zip"
cd /d "%~dp0..\bundled"
zip -q -9 -r "%TargetDir%\zero-install-backend.zip" .
cd /d "%~dp0..\build\Backend\Release"
zip -q -9 -r "%TargetDir%\zero-install-backend.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.*
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-backend.zip" "%~dp0..\lgpl.txt"
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-backend.zip" "%~dp0..\3rd party code.txt"
if errorlevel 1 pause
cd /d "%~dp0"

echo Building Frontend archive...
if exist "%TargetDir%\zero-install.zip" del "%TargetDir%\zero-install.zip"
cd "%~dp0..\bundled"
zip -q -9 -r "%TargetDir%\zero-install.zip" .
cd "%~dp0..\build\Frontend\Release"
zip -q -9 -r "%TargetDir%\zero-install.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* *.xml
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install.zip" "%~dp0..\lgpl.txt"
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install.zip" "%~dp0..\3rd party code.txt"
bsdtar -cjf "%TargetDir%\zero-install.tar.bz2" --exclude=*.log --exclude=*.pdb --exclude=*.mdb --exclude=*.vshost.exe --exclude=Test.* --exclude=nunit.* --exclude=Mono.* --exclude=*.xml -C "%~dp0.." "lgpl.txt" -C "%~dp0.." "3rd party code.txt" -C "%~dp0..\bundled" . -C "%~dp0..\build\Frontend\Release" .
if errorlevel 1 pause

echo Building Tools archive...
if exist "%TargetDir%\zero-install-tools.zip" del "%TargetDir%\zero-install-tools.zip"
cd "%~dp0..\bundled"
zip -q -9 -r "%TargetDir%\zero-install-tools.zip" GnuPG
cd "%~dp0..\build\Tools\Release"
zip -q -9 -r "%TargetDir%\zero-install-tools.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* *.xml
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-tools.zip" "%~dp0..\lgpl.txt"
if errorlevel 1 pause
zip -q -9 -j "%TargetDir%\zero-install-tools.zip" "%~dp0..\3rd party code.txt"
if errorlevel 1 pause

echo Building Updater archive...
bsdtar -cjf "%TargetDir%\zero-install-updater.tar.bz2" --exclude=*.log --exclude=*.pdb --exclude=*.mdb --exclude=*.vshost.exe --exclude=Test.* --exclude=nunit.* --exclude=Mono.* --exclude=*.xml --exclude=SevenZip.* --exclude=C5.* -C "%~dp0..\build\Updater\Release" .
if errorlevel 1 pause
cd /d "%~dp0"


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
"%ProgramFiles_temp%\Inno Setup 5\iscc.exe" /Q setup.iss
if errorlevel 1 pause

if "%1"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%2"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%3"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%4"=="+run" "%TargetDir%\%SetupEXE%" /silent

:end
