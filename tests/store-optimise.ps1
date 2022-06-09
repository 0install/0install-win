0install-win store optimise --batch | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
