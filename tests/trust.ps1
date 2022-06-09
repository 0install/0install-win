0install trust remove 88C8A1F375928691D7365C0259AA3927C24E4E1E apps.0install.net
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

Write-Output "Error expected here:"
0install select --refresh --batch https://apps.0install.net/devel/terraform.xml
if ($LASTEXITCODE -ne 27) {throw "Exit Code: $LASTEXITCODE"}

0install trust add 88C8A1F375928691D7365C0259AA3927C24E4E1E apps.0install.net
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
