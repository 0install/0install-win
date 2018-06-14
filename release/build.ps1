Param ([switch]$GitHubRelease)
$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$Version = Get-Content ..\VERSION
if ($GitHubRelease -ne "stable") {$Version += "-pre"}

# Ensure 0install is in the PATH
if (!(Get-Command 0install -ErrorAction SilentlyContinue)) { $env:PATH = "$(Resolve-Path ..\build\Release);$env:PATH" }

# Exclude .NET XML Documentation and Debug Symbols from release
rm -Force ..\build\Release\*.xml,..\build\Release\*.pdb -Exclude *.VisualElementsManifest.xml

# Build feed and archive
$stability = if($Version.Contains('-')) {'developer'} else {'stable'}
cmd /c "0install run --batch http://0install.net/tools/0template.xml ZeroInstall.xml.template version=$Version stability=$stability 2>&1" # Redirect stderr to stdout
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

# Patch archive URL to point to GitHub Release
if ($GitHubRelease) {
    $path = Resolve-Path "ZeroInstall-$Version.xml"
    [xml]$xml = Get-Content $path
    $xml.interface.group.implementation.archive.href = "https://github.com/0install/0install-win/releases/download/$Version/$($xml.interface.group.implementation.archive.href)"
    $xml.Save($path)
}

popd
