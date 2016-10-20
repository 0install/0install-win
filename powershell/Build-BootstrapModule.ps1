Param ([Parameter(Mandatory=$True)] [string]$Version)
#Builds PowerShell Module for Bootstrap
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

(Get-Content "$ScriptDir\0install.psd1.template" -Encoding UTF8) `
  -replace '{version}', $Version |
  Set-Content "$ScriptDir\..\build\Modules\0install\0install.psd1" -Encoding UTF8

copy "$ScriptDir\..\build\Bootstrap\zero-install.exe" "$ScriptDir\..\build\Modules\0install\0install.dll"
