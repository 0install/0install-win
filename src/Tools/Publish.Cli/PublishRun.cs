/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Cli;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Info;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NanoByte.Common.Undo;
using NDesk.Options;
using ZeroInstall.Publish.Cli.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;
using SharedResources = ZeroInstall.Publish.Properties.Resources;

namespace ZeroInstall.Publish.Cli
{
    /// <summary>
    /// Represents a single run of the 0publish tool.
    /// </summary>
    public sealed class PublishRun : IDisposable
    {
        #region Variables
        /// <summary>
        /// List of operational modes for the feed editor that can be selected via command-line arguments.
        /// </summary>
        private enum OperationMode
        {
            /// <summary>Modify an existing <see cref="Feed"/> or create a new one.</summary>
            Normal,

            /// <summary>Combine all specified <see cref="Feed"/>s into a single <see cref="Catalog"/> file.</summary>
            Catalog
        }

        /// <summary>
        /// The operational mode for the feed editor.
        /// </summary>
        private OperationMode _mode;

        /// <summary>
        /// The feeds to apply the operation on.
        /// </summary>
        private ICollection<FileInfo> _feeds;

        /// <summary>
        /// The file to store the aggregated <see cref="Catalog"/> data in.
        /// </summary>
        private string _catalogFile;

        /// <summary>
        /// Download missing archives, calculate manifest digests, etc..
        /// </summary>
        private bool _addMissing;

        /// <summary>
        /// Add any downloaded archives to the implementation store.
        /// </summary>
        private bool _keepDownloads;

        /// <summary>
        /// Add XML signature blocks to the feesd.
        /// </summary>
        private bool _xmlSign;

        /// <summary>
        /// Remove any existing signatures from the feeds.
        /// </summary>
        private bool _unsign;

        /// <summary>
        /// A key specifier (key ID, fingerprint or any part of a user ID) for the secret key to use to sign the feeds.
        /// </summary>
        /// <remarks>Will use existing key or default key when left at <see langword="null"/>.</remarks>
        [CanBeNull]
        private string _key;

        /// <summary>
        /// The passphrase used to unlock the <see cref="OpenPgpSecretKey"/>.
        /// </summary>
        private string _openPgpPassphrase;
        #endregion

        #region Constructor
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <exception cref="OperationCanceledException">The user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException"><paramref name="args"/> contains unknown options.</exception>
        public PublishRun(IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            #endregion

            var additionalArgs = BuildOptions().Parse(args);
            try
            {
                _feeds = ArgumentUtils.GetFiles(additionalArgs, "*.xml");
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                // Report as an invalid command-line argument
                throw new OptionException(ex.Message, "");
            }
            #endregion
        }

