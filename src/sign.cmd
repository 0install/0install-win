@echo off
::Adds AuthentiCode signatures to all compiled EXEs. Assumes "build.cmd Release" has already been executed.

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



if not "%PfxPath%" == "" (
echo Signing executables...
FOR %%A IN ("%~dp0..\build\Release\Frontend\*.exe") DO signtool sign /t http://timestamp.comodoca.com/authenticode /f "%PfxPath%" /p %PfxPassword% /v "%%A" > NUL
if errorlevel 1 pause
FOR %%A IN ("%~dp0..\build\Release\Tools\*.exe") DO signtool sign /t http://timestamp.comodoca.com/authenticode /f "%PfxPath%" /p %PfxPassword% /v "%%A" > NUL
if errorlevel 1 pause
FOR %%A IN ("%~dp0..\build\Release\Updater\*.exe") DO signtool sign /t http://timestamp.comodoca.com/authenticode /f "%PfxPath%" /p %PfxPassword% /v "%%A" > NUL
if errorlevel 1 pause
)



goto end
rem Error messages

:err_no_vs
echo ERROR: No Visual Studio installation found. >&2
pause
goto end

:end
