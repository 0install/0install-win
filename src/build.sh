#!/bin/sh
#Compiles the xbuild solution.
cd `dirname $0`

# Handle Windows-style paths in project files
export MONO_IOMAP=all

xbuild ZeroInstall_MonoDevelop.sln /nologo /v:q /tv:3.0
