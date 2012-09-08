#!/bin/sh
#Sets the current version number as an environment variable
cd `dirname $0`

export VERSION=`cat version`
export VERSION_TOOLS=`cat version-tools`
export VERSION_UPDATER=`cat version-updater`

echo \#\#teamcity[buildNumber \'$VERSION\_{build.number}\']
echo \#\#teamcity[setParameter name=\'build.version\' value=\'$VERSION\']
echo \#\#teamcity[publishArtifacts \'version\']
echo \#\#teamcity[setParameter name=\'build.version_tools\' value=\'$VERSION_TOOLS\']
echo \#\#teamcity[publishArtifacts \'version-tools\']
echo \#\#teamcity[setParameter name=\'build.version_updater\' value=\'$VERSION_UPDATER\']
echo \#\#teamcity[publishArtifacts \'version-updater\']
