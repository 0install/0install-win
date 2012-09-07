@echo off
::Sets the current version number as an environment variable

set /p version= < "%~dp0version"
set /p version_tools= < "%~dp0version-tools"
set /p version_updater= < "%~dp0version-updater"

echo ##teamcity[buildNumber '%version%_{build.number}']