@echo off
::Creates a portable ZIP archive, an Inno Setup installer and an MSI wrapper. Assumes "..\src\build.cmd Release" and "..\bundled\download-solver.ps1" have already been executed.
set /p version= < "%~dp0..\VERSION"
set /p version_updater= < "%~dp0..\VERSION_UPDATER"

rem Bundled tool executables (e.g. "zip")
path %~dp0utils;%path%

rem Handle WOW
if %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles%
if not %PROCESSOR_ARCHITECTURE%==x86 set ProgramFiles_temp=%ProgramFiles(x86)%

rem Check Inno Setup installation
if "%INNOSETUP_DIR%" == "" set INNOSETUP_DIR=%ProgramFiles_temp%\Inno Setup 5
if not exist "%INNOSETUP_DIR%" goto err_no_innosetup

rem Find Wix installation
if "%WIX_DIR%" == "" if exist "%ProgramFiles_temp%\WiX Toolset v4.0" set WIX_DIR=%ProgramFiles_temp%\WiX Toolset v4.0
if "%WIX_DIR%" == "" if exist "%ProgramFiles_temp%\WiX Toolset v3.10" set WIX_DIR=%ProgramFiles_temp%\WiX Toolset v3.10
if "%WIX_DIR%" == "" if exist "%ProgramFiles_temp%\WiX Toolset v3.9" set WIX_DIR=%ProgramFiles_temp%\WiX Toolset v3.9
if not exist "%WIX_DIR%" goto err_no_wix

rem Determine VS version
if defined VS140COMNTOOLS (
  ::Visual Studio 2015
  call "%VS140COMNTOOLS%vsvars32.bat"
  goto vs_ok
)
if defined VS120COMNTOOLS (
  ::Visual Studio 2013
  call "%VS120COMNTOOLS%vsvars32.bat"
  goto vs_ok
)
if defined VS110COMNTOOLS (
  ::Visual Studio 2012
  call "%VS110COMNTOOLS%vsvars32.bat"
  goto vs_ok
)
if defined VS100COMNTOOLS (
  ::Visual Studio 2010
  call "%VS100COMNTOOLS%vsvars32.bat"
  goto vs_ok
)
goto err_no_vs
:vs_ok



echo Building portable ZIP archive...
if exist "%~dp0..\build\Installer\zero-install.zip" del "%~dp0..\build\Installer\zero-install.zip"
if not exist "%~dp0..\build\Installer" mkdir "%~dp0..\build\Installer"

zip -q -9 -j "%~dp0..\build\Installer\zero-install.zip" "%~dp0_portable" "%~dp0..\COPYING.txt" "%~dp0..\3rd party code.txt"
if errorlevel 1 exit /b %errorlevel%

cd /d "%~dp0..\build\Release\Frontend"
zip -q -9 -r "%~dp0..\build\Installer\zero-install.zip" . --exclude *.xml *.pdb *.mdb *.vshost.exe
if errorlevel 1 exit /b %errorlevel%

cd /d "%~dp0..\bundled"
zip -q -9 -r "%~dp0..\build\Installer\zero-install.zip" Solver
if errorlevel 1 exit /b %errorlevel%


echo Building machine-wide installer...
cd /d "%~dp0"
"%INNOSETUP_DIR%\iscc.exe" /q "/dVersion=%version%" zero-install.iss
if errorlevel 1 exit /b %errorlevel%

echo Building per-user installer...
cd /d "%~dp0"
"%INNOSETUP_DIR%\iscc.exe" /q "/dVersion=%version%" /dPerUser=1 zero-install.iss
if errorlevel 1 exit /b %errorlevel%

if "%1"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen
if "%2"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen
if "%3"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen
if "%4"=="+run" "%~dp0..\build\Installer\zero-install.exe" /silent /mergetasks=!desktopicon,!ngen


echo Building MSI wrappers for EXE installers...
cd /d "%~dp0"

"%WIX_DIR%\bin\candle.exe" -nologo zero-install.wxs
if errorlevel 1 exit /b %errorlevel%
"%WIX_DIR%\bin\light.exe" -nologo zero-install.wixobj -sval -out "..\build\Installer\zero-install.msi" -spdb
if errorlevel 1 exit /b %errorlevel%
del zero-install.wixobj

"%WIX_DIR%\bin\candle.exe" -nologo zero-install-per-user.wxs
if errorlevel 1 exit /b %errorlevel%
"%WIX_DIR%\bin\light.exe" -nologo zero-install-per-user.wixobj -sval -out "..\build\Installer\zero-install-per-user.msi" -spdb
if errorlevel 1 exit /b %errorlevel%
del zero-install-per-user.wixobj



exit /b 0
rem Error messages

:err_no_innosetup
echo ERROR: No Inno Setup 5 installation found. >&2
echo Download here: http://www.jrsoftware.org/isdl.php >&2
exit /b 1

:err_no_wix
echo ERROR: No WiX Toolset installation found. >&2
echo Download here: http://www.wixtoolset.org/releases/ >&2
exit /b 1

:err_no_vs
echo ERROR: No Visual Studio installation found. >&2
exit /b 1
