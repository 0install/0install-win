@echo off
::Compiles the source code and then creates an installer.
::Use command-line argument "+doc" to additionally create source code documentation.
if "%1"=="+doc" set BUILD_DOC=TRUE
if "%2"=="+doc" set BUILD_DOC=TRUE
if "%3"=="+doc" set BUILD_DOC=TRUE
if "%4"=="+doc" set BUILD_DOC=TRUE

echo.
call "%~dp0src\build.cmd" Release
if errorlevel 1 pause

if defined signing_cert_path (
echo.
call "%~dp0src\sign.cmd"
if errorlevel 1 pause
)

::Auto-download solver if missing
if not exist "%~dp0bundled\Solver" (
  echo.
  cd /d "%~dp0bundled"
  powershell -NonInteractive -Command - < download-solver.ps1
  cd /d "%~dp0"
)

echo.
call "%~dp0nuget\build.cmd" %*
if errorlevel 1 pause

echo.
call "%~dp0installer\build.cmd" %*
if errorlevel 1 pause

if defined signing_cert_path (
echo.
call "%~dp0installer\sign.cmd"
if errorlevel 1 pause
)

::Optionally create debug build and documentation
if "%BUILD_DOC%"=="TRUE" (
  echo.
  call "%~dp0src\build.cmd" DebugWithGtk
  if errorlevel 1 pause

  echo.
  call "%~dp0doc\build.cmd"
  if errorlevel 1 pause
)