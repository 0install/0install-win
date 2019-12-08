# Zero Install for Windows

[![Build status](https://img.shields.io/appveyor/ci/0install/0install-win.svg)](https://ci.appveyor.com/project/0install/0install-win)  
This is the Windows version of Zero Install. It extends the cross-platform core [Zero Install .NET](https://github.com/0install/0install-dotnet) with a GUI and various OS-specific integrations.

Zero Install is a decentralized cross-platform software installation system. You can learn more at [0install.net](http://0install.net/).

**[Download Zero Install for Windows](http://0install.de/downloads/)**

## Building

The source code is in [`src/`](src/) and generated build artifacts are placed in `artifacts/`.  
There is a template in [`feed/`](feed/) for generating a [Zero Install feed](https://0install.github.io/docs/packaging/) from the artifacts. For official releases this is published at: http://0install.de/feeds/ZeroInstall.xml

You need [Visual Studio 2017 or 2019](https://www.visualstudio.com/downloads/) to build this project.

Run `.\build.ps1` in PowerShell to build everything. This script takes a version number as an input argument. The source code itself only contains dummy version numbers. The actual version is picked by continuous integration using [GitVersion](http://gitversion.readthedocs.io/).

If you wish to deploy the build after compilation as the default Zero Install instance in your user profile run `.\build.ps1 -Deploy`. To deploy it for all users use `.\build.ps1 -Deploy -Machine`.

## Contributing

We welcome contributions to this project such as bug reports, recommendations, pull requests and [translations](https://www.transifex.com/eicher/0install-win/). If you have any questions feel free to pitch in on our [friendly mailing list](http://0install.net/support.html#lists).

This repository contains an [EditorConfig](http://editorconfig.org/) file. Please make sure to use an editor that supports it to ensure consistent code style, file encoding, etc.. For full tooling support for all style and naming conventions consider using JetBrain's [ReSharper](https://www.jetbrains.com/resharper/) or [Rider](https://www.jetbrains.com/rider/) products.
