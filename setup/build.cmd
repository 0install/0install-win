@echo off
::Creates an Inno Setup installer. Assumes "..\vs\build.cmd Release" has already been executed.
cd /d "%~dp0"

rem Project settings
set BuildDir=..\build\Setup
set SetupEXE=0install.exe

rem Check Inno Setup 5
if not exist "%ProgramFiles%\Inno Setup 5" goto err_no_is
path %ProgramFiles%\Inno Setup 5;%path%



echo Building Inno Setup...
iscc /Q setup.iss
if errorlevel 1 pause

if "%1"=="+run" "%BuildDir%\%SetupEXE%" /silent
if "%2"=="+run" "%BuildDir%\%SetupEXE%" /silent
if "%3"=="+run" "%BuildDir%\%SetupEXE%" /silent

echo Building Inno Setup Update...
iscc /Q update.iss
if errorlevel 1 pause

goto end



rem Error messages

:err_no_is
cls
echo.
echo ERROR! No Inno Setup 5 installation found.
pause
goto end

:end
