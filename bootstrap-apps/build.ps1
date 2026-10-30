$ErrorActionPreference = "Stop"

$apps = Get-Content "$PSScriptRoot\list.txt" | ? { $_.Trim() }
$zeroinstall = "$PSScriptRoot\..\artifacts\Release\net48\publish\0install.exe"

$outputDir = "$PSScriptRoot\..\artifacts\BootstrapApps"
mkdir -Force $outputDir | Out-Null

pushd $outputDir
foreach ($app in $apps) {
    echo "Build bootstrappers for $app"
    . $zeroinstall run --batch https://apps.0install.net/0install/0bootstrap.xml --template=..\Bootstrap\ --force $app
    . $zeroinstall run --batch https://apps.0install.net/0install/0bootstrap.xml --template=..\Bootstrap\ --force --integrate-args="--no-download" $app
}
popd
