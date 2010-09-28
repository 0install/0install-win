#!/bin/sh
#Compiles the Visual Studio solution.
cd ${0%%/*}

# Handle Windows-style paths in project files
MONO_IOMAP=all

xbuild ZeroInstall_Mono.sln
