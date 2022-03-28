// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

ProgramUtils.Init();

using var handler = new CliTaskHandler();
return (int)ProgramUtils.Run(args, handler, gui: false);
