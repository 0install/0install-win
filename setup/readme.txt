This folder contains an Inno Setup Script for creating a Windows Installer.
The resulting setup executable is placed in ..\build\Setup.

The "build.cmd" script assumes "..\vs\build.cmd Release" has already been executed.
A portable Python 2.6 distribution with installed Zero Install scripts must be located at ..\build\Portable\Python.
A portable GnuPG distribution must be located at ..\build\Portable\GnuPG.

Place a normal Python installation with pygobject and pywin32 in ..\build\Portable\Python. Place a GTK+ installation at "%apps%\GTK+" (some libraries need to be extracted from there).
Now execute cleanup_python.cmd to minimize it and make it portable. Then call "[SVN_CHECKOUT_DIR]\build\Portable\Python\python.exe setup.py install" in the directory of the Python code GIT checkout to install the Zero Install scripts.

Code based on this project is used to add the install directory to the System PATH:
http://legroom.net/software/modpath

Code based on this project is used to automatically download and install dependencies such as the .NET Framework:
http://www.codeproject.com/KB/install/dotnetfx_innosetup_instal.aspx

The "build.cmd" script will build the Setup using Inno Setup 5. It assumes Inno Setup 5 with Preprocessor support has been installed to its default directory.
Download: Inno Setup QuickStart Pack from http://www.jrsoftware.org/isdl.php#qsp

To perform an unattended installed launch the Setup with the following arguments:
/silent /suppressmsgboxes /norestart
This causes the dependency downloads to start without any confirmation.