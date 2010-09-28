#!/bin/sh
#Compiles the Visual Studio solution.

# Handle Windows-style paths in project files
MONO_IOMAP=all

xbuild ZeroInstall_Mono.sln
