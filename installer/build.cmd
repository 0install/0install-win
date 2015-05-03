@echo off
::Creates an Inno Setup installer. Assumes "..\src\build.cmd Release" and "..\bundled\download-solver.ps1" have already been executed.
call "%~dp0..\version.cmd" > NUL

rem Handle WOW
if %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles%
if not %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles(x86)%

rem Check for Inno Setup 5 installation (32-bit)
if not exist "%ProgramFiles_temp%\Inno Setup 5" (
  echo ERROR: No Inno Setup 5 installation found. >&2
  pause
  goto end
)


echo Building system-wide installer...
cd /d "%~dp0"
"%ProgramFiles_temp%\Inno Setup 5\iscc.exe" /q "/dVersion=%version%" zero-install.iss
if errorlevel 1 pause


echo Building per-user installer...
cd /d "%~dp0"
"%ProgramFiles_temp%\Inno Setup 5\iscc.exe" /q "/dVersion=%version%" /dPerUser=1 zero-install.iss
if errorlevel 1 pause


if "%1"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen
if "%2"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen
if "%3"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen
if "%4"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen

:end
