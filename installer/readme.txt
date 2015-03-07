This folder contains an Inno Setup Script for creating a Windows installer.
The resulting installer executable is placed in ..\build\Setup.

The "build.cmd" script assumes "..\src\build.cmd Release" has already been executed.
An external solver must be located at ..\bundled. Please consult the main "readme.txt" for details on how to get this.

Code based on this project is used to add the install directory to the System PATH:
http://legroom.net/software/modpath

Code based on this project is used to automatically download and install dependencies such as the .NET Framework:
http://www.codeproject.com/KB/install/dotnetfx_innosetup_instal.aspx

The "build.cmd" script will build the installer using Inno Setup 5.5.5 or newer. It assumes Inno Setup with Preprocessor support has been installed to its default directory.
Download: Inno Setup from http://www.jrsoftware.org/isdl.php
Note: You must use the non-Unicode version of Zero Install for compatibility with isxdl.dll.

To perform an unattended installed launch the installer with the following arguments:
/silent /suppressmsgboxes /norestart
This causes the dependency downloads to start without any confirmation.