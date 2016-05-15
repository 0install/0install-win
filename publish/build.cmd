@echo off
::Creates release archives and feeds. Assumes "..\src\build.cmd ReleaseNet20" has already been executed.
set /p version= < "%~dp0..\VERSION"

pushd "%~dp0"


echo Preparing build directory content for release...

copy /y ..\COPYING.txt ..\build\ReleaseNet20\Frontend\ > NUL
copy /y "..\3rd party code.txt" ..\build\ReleaseNet20\Frontend\ > NUL
del ..\build\ReleaseNet20\Frontend\*.xml
del ..\build\ReleaseNet20\Frontend\*.pdb

copy /y ..\COPYING.txt ..\build\Release\Tools\ > NUL
copy /y "..\3rd party code.txt" ..\build\Release\Tools\ > NUL
del ..\build\Release\Tools\*.xml
del ..\build\Release\Tools\*.pdb


echo Building release archives and feeds from templates...

..\build\ReleaseNet20\Frontend\0install.exe run http://0install.net/tools/0template.xml ZeroInstall.xml.template version=%version%
..\build\ReleaseNet20\Frontend\0install.exe run http://0install.net/tools/0template.xml ZeroInstall_Tools.xml.template version=%version%
if not exist ..\build\Publish mkdir ..\build\Publish
del /q ..\build\Publish\*.*
move *.xml ..\build\Publish > NUL
move *.tar.gz ..\build\Publish > NUL


popd