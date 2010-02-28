@echo off

echo.
call "%~dp0vs\build.cmd" %*

echo.
call "%~dp0wrappers\build.cmd" %*

echo.
call "%~dp0setup\build.cmd" %*