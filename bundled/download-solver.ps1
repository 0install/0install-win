# This script will download and extract the external solver to the current directory

$sourceUrl = "https://0install.de/teamcity/guestAuth/repository/downloadAll/ZeroInstall_CSharp_PythonSolver/.lastSuccessful/artifacts.zip"
$tempFile = [System.IO.Path]::GetTempFileName()
$targetDir = (get-location).Path

echo "Downloading external solver..."
$webClient = New-Object System.Net.WebClient
$webClient.DownloadFile($sourceUrl, $tempFile)

echo "Extracting external solver..."
$null = [Reflection.Assembly]::LoadFile($targetDir + "\..\src\packages\ICSharpCode.SharpZipLib.Patched.0.86.1\lib\net20\ICSharpCode.SharpZipLib.dll")
$fastZip = New-Object ICSharpCode.SharpZipLib.Zip.FastZip
$fastZip.ExtractZip($tempFile, $targetDir, "")

[System.IO.File]::Delete($tempFile)
