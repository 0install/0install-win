Param ($Version = "0.1.0-pre")
$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# Ensure 0install is in the PATH
if (!(Get-Command 0install -ErrorAction SilentlyContinue)) {
    $env:PATH = "$(Resolve-Path ..\artifacts\Release);$env:PATH"
}

# Exclude .NET XML Documentation and Debug Symbols from release
rm -Force ..\artifacts\Release\*.xml,..\artifacts\Release\*.pdb -Exclude *.VisualElementsManifest.xml

# Build feed and archive
$stability = if($Version.Contains('-')) {'developer'} else {'stable'}
cmd /c "0install run --batch http://0install.net/tools/0template.xml 0install-win.xml.template version=$Version stability=$stability 2>&1" # Redirect stderr to stdout
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

popd
