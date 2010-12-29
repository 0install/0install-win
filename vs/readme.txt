This folder contains the source code for Zero Install for Windows (Backend, Frontend and Tools).

The "build.cmd" script will compile the code using the Visual Studio 2008 or 2010 build environment on Windows.
The "build.sh" script will compile the code using Mono's xbuild on Unix-like systems.

Use "ZeroInstall_VS2008.sln" to open the solution in Visual Studio 2008 (Service Pack 1 recommended).
Use "ZeroInstall_VS2010.sln" to open the solution in Visual Studio 2010.
Use "ZeroInstall_VCE2010.sln" to open the solution in Visual C# Express 2010.
Use "ZeroInstall_MonoDevelop.sln" to open the solution in MonoDevelop 2.4 or newer. (currently doesn't work properly due to an old version of NUnit being bundled)
Use "ZeroInstall_xbuild.sln" to compile with xbuild.
Don't use "ZeroInstall_xbuild.sln" to compile with xbuild without NUnit tests.