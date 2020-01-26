Param ($Version = "1.0.0-pre", [Switch]$Deploy, [Switch]$Machine)
$ErrorActionPreference = "Stop"

pushd $PSScriptRoot

src\build.ps1 $Version
feed\build.ps1 $Version

if ($Deploy) {
  if ($Machine) {
    artifacts\Bootstrap\0install\0install.exe --feed="$PSScriptRoot\feed\0install-win-$Version.xml" maintenance deploy --batch --machine
  } else {
    artifacts\Bootstrap\0install\0install.exe --feed="$PSScriptRoot\feed\0install-win-$Version.xml" maintenance deploy --batch
  }
}

popd
