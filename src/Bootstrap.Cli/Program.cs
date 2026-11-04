// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Bootstrap;

ProgramUtils.Init();

using var handler = new CliBootstrapHandler();
return (int)ProgramUtils.Run(args, handler);
