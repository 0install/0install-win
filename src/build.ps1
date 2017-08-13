$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

nuget restore
. "$(. "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath -format value)\Common7\IDE\devenv.com" ZeroInstall.sln /Build Release
nuget pack ZeroInstall.Frontend.nuspec -Properties "Configuration=Release;Version=$(Get-Content ..\VERSION)" -Symbols -OutputDirectory ..\build

popd
