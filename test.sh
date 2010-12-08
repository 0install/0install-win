#!/bin/sh
#Runs the unit tests.
cd `dirname $0`

# Store all temporary files created by unit tests in a separate directory
export TMPDIR=/tmp/0install-unit-tests
mkdir $TMPDIR || exit 1

# Clean up temporary files if all unit tests passed
nunit-console UnitTests.nunit && rm -rf $TMPDIR
