#!/bin/sh
#Removes compilation artifacts and other temporary files.
cd ${0%%/*}

#Clear binaries (leave Documentation and Portable intact)
rm -rf build/Backend build/Frontend build/Tools build/Setup

#Clear object caches
rm -f vs/*.cache
rm -rf vs/*/obj
rm -f vs/*/.pidb
