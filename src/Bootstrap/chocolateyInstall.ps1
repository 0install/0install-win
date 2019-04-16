. "$PSScriptRoot\0install.exe" --version=1.0.0-pre maintenance deploy --machine --batch
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
