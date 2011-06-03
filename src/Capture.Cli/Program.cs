/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using System.Reflection;
using Common;
using NDesk.Options;
using ZeroInstall.Capture.Cli.Properties;

namespace ZeroInstall.Capture.Cli
{
    #region Enumerations
    /// <summary>
    /// An errorlevel is returned to the original caller after the application terminates, to indicate success or the reason for failure.
    /// </summary>
    public enum ErrorLevel
    {
        ///<summary>Everything is OK.</summary>
        OK = 0,

        /// <summary>The user canceled the operation.</summary>
        UserCanceled = 1,

        /// <summary>The arguments passed on the command-line were not valid.</summary>
        InvalidArguments = 2,

        /// <summary>An unknown or not supported feature was requested.</summary>
        NotSupported = 3,

        /// <summary>An IO error occurred.</summary>
        IOError = 10,

        /// <summary>An network error occurred.</summary>
        WebError = 11
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for capturing application installations to feeds.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            // Automatically show help for missing args
            if (args.Length == 0) args = new[] { "--help" };

            ParseResults results;
            try { results = ParseArgs(args); }
            #region Error handling
            catch (UserCancelException)
            {
                // This is reached if --help, --version or similar was used
                return 0;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            try { return Execute(results); }
            #region Error hanlding
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.IOError;
            }
            #endregion
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <returns>The options detected by the parsing process.</returns>
        /// <exception cref="UserCancelException">Thrown if the user asked to see help information, version information, etc..</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="args"/> contains unknown options.</exception>
        public static ParseResults ParseArgs(IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Prepare a structure for storing settings found in the arguments
            var parseResults = new ParseResults();

            #region Define options
            var options = new OptionSet
            {
                // Version information
                {"V|version", Resources.OptionVersion, unused => {
                    var assembly = Assembly.GetEntryAssembly().GetName();
                    Console.WriteLine(@"Zero Install Capture CLI v{0}", assembly.Version);
                    throw new UserCancelException();
                }},

                // ToDo
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                PrintUsage();
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);

                throw new UserCancelException();
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            var additionalArgs = options.Parse(args);
            // ToDo: Determine capture directory

            // Return the now filled results structure
            return parseResults;
        }
        #endregion

        #region Help
        private static void PrintUsage()
        {
            const string usage = "{0}\t{1}\n\t{2}\n\t{3}\n\t{4}\n";
            //Console.WriteLine(usage, Resources.Usage, Resources.UsageInit, Resources.UsageCapturePre, Resources.UsageCapturePost, Resources.UsageCollect);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <returns>The error code to end the process with.</returns>
        private static int Execute(ParseResults results)
        {
            // ToDo
            return 0;
        }
        #endregion

        //--------------------//

        // ToDo
    }
}
