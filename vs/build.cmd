@echo off
cd /d "%~dp0"

rem Project settings
set ProgSLN=ZeroInstall

rem Determine VS version
if exist "%VS100COMNTOOLS%" (
  ::Visual Studio 2010
  set VS_COMNTOOLS=%VS100COMNTOOLS%
  set ProgSLN=%ProgSLN%_VS2010.sln
  goto compile
)
if exist "%VS90COMNTOOLS%" (
  ::Visual Studio 2008
  set VS_COMNTOOLS=%VS90COMNTOOLS%
  set ProgSLN=%ProgSLN%_VS2008.sln
  goto compile
)
goto err_no_vs



:compile
call "%VS_COMNTOOLS%vsvars32.bat"
echo Compiling Visual Studio solution...
if exist ..\bin\Release rd /s /q ..\bin\Release
msbuild "%ProgSLN%" /t:Rebuild /p:Configuration=Release /v:q /nologo
if errorlevel 1 pause
goto end



rem Error messages

:err_no_vs
cls
echo.
echo ERROR! No Visual Studio installation found.
pause
goto end

:end