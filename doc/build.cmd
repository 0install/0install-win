@echo off
::Compiles the source documentation. Assumes "..\src\build.cmd Debug" has already been executed.

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
if exist "%~dp0..\build\Documentation" rd /s /q "%~dp0..\build\Documentation"

msbuild "%~dp0Backend\Backend.shfbproj" /p:Configuration=Debug /v:q /nologo
move "%~dp0..\build\Documentation\Backend\Backend.chm" "%~dp0..\build\Documentation\Backend.chm"
msbuild "%~dp0Frontend\Frontend.shfbproj" /p:Configuration=Debug /v:q /nologo
msbuild "%~dp0Tools\Tools.shfbproj" /p:Configuration=Debug /v:q /nologo

goto end


rem Error messages

:err_no_vs
echo ERROR: No Visual Studio installation found. >&2
pause
goto end

:end
