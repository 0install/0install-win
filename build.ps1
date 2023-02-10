Param ([String]$Version = "1.0.0-pre", [Switch]$Deploy, [Switch]$Machine)
$ErrorActionPreference = "Stop"
pushd $PSScriptRoot

src\build.ps1 $Version
.\0install.ps1 run --batch https://apps.0install.net/0install/0template.xml 0install-win.xml.template version=$Version

if ($Deploy) {
  if ($Machine) {
    artifacts\Release\net472\0install.exe self deploy --batch --machine
  } else {
    artifacts\Release\net472\0install.exe self deploy --batch
  }
}

popd
