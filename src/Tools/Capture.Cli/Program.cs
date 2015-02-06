/*
 * Copyright 2011 Bastian Eicher
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
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Net;
using NDesk.Options;
using ZeroInstall.Capture.Cli.Properties;
using SharedResources = ZeroInstall.Capture.Properties.Resources;

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

        /// <summary>A warning regarding potentially unintended usage caused the operation to be canceled.</summary>
        Warning = 9,

        /// <summary>An IO error occurred.</summary>
        IOError = 10
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
        private static int Main(string[] args)
        {
            NetUtils.ApplyProxy();

            // Automatically show help for missing args
            if (args == null) args = new string[0];
            if (args.Length == 0) args = new[] {"--help"};

            ParseResults results;
            try
            {
                results = ParseArgs(args);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                // This is reached if --help, --version or similar was used
                return (int)ErrorLevel.OK;
            }
            catch (OptionException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            try
            {
                return (int)Execute(results);
            }
                #region Error hanlding
            catch (OperationCanceledException)
            {
                return (int)ErrorLevel.UserCanceled;
            }
            catch (OptionException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (IOException ex)
            {
                Log.Error(ex);
                return (int)ErrorLevel.IOError;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
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
        /// <exception cref="OperationCanceledException">The user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException"><paramref name="args"/> contains unknown options.</exception>
        public static ParseResults ParseArgs([NotNull, ItemNotNull] IEnumerable<string> args)
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
                {
                    "V|version", () => Resources.OptionVersion, unused =>
                    {
                        var assembly = Assembly.GetEntryAssembly().GetName();
                        Console.WriteLine(@"Zero Install Capture CLI v{0}", assembly.Version);
                        throw new OperationCanceledException();
                    }
                },
                {"f|force", () => Resources.OptionForce, unused => parseResults.Force = true},
                {
                    "installation-dir=", () => Resources.OptionInstallationDir, value =>
                    {
                        try
                        {
                            parseResults.InstallationDirectory = Path.GetFullPath(value);
                        }
                            #region Error handling
                        catch (ArgumentException ex)
                        {
                            // Wrap exception since only certain exception types are allowed
                            throw new OptionException(ex.Message, "installation-dir");
                        }
                        catch (NotSupportedException ex)
                        {
                            // Wrap exception since only certain exception types are allowed
                            throw new OptionException(ex.Message, "installation-dir");
                        }
                        #endregion
                    }
                },
                {"main-exe=", () => Resources.OptionMainExe, value => parseResults.MainExe = value},
                {"files", () => Resources.OptionFiles, unused => parseResults.GetFiles = true}
            };
            #endregion

            #region Help text
            options.Add("h|help|?", () => Resources.OptionHelp, unused =>
            {
                PrintUsage();
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);

                throw new OperationCanceledException();
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            var additionalArgs = options.Parse(args);

            // Determine the specific cature command to be executed
            if (additionalArgs.Count == 0) throw new OptionException(string.Format(Resources.MissingArguments, "0capture"), "");
            parseResults.Command = additionalArgs[0];

            // Determine the capture directory to use
            try
            {
                parseResults.DirectoryPath = (additionalArgs.Count >= 2) ? Path.GetFullPath(additionalArgs[1]) : Environment.CurrentDirectory;
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new OptionException(ex.Message, "");
            }
            catch (NotSupportedException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new OptionException(ex.Message, "");
            }
            #endregion

            // Return the now filled results structure
            return parseResults;
        }
        #endregion

        #region Help
        private static void PrintUsage()
        {
            var usage = new StringBuilder(Resources.Usage);
            foreach (string command in new[] {"init", "snap-pre", "snap-post", "collect"})
                usage.AppendLine("\t0capture " + command + " [DIRECTORY] [OPTIONS]");
            Console.WriteLine(usage);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        private static ErrorLevel Execute(ParseResults results)
        {
            switch (results.Command)
            {
                case "init":
                {
                    CaptureDir.Create(results.DirectoryPath);
                    Console.WriteLine(SharedResources.CaptureDirInitialized);
                    return ErrorLevel.OK;
                }

                case "snap-pre":
                {
                    var captureDir = CaptureDir.Open(results.DirectoryPath);

                    #region Safety warnings
                    if (captureDir.SnapshotPre != null && !results.Force)
                    {
                        Log.Error(Resources.WarnOverwritePreInstallSnapshot);
                        return ErrorLevel.Warning;
                    }
                    if (captureDir.SnapshotPost != null && !results.Force)
                    {
                        Log.Error(Resources.WarnExistingPostInstallSnapshot);
                        return ErrorLevel.Warning;
                    }
                    #endregion

                    captureDir.TakeSnapshotPre();
                    Console.WriteLine(SharedResources.PreInstallSnapshotCreated);
                    return ErrorLevel.OK;
                }

                case "snap-post":
                {
                    var captureDir = CaptureDir.Open(results.DirectoryPath);

                    #region Safety warnings
                    if (captureDir.SnapshotPost != null && !results.Force)
                    {
                        Log.Error(Resources.WarnOverwritePostInstallSnapshot);
                        return ErrorLevel.Warning;
                    }
                    if (captureDir.SnapshotPre == null && !results.Force)
                    {
                        Log.Error(Resources.WarnMissingPreInstallSnapshot);
                        return ErrorLevel.Warning;
                    }
                    #endregion

                    captureDir.TakeSnapshotPost();
                    Console.WriteLine(SharedResources.PostInstallSnapshotCreated);
                    return ErrorLevel.OK;
                }

                case "collect":
                {
                    var captureDir = CaptureDir.Open(results.DirectoryPath);

                    #region Safety warnings
                    if (File.Exists(Path.Combine(captureDir.DirectoryPath, "feed.xml")) && !results.Force)
                    {
                        Log.Error(Resources.WarnOverwriteCollect);
                        return ErrorLevel.Warning;
                    }
                    #endregion

                    captureDir.Collect(results.InstallationDirectory, results.MainExe, results.GetFiles);
                    Console.WriteLine(SharedResources.InstallDataCollected);
                    return ErrorLevel.OK;
                }

                default:
                {
                    Log.Error(string.Format(Resources.UnknownMode, "0publish"));
                    return ErrorLevel.NotSupported;
                }
            }
        }
        #endregion
    }
}
