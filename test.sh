#!/bin/sh
#Runs the unit tests.
cd `dirname $0`

nunit-console UnitTests.nunit
