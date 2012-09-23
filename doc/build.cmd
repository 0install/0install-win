@echo off
::Compiles the source documentation. Assumes "..\src\build.cmd Debug" has already been executed.
cd /d "%~dp0"

rem Determine VS version
if exist %VS110COMNTOOLS% (
  ::Visual Studio 2012
  set VS_COMNTOOLS=%VS110COMNTOOLS%
  set ProgSLN=%ProgSLN%_VS2012.sln
  goto compile
)
if exist %VS100COMNTOOLS% (
  ::Visual Studio 2010
  set VS_COMNTOOLS=%VS100COMNTOOLS%
  set ProgSLN=%ProgSLN%_VS2010.sln
  goto compile
)
if exist %VS90COMNTOOLS% (
  ::Visual Studio 2008
  set VS_COMNTOOLS=%VS90COMNTOOLS%
  set ProgSLN=%ProgSLN%_VS2008.sln
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
echo ERROR: No Visual Studio installation found. >&2
pause
goto end

:end
