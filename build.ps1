Param ($Version = "1.0.0-pre", [Switch]$Deploy, [Switch]$Machine)
$ErrorActionPreference = "Stop"

$RootDir = $PSScriptRoot
pushd $RootDir

src\build.ps1 $Version
feed\build.ps1 $Version

if ($Deploy) {
  if ($Machine) {
    artifacts\Bootstrap\0install\0install.exe --feed="$RootDir\feed\ZeroInstall-$Version.xml" maintenance deploy --batch --machine
  } else {
    artifacts\Bootstrap\0install\0install.exe --feed="$RootDir\feed\ZeroInstall-$Version.xml" maintenance deploy --batch
  }
}

popd
