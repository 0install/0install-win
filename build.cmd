@echo off
::Compiles the source code and creates publishable artifacts.

echo.
call "%~dp0src\build.cmd" Release
if errorlevel 1 pause

if defined signing_cert_path (
echo.
call "%~dp0src\sign.cmd"
if errorlevel 1 pause
)

echo.
call "%~dp0nuget\build.cmd" %*
if errorlevel 1 pause

echo.
call "%~dp0publish\build.cmd" %*
if errorlevel 1 pause

echo.
call "%~dp0doc\build.cmd" %*
if errorlevel 1 pause