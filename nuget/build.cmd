@echo off
::Creates NuGet packages. Assumes "..\src\build.cmd Release" has already been executed.
call "%~dp0..\Get-Version.cmd" > NUL

rem Project settings
set TargetDir=%~dp0..\build\Packages

rem Prepare clean output directory
if not exist "%TargetDir%" mkdir "%TargetDir%"
del /q "%TargetDir%\*"


echo Building NuGet packages...
FOR %%A IN ("%~dp0*.nuspec") DO (
  nuget pack "%%A" -Symbols -NoPackageAnalysis -Version "%version%" -OutputDirectory "%TargetDir%"
  if errorlevel 1 exit /b %errorlevel%
)