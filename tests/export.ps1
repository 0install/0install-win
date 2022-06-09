0install export https://apps.0install.net/devel/cloc.xml $env:TEMP\0install-export
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

& $env:TEMP\0install-export\import.cmd
if ((0, 1) -NotContains $LASTEXITCODE) {throw "Exit Code: $LASTEXITCODE"} # Expect OK or NoChanges

Remove-Item -Recurse $env:TEMP\0install-export
