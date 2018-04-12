$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

. $(Resolve-Path "packages\xunit.runner.console.*\tools\net452\xunit.console.exe") "UnitTests\bin\Release\ZeroInstall.Frontend.UnitTests.dll"

popd
