This folder contains an Inno Setup Script for creating a Windows Installer.
The resulting setup executable is placed in ..\build\Setup.

The "build.cmd" script assumes "..\vs\build.cmd Release" has already been executed.
A properly configured portable Python distribution and a portable GnuPG distribution must be located at ..\build\Portable. Please consult the main "readme.txt" for details on how to create these portable distributions.

Code based on this project is used to add the install directory to the System PATH:
http://legroom.net/software/modpath

Code based on this project is used to automatically download and install dependencies such as the .NET Framework:
http://www.codeproject.com/KB/install/dotnetfx_innosetup_instal.aspx

The "build.cmd" script will build the Setup using Inno Setup 5. It assumes Inno Setup 5 with Preprocessor support has been installed to its default directory.
Download: Inno Setup QuickStart Pack from http://www.jrsoftware.org/isdl.php#qsp

To perform an unattended installed launch the Setup with the following arguments:
/silent /suppressmsgboxes /norestart
This causes the dependency downloads to start without any confirmation.