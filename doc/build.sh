#!/bin/bash
set -e
cd `dirname $0`

rm -rf ../build/Documentation
mkdir -p ../build/Documentation
mono ../build/Release/0install.exe run http://0install.de/feeds/Doxygen.xml
