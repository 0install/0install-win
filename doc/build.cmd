@echo off
::Compiles the source documentation. Assumes "..\src\build.cmd Debug" has already been executed.
cd /d "%~dp0"

rem Determine VS version
if defined VS110COMNTOOLS (
  ::Visual Studio 2012
  call "%VS110COMNTOOLS%vsvars32.bat"
  set ProgSLN=%ProgSLN%_VS2012.sln
  goto compile
)
if defined VS100COMNTOOLS (
  ::Visual Studio 2010
  call "%VS100COMNTOOLS%vsvars32.bat"
  set ProgSLN=%ProgSLN%_VS2010.sln
  goto compile
)
if defined VS90COMNTOOLS (
  ::Visual Studio 2008
  call "%VS90COMNTOOLS%vsvars32.bat"
  set ProgSLN=%ProgSLN%_VS2008.sln
  goto compile
)
goto err_no_vs


:compile
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
