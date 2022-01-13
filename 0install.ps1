$ErrorActionPreference = "Stop"

function Download-ZeroInstall {
    $dir = "$env:LOCALAPPDATA\0install.net\bootstrapper"
    $file = "$dir\0install.exe"
    if (!(Test-Path $file)) {
        mkdir -Force $dir | Out-Null
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]'Tls11,Tls12'
        Invoke-WebRequest "https://get.0install.net/0install.exe" -OutFile $file
    }
    return $file
}

function Run-ZeroInstall {
    if (Get-Command 0install -ErrorAction SilentlyContinue) {
        0install @args | %{ "$_" }
    } else {
        . $(Download-ZeroInstall) @args | %{ "$_" }
    }
}

Run-ZeroInstall @args
