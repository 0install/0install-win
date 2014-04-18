@echo off
::Sets the current version number as an environment variable

set /p version= < "%~dp0VERSION"
set /p version_updater= < "%~dp0VERSION_UPDATER"

echo ##teamcity[buildNumber '%version%_{build.number}']
echo ##teamcity[setParameter name='build.version' value='%version%']
echo ##teamcity[setParameter name='build.version_updater' value='%version_updater%']
