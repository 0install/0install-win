#!/bin/sh
set -e
cd `dirname $0`

src/build.sh
doc/build.sh
release/build.sh
