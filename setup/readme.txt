This folder contains the Inno Setup Script for the Zero Install Windows Setup.

The Script assumes vs/build.cmd has already been executed.
A portable Python 2.6 distribution with installed Zero Install scripts must be located at build/Portable/python.
A portable GnuPG distribution must be located at bin/Portable/gnupg.
A portable GTK+ 2 distribution must be located at bin/Portable/gtk.

Code based on this project is used to add the install directory to the System PATH:
http://legroom.net/software/modpath

Code based on this project is used to automatically download and install dependencies such as the .NET Framework:
http://www.codeproject.com/KB/install/dotnetfx_innosetup_instal.aspx

The "build.cmd" script will build the Setup using Inno Setup 5. It assumes Inno Setup 5 with Preprocessor support has been installed to its default directory.
Download: Inno Setup QuickStart Pack from http://www.jrsoftware.org/isdl.php#qsp

To perform an unattended installed launch the Setup with the following arguments:
/silent /suppressmsgboxes /norestart
This causes the dependency downloads to start without any confirmation.