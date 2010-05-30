@echo off

echo.
call "%~dp0vs\build.cmd" %*

echo.
call "%~dp0setup\build.cmd" %*