Invoke-WebRequest https://apps.0install.net/devel/terraform.xml -OutFile "$env:temp\terraform.xml"
0install run "$env:temp\terraform.xml" --help | Out-Null
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
