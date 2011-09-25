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
using Common.Collections;
using Common.Compression;
using Common.Net;
using Common.Storage;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Publish.Cli
{
    public static partial class Program
    {
        private static readonly ITaskHandler _handler = new CliTaskHandler();

        /// <summary>
        /// Applies user-selected modifications to a feed.
        /// </summary>
        /// <param name="feed">The feed to modify.</param>
        /// <param name="options">The modifications to apply.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the operation.</exception>
        /// <exception cref="IOException">Thrown if there is a problem access a temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to a temporary file is not permitted.</exception>
        private static void HandleModify(Feed feed, ParseResults options)
        {
            if (options.AddMissing)
                AddMissing(feed.Elements, options.StoreDownloads);
        }

        private static void AddMissing(IEnumerable<Element> elements, bool store)
        {
            foreach (var element in elements)
            {
                var implementation = element as Implementation;
                if (implementation != null)
                    AddMissing(implementation, store);
                else
                {
                    var group = element as Group;
                    if (group != null) AddMissing(group.Elements, store);
                }
            }
        }

        private static void AddMissing(Implementation implementation, bool store)
        {
            if (implementation.ManifestDigest == default(ManifestDigest))
            {
                foreach (var archive in EnumerableUtils.OfType<Archive>(implementation.RetrievalMethods))
                {
                    var digest = DownloadMissing(archive, store);
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

        private static ManifestDigest DownloadMissing(Archive archive, bool store)
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
                    if (store)
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
    }
}
