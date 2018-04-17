// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Collections;
using ZeroInstall.Commands;
using ZeroInstall.Commands.CliCommands;

namespace ZeroInstall.Alias.Cli
{
    /// <summary>
    /// A shortcut for '0install add-alias'.
    /// </summary>
    /// <seealso cref="AddAlias"/>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static int Main(string[] args)
        {
            ProgramUtils.Init();

            using (var handler = new CliCommandHandler())
                return (int)ProgramUtils.Run("0install", args.Prepend(AddAlias.Name), handler);
        }
    }
}
