The directory "vs" contains the Visual Studio project with the actual source code.
The directory "lib" contains pre-compiled 3rd party libraries. Their licensing conditions are detailed in "3rd party code.txt".
The directory "doc" contains scripts for generating source code and developer documentation.
The directory "setup" contains scripts for creating a Windows Installer.
The directory "wrappers" contains soon to be deprecated C# wrappers around the original Python code.

"build.cmd" will call the build scripts in the subdirectories to create a complete Zero Install for Windows distribution in build/Frontend/Setup. With the command-line argument "doc" it will create source code documentation instead.
Please read the "readme.txt" files in the subfolders before using this script.

"cleanup.cmd" will delete any temporary files created by the build process or Visual Studio.
It is recommended that you run this script once before opening the Visual Studio project for the first time.

Open "UnitTests.nunit" with NUnit (http://nunit.org/) to run Unit tests for the solution.
Note: You must perform a Debug build in Visual Studio first (see vs/readme.txt) before you can run the Unit tests.