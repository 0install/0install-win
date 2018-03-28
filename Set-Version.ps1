Param ([Parameter(Mandatory=$True)] [string]$NewVersion)
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

function SearchAndReplace($FilePath, $PatternLeft, $PatternRight)
{
  (Get-Content "$ScriptDir\$FilePath" -Encoding UTF8) `
    -replace "$PatternLeft.*$PatternRight", ($PatternLeft.Replace('\', '') + $NewVersion + $PatternRight.Replace('\', '')) |
    Set-Content "$ScriptDir\$FilePath" -Encoding UTF8
}

[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)
SearchAndReplace doc\Doxyfile -PatternLeft 'PROJECT_NUMBER = "' -PatternRight '"'
SearchAndReplace src\GlobalAssemblyInfo.cs -PatternLeft 'AssemblyVersion\("' -PatternRight '"\)'
SearchAndReplace src\OneGet\provider.manifest -PatternLeft 'version="' -PatternRight '" versionScheme="multipartnumeric"'
SearchAndReplace src\Bootstrap.OneGet\0install.psd1 -PatternLeft "ModuleVersion = '" -PatternRight "'"
