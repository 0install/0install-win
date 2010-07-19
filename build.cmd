@echo off
::Compiles the source code and then creates an installer.
::Use command-line argument "+doc" to additionally create source code documentation.

echo.
call "%~dp0vs\build.cmd" Release
echo.
call "%~dp0setup\build.cmd"

if "%1"=="+doc" (
  echo.
  call "%~dp0vs\build.cmd" Debug
  echo.
  call "%~dp0doc\build.cmd"
)