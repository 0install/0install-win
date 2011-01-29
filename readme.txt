The directory "src" contains the Visual Studio project with the actual source code.
The directory "lib" contains pre-compiled 3rd party libraries. Their licensing conditions are detailed in "3rd party code.txt".
The directory "doc" contains scripts for generating source code and developer documentation.
The directory "setup" contains scripts for creating a Windows Installer.
The directory "build" contains the results of various compilation processes. It is created on first usage. It can contain the following subdirectories:
- Backend: Contains the libraries forming the Zero Install Backend.
- Frontend: Contains the executables for the Zero Install Frontend plus all required libraries (including the Backend).
- Tools: Contains the executables for Zero Install Tools such as the Feed Editor plus all required libraries (including the Backend).
- Publish: Contains generated ZIP archives and Setup EXE files.
- Documentation: Contains the generated source code documentation.
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



Windows:

"build.cmd" will call build scripts in subdirectories to create a complete Zero Install for Windows release in build/Frontend/Setup.
Note: Please read setup/readme.txt aswell for information about required tools.

"cleanup.cmd" will delete any temporary files created by the build process or Visual Studio.
Note: It is recommended that you run this script once before opening the Visual Studio project for the first time.

Open "UnitTests.nunit" with the NUnit GUI (http://nunit.org/) to run the unit tests.
Note: You must perform a Debug build first (using Visual Studio or src/build.cmd) before you can run the unit tests.



Linux:

"build.sh" will perform a partial debug compilation using Mono's xbuild. A setup package will not be built.

"cleanup.sh" will delete any temporary files created by the xbuild build process.

"test.sh" will run the unit tests using the NUnit console runner.
Note: You must perform a Debug build first (using MonoDevelop or src/build.sh) before you can run the unit tests.