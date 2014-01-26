#!/bin/sh
#Compiles the xbuild solution.
cd `dirname $0`

#Handle Windows-style paths in project files
export MONO_IOMAP=all

#Restore NuGet packages
mono --runtime=v4.0 .nuget/NuGet.exe restore ZeroInstall_MonoDevelop.sln

xbuild ZeroInstall_MonoDevelop.sln /nologo /v:q
