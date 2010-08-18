@echo off
::Compiles the source documentation. Assumes "..\vs\build.cmd Debug" has already been executed.
cd /d "%~dp0"

rem Determine VS version
if exist "%VS100COMNTOOLS%" (
  ::Visual Studio 2010
  set VS_COMNTOOLS=%VS100COMNTOOLS%
  goto compile
)
if exist "%VS90COMNTOOLS%" (
  ::Visual Studio 2008
  set VS_COMNTOOLS=%VS90COMNTOOLS%
  goto compile
)
goto err_no_vs



:compile
call "%VS_COMNTOOLS%vsvars32.bat"
echo Compiling source documentation...
if exist ..\build\Documentation rd /s /q ..\build\Documentation

FOR %%A IN (Backend\Backend.shfbproj Frontend\Frontend.shfbproj Tools\Tools.shfbproj) DO (
  msbuild "%%A" /p:Configuration=Debug /v:q /nologo
  if errorlevel 1 pause
)

goto end



rem Error messages

:err_no_vs
cls
echo.
echo ERROR! No Visual Studio installation found.
pause
goto end

:end
