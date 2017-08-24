$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

if (!(Test-Path ..\build\PowerShellModules\0install)) { mkdir ..\build\PowerShellModules\0install | Out-Null }
(Get-Content 0install.psd1.template -Encoding UTF8) `
  -replace '{version}', $(Get-Content ..\VERSION) |
  Set-Content ..\build\PowerShellModules\0install\0install.psd1 -Encoding UTF8

copy ..\build\Bootstrap\zero-install.exe ..\build\PowerShellModules\0install\0install.dll

popd
