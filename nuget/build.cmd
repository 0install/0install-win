@echo off
::Creates NuGet packages. Assumes "..\src\build.cmd Release", "..\src\build.cmd ReleaseNet35" and "..\src\build.cmd ReleaseNet20" have already been executed.
set /p version= < "%~dp0..\VERSION"

rem Project settings
set TargetDir=%~dp0..\build\Packages

rem Prepare clean output directory
if not exist "%TargetDir%" mkdir "%TargetDir%"
del /q "%TargetDir%\*"


echo Building NuGet packages...
FOR %%A IN ("%~dp0*.nuspec") DO (
  nuget pack "%%A" -Symbols -NoPackageAnalysis -Version "%version%" -Properties VersionSuffix="" -OutputDirectory "%TargetDir%"
  if errorlevel 1 exit /b %errorlevel%
)