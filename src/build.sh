#!/bin/sh
#Compiles the xbuild solution.
cd `dirname $0`

#Handle Windows-style paths in project files
export MONO_IOMAP=all

#Project settings
SOLUTION_FILE=ZeroInstall_Mono.sln

echo Restoring NuGet packages...
mono --runtime=v4.0 .nuget/NuGet.exe restore $SOLUTION_FILE

echo Compiling solution...
xbuild $SOLUTION_FILE /nologo /v:q
