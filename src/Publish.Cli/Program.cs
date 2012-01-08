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
    public static partial class Program
    {
        #region Startup
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static int Main(string[] args)
        {
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
                return 0;
            }
            catch (OptionException ex)
            {
                Log.Error(ex.Message);
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
            catch (KeyNotFoundException ex)
            {
                Log.Error(ex.Message);
                return (int)ErrorLevel.InvalidArguments;
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
            catch (NotSupportedException ex)
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
        /// <exception cref="OperationCanceledException">Thrown if the user asked to see help information, version information, etc..</exception>
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
                {
                    "V|version", Resources.OptionVersion, unused =>
                    {
                        Console.WriteLine(AppInfo.Name + " " + AppInfo.Version + Environment.NewLine + AppInfo.Copyright + Environment.NewLine + Resources.LicenseInfo);
                        throw new OperationCanceledException(); // Don't handle any of the other arguments
                    }
                    },
                // Mode selection
                {
                    "catalog=", Resources.OptionCatalog, delegate(string catalogFile)
                    {
                        if (string.IsNullOrEmpty(catalogFile)) return;
                        parseResults.Mode = OperationMode.Catalog;
                        parseResults.CatalogFile = catalogFile;
                    }
                    },
                // Modiciations
                {"add-missing", Resources.OptionAddMissing, unused => parseResults.AddMissing = true},
                {"store-downloads", Resources.OptionsStoreDownloads, unused => parseResults.StoreDownloads = true},
                // Signatures
                {"x|xmlsign", Resources.OptionXmlSign, unused => parseResults.XmlSign = true},
                {"u|unsign", Resources.OptionUnsign, unused => parseResults.Unsign = true},
                {"k|key=", Resources.OptionKey, user => parseResults.Key = user},
                {"gpg-passphrase=", Resources.OptionGnuPGPassphrase, user => parseResults.OpenPgpPassphrase = user},
            };
            #endregion

            #region Help text
            options.Add("h|help|?", Resources.OptionHelp, unused =>
            {
                PrintUsage();
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);

                // Don't handle any of the other arguments
                throw new OperationCanceledException();
            });
            #endregion

            // Parse the arguments and call the hooked handlers
            var additionalArgs = options.Parse(args);
            try
            {
                parseResults.Feeds = ArgumentUtils.GetFiles(additionalArgs, "*.xml");
            }
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
        /// <param name="options">The parser results to be executed.</param>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the operation.</exception>
        /// <exception cref="OptionException">Thrown if the specified feed file paths were invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if a feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if a feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a feed file or the catalog file is not permitted.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if an OpenPGP key could not be found.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        /// <exception cref="NotSupportedException">Thrown if a MIME type doesn't belong to a known and supported archive type.</exception>
        private static ErrorLevel Execute(ParseResults options)
        {
            switch (options.Mode)
            {
                case OperationMode.Normal:
                    if (options.Feeds.Count == 0)
                    {
                        Log.Error(string.Format(Resources.MissingArguments, "0publish"));
                        return ErrorLevel.InvalidArguments;
                    }

                    foreach (var file in options.Feeds)
                    {
                        var feed = SignedFeed.Load(file.FullName);
                        HandleModify(feed.Feed, options);
                        SaveFeed(feed, file.FullName, options);
                    }
                    return ErrorLevel.OK;

                case OperationMode.Catalog:
                    // Default to using all XML files in the current directory
                    if (options.Feeds.Count == 0)
                        options.Feeds = ArgumentUtils.GetFiles(new[] {Environment.CurrentDirectory}, "*.xml");

                    var catalog = new Catalog();

                    foreach (var feedFile in options.Feeds)
                    {
                        var feed = Feed.Load(feedFile.FullName);
                        feed.Strip();
                        catalog.Feeds.Add(feed);
                    }

                    if (catalog.Feeds.IsEmpty) throw new FileNotFoundException(Resources.NoFeedFilesFound);

                    catalog.Save(options.CatalogFile);
                    XmlStorage.AddStylesheet(options.CatalogFile, "catalog.xsl");
                    return ErrorLevel.OK;

                default:
                    Log.Error(string.Format(Resources.UnknownMode, "0publish"));
                    return ErrorLevel.NotSupported;
            }
        }

        /// <summary>
        /// Saves a feed applying any signature options.
        /// </summary>
        /// <param name="feed">The feed to save.</param>
        /// <param name="path">The path to save the feed to.</param>
        /// <param name="options">The parser results to be handled.</param>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a feed file or the catalog file is not permitted.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if an OpenPGP key could not be found.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        private static void SaveFeed(SignedFeed feed, string path, ParseResults options)
        {
            if (options.Unsign)
            {
                // Remove any existing signatures
                feed.SecretKey = null;
            }
            else
            {
                var openPgp = OpenPgpProvider.Default;

                // Use default secret key if there are no existing signatures
                if (options.XmlSign && feed.SecretKey == null)
                    feed.SecretKey = openPgp.GetSecretKey(null);

                // Use specific secret key for signature
                if (!string.IsNullOrEmpty(options.Key))
                    feed.SecretKey = openPgp.GetSecretKey(options.Key);
            }

            // Ask for passphrase to unlock secret key
            if (feed.SecretKey != null && string.IsNullOrEmpty(options.OpenPgpPassphrase))
                options.OpenPgpPassphrase = CliUtils.ReadPassword(Resources.PleaseEnterGnuPGPassphrase);

            feed.Save(path, options.OpenPgpPassphrase);
        }
        #endregion
    }
}
