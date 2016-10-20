@echo off
::Creates PowerShell modules. Assumes "..\src\build.cmd ReleaseNet20" has already been executed.
set /p version= < "%~dp0..\VERSION"

pushd "%~dp0"


echo Building PowerShell Module for Bootstrap...
if not exist ..\build\Modules\0install mkdir ..\build\Modules\0install
del /q ..\build\Modules\0install\*.*
powershell -NoProfile -ExecutionPolicy Bypass -File Build-BootstrapModule.ps1 %version%

popd