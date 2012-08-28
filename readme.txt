The directory "src" contains the Visual Studio project with the actual source code.
The directory "lib" contains pre-compiled 3rd party libraries. Their licensing conditions are detailed in "3rd party code.txt".
The directory "doc" contains scripts for generating source code and developer documentation.
The directory "setup" contains scripts for creating a Windows Installer.
The directory "bundled" contains a portable GnuPG distribution (Windows only) and an external solver (multiple platforms). See below how these files are aquired.
The directory "build" contains the results of various compilation processes. It is created on first usage. It can contain the following subdirectories:
- Backend: Contains the libraries forming the Zero Install Backend.
- Frontend: Contains the executables for the Zero Install Frontend plus all required libraries (including the Backend).
- Tools: Contains the executables for Zero Install Tools such as the Feed Editor plus all required libraries (including the Backend).
- Setup: Contains generated ZIP archives and Setup EXE files.
- Documentation: Contains the generated source code documentation.

To add a portable GnuPG distribution:
- Copy GnuPG 1.4.x for Windows to bundled/GnuPG.
- Copy iconv.dll (e.g. from a GTK+ installation) into bundled/GnuPG.

To add the external solver:
- Install Python 2.7.x for Windows.
- Install compatible versions of pygobject, pywin32 and py2exe.
- Perform a Git clone of git://repo.or.cz/zeroinstall/solver.git.
- Open a console and change the current directory to the GIT checkout.
- Call "python setup.py py2exe".
- Copy the content of the "dist" directory as well as the "0solve" file to bundled/Solver.
or
- Download http://0install.de/files/zero-install-solver.zip.
- Extract the archive to bundled.



Windows:

"build.cmd" will call build scripts in subdirectories to create a complete Zero Install for Windows release in build/Frontend/Setup.
Note: Please read setup/readme.txt aswell for information about required tools.

"cleanup.cmd" will delete any temporary files created by the build process or Visual Studio.

Open "UnitTests.nunit" with the NUnit GUI (http://nunit.org/) to run the unit tests.
Note: You must perform a Debug build first (using Visual Studio or src/build.cmd) before you can run the unit tests.



Linux:

"build.sh" will perform a partial debug compilation using Mono's xbuild. A setup package will not be built.

"cleanup.sh" will delete any temporary files created by the xbuild build process.

"test.sh" will run the unit tests using the NUnit console runner.
Note: You must perform a Debug build first (using MonoDevelop or src/build.sh) before you can run the unit tests.



Environment variables:

$ZEROINSTALL_PORTABLE_BASE: Used by the C# to to inform the Python code of Portable mode.
