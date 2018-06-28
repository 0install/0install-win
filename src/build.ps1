﻿$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$vsDir = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath -format value
$msBuild = "$vsDir\MSBuild\15.0\Bin\amd64\MSBuild.exe"

. $msBuild -v:Quiet -t:Restore -t:Build -p:Configuration=Release

# Package OneGet Bootstrap as PowerShell Module
nuget pack OneGet.Bootstrap\0install.nuspec -Properties Version=$(Get-Content ..\VERSION) -OutputDirectory ..\artifacts

# Package Symbols
nuget pack ZeroInstall.Frontend.nuspec -Symbols -Properties Version=$(Get-Content ..\VERSION) -OutputDirectory ..\artifacts
Remove-Item "..\artifacts\ZeroInstall.Frontend.$(Get-Content ..\VERSION).nupkg"

popd
