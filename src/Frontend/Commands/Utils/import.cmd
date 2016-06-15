@echo off
FOR %%A IN ("%~dp0content\*.xml") DO 0install import --batch "%%A"
FOR %%A IN ("%~dp0content\*.tbz2") DO 0install store add --batch %%~nA "%%A"
