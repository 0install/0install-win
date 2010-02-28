@echo off
cd /d "%~dp0"

rem Project settings
set ProgISS=setup.iss
set ProgUpdateISS=update.iss
set SetupTarget=..\..\build\windows_setup
set SetupName=0install.exe

rem Check Inno Setup 5
if not exist "%ProgramFiles%\Inno Setup 5" goto err_no_is
path %ProgramFiles%\Inno Setup 5;%path%



echo Building Inno Setup...
iscc /Q "%ProgISS%"
if errorlevel 1 pause

if "%1"=="+run" "..\..\build\windows_setup\%SetupName%" /silent
if "%2"=="+run" "..\..\build\windows_setup\%SetupName%" /silent
if "%3"=="+run" "..\..\build\windows_setup\%SetupName%" /silent

echo Building Inno Setup Update...
iscc /Q "%ProgUpdateISS%"
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