@echo off
::Reads the current version numbers to the console and environment variables

set /p version= < "%~dp0VERSION"
set /p version_updater= < "%~dp0VERSION_UPDATER"

echo ##teamcity[buildNumber '%version%_{build.number}']
echo ##teamcity[setParameter name='build.version' value='%version%']
echo ##teamcity[setParameter name='build.version_updater' value='%version_updater%']
