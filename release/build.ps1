$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# Ensure 0install is in the PATH
if (!(Get-Command 0install -ErrorAction SilentlyContinue)) { $env:PATH = "$(Resolve-Path ..\build\Release);$env:PATH" }

rm -Force ..\build\Release\*.xml
rm -Force ..\build\Release\*.pdb

cmd /c "0install run --batch http://0install.net/tools/0template.xml ZeroInstall.xml.template version=$(Get-Content ..\VERSION) 2>&1" # Redirect stderr to stdout
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

popd
