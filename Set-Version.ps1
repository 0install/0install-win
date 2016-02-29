Param ([Parameter(Mandatory=$True)] [string]$NewVersion)
#Sets a new version number in all relevant locations
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

[System.IO.File]::WriteAllText("$ScriptDir\VERSION", $NewVersion)

(Get-Content "$ScriptDir\src\GlobalAssemblyInfo.cs" -Encoding UTF8) `
  -replace 'AssemblyVersion\(".*"\)', ('AssemblyVersion("' + $NewVersion + '")') |
  Set-Content "$ScriptDir\src\GlobalAssemblyInfo.cs" -Encoding UTF8
(Get-Content "$ScriptDir\src\Frontend\OneGet\provider.manifest" -Encoding UTF8) `
  -replace 'version=".*" versionScheme="multipartnumeric"', ('version="' + $NewVersion + '.0" versionScheme="multipartnumeric"') |
  Set-Content "$ScriptDir\src\Frontend\OneGet\provider.manifest" -Encoding UTF8

(Get-Content "$ScriptDir\doc\Backend.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$ScriptDir\doc\Backend.Doxyfile" -Encoding UTF8
(Get-Content "$ScriptDir\doc\Frontend.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$ScriptDir\doc\Frontend.Doxyfile" -Encoding UTF8
(Get-Content "$ScriptDir\doc\Tools.Doxyfile" -Encoding UTF8) `
  -replace 'PROJECT_NUMBER = ".*"', ('PROJECT_NUMBER = "' + $NewVersion + '"') |
  Set-Content "$ScriptDir\doc\Tools.Doxyfile" -Encoding UTF8

(Get-Content "$ScriptDir\ZeroInstall.xml" -Encoding UTF8) `
  -replace 'version=".*" stability="testing"', ('version="' + $NewVersion + '-post-1" stability="testing"') `
  -replace 'version=".*" stability="developer"', ('version="' + $NewVersion + '-post-2" stability="developer"') |
  Set-Content "$ScriptDir\ZeroInstall.xml" -Encoding UTF8
(Get-Content "$ScriptDir\ZeroInstall_Tools.xml" -Encoding UTF8) `
  -replace 'version=".*" stability="testing"', ('version="' + $NewVersion + '-post-1" stability="testing"') `
  -replace 'version=".*" stability="developer"', ('version="' + $NewVersion + '-post-2" stability="developer"') |
  Set-Content "$ScriptDir\ZeroInstall_Tools.xml" -Encoding UTF8
