0install-win download --batch https://apps.0install.net/devel/terraform.xml | Out-Null
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
