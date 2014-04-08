@echo off
::Compiles the source code and then creates an installer.
::Use command-line argument "+doc" to additionally create source code documentation.
if "%1"=="+doc" set BUILD_DOC=TRUE
if "%2"=="+doc" set BUILD_DOC=TRUE
if "%3"=="+doc" set BUILD_DOC=TRUE
if "%4"=="+doc" set BUILD_DOC=TRUE

echo.
call "%~dp0src\build.cmd" Release

::Auto-download solver if missing
if not exist "%~dp0bundled\Solver" (
  echo.
  cd /d "%~dp0bundled"
  powershell -NonInteractive -Command - < download-solver.ps1
  cd /d "%~dp0"
)

echo.
call "%~dp0nuget\build.cmd" %*
echo.
call "%~dp0installer\build.cmd" %*

::Optionally create debug build and documentation
if "%BUILD_DOC%"=="TRUE" (
  echo.
  call "%~dp0src\build.cmd" DebugWithGtk
  echo.
  call "%~dp0doc\build.cmd"
)