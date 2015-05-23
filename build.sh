#!/bin/sh
#Compiles the source code.
cd `dirname $0`

#Create debug build
src/build.sh
