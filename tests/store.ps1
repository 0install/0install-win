$digest = 0install digest "$PSScriptRoot\..\artifacts\Release\net472" --algorithm=sha256new --batch
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

0install store add $digest "$PSScriptRoot\..\artifacts\Release\net472"
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

0install store find $digest
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

0install store verify $digest
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

$output = 0install store list-implementations --batch
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
if ($output.Where({ $_.Contains($digest) }, 'First').Count -eq 0) { throw "Wrong output" }

0install store export $digest "$env:TEMP\0install-export.tar.gz"
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

0install store remove $digest
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

0install store add $digest "$env:TEMP\0install-export.tar.gz"
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

Remove-Item "$env:TEMP\0install-export.tar.gz"

0install store remove $digest
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
