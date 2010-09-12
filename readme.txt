The directory "vs" contains the Visual Studio project with the actual source code.
The directory "lib" contains pre-compiled 3rd party libraries. Their licensing conditions are detailed in "3rd party code.txt".
The directory "doc" contains scripts for generating source code and developer documentation.
The directory "setup" contains scripts for creating a Windows Installer.
The directory "wrappers" contains soon to be deprecated C# wrappers around the original Python code.
The directory "build" contains the results of various compilation processes. It is created on first usage. It can contain the following subdirectories:
- Backend: Contains the libraries forming the Zero Install Backend.
- Frontend: Contains the executables for the Zero Install Frontend plus all required libraries (including the Backend).
- Tools: Contains the executables for Zero Install Tools such as the Feed Editor plus all required libraries (including the Backend).
- Documentation: Contains the generated source code documentation.
- Setup: Contains generated Setup EXE files.
- Portable: You MUST place a properly configured portable Python distribution and a portable GnuPG distribution here.

To create a suitable Python distribution:
- Install Python 2.6 for Windows, pygobject and pywin32. Copy the files to build\Portable\Python.
- Place a GTK+ installation at "%apps%\GTK+" (some libraries need to be extracted from there).
- Execute cleanup_python.cmd to minimize the Python distribution and copy some libraries to make it portable.
- Perform a Git checkout of git://repo.or.cz/zeroinstall/solver.git.
- Call "[SVN_CHECKOUT_DIR]\build\Portable\Python\python.exe setup.py install" in the directory of the GIT checkout to install the Zero Install scripts into the portable Python distribution.

To create a suitable GnuPG distribution:
- Extract a current GnuPG 2.x for Windows release to build\Portable\GnuPG.
- Remove the documentation directory.
- Copy iconv.dll from the GTK+ installation into build\Portable\GnuPG.


"build.cmd" will call the build scripts in the subdirectories to create a complete Zero Install for Windows distribution in build/Frontend/Setup. With the command-line argument "doc" it will create source code documentation instead.
Please read the "readme.txt" files in the subfolders before using this script.

"cleanup.cmd" will delete any temporary files created by the build process or Visual Studio.
It is recommended that you run this script once before opening the Visual Studio project for the first time.

Open "UnitTests.nunit" with NUnit (http://nunit.org/) to run Unit tests for the solution.
Note: You must perform a Debug build in Visual Studio first (see vs/readme.txt) before you can run the Unit tests.