// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Commands;
using ZeroInstall.Commands.Basic;

ProgramUtils.Init();

using var handler = new CliCommandHandler();
return (int)ProgramUtils.Run("0install", args.Prepend(StoreMan.Name), handler);
