Param ([Switch]$Deploy, [Switch]$Machine, [Switch]$Portable)
$ErrorActionPreference = "Stop"

$previousPath = $env:PATH
$env:PATH = "$PSScriptRoot\artifacts\Release\net472\win;$env:PATH"
if ($Deploy) {
  if ($Portable) {
    Write-Output "Deploying portable instance for integration tests"
    0install self deploy --batch --portable "$env:TEMP\0install-portable"
    $env:PATH = "$env:TEMP\0install-portable;$env:PATH"
  } elseif ($Machine) {
    Write-Output "Deploying machine-wide instance for integration tests"
    0install self deploy --batch --machine
    $env:PATH = "$env:appdata\Programs\Zero Install;$env:PATH"
  } else {
    Write-Output "Deploying per-user instance for integration tests"
    0install self deploy --batch
    $env:PATH = "$env:ProgramFiles\Zero Install;$env:PATH"
  }
} else {
  Write-Output "Using local build for integration tests"
}

foreach ($script in Get-ChildItem "$PSScriptRoot\tests" -Filter "*.ps1") {
  Write-Output $script.Name
  & $script.FullName
}

if ($Deploy) {
  if ($Portable) {
    Remove-Item -Recurse "$env:TEMP\0install-portable"
  } else {
    0install self remove --batch
    Start-Sleep -Seconds 5
  }
}
$env:PATH = $previousPath
