0install update --batch https://apps.0install.net/gui/notepad-plus-plus.xml | Out-Null
if ((0, 1) -NotContains $LASTEXITCODE) {throw "Exit Code: $LASTEXITCODE"} # Expect OK or NoChanges
