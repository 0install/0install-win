Zero Install
============
Zero Install is a decentralized cross-platform software-installation system available under the LGPL.

- **[Website](http://0install.de/)**
- **[Developer information](http://0install.de/dev/)**


Source directory structure
--------------------------
- The directory `src` contains the Visual Studio solution with the actual source code.
- The directory `lib` contains pre-compiled 3rd party libraries which are not available via NuGet.
- The directory `doc` contains scripts for generating source code documentation.
- The directory `nuget` contains NuGet spec files used to generate NuGet packages for Zero Install.
- The directory `publish` contains scripts for creating feeds and archives for publishing.
- The directory `build` contains the results of various compilation processes. It is created on first usage. It can contain the following subdirectories:
  - `Debug`|`DebugNet35`|`DebugNet35`: Contains Debug builds targeting the .NET Framework 4.0, 3.5 and 2.0 respectively.
  - `Release`|`ReleaseNet35`|`ReleaseNet35`: Contains Release builds targeting the .NET Framework 4.0, 3.5 and 2.0 respectively.
    - `Backend`: Contains the libraries forming the Zero Install Backend.
    - `Frontend`: Contains the executables for the Zero Install Frontend plus all required libraries (including the Backend).
    - `Tools`: Contains the executables for Zero Install Tools such as the Feed Editor plus all required libraries (including the Backend).
    - `Samples`: Contains the executables for the Zero Install API samples.
  - `Bootstrap`: Contains a standalone executable enabling end-users to aquire published feeds and archives.
  - `Documentation`: Contains the generated source code documentation.
  - `Packages`: Contains the generated NuGet packages.
  - `Publish`: Contains the generated feeds and archives for publishing.
- The top-level directory contains local Zero Install feeds referencing the contents of the `build` directory. They can be registered with `0install add-feed` in order to replace the online versions of Zero Install and its tools with your local builds.

`VERSION` contains the version number used by build scripts.
Use `.\Set-Version.ps1 "X.Y.Z"` in PowerShall to change the version number. This ensures that the version also gets set in other locations (e.g. AssemblyInfo).


Building on Windows
-------------------
`build.cmd` will call build scripts in subdirectories to create a Zero Install for Windows release.

If you wish to add an AuthentiCode signature to the compiled binaries set the `signing_cert_path` environment variable to the certificate's file path and `signing_cert_pass` to the password used to decrypt the file before executing the build scripts.
For example:
```
set signing_cert_path=C:\mycert.pfx
set signing_cert_pass=mypass
build.cmd
```

If you wish to deploy the release after compilation as the default Zero Install instance in your user profile run `build.cmd deploy`. To deploy it for all users use `build.cmd deploy --machine` instead.

`cleanup.cmd` will delete any temporary files created by the build process or Visual Studio.


Building on Linux
-----------------
`build.sh` will perform a partial debug compilation using Mono's xbuild. Some parts of a full release can currently only be built on Windows.

`cleanup.sh` will delete any temporary files created by the xbuild build process.

`test.sh` will run the unit tests using the NUnit console runner.
Note: You must perform a Debug build first (using `src/build.sh`) before you can run the unit tests.


Environment variables
---------------------
- `ZEROINSTALL_PORTABLE_BASE`: Set by the C# code to to inform the OCaml code of Portable mode.
- `ZEROINSTALL_EXTERNAL_FETCHER`: Set by the C# code to make the OCaml code delegate downloading files back to the C# implementation.
- `ZEROINSTALL_EXTERNAL_STORE`: Set by the C# code to make the OCaml code delegate extracting archives back to the C# implementation.
