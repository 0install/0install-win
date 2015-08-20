#!/bin/sh
#Reads the current version numbers to the console and environment variables
cd `dirname $0`

export VERSION=`cat VERSION`
export VERSION_UPDATER=`cat VERSION_UPDATER`

echo \#\#teamcity[buildNumber \'$VERSION\_{build.number}\']
echo \#\#teamcity[setParameter name=\'build.version\' value=\'$VERSION\']
echo \#\#teamcity[setParameter name=\'build.version_updater\' value=\'$VERSION_UPDATER\']
