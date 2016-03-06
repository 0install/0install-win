@echo off
::Creates release archives and feeds. Assumes "..\src\build.cmd Release" has already been executed.
set /p version= < "%~dp0..\VERSION"

pushd "%~dp0"

echo Preparing build directory content for release...
copy /y ..\COPYING.txt ..\build\Release\Frontend\ > NUL
copy /y "..\3rd party code.txt" ..\build\Release\Frontend\ > NUL
del ..\build\Release\Frontend\*.xml
del ..\build\Release\Frontend\*.pdb

copy /y ..\COPYING.txt ..\build\Release\Tools\ > NUL
copy /y "..\3rd party code.txt" ..\build\Release\Tools\ > NUL
del ..\build\Release\Tools\*.xml
del ..\build\Release\Tools\*.pdb

echo Building release archives and feeds from templates...
..\build\Release\Frontend\0install.exe run http://0install.net/tools/0template.xml ZeroInstall.xml.template version=%version%
..\build\Release\Frontend\0install.exe run http://0install.net/tools/0template.xml ZeroInstall_Tools.xml.template version=%version%
if not exist ..\build\Publish mkdir ..\build\Publish
del /q ..\build\Publish\*.*
move *.xml ..\build\Publish > NUL
move *.tar.gz ..\build\Publish > NUL

popd