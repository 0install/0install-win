/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Exports feeds and implementations.
    /// </summary>
    public class Exporter : IExporter
    {
        #region Dependencies
        private readonly IFeedCache _feedCache;
        private readonly IOpenPgp _openPgp;
        private readonly IStore _store;

        /// <summary>
        /// Creates a new exporter.
        /// </summary>
        /// <param name="feedCache">Used to get local feed files.</param>
        /// <param name="openPgp">Used to get export keys feeds were signed with.</param>
        /// <param name="store">Used to get cached implementations.</param>
        public Exporter([NotNull] IFeedCache feedCache, [NotNull] IOpenPgp openPgp, [NotNull] IStore store)
        {
            _feedCache = feedCache;
            _openPgp = openPgp;
            _store = store;
        }
        #endregion

        /// <inheritdoc/>
        public void ExportFeeds(Selections selections, string path)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var feedUris = selections.Implementations.Select(x => x.FromFeed ?? x.InterfaceUri).Distinct().ToList();

            foreach (var feedUri in feedUris)
            {
                Log.Info("Exporting feed " + feedUri);
                File.Copy(
                    sourceFileName: _feedCache.GetPath(feedUri),
                    destFileName: Path.Combine(path, feedUri.PrettyEscape() + ".xml"),
                    overwrite: true);
            }

            foreach (var signature in feedUris.SelectMany(_feedCache.GetSignatures).OfType<ValidSignature>().Distinct())
            {
                Log.Info("Exporting GPG key " + signature.FormatKeyID());
                _openPgp.DeployPublicKey(signature, path);
            }
        }

        /// <inheritdoc/>
        public void ExportImplementations(Selections selections, string path, ITaskHandler handler)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            foreach (var digest in selections.Implementations.Select(x => x.ManifestDigest).Distinct())
            {
                string sourcePath = _store.GetPath(digest);
                if (sourcePath == null)
                {
                    Log.Warn("Implementation " + digest + " missing from cache");
                    continue;
                }

                using (var generator = ArchiveGenerator.Create(sourcePath, Path.Combine(path, digest.Best + ".tbz2"), Archive.MimeTypeTarBzip))
                    handler.RunTask(generator);
            }
        }

        /// <inheritdoc/>
        public void DeployBootstrap(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = typeof(Exporter).GetEmbedded("import.cmd"))
                stream.CopyToFile(Path.Combine(path, "import.cmd"));
        }
    }
}