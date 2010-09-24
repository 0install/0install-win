using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Common;
using Common.CliControls;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Publish.Arguments;
using ZeroInstall.Publish.Cli.Properties;

namespace ZeroInstall.Publish.Cli
{
    #region Enumerations
    /// <summary>
    /// A list of errorlevels that are returned to the original caller after the application terminates, to indicate success or the reason for failure.
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
    /// Command-line application for modifiying <see cref="Feed"/>s.
    /// </summary>
    public static class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            ParseResults results;
            OperationMode mode;

            try { mode = ParseArgs(args, out results); }
            #region Error handling
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            switch (mode)
            {
                case OperationMode.Normal:
                    if (string.IsNullOrEmpty(results.Feed))
                    {
                        Log.Error(string.Format(Resources.MissingArguments, "0launch"));
                        return (int)ErrorLevel.InvalidArguments;
                    }
                    
                    return Execute(results);

                case OperationMode.Version:
                    // ToDo: Read version number from assembly data
                    Console.WriteLine(@"Zero Install Injector CLI v{0}", Assembly.GetEntryAssembly().GetName().Version);
                    return (int)ErrorLevel.OK;

                case OperationMode.Help:
                    return (int)ErrorLevel.OK;

                default:
                    Log.Error("Unknown operation mode");
                    return (int)ErrorLevel.NotSupported;
            }
        }
        #endregion

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The arguments to be parsed.</param>
        /// <param name="results">The options detected by the parsing process.</param>
        /// <returns>The operation mode selected by the parsing process.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        public static OperationMode ParseArgs(IEnumerable<string> args, out ParseResults results)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            // Prepare a structure for storing settings found in the arguments
            var mode = OperationMode.Normal;
            var parseResults = new ParseResults();

            #region Define options
            var options = new OptionSet
            {
                // Mode selection
                {"V|version", Resources.OptionVersion, unused => mode = OperationMode.Version},
                
                {"x|xmlsign", Resources.OptionXmlSign, unused => parseResults.XmlSign = true},
                {"u|unsign", Resources.OptionUnsign, unused => parseResults.Unsign = true},
                
                {"gpg-user=", Resources.OptionGnuPGUser, user => parseResults.GnuPGUser = user},
                {"gpg-passphrase=", Resources.OptionGnuPGPassphrase, user => parseResults.GnuPGPassphrase = user},
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                mode = OperationMode.Help;

                Console.WriteLine(@"Usage: 0publish [options] feed.xml");
                options.WriteOptionDescriptions(Console.Out);
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            var additionalArgs = options.Parse(args);
            if (additionalArgs.Count > 0) parseResults.Feed = additionalArgs[0];

            // Return the now filled results structure
            results = parseResults;
            return mode;
        }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <returns>The error level to report to the original caller. See <see cref="ErrorLevel"/>.</returns>
        private static int Execute(ParseResults results)
        {
            if (results.Unsign)
            {
                try { FeedUtils.UnsignFeed(results.Feed); }
                #region Error hanlding
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
            if (results.XmlSign)
            {
                if (string.IsNullOrEmpty(results.GnuPGPassphrase))
                    results.GnuPGPassphrase = InputUtils.ReadPassword("Please enter your GnuPG passphrase: ");

                try { FeedUtils.SignFeed(results.Feed, results.GnuPGUser, results.GnuPGPassphrase); }
                #region Error hanlding
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
                catch (UnhandledErrorsException ex)
                {
                    Log.Error(ex.Message);
                    return (int)ErrorLevel.IOError;
                }
                #endregion
            }

            return (int)ErrorLevel.OK;
        }
        #endregion
    }
}
