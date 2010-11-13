/*
 * Copyright 2010 Bastian Eicher
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

using System;
using System.Collections.Generic;
using System.Reflection;
using Common;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Store.Management.Cli.Properties;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.Cli
{
    #region Enumerations
    /// <summary>
    /// An errorlevel is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum ErrorLevel
    {
        OK = 0,
        UserCanceled = 1,
        InvalidArguments = 2,
        NotSupported = 3,
        IOError = 10,
        WebError = 11,
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for managing caches of Zero Install implementations.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            IList<string> options;
            try { options = ParseArgs(args); }
            #region Error handling
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            if (options.Count == 0) return (int)ErrorLevel.OK;

            switch (options[0])
            {
                case "add":
                case "copy":
                case "find":
                    Log.Error("Not implemented yet");
                    return (int)ErrorLevel.NotSupported;

                case "remove":
                    StoreProvider.Default.Remove(new ManifestDigest(options[1]));
                    return (int)ErrorLevel.OK;

                case "list":
                    foreach (string implementation in StoreProvider.Default.ListAll())
                        Console.WriteLine(implementation);
                    return (int)ErrorLevel.OK;

                case "manifest":
                case "optimise":
                case "verify":
                    Log.Error("Not implemented yet");
                    return (int)ErrorLevel.NotSupported;

                default:
                    Log.Error("Unknown command");
                    return (int)ErrorLevel.NotSupported;
            }
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The arguments to be parsed.</param>
        /// <returns>Any unparsed commands left over.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        public static IList<string> ParseArgs(IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"V|version", Resources.OptionVersion, unused => Console.WriteLine(@"Zero Install Store management CLI v{0}", Assembly.GetEntryAssembly().GetName().Version)}
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                Console.WriteLine(@"Usage:
    0store add DIGEST ARCHIVE [EXTRACT]
    0store copy DIRECTORY [DIRECTORY]
    0store find DIGEST
    0store remove DIGEST
    0store list
    0store manifest DIRECTORY [ALGORITHM]
    0store optimise [CACHE]
    0store verify (DIGEST|DIRECTORY)");
                options.WriteOptionDescriptions(Console.Out);
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            return options.Parse(args);
        }
        #endregion
    }
}