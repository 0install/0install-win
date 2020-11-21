$ErrorActionPreference = "Stop"

function download {
    $downloadDir = "$env:LOCALAPPDATA\0install.net\bootstrapper"
    if (!(Test-Path "$downloadDir\0install.exe")) {
        mkdir -Force $downloadDir | Out-Null
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]'Tls11,Tls12'
        Invoke-WebRequest "https://get.0install.net/0install.exe" -OutFile "$downloadDir\0install.exe"
    }
    return $downloadDir
}

$ErrorActionPreference = "SilentlyContinue"
if (Get-Command 0install) {
    0install @args | %{ "$_" }
} else {
    . "$(download)\0install.exe" @args | %{ "$_" }
}
