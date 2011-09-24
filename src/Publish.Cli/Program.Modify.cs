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
using Common.Cli;
using Common.Collections;
using Common.Compression;
using Common.Net;
using Common.Storage;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Publish.Cli.Properties;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Publish.Cli
{
    public static partial class Program
    {
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <param name="options">The parser results to be executed.</param>
        /// <exception cref="InvalidDataException">Thrown if the feed file is damaged.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the feed file could not be found.</exception>
        /// <exception cref="IOException">Thrown if a file could not be read or written or if the GnuPG could not be launched or the feed file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the feed file is not permitted.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if a OpenPGP key could not be found.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        public static void ModifyFeeds(ParseResults options)
        {
            foreach (var file in options.Feeds)
            {
                var feed = SignedFeed.Load(file.FullName);

                HandleModifications(feed.Feed, options);
                HandleSigning(feed, ref options);

                feed.Save(file.FullName, options.GnuPGPassphrase);
            }
        }
        
        private static readonly ITaskHandler _handler = new CliTaskHandler();

        private static void HandleModifications(Feed feed, ParseResults options)
        {
            if (options.AddMissing)
                AddMissing(feed.Elements, false);
        }

        private static void AddMissing(IEnumerable<Element> elements, bool cache)
        {
            foreach (var element in elements)
            {
                var implementation = element as Implementation;
                if (implementation != null)
                    AddMissing(implementation, cache);
                else
                {
                    var group = element as Group;
                    if (group != null) AddMissing(group.Elements, cache);
                }
            }
        }

        private static void AddMissing(Implementation implementation, bool cache)
        {
            if (implementation.ManifestDigest == default(ManifestDigest))
            {
                foreach (var archive in EnumerableUtils.OfType<Archive>(implementation.RetrievalMethods))
                {
                    var digest = DownloadMissing(archive, cache);
                    if (implementation.ManifestDigest == default(ManifestDigest))
                    {
                        implementation.ManifestDigest = digest;
                        if (string.IsNullOrEmpty(implementation.ID)) implementation.ID = "sha1new=" + digest.Sha1New;
                    }
                    else if (digest != implementation.ManifestDigest)
                        throw new DigestMismatchException(implementation.ManifestDigest.ToString(), null, digest.ToString(), null);
                }
            }
        }

        private static ManifestDigest DownloadMissing(Archive archive, bool cache)
        {
            if (string.IsNullOrEmpty(archive.MimeType)) archive.MimeType = ArchiveUtils.GuessMimeType(archive.Location.ToString());

            using (var tempFile = new TemporaryFile("0publish"))
            {
                _handler.RunTask(new DownloadFile(archive.Location, tempFile.Path), null);

                using (var tempDir = new TemporaryDirectory("0publish"))
                {
                    using (var extractor = Extractor.CreateExtractor(archive.MimeType, tempFile.Path, 0, tempDir.Path))
                    {
                        extractor.SubDir = archive.Extract;
                        _handler.RunTask(extractor, null);
                    }

                    var digest = Manifest.CreateDigest(tempDir.Path, _handler);
                    if (cache)
                    {
                        try
                        {
                            StoreProvider.CreateDefault().AddDirectory(tempDir.Path, digest, _handler);
                        }
                        catch (ImplementationAlreadyInStoreException)
                        {}
                    }

                    archive.Size = new FileInfo(tempFile.Path).Length;
                    return digest;
                }
            }
        }

        private static void HandleSigning(SignedFeed feed, ref ParseResults options)
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
            if (feed.SecretKey != null && string.IsNullOrEmpty(options.GnuPGPassphrase))
                options.GnuPGPassphrase = CliUtils.ReadPassword(Resources.PleaseEnterGnuPGPassphrase);
        }
    }
}
