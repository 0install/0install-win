@echo off
for /f "delims=" %%i in ('dir /b "%~dp0interfaces"') do 0install import --batch "%~dp0interfaces\%%i"
for /f "delims=" %%i in ('dir /b /ad "%~dp0implementations"') do 0install store copy --batch "%~dp0implementations\%%i"