@echo off
cd /d "%~dp0"

rem Use the Visual C# 2005 compiler
echo Setting environment for using .NET Framework 2.0 tools.
path %windir%\Microsoft.NET\Framework\v2.0.50727;%PATH%

set TargetDir=..\build\Release
if not exist "%TargetDir%" mkdir "%TargetDir%"

echo Compiling 0launch commad-line wrapper...
csc /out:"%TargetDir%\0launch.exe" /target:exe 0launch.cs /warnaserror+ /optimize+ /debug- /nologo
if errorlevel 1 pause

echo Compiling 0launch GUI wrapper...
csc /out:"%TargetDir%\0launchw.exe" /target:winexe 0launchw.cs /warnaserror+ /optimize+ /debug- /nologo
if errorlevel 1 pause

echo Compiling 0store commad-line wrapper...
csc /out:"%TargetDir%\0store.exe" /target:exe 0store.cs /warnaserror+ /optimize+ /debug- /nologo
if errorlevel 1 pause

echo Compiling 0store GUI wrapper...
csc /out:"%TargetDir%\0storew.exe" /target:winexe 0storew.cs /warnaserror+ /optimize+ /debug- /nologo
if errorlevel 1 pause
