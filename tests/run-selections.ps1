0install select --xml https://apps.0install.net/devel/terraform.xml > "$env:temp\terraform_selections.xml"
0install run "$env:temp\terraform_selections.xml" --help | Out-Null
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
