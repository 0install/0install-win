Param ($Version = "0.1.0-pre")
$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

function SearchAndReplace($Value, $FilePath, $PatternLeft, $PatternRight)
{
  (Get-Content $FilePath -Encoding UTF8) `
    -replace "$PatternLeft.*$PatternRight", ($PatternLeft.Replace('\', '') + $Value + $PatternRight.Replace('\', '')) |
    Set-Content $FilePath -Encoding UTF8
}

# Inject version number
Set-Content -Path "Central.WinForms\VERSION" -Value $Version -Encoding UTF8
$AssemblyVersion = $Version.Split("-")[0]
SearchAndReplace $AssemblyVersion GlobalAssemblyInfo.cs -PatternLeft 'AssemblyVersion\("' -PatternRight '"\)'
SearchAndReplace $AssemblyVersion OneGet\provider.manifest -PatternLeft 'version="' -PatternRight '" versionScheme="multipartnumeric"'
SearchAndReplace $AssemblyVersion OneGet.Bootstrap\0install.psd1 -PatternLeft "ModuleVersion = '" -PatternRight "'"

# Compile source code
$vsDir = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath -format value
$msBuild = "$vsDir\MSBuild\15.0\Bin\amd64\MSBuild.exe"
. $msBuild -v:Quiet -t:Restore -t:Build -p:Configuration=Release

# Package OneGet Bootstrap as PowerShell Module
nuget pack OneGet.Bootstrap\0install.nuspec -Properties Version=$Version -OutputDirectory ..\artifacts

popd
