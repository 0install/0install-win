. "$PSScriptRoot\0install.exe" --version=1.0.0-pre self deploy --machine --batch
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
