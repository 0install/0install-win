@echo off
::Adds AuthentiCode signatures to all binaries. Assumes "build.cmd Release" has already been executed.
if not "%1" == "" set signing_cert_path=%*
set timestamp_server=http://timestamp.comodoca.com/authenticode

rem Determine VS version
if defined VS140COMNTOOLS (
  ::Visual Studio 2015
  call "%VS140COMNTOOLS%vsvars32.bat"
  goto vs_ok
)
echo ERROR: No Visual Studio installation found. >&2
exit /b 1
:vs_ok



echo Signing binaries with "%signing_cert_path%"...

FOR %%A IN ("%~dp0..\build\Release\Frontend\*.exe") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Frontend\ZeroInstall.*.dll") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Tools\*.exe") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Tools\ZeroInstall.*.dll") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%

FOR %%A IN ("%~dp0..\build\ReleaseNet35\Frontend\*.exe") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\ReleaseNet35\Frontend\ZeroInstall.*.dll") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%

FOR %%A IN ("%~dp0..\build\ReleaseNet20\Frontend\*.exe") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\ReleaseNet20\Frontend\ZeroInstall.*.dll") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%~dp0..\build\ReleaseNet20\Bootstrap\zero-install.exe"
if errorlevel 1 exit /b %errorlevel%
