@echo off
::Compiles the Visual Studio solution.
pushd "%~dp0"

set SOLUTION_FILE=ZeroInstall.sln
set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Debug

echo Restoring NuGet packages...
nuget restore %SOLUTION_FILE% -Verbosity quiet
if errorlevel 1 exit /b %errorlevel%

echo Compiling Visual Studio solution (%CONFIG%)...
for /f "delims=" %%a in ('powershell "cd ${env:ProgramFiles(x86)}; (dir -Recurse \"Microsoft Visual Studio\" | where -Property name -eq devenv.com | select -First 1).FullName"') do "%%a" %SOLUTION_FILE% /Rebuild %CONFIG%
if errorlevel 1 exit /b %errorlevel%

popd
