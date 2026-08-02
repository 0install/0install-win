$dir = Join-Path ([Environment]::GetFolderPath("LocalApplicationData")) "0install-test-store"

0install store add-dir $dir
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

$output = 0install store list --batch
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
if ($output -NotContains "ReadWrite: $dir") { throw "Wrong output: $output" }

0install store remove-dir $dir
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

Remove-Item $dir
