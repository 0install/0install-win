#!/bin/sh
#Compiles the source code.
cd `dirname $0`

#Auto-download solver if missing
if [ ! -d bundled/Solver ]; then
  cd bundled
  ./download-solver.sh
  cd ..
fi

#Create debug build
src/build.sh
