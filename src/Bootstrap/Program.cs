// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

ProgramUtils.Init();

using var handler = new CliBootstrapHandler();
return (int)ProgramUtils.Run(args, handler);
