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
using Common;
using Common.Cli;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Publish.Cli.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Publish.Cli
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
        IOError = 10
    }
    #endregion

    /// <summary>
    /// Launches a command-line tool for editing Zero Install feed XMLs.
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
            catch (OptionException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            #endregion

            try { return (int)Execute(results); }
            #region Error hanlding
            catch (UserCancelException)
            {
                return (int)ErrorLevel.UserCanceled;
            }
            catch (OptionException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message));
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
            catch (WrongPassphraseException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
            }
            catch (UnhandledErrorsException ex)
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
        /// <exception cref="OptionException">Thrown if <paramref name="args"/> contains unknown options.</exception>
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
                    Console.WriteLine(AppInfo.Name + " " + AppInfo.Version + Environment.NewLine + AppInfo.Copyright + Environment.NewLine + Resources.LicenseInfo);
                    throw new UserCancelException(); // Don't handle any of the other arguments
                }},

                // Mode selection
                {"catalog=", Resources.OptionCatalog, delegate(string catalogFile)
                {
                    if (string.IsNullOrEmpty(catalogFile)) return;
                    parseResults.Mode = OperationMode.Catalog;
                    parseResults.CatalogFile = catalogFile;
                }},

                // Signatures
                {"x|xmlsign", Resources.OptionXmlSign, unused => parseResults.XmlSign = true},
                {"u|unsign", Resources.OptionXmlSign, unused => parseResults.Unsign = true},
                {"k|key=", Resources.OptionKey, user => parseResults.Key = user},
                {"gpg-passphrase=", Resources.OptionGnuPGPassphrase, user => parseResults.GnuPGPassphrase = user},
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                PrintUsage();
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);

                // Don't handle any of the other arguments
                throw new UserCancelException();
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            var additionalArgs = options.Parse(args);
            try { parseResults.Feeds = ArgumentUtils.GetFiles(additionalArgs, "*.xml"); }
            #region Error handling
            catch (FileNotFoundException ex)
            {
                // Report as an invalid command-line argument
                throw new OptionException(ex.Message, "", ex);
            }
            #endregion

            // Return the now filled results structure
            return parseResults;
        }
        #endregion

        #region Help
        private static void PrintUsage()
        {
            var usages = new[] {Resources.UsageFeed};
            Console.WriteLine(Resources.Usage + "\t" + StringUtils.Concatenate(usages, Environment.NewLine + "\t") + "\n");
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="UserCancelException">Thrown if the user canceled the operation.</exception>
        /// <exception cref="OptionException">Thrown if the specified feed file paths were invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if a feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if a feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a feed file or the catalog file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        private static ErrorLevel Execute(ParseResults results)
        {
            switch (results.Mode)
            {
                case OperationMode.Normal:
                    if (results.Feeds.Count == 0)
                    {
                        Log.Error(string.Format(Resources.MissingArguments, "0publish"));
                        return ErrorLevel.InvalidArguments;
                    }

                    ModifyFeeds(results);
                    return ErrorLevel.OK;

                case OperationMode.Catalog:
                    // Default to using all XML files in the current directory
                    if (results.Feeds.Count == 0)
                        results.Feeds = ArgumentUtils.GetFiles(new[] { Environment.CurrentDirectory }, "*.xml");

                    CreateCatalog(results);
                    return ErrorLevel.OK;

                default:
                    Log.Error(string.Format(Resources.UnknownMode, "0publish"));
                    return ErrorLevel.NotSupported;
            }
        }
        #endregion

        //--------------------//

        #region Modify feeds
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="InvalidDataException">Thrown if the feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        public static void ModifyFeeds(ParseResults results)
        {
            foreach (var file in results.Feeds)
            {
                bool wasSigned = false;

                var feed = Feed.Load(file.FullName);

                // ToDo: Apply modifications

                feed.Save(file.FullName);

                // Always remove existing signatures since they will become invalid if anything is changed
                FeedUtils.UnsignFeed(file.FullName);
                FeedUtils.AddStylesheet(file.FullName);

                if ((wasSigned && !results.Unsign) || results.XmlSign)
                {
                    if (string.IsNullOrEmpty(results.GnuPGPassphrase))
                        results.GnuPGPassphrase = CliUtils.ReadPassword(Resources.PleaseEnterGnuPGPassphrase);

                    FeedUtils.SignFeed(file.FullName, results.Key, results.GnuPGPassphrase);
                }
            }
        }
        #endregion

        #region Catalog
        /// <summary>
        /// Creates a <see cref="Catalog"/> from the <see cref="Feed"/>s specified in the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="OptionException">Thrown if the specified feed file paths were invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if a feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the a files could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a feed file or the catalog file is not permitted.</exception>
        public static void CreateCatalog(ParseResults results)
        {
            var catalog = new Catalog();

            foreach (var feed in results.Feeds)
                catalog.Feeds.Add(Feed.Load(feed.FullName));

            if (catalog.Feeds.IsEmpty) throw new FileNotFoundException(Resources.NoFeedFilesFound);

            catalog.Simplify();
            catalog.Save(results.CatalogFile);
            XmlStorage.AddStylesheet(results.CatalogFile, "catalog.xsl");
        }
        #endregion
    }
}
