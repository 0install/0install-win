@echo off
::Compiles the source documentation.
pushd "%~dp0"

rem Look for NanoByte.Common documentation build in sibling directory. On build servers this should be handled using dependency injection instead.
if exist ..\..\..\nano-byte\common\build\Documentation\common.tag copy ..\..\..\nano-byte\common\build\Documentation\common.tag ..\build\common.tag > NUL

echo Building source documentation...
if exist ..\build\Documentation rd /s /q ..\build\Documentation
mkdir ..\build\Documentation
..\build\ReleaseNet20\Frontend\0install.exe run http://0install.de/feeds/Doxygen.xml Backend.Doxyfile
if errorlevel 1 exit /b %errorlevel%
..\build\ReleaseNet20\Frontend\0install.exe run http://0install.de/feeds/Doxygen.xml Frontend.Doxyfile
if errorlevel 1 exit /b %errorlevel%
..\build\ReleaseNet20\Frontend\0install.exe run http://0install.de/feeds/Doxygen.xml Tools.Doxyfile
if errorlevel 1 exit /b %errorlevel%
copy index.html ..\build\Documentation\index.html > NUL

popd