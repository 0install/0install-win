#!/bin/bash
set -e
cd `dirname $0`

rm -f ../build/Release/*.xml ../build/Release/*.pdb

mono ../build/Release/0install.exe run http://0install.net/tools/0template.xml ZeroInstall.xml.template version=$(< ../VERSION)
