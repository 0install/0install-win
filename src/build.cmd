@echo off
::Compiles the Visual Studio solution.
pushd "%~dp0"

rem Project settings
set SOLUTION_FILE=ZeroInstall.sln

rem Determine VS version
if defined VS140COMNTOOLS (
  ::Visual Studio 2015
  call "%VS140COMNTOOLS%vsvars32.bat"
  goto vs_ok
)
echo ERROR: No Visual Studio installation found. >&2
exit /b 1
:vs_ok



set config=%1
if "%config%"=="" set config=Debug

echo Restoring NuGet packages...
nuget restore %SOLUTION_FILE% -Verbosity quiet
if errorlevel 1 exit /b %errorlevel%

echo Compiling Visual Studio solution (%config%)...
if exist ..\build\%config% rd /s /q ..\build\%config%
msbuild %SOLUTION_FILE% /nologo /v:q /m /t:Rebuild /p:Configuration=%config%
if errorlevel 1 exit /b %errorlevel%

popd