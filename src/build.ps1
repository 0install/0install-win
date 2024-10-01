Param ([String]$Version = "1.0.0-pre")
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

function Find-MSBuild {
    if (Test-Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe") {
        $vsDir = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -products * -property installationPath -format value -version 17.8
        if ($vsDir) { return "$vsDir\MSBuild\Current\Bin\amd64\MSBuild.exe" }
    }
}

function Run-MSBuild {
    $msbuild = Find-Msbuild
    if (!$msbuild) { throw "You need Visual Studio 2022 v17.8 or newer to build this project" }
    . $msbuild @args
    if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
}

function SearchAndReplace($Value, $FilePath, $PatternLeft, $PatternRight) {
    (Get-Content $FilePath -Encoding UTF8) `
        -replace "$PatternLeft.*$PatternRight", ($PatternLeft.Replace('\', '') + $Value + $PatternRight.Replace('\', '')) |
        Set-Content $FilePath -Encoding UTF8
}

function Add-Manifest($Manifest, $Binary) {
    mt -nologo -manifest $Manifest -outputresource:"$Binary;#101"
    if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
}

# Inject version number
SearchAndReplace $Version Bootstrap\chocolateyInstall.ps1 -PatternLeft '--version=' -PatternRight ' self'

# Compile source code
Run-MSBuild /v:Quiet /t:Restore /t:Build /p:Configuration=Release /p:Version=$Version
Out-File ..\artifacts\VERSION -Encoding ASCII -InputObject $Version
Remove-Item ..\artifacts\Release\net472\* -Include *.xml,*.pdb -Exclude ZeroInstall.VisualElementsManifest.xml
Remove-Item ..\artifacts\Release\net472\*\Microsoft.CodeAnalysis*.resources.dll

# Build Windows Installer package
pushd Bootstrap.WinForms
dotnet tool restore
dotnet wix build zero-install.wsx -o ..\..\artifacts\Bootstrap\zero-install.msi -pdbtype none
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
popd

# Generate bootstrap package for Chocolatey
if (Get-Command choco -ErrorAction SilentlyContinue) {
    choco pack Bootstrap\Chocolatey.nuspec --version $Version --outdir ..\artifacts
    move -Force ..\artifacts\0install.$Version.nupkg ..\artifacts\0install.chocolatey.$Version.nupkg
} else {
    Write-Warning "You need choco.exe to build the 0install Chocolatey package"
}

popd
