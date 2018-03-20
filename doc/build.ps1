$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

if (Test-Path ..\build\Documentation) {rm -Recurse -Force ..\build\Documentation}
mkdir ..\build\Documentation | Out-Null

# Download tag files for external references
Invoke-WebRequest http://nano-byte.de/common/api/nanobyte-common.tag -OutFile nanobyte-common.tag
Invoke-WebRequest http://0install.de/api/backend.tag -OutFile 0install-backend.tag

..\build\Release\0install.exe run --batch http://0install.de/feeds/Doxygen.xml

popd
