0install catalog refresh
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install catalog search vlc
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install catalog add https://apps.0install.net/0install/catalog.xml
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}

0install catalog remove https://apps.0install.net/0install/catalog.xml
if ($LASTEXITCODE -ne 0) {throw "Exit Code: $LASTEXITCODE"}
