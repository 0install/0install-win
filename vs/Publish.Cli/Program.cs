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
using System.IO;
using System.Reflection;
using Common;
using Common.Cli;
using Common.Storage;
using NDesk.Options;
using ZeroInstall.Model;
using ZeroInstall.Publish.Arguments;
using ZeroInstall.Publish.Cli.Properties;

namespace ZeroInstall.Publish.Cli
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
                    if (results.Feeds.Count == 0)
                    {
                        Log.Error(string.Format(Resources.MissingArguments, "0publish"));
                        return (int)ErrorLevel.InvalidArguments;
                    }

                    try { Execute(results); }
                    #region Error hanlding
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
                    catch (UnhandledErrorsException ex)
                    {
                        Log.Error(ex.Message);
                        return (int)ErrorLevel.IOError;
                    }
                    #endregion

                    return (int)ErrorLevel.OK;

                case OperationMode.Catalog:
                    // Default to using all XML files in the current directory
                    if (results.Feeds.Count == 0)
                        results.Feeds = ArgumentUtils.GetFiles(new[] {Environment.CurrentDirectory}, "*.xml");

                    try { CreateCatalog(results); }
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

                    return (int)ErrorLevel.OK;

                case OperationMode.Version:
                    Console.WriteLine(@"Zero Install Publish CLI v{0}", Assembly.GetEntryAssembly().GetName().Version);
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
                {"catalog=", Resources.OptionCatalog, catalogFile => { mode = OperationMode.Catalog; parseResults.CatalogFile = catalogFile; } },

                // Signatures
                {"x|xmlsign", Resources.OptionXmlSign, unused => parseResults.XmlSign = true},
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
            parseResults.Feeds = ArgumentUtils.GetFiles(additionalArgs, "*.xml");

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
        /// <exception cref="InvalidOperationException">Thrown if the feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public static void Execute(ParseResults results)
        {
            foreach (var feed in results.Feeds)
            {
                // Always start off by removing signatures since they will become invalid if anything is changed
                FeedUtils.UnsignFeed(feed.FullName);
                FeedUtils.AddStylesheet(feed.FullName);

                if (results.XmlSign)
                {
                    if (string.IsNullOrEmpty(results.GnuPGPassphrase))
                        results.GnuPGPassphrase = InputUtils.ReadPassword(Resources.PleaseEnterGnuPGPassphrase);

                    FeedUtils.SignFeed(feed.FullName, results.GnuPGUser, results.GnuPGPassphrase);
                }
            }
        }
        #endregion

        #region Catalog
        /// <summary>
        /// Creates a <see cref="Catalog"/> from the <see cref="Feed"/>s specified in the command-line arguments.
        /// </summary>
        /// <param name="results">The parser results to be executed.</param>
        /// <exception cref="ArgumentException">Throw if the specified feed file paths were invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed files could not be found.</exception>
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
