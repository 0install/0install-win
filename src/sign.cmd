@echo off
::Adds AuthentiCode signatures to all binaries. Assumes "build.cmd Release" has already been executed.
if not "%1" == "" set signing_cert_path=%*
set timestamp_server=http://timestamp.globalsign.com/?signature=sha2

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

FOR %%A IN ("%~dp0..\build\Release\Frontend\*.exe") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Frontend\ZeroInstall.*.dll") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Tools\*.exe") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Tools\ZeroInstall.*.dll") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%

FOR %%A IN ("%~dp0..\build\ReleaseNet35\Frontend\*.exe") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\ReleaseNet35\Frontend\ZeroInstall.*.dll") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%

FOR %%A IN ("%~dp0..\build\ReleaseNet20\Frontend\*.exe") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\ReleaseNet20\Frontend\ZeroInstall.*.dll") DO signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%%A"
if errorlevel 1 exit /b %errorlevel%
signtool sign /f "%signing_cert_path%" /fd sha256 /p "%signing_cert_pass%" /tr %timestamp_server% /td sha256 /q "%~dp0..\build\ReleaseNet20\Bootstrap\zero-install.exe"
if errorlevel 1 exit /b %errorlevel%
