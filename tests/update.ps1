0install-win update --batch https://apps.0install.net/devel/terraform.xml | Out-Null
if ((0, 1) -NotContains $LASTEXITCODE) {throw "Exit Code: $LASTEXITCODE"} # Expect OK or NoChanges
