0install add terraform https://apps.0install.net/devel/terraform.xml --no-download --batch
if ($LASTEXITCODE -eq 50) {
  Write-Output "Skipping desktop integration tests in portable mode"
  return
}
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

$output = 0install list-apps terraform --batch
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
if ($output -NotContains "https://apps.0install.net/devel/terraform.xml: HashiCorp Terraform [AppAlias: terraform]") { throw "Wrong output" }

& "$env:appdata\0install.net\desktop-integration\aliases\terraform.exe" --help | Out-Null
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install-win integrate https://apps.0install.net/gui/vlc.xml --add-standard --no-download | Out-Null
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
if (-Not(Test-Path "$env:appdata\Microsoft\Windows\Start Menu\Programs\AudioVideo\VLC media player.lnk")) {throw "Missing menu entry"}

0install repair-all
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install remove https://apps.0install.net/devel/terraform.xml
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install remove https://apps.0install.net/gui/vlc.xml
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
Start-Sleep -Seconds 5  # May trigger implicit removal of "library mode" instance of Zero Install
