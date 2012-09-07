#!/bin/sh
#Sets the current version number as an environment variable
cd `dirname $0`

export VERSION=`cat version`
export VERSION_TOOLS=`cat version-tools`
export VERSION_UPDATER=`cat version-updater`

echo \#\#teamcity[buildNumber '$VERSION_{build.number}']
