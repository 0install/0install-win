#!/bin/sh
#Removes compilation artifacts and other temporary files.
cd `dirname $0`

# Clear binaries (leave Documentation and Portable intact)
rm -rf build/Backend build/Frontend build/Tools build/Setup

# Clear object caches
rm -f vs/*.cache
rm -rf vs/*/obj
rm -f vs/*/.pidb

# Remove MonoDevelop user preferences
rm -f vs/*.userprefs

# Remove NUnit logs
rm -f *.VisualState.xml TestResult.xml
