0install config
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

0install config self_update_uri
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

0install config self_update_uri '""'
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

Write-Output "Error expected here:"
0install self update
if ($LASTEXITCODE -eq 0) { throw "Exit Code: $LASTEXITCODE" }

0install config self_update_uri default
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
