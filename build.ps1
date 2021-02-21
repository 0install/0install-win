Param ($Version = "1.0.0-pre", [Switch]$Deploy, [Switch]$Machine)
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

src\build.ps1 $Version

if ($Deploy) {
  if ($Machine) {
    artifacts\Release\net45\win\0install.exe self deploy --batch --machine
  } else {
    artifacts\Release\net45\win\0install.exe self deploy --batch
  }
}

popd
