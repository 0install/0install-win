0install add-feed https://apps.0install.net/0install/0install.xml https://apps.0install.net/0install/0install-win.xml
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }

$output = 0install list-feeds https://apps.0install.net/0install/0install.xml --batch
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
if ($output -NotContains "https://apps.0install.net/0install/0install-win.xml") { throw "Wrong output" }

0install remove-feed https://apps.0install.net/0install/0install.xml https://apps.0install.net/0install/0install-win.xml
if ($LASTEXITCODE -ne 0) { throw "Exit Code: $LASTEXITCODE" }
