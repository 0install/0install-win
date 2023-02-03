Param ($Version = "1.0.0-pre")
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

function Find-MSBuild {
    if (Test-Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe") {
        $vsDir = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -products * -property installationPath -format value -version 17.4
        if ($vsDir) { return "$vsDir\MSBuild\Current\Bin\amd64\MSBuild.exe" }
    }
}

function Run-MSBuild {
    $msbuild = Find-Msbuild
    if (!$msbuild) { throw "You need Visual Studio 2022 v17.4 or newer to build this project" }
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
$AssemblyVersion = $Version.Split("-")[0]
SearchAndReplace $AssemblyVersion OneGet\provider.manifest -PatternLeft 'version="' -PatternRight '" versionScheme="multipartnumeric"'
SearchAndReplace $AssemblyVersion OneGet.Bootstrap\0install.psd1 -PatternLeft "ModuleVersion = '" -PatternRight "'"

# Compile source code
Run-MSBuild /v:Quiet /t:Restore /t:Build /p:Configuration=Release /p:Version=$Version
Out-File ..\artifacts\VERSION -Encoding ASCII -InputObject $Version
rm ..\artifacts\Release\net472\*.xml -Exclude ZeroInstall.VisualElementsManifest.xml

# Generate bootstrap package for PowerShell Gallery (OneGet)
$env:PATH = "$env:PATH;${env:ProgramFiles(x86)}\Windows Kits\10\bin\x64;${env:ProgramFiles(x86)}\Windows Kits\8.1\bin\x64"
if (Get-Command mt -ErrorAction SilentlyContinue) {
    Add-Manifest OneGet\provider.manifest ..\artifacts\Release\net472\ZeroInstall.OneGet.dll
    Add-Manifest OneGet\provider.manifest OneGet.Bootstrap\bin\Release\net472\0install.dll

    ..\0install.ps1 run --batch https://apps.0install.net/dotnet/nuget.xml pack OneGet.Bootstrap\PowerShell.nuspec -NoPackageAnalysis -Properties Version=$Version -OutputDirectory ..\artifacts
    move -Force ..\artifacts\0install.$Version.nupkg ..\artifacts\0install.powershell.$Version.nupkg
} else {
    Write-Warning "You need mt.exe to build the 0install OneGet provider"
}

# Generate bootstrap package for Chocolatey
if (Get-Command choco -ErrorAction SilentlyContinue) {
    choco pack Bootstrap\Chocolatey.nuspec --version $Version --outdir ..\artifacts
    move -Force ..\artifacts\0install.$Version.nupkg ..\artifacts\0install.chocolatey.$Version.nupkg
} else {
    Write-Warning "You need choco.exe to build the 0install Chocolatey package"
}

popd
