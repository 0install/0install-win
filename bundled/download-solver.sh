#!/bin/sh
#This script will download and extract the external solver to the current directory

echo "Downloading external solver..."
wget -q --no-check-certificate https://0install.de/teamcity/guestAuth/repository/downloadAll/bt29/.lastSuccessful/artifacts.zip

echo "Extracting external solver..."
unzip -qo artifacts.zip

rm artifacts.zip