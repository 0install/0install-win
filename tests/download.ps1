0install-win download --batch https://apps.0install.net/gui/naps2.xml | Out-Null
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
