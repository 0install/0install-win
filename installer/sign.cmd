@echo off
::Adds AuthentiCode signatures to all installers. Assumes "build.cmd" has already been executed.
if not "%1" == "" set signing_cert_path=%*

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



echo Signing installers with "%signing_cert_path%"...
FOR %%A IN ("%~dp0..\build\Installer\*.exe") DO signtool sign /t http://timestamp.comodoca.com/authenticode /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%
FOR %%A IN ("%~dp0..\build\Installer\*.msi") DO signtool sign /t http://timestamp.comodoca.com/authenticode /f "%signing_cert_path%" /p "%signing_cert_pass%" /v "%%A"
if errorlevel 1 exit /b %errorlevel%



exit /b 0
rem Error messages

:err_no_vs
echo ERROR: No Visual Studio installation found. >&2
exit /b 1
