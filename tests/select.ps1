0install select https://apps.0install.net/devel/terraform.xml
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

$output = 0install list
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
if ($output -NotContains "https://apps.0install.net/devel/terraform.xml") { throw "Wrong output" }
