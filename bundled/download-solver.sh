#!/bin/sh
#This script will download and extract the external solver to the current directory

echo "Downloading external solver..."
wget -q http://0install.de/files/zero-install-solver.zip

echo "Extracting external solver..."
unzip -qo zero-install-solver.zip

rm zero-install-solver.zip