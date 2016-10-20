@echo off

rem Compile source code
echo.
call "%~dp0src\build.cmd" Release
call "%~dp0src\build.cmd" ReleaseNet35
call "%~dp0src\build.cmd" ReleaseNet20
if errorlevel 1 pause

rem Add AuthentiCode signatures
if defined signing_cert_path (
echo.
call "%~dp0src\sign.cmd"
if errorlevel 1 pause
)

rem Create NuGet packages
echo.
call "%~dp0nuget\build.cmd"
if errorlevel 1 pause

rem Create PowerShell modules
echo.
call "%~dp0powershell\build.cmd"
if errorlevel 1 pause

rem Create feeds and archives for publishing
echo.
call "%~dp0publish\build.cmd"
if errorlevel 1 pause

rem Build API documentation
echo.
call "%~dp0doc\build.cmd"
if errorlevel 1 pause

rem Deploy compiled binaries
if "%1" == "deploy" "%~dp0build\ReleaseNet20\Bootstrap\zero-install.exe" --feed="%~dp0build\Publish\ZeroInstall-%version%.xml" maintenance %* --batch