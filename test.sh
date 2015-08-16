#!/bin/sh
#Runs the unit tests.
cd `dirname $0`/build

cd Debug/Backend
nunit-console ZeroInstall.Backend.UnitTests.dll
cd ../..

cd Debug/Frontend
nunit-console ZeroInstall.Frontend.UnitTests.dll
cd ../..

cd Debug/Tools
nunit-console ZeroInstall.Tools.UnitTests.dll
cd ../..