        private OptionSet BuildOptions()
        {
            var options = new OptionSet
            {
                // Version information
                {
                    "V|version", () => Resources.OptionVersion, unused =>
                    {
                        Console.WriteLine(AppInfo.Current.Name + @" " + AppInfo.Current.Version + Environment.NewLine + AppInfo.Current.Copyright + Environment.NewLine + SharedResources.LicenseInfo);
                        throw new OperationCanceledException(); // Don't handle any of the other arguments
                    }
                },

                // Mode selection
                {
                    "catalog=", () => Resources.OptionCatalog, delegate(string catalogFile)
                    {
                        if (string.IsNullOrEmpty(catalogFile)) return;
                        _mode = OperationMode.Catalog;
                        _catalogFile = catalogFile;
                    }
                },

                // Modifications
                {"add-missing", () => Resources.OptionAddMissing, unused => _addMissing = true},
                {"keep-downloads", () => Resources.OptionsKeepDownloads, unused => _keepDownloads = true},

                // Signatures
                {"x|xmlsign", () => Resources.OptionXmlSign, unused => _xmlSign = true},
                {"u|unsign", () => Resources.OptionUnsign, unused => _unsign = true},
                {"k|key=", () => Resources.OptionKey, user => _key = user},
                {"gpg-passphrase=", () => Resources.OptionGnuPGPassphrase, passphrase => _openPgpPassphrase = passphrase}
            };
            options.Add("h|help|?", () => Resources.OptionHelp, unused =>
            {
                var usages = new[] {Resources.UsageFeed};
                // ReSharper disable LocalizableElement
                Console.WriteLine(Resources.Usage + "\t" + string.Join(Environment.NewLine + "\t", usages) + "\n");
                // ReSharper restore LocalizableElement
                Console.WriteLine(Resources.Options);
                options.WriteOptionDescriptions(Console.Out);

                // Don't handle any of the other arguments
                throw new OperationCanceledException();
            });
            return options;
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        /// <exception cref="OptionException">The specified feed file paths were invalid.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="InvalidDataException">A feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">A feed file could not be found.</exception>
        /// <exception cref="IOException">A file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a feed file or the catalog file is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An existing digest does not match the newly calculated one.</exception>
        /// <exception cref="KeyNotFoundException">An OpenPGP key could not be found.</exception>
        /// <exception cref="NotSupportedException">A MIME type doesn't belong to a known and supported archive type.</exception>
        public ExitCode Execute()
        {
            switch (_mode)
            {
                case OperationMode.Normal:
                    if (_feeds.Count == 0)
                    {
                        Log.Error(string.Format(Resources.MissingArguments, "0publish"));
                        return ExitCode.InvalidArguments;
                    }

                    foreach (var feedEditing in _feeds.Select(file => FeedEditing.Load(file.FullName)))
                    {
                        HandleModify(feedEditing);
                        SaveFeed(feedEditing);
                    }
                    return ExitCode.OK;

                case OperationMode.Catalog:
                    // Default to using all XML files in the current directory
                    if (_feeds.Count == 0)
                        _feeds = ArgumentUtils.GetFiles(new[] {Environment.CurrentDirectory}, "*.xml");

                    var catalog = new Catalog();
                    foreach (var feed in _feeds.Select(feedFile => XmlStorage.LoadXml<Feed>(feedFile.FullName)))
                    {
                        feed.Strip();
                        catalog.Feeds.Add(feed);
                    }
                    if (catalog.Feeds.Count == 0) throw new FileNotFoundException(Resources.NoFeedFilesFound);

                    SaveCatalog(catalog);
                    return ExitCode.OK;

                default:
                    Log.Error(string.Format(Resources.UnknownMode, "0publish"));
                    return ExitCode.InvalidArguments;
            }
        }
        #endregion

        #region Storage
        /// <summary>
        /// Saves a feed.
        /// </summary>
        /// <exception cref="IOException">A file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a feed file is not permitted.</exception>
        /// <exception cref="KeyNotFoundException">An OpenPGP key could not be found.</exception>
        private void SaveFeed(FeedEditing feedEditing)
        {
            if (_unsign)
            {
                // Remove any existing signatures
                feedEditing.SignedFeed.SecretKey = null;
            }
            else
            {
                var openPgp = OpenPgpFactory.CreateDefault();
                if (_xmlSign)
                { // Signing explicitly requested
                    if (feedEditing.SignedFeed.SecretKey == null)
                    { // No previous signature
                        // Use user-specified key or default key
                        feedEditing.SignedFeed.SecretKey = openPgp.GetSecretKey(_key);
                    }
                    else
                    { // Existing siganture
                        if (!string.IsNullOrEmpty(_key)) // Use new user-specified key
                            feedEditing.SignedFeed.SecretKey = openPgp.GetSecretKey(_key);
                        //else resign implied
                    }
                }
                //else resign implied
            }

            // If no signing or unsigning was explicitly requested and the content did not change
            // there is no need to overwrite (and potentiall resign) the file
            if (!_xmlSign && !_unsign && !feedEditing.Changed) return;

            while (true)
            {
                try
                {
                    Debug.Assert(feedEditing.Path != null);
                    feedEditing.SignedFeed.Save(feedEditing.Path, _openPgpPassphrase);
                    break;
                }
                catch (WrongPassphraseException ex)
                {
                    if (!string.IsNullOrEmpty(_openPgpPassphrase)) Log.Error(ex);
                }

                // Ask for passphrase to unlock secret key if we were unable to save without it
                _openPgpPassphrase = CliUtils.ReadPassword(string.Format(Publish.Properties.Resources.AskForPassphrase, feedEditing.SignedFeed.SecretKey));
            }
        }

        /// <summary>
        /// Saves a catalog.
        /// </summary>
        /// <param name="catalog">The catalog to save.</param>
        /// <exception cref="IOException">A file could not be read or written or if the GnuPG could not be launched or the catalog file could not be written.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a catalog file is not permitted.</exception>
        /// <exception cref="KeyNotFoundException">An OpenPGP key could not be found.</exception>
        private void SaveCatalog(Catalog catalog)
        {
            if (_xmlSign)
            {
                var openPgp = OpenPgpFactory.CreateDefault();
                var signedCatalog = new SignedCatalog(catalog, openPgp.GetSecretKey(_key));

                while (true)
                {
                    try
                    {
                        signedCatalog.Save(_catalogFile, _openPgpPassphrase);
                        break;
                    }
                    catch (WrongPassphraseException ex)
                    {
                        if (!string.IsNullOrEmpty(_openPgpPassphrase)) Log.Error(ex);
                    }

                    // Ask for passphrase to unlock secret key if we were unable to save without it
                    _openPgpPassphrase = CliUtils.ReadPassword(string.Format(Publish.Properties.Resources.AskForPassphrase, signedCatalog.SecretKey));
                }
            }
            else catalog.SaveXml(_catalogFile);
        }
        #endregion

        #region Modify
        private readonly ITaskHandler _handler = new CliTaskHandler();

        /// <summary>
        /// Applies user-selected modifications to a feed.
        /// </summary>
        /// <param name="feedEditing">The feed to modify.</param>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">There is a problem access a temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a temporary file is not permitted.</exception>
        /// <exception cref="DigestMismatchException">An existing digest does not match the newly calculated one.</exception>
        private void HandleModify(FeedEditing feedEditing)
        {
            if (_addMissing)
                AddMissing(feedEditing.SignedFeed.Feed.Elements, feedEditing);
        }

        private void AddMissing(IEnumerable<Element> elements, ICommandExecutor executor)
        {
            new PerTypeDispatcher<Element>(ignoreMissing: true)
            {
                (Implementation implementation) => implementation.AddMissing(_handler, executor, _keepDownloads),
                (Group group) => AddMissing(group.Elements, executor) // recursion
            }.Dispatch(elements);
        }
        #endregion

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}
