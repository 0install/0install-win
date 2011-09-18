@echo off
::Creates an Inno Setup installer. Assumes "..\src\build.cmd Release" has already been executed.
cd /d "%~dp0"

rem Project settings
set TargetDir=%~dp0..\build\Setup
set SetupEXE=zero-install.exe

rem Handle WOW
if %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles%
if not %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles(x86)%

rem Check for Inno Setup 5 installation (32-bit)
if not exist "%ProgramFiles_temp%\Inno Setup 5" (
  echo ERROR: No Inno Setup 5 installation found. >&2
  pause
  goto end
)
path %ProgramFiles_temp%\Inno Setup 5;%path%



echo Building Inno Setup...
iscc /Q setup.iss
if errorlevel 1 pause

if "%1"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%2"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%3"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%4"=="+run" "%TargetDir%\%SetupEXE%" /silent



rem Purge old files
if exist "%TargetDir%\zero-install-backend.zip" del "%TargetDir%\zero-install-backend.zip"
if exist "%TargetDir%\zero-install.zip" del "%TargetDir%\zero-install.zip"
if exist "%TargetDir%\zero-install-tools.zip" del "%TargetDir%\zero-install-tools.zip"
if exist "%TargetDir%\zero-install-updater.zip" del "%TargetDir%\zero-install-updater.zip"

echo Building Backend archive...
cd "%~dp0..\bundled"
zip -9 -r "%TargetDir%\zero-install-backend.zip" . --exclude *.svn > NUL
cd "%~dp0..\build\Backend\Release"
zip -9 -r "%TargetDir%\zero-install-backend.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\zero-install-backend.zip" "%~dp0..\lgpl.txt" > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\zero-install-backend.zip" "%~dp0..\3rd party code.txt" > NUL
if errorlevel 1 pause

echo Building Frontend archive...
cd "%~dp0..\bundled"
zip -9 -r "%TargetDir%\zero-install.zip" . --exclude *.svn > NUL
cd "%~dp0..\build\Frontend\Release"
zip -9 -r "%TargetDir%\zero-install.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* *.xml > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\zero-install.zip" "%~dp0..\lgpl.txt" > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\zero-install.zip" "%~dp0..\3rd party code.txt" > NUL
if errorlevel 1 pause

echo Building Tools archive...
cd "%~dp0..\bundled"
zip -9 -r "%TargetDir%\zero-install-tools.zip" GnuPG --exclude *.svn > NUL
cd "%~dp0..\build\Tools\Release"
zip -9 -r "%TargetDir%\zero-install-tools.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* *.xml > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\zero-install-tools.zip" "%~dp0..\lgpl.txt" > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\zero-install-tools.zip" "%~dp0..\3rd party code.txt" > NUL
if errorlevel 1 pause

echo Building Updater archive...
cd "%~dp0..\build\Updater\Release"
zip -9 -r "%TargetDir%\zero-install-updater.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* SevenZip.* C5.* *.xml > NUL
if errorlevel 1 pause

:end
