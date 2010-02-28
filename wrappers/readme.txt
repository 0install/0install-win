This folder contains the C# source code for the Windows wrappers / launchers for Zero Install Python commands.

The launchers start off by checking the bundled Python, GnuPG and GTK+ distributions are available. They then modify the process PATH to use these non-shared distributions.
If everything checks out the Python interpreter is launched. Any given command-line arguments are passed through to the script. Some (GUI related) modifications to the arguments may be made to ensure correct behavior.

The wrappers named like the Python scripts themselves (e.g. 0launch.exe for 0launch) are command-line applications and use python.exe.
The wrappers with a "w" appended to the name (e.g. 0launchw.exe for 0launch) are GUI applications and use pythonw.exe.

The "build.cmd" script will compile the wrappers using the Visual C# 2005 compiler (.NET Framework 2.0 or later).