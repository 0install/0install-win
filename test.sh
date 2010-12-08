#!/bin/sh
#Runs the unit tests.
cd `dirname $0`

# Store all temporary files created by unit tests in a separate directory
export TMPDIR=`mktemp -d -t 0install-unit-tests.XXXXXXXXXX` || exit 1

nunit-console UnitTests.nunit
