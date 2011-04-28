#!/bin/sh
#Removes compilation artifacts and other temporary files.
cd `dirname $0`

# Clear binaries (leave Documentation and Portable intact)
rm -rf build/Backend build/Frontend build/Tools build/Updater build/Publish

# Clear object caches
rm -f src/*.cache
rm -rf src/*/obj
rm -f src/*/.pidb

# Remove MonoDevelop user preferences
rm -f src/*.userprefs

# Remove NUnit logs
rm -f *.VisualState.xml TestResult.xml
