@echo off
::Compiles the source documentation. Assumes "..\src\build.cmd Debug" has already been executed.

rem Determine VS version
if defined VS120COMNTOOLS (
  ::Visual Studio 2013
  call "%VS120COMNTOOLS%vsvars32.bat"
  goto compile
)
if defined VS110COMNTOOLS (
  ::Visual Studio 2012
  call "%VS110COMNTOOLS%vsvars32.bat"
  goto compile
)
if defined VS100COMNTOOLS (
  ::Visual Studio 2010
  call "%VS100COMNTOOLS%vsvars32.bat"
  goto compile
)
goto err_no_vs


:compile
echo Building source documentation...
if exist "%~dp0..\build\Documentation" rd /s /q "%~dp0..\build\Documentation"
FOR %%A IN ("%~dp0*.shfbproj") DO (
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
