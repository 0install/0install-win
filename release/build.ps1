$ErrorActionPreference = "Stop"
pushd $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

rm -Force ..\build\Release\*.xml
rm -Force ..\build\Release\*.pdb

..\build\Release\0install.exe run http://0install.net/tools/0template.xml ZeroInstall.xml.template version=$(Get-Content ..\VERSION)

popd
