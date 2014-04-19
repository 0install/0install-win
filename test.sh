#!/bin/sh
#Runs the unit tests.

cd `dirname $0`/build/Backend/Debug
nunit-console ZeroInstall.Backend.UnitTests.dll

cd `dirname $0`/build/Frontend/Debug
nunit-console ZeroInstall.Frontend.UnitTests.dll

cd `dirname $0`/build/Tools/Debug
nunit-console ZeroInstall.Tools.UnitTests.dll
