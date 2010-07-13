@echo off
::Compiles the source code and then creates an installer or source code documentation.
::Use command-line argument "doc" to create source code documentation.

if not "%1"=="doc" (
  echo.
  call "%~dp0vs\build.cmd" Release
  echo.
  call "%~dp0setup\build.cmd"
)

if "%1"=="doc" (
  echo.
  call "%~dp0vs\build.cmd" Debug
  echo.
  call "%~dp0doc\build.cmd"
)