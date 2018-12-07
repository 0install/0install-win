Param ($Version = "0.1.0-pre", [Switch]$Deploy, [Switch]$Machine)
$ErrorActionPreference = "Stop"

$RootDir = $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
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
