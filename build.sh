#!/bin/sh
#Compiles the source code and runs unit tests.
cd `dirname $0`

vs/build.sh && nunit-console UnitTests.nunit
