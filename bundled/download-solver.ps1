# This script will download and extract the external solver to the current directory

$sourceUrl = "http://0install.de/files/zero-install-solver.zip"
$tempFile = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "zero-install-solver.zip")
$targetDir = (get-location).Path

echo "Downloading external solver..."
$webClient = New-Object System.Net.WebClient
$webClient.DownloadFile($sourceUrl, $tempFile)

echo "Extracting external solver..."
$shellApp = New-Object -com shell.application
$zipFile = $shellApp.namespace($tempFile)
$targetFolder = $shellApp.namespace($targetDir)
$targetFolder.Copyhere($zipFile.items(), 4+16+1024)