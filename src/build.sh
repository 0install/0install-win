#!/bin/sh
set -e
cd `dirname $0`

#Handle Windows-style paths in project files
export MONO_IOMAP=all

mono NuGet.exe restore -Verbosity quiet
xbuild /nologo /v:q

nuget pack ZeroInstall.Frontend.nuspec -Properties "Configuration=Release;Version=$(< ../VERSION)" -Symbols -OutputDirectory ..\build
