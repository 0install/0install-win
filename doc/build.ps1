$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

if (Test-Path ..\build\Documentation) {rm -Recurse -Force ..\build\Documentation}
mkdir ..\build\Documentation | Out-Null
..\build\Release\0install.exe run http://0install.de/feeds/Doxygen.xml

popd
