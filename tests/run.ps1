0install run https://apps.0install.net/devel/terraform.xml --help | Out-Null
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
