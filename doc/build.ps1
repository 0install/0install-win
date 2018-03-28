$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# Ensure 0install is in the PATH
if (!(Get-Command 0install -ErrorAction SilentlyContinue)) { $env:PATH = "$(Resolve-Path ..\build\Release);$env:PATH" }

if (Test-Path ..\build\Documentation) {rm -Recurse -Force ..\build\Documentation}
mkdir ..\build\Documentation | Out-Null

# Download tag files for external references
Invoke-WebRequest http://nano-byte.de/common/api/nanobyte-common.tag -OutFile nanobyte-common.tag
Invoke-WebRequest http://0install.de/api/backend/0install-dotnet.tag -OutFile 0install-dotnet.tag

cmd /c "0install run --batch http://0install.de/feeds/Doxygen.xml 2>&1" # Redirect stderr to stdout

popd
