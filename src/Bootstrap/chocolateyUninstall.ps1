0install maintenance remove --batch
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
