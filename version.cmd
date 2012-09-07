@echo off
::Sets the current version number as an environment variable

set /p version= < "%~dp0version"
set /p version_tools= < "%~dp0version-tools"
set /p version_updater= < "%~dp0version-updater"

echo ##teamcity[buildNumber '%version%_{build.number}']
echo ##teamcity[setParameter name='build.version' value='%version%']
echo ##teamcity[publishArtifacts 'version']
echo ##teamcity[setParameter name='build.version_tools' value='%version_tools%']
echo ##teamcity[publishArtifacts 'version_tools']
echo ##teamcity[setParameter name='build.version_updater' value='%version_updater%']
echo ##teamcity[publishArtifacts 'version_updater']
