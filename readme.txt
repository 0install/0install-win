The directory "vs" contains the Visual Studio project with the actual source code.
The directory "lib" contains pre-compiled 3rd party libraries. Their licensing conditions are detailed in "3rd party code.txt".
The directory "doc" contains scripts for generating source code and developer documentation.
The directory "setup" contains scripts for creating a Windows Installer.
The directory "build" contains the results of various compilation processes. It is created on first usage. It can contain the following subdirectories:
- Backend: Contains the libraries forming the Zero Install Backend.
- Frontend: Contains the executables for the Zero Install Frontend plus all required libraries (including the Backend).
- Tools: Contains the executables for Zero Install Tools such as the Feed Editor plus all required libraries (including the Backend).
- Documentation: Contains the generated source code documentation.
- Setup: Contains generated Setup EXE files.
- Bundled: Contains a portable Python distribution (Windows only), a portable GnuPG distribution (Windows only) and the Python solver script (all platforms). See below how to get these files.

To create a portable Python distribution:
- Install Python 2.6.x for Windows with no additional features selected in the MSI installer.
- Install compatible versions of pygobject and pywin32.
- Copy the entire content of C:\Python26 to build\Bundled\Python.
- Place a GTK+ 2.x installation at %apps%\GTK+ (some libraries need to be extracted from there).
- Execute cleanup_python.cmd to minimize the Python distribution and copy some libraries to make it portable.
- You may now uninstall the Python installation located at C:\Python26 if you wish.

To create a portable GnuPG distribution:
- Extract GnuPG 1.4.x for Windows to build\Bundled\GnuPG.
- Remove the documentation directory.
- Copy iconv.dll from the GTK+ installation into build\Bundled\GnuPG.

To get the Python solver script:
- Perform a Git clone of git://repo.or.cz/zeroinstall/solver.git.
- Open a console and change the current directory to the GIT checkout.
- On Windows call "install_full.cmd [SVN_CHECKOUT_DIR]\build\Bundled\Python".
- On Linux call "install_copy.sh [SVN_CHECKOUT_DIR]/build/Bundled/Python".


"build.cmd" will call the build scripts in the subdirectories to create a complete Zero Install for Windows distribution in build/Frontend/Setup. With the command-line argument "doc" it will create source code documentation instead.
Please read the "readme.txt" files in the subfolders before using this script.
"build.sh" will perform a partial compilation using Mono's xbuild on Unix-like systems. Unit tests and the setup package will not be built.

"cleanup.cmd" will delete any temporary files created by the build process or Visual Studio.
It is recommended that you run this script once before opening the Visual Studio project for the first time.
"cleanup.sh" will delete any temporary files created by the build process on Unix-like systems.

Open "UnitTests.nunit" with NUnit (http://nunit.org/) to run Unit tests for the solution.
Note: You must perform a Debug build in Visual Studio first (see vs/readme.txt) before you can run the Unit tests.