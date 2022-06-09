0install add https://apps.0install.net/devel/terraform.xml --no-download
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

$output = 0install list-apps terraform --batch
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
if ($output -NotContains "https://apps.0install.net/devel/terraform.xml: HashiCorp Terraform []") { throw "Wrong output" }

0install list-apps --xml > "$env:TEMP\0install-applist.xml"
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install remove https://apps.0install.net/devel/terraform.xml
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install import-apps "$env:TEMP\0install-applist.xml" --no-download
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

$output = 0install list-apps terraform --batch
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
if ($output -NotContains "https://apps.0install.net/devel/terraform.xml: HashiCorp Terraform []") { throw "Wrong output" }

0install remove https://apps.0install.net/devel/terraform.xml
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
