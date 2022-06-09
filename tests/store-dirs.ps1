0install store add-dir $env:TEMP\implementations
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

$output = 0install store list --batch
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
if ($output -NotContains "ReadWrite: $env:TEMP\implementations") { throw "Wrong output" }

0install store remove-dir $env:TEMP\implementations
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
