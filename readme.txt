"build.cmd" will call the build scripts in the subdirectories to create a complete Zero Install for Windows distribution in bin/Setup.
Please read the "readme.txt" files in the subfolders before using this script.

"cleanup.cmd" will delete any temporary files created by the build process or Visual Studio.
It is recommended that you run this script once before opening the Visual Studio project for the first time.

Open "Unit Tests.nunit" with NUnit (http://nunit.org/) to run Unit tests for the solution.
Note: You must perform a Debug build in Visual Studio first (see vs/readme.txt) before you can run the Unit tests.