# Zero Install for Windows

[![Build status](https://img.shields.io/appveyor/ci/0install/0install-win.svg)](https://ci.appveyor.com/project/0install/0install-win)  
This is the Windows version of Zero Install. It extends the cross-platform core [Zero Install .NET](https://github.com/0install/0install-dotnet) with a GUI and various OS-specific integrations.

Zero Install is a decentralized cross-platform software installation system. You can learn more at [0install.net](https://0install.net/).

**[Download Zero Install for Windows](https://0install.net/injector.html#windows)**

## Building

The source code is in [`src/`](src/) and generated artifacts are placed in `artifacts/`.  
The source code does not contain version numbers. Instead the version is determined during CI using [GitVersion](https://gitversion.net/).

To build install [Visual Studio 2019 v16.8 or newer](https://www.visualstudio.com/downloads/) and run `.\build.ps1`.  
If you wish to deploy the build after compilation as the default Zero Install instance in your user profile run `.\build.ps1 -Deploy`. To deploy it for all users use `.\build.ps1 -Deploy -Machine`.

## Contributing

We welcome contributions to this project such as bug reports, recommendations, pull requests and [translations](https://www.transifex.com/eicher/0install-win/). If you have any questions feel free to pitch in on our [friendly mailing list](https://0install.net/support.html#lists).

This repository contains an [EditorConfig](http://editorconfig.org/) file. Please make sure to use an editor that supports it to ensure consistent code style, file encoding, etc.. For full tooling support for all style and naming conventions consider using JetBrains' [ReSharper](https://www.jetbrains.com/resharper/) or [Rider](https://www.jetbrains.com/rider/) products.

## Privacy and code signing policy

Zero Install for Windows contacts various servers automatically during normal operation (e.g., to check for updates). No personal information is transmitted to these systems unless specifically requested by the user (e.g., to synchronize apps between computers). See the [documentation](https://docs.0install.net/details/servers/) for details.

This program uses free code signing provided by [SignPath.io](https://signpath.io/) and a certificate by the [SignPath Foundation](https://sig.fo/). Signed releases are published by [Bastian Eicher](https://github.com/bastianeicher).
