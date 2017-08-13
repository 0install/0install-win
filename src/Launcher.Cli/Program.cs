/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using NanoByte.Common.Collections;
using ZeroInstall.Commands;
using ZeroInstall.Commands.CliCommands;

namespace ZeroInstall.Launcher.Cli
{
    /// <summary>
    /// A shorcut for '0install run'.
    /// </summary>
    /// <seealso cref="Run"/>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static int Main(string[] args)
        {
            ProgramUtils.Init();

            using (var handler = new CliCommandHandler())
                return (int)ProgramUtils.Run("0install", args.Prepend(Run.Name), handler);
        }
    }
}
