#!/bin/sh
#This script will download and extract the external solver to the current directory

echo "Downloading external solver..."
wget -q --no-check-certificate https://0install.de/teamcity/repository/downloadAll/bt29/.lastSuccessful/artifacts.zip

echo "Extracting external solver..."
unzip -qo zero-install-solver.zip

rm zero-install-solver.zip