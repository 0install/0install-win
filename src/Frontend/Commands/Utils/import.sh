#!/bin/bash

CONTENT_DIR=`dirname $0`/content
for f in "$CONTENT_DIR/*.xml"; do 0install import "$f"; done
for f in "$CONTENT_DIR/*.tbz2"; do 0install store add $(basename "$f" .tbz2) "$f"; done
