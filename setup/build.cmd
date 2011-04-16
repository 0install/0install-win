@echo off
::Creates an Inno Setup installer. Assumes "..\src\build.cmd Release" has already been executed.
cd /d "%~dp0"

rem Project settings
set TargetDir=%~dp0..\build\Publish
set SetupEXE=0install.exe
set SetupUpdateEXE=0install_upd.exe

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



rem Purge old files
if exist "%TargetDir%" rd /s /q "%TargetDir%"
mkdir "%TargetDir%"



echo Building Inno Setup...
iscc /Q setup.iss
if errorlevel 1 pause

if "%1"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%2"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%3"=="+run" "%TargetDir%\%SetupEXE%" /silent
if "%4"=="+run" "%TargetDir%\%SetupEXE%" /silent

echo Building Inno Setup Update...
iscc /Q update.iss
if errorlevel 1 pause



echo Building Bundled archive...
cd "%~dp0..\build\Bundled"
zip -9 -r "%TargetDir%\bundled.zip" . > NUL
if errorlevel 1 pause

rem Bundled content also needs to be copied into the other archive
copy "%TargetDir%\bundled.zip" "%TargetDir%\0install_backend.zip" > NUL
copy "%TargetDir%\bundled.zip" "%TargetDir%\0install.zip" > NUL
copy "%TargetDir%\bundled.zip" "%TargetDir%\0install_tools.zip" > NUL

echo Building Backend archive...
cd "%~dp0..\build\Backend\Release"
zip -9 -r "%TargetDir%\0install_backend.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\0install_backend.zip" "%~dp0..\lgpl.txt" > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\0install_backend.zip" "%~dp0..\3rd party code.txt" > NUL
if errorlevel 1 pause

echo Building Frontend archive...
cd "%~dp0..\build\Frontend\Release"
zip -9 -r "%TargetDir%\0install.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* *.xml > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\0install.zip" "%~dp0..\lgpl.txt" > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\0install.zip" "%~dp0..\3rd party code.txt" > NUL
if errorlevel 1 pause

echo Building Tools archive...
cd "%~dp0..\build\Tools\Release"
zip -9 -r "%TargetDir%\0install_tools.zip" . --exclude *.log *.pdb *.mdb *.vshost.exe Test.* nunit.* Mono.* *.xml > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\0install_tools.zip" "%~dp0..\lgpl.txt" > NUL
if errorlevel 1 pause
zip -9 -j "%TargetDir%\0install_tools.zip" "%~dp0..\3rd party code.txt" > NUL
if errorlevel 1 pause

:end
