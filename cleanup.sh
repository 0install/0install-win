#!/bin/sh
#Removes compilation artifacts and other temporary files.
cd `dirname $0`

#Compiled artifacts
rm -rf build

#Solution-wide
rm -f src/*.userprefs src/*.suo src/*.user src/*.cache

#Per-project
rm -rf src/*/obj
rm -f src/*/*.pidb
rm -f src/*/*.csproj.user

#NUnit logs
rm -f *.VisualState.xml TestResult.xml
