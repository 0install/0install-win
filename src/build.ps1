$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$vsDir = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath -format value
$msBuild = "$vsDir\MSBuild\15.0\Bin\amd64\MSBuild.exe"

nuget restore
. $msBuild -v:Quiet -t:Build -p:Configuration=Release

# Package PowerShell Module
nuget pack 0install.nuspec -Properties Version=$(Get-Content ..\VERSION) -OutputDirectory ..\build

# Package Symbols
nuget pack ZeroInstall.Frontend.nuspec -Symbols -Properties Version=$(Get-Content ..\VERSION) -OutputDirectory ..\build
Remove-Item "..\build\ZeroInstall.Frontend.$(Get-Content ..\VERSION).nupkg"

popd
