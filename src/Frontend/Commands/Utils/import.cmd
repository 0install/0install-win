@echo off
FOR %%A IN ("%~dp0*.xml") DO 0install import --batch "%%A"
FOR %%A IN ("%~dp0*.tbz2") DO 0install store add --batch %%~nA "%%A"