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



echo Signing binaries with "%signing_cert_path%"...
FOR %%A IN ("%~dp0..\build\Release\Frontend\*.exe") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Tools\*.exe") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Updater\*.exe") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Frontend\ZeroInstall.*.dll") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Tools\ZeroInstall.*.dll") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Release\Updater\ZeroInstall.*.dll") DO signtool sign /t %timestamp_server% /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%



exit /b 0
rem Error messages

:err_no_vs
echo ERROR: No Visual Studio installation found. >&2
exit /b 1
