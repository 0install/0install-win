﻿Param ([Switch]$Deploy, [Switch]$Machine)
$ErrorActionPreference = "Stop"

$RootDir = $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
pushd $RootDir

src\build.ps1
release\build.ps1

if ($Deploy) {
  if ($Machine) {
    artifacts\Bootstrap\zero-install.exe --feed="$RootDir\release\ZeroInstall-$(Get-Content VERSION).xml" maintenance deploy --batch --machine
  } else {
    artifacts\Bootstrap\zero-install.exe --feed="$RootDir\release\ZeroInstall-$(Get-Content VERSION).xml" maintenance deploy --batch
  }
}

popd
