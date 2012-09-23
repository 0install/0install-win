@echo off
::Compiles the Visual Studio solution.
cd /d "%~dp0"

rem Project settings
set ProgSLN=ZeroInstall

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
set config=%1
if "%config%"=="" set config=Debug

call "%VS_COMNTOOLS%vsvars32.bat"
echo Compiling Visual Studio solution (%config%)...
if exist ..\build\%config% rd /s /q ..\build\%config%
msbuild "%ProgSLN%" /nologo /v:q /t:Rebuild /p:Configuration=%config%
if errorlevel 1 pause
goto end



rem Error messages

:err_no_vs
echo ERROR: No Visual Studio installation found. >&2
pause
goto end

:end
