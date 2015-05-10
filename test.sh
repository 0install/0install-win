#!/bin/sh
#Runs the unit tests.

cd `dirname $0`/build/Debug/Backend
nunit-console ZeroInstall.Backend.UnitTests.dll

cd `dirname $0`/build/Debug/Frontend
nunit-console ZeroInstall.Frontend.UnitTests.dll

cd `dirname $0`/build/Debug/Tools
nunit-console ZeroInstall.Tools.UnitTests.dll
