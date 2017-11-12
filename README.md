Zero Install for Windows
========================

Zero Install is a decentralized cross-platform software-installation system available under the LGPL.

[![TeamCity Build status](https://0install.de/teamcity/app/rest/builds/buildType:(id:ZeroInstall_Windows_Build)/statusIcon)](https://0install.de/teamcity/viewType.html?buildTypeId=ZeroInstall_Windows_Build&guest=1)

**[Download Zero Install for Windows](http://0install.de/downloads/)**

Zero Install for Windows is built upon the **[Zero Install .NET Backend](https://github.com/0install/0install-dotnet)**.

Directory structure
-------------------
- `src` contains source code.
- `lib` contains pre-compiled 3rd party libraries which are not available via NuGet.
- `doc` contains scripts for generating source code documentation.
- `nuget` contains NuGet spec files used to generate NuGet packages for Zero Install.
- `powershell` contains scripts for creating PowerShell modules.
- `build` contains the results of various compilation processes. It is created on first usage.
- `release` contains scripts for creating a Zero Install feed and archive for publishing a build.

`VERSION` contains the version number used by build scripts.
Use `.\Set-Version.ps1 "X.Y.Z"` in PowerShall to change the version number. This ensures that the version also gets set in other locations (e.g. AssemblyInfo).

Building
--------
- You need to install [Visual Studio 2017](https://www.visualstudio.com/downloads/) to build this project.
- The file `VERSION` contains the current version number of the project.
- Run `.\Set-Version.ps1 "X.Y.Z"` in PowerShall to change the version number. This ensures that the version also gets set in other locations (e.g. `GlobalAssemblyInfo.cs`).
- Run `.\build.ps1` in PowerShell to build everything.
- If you wish to deploy the release after compilation as the default Zero Install instance in your user profile run `.\build.ps1 -Deploy`.
- To deploy it for all users use `.\build.ps1 -Deploy -Machine`.

Environment variables
---------------------
- `ZEROINSTALL_PORTABLE_BASE`: Set by the C# code to to inform the OCaml code of Portable mode.
- `ZEROINSTALL_EXTERNAL_FETCHER`: Set by the C# code to make the OCaml code delegate downloading files back to the C# implementation.
- `ZEROINSTALL_EXTERNAL_STORE`: Set by the C# code to make the OCaml code delegate extracting archives back to the C# implementation.
