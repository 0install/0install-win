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

namespace ZeroInstall.Commands.Utils
{
    /// <summary>
    /// Exports feeds and implementations listed in a set of <see cref="Selections"/>.
    /// </summary>
    public class Exporter
    {
        private readonly Selections _selections;
        private readonly string _destination;

        /// <summary>
        /// Creates a new exporter-
        /// </summary>
        /// <param name="selections">A list of <see cref="ImplementationSelection"/>s to check for referenced feeds.</param>
        /// <param name="destination">The path of the directory to export to.</param>
        public Exporter([NotNull] Selections selections, [NotNull] string destination)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            #endregion

            _selections = selections;
            _destination = destination;
        }

        /// <summary>
        /// Exports all feeds listed in a <see cref="Selections"/> document along with any OpenPGP public key files required for validation.
        /// </summary>
        /// <param name="feedCache">Used to get local feed files.</param>
        /// <param name="openPgp">Used to get export keys feeds were signed with.</param>
        public void ExportFeeds([NotNull] IFeedCache feedCache, [NotNull] IOpenPgp openPgp)
        {
            #region Sanity checks
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            var feedUris = _selections.Implementations.Select(x => x.FromFeed ?? x.InterfaceUri).Distinct().ToList();

            foreach (var feedUri in feedUris)
            {
                Log.Info("Exporting feed " + feedUri);
                File.Copy(
                    sourceFileName: feedCache.GetPath(feedUri),
                    destFileName: Path.Combine(_destination, feedUri.PrettyEscape() + ".xml"),
                    overwrite: true);
            }

            foreach (var signature in feedUris.SelectMany(feedCache.GetSignatures).OfType<ValidSignature>().Distinct())
            {
                Log.Info("Exporting GPG key " + signature.FormatKeyID());
                openPgp.DeployPublicKey(signature, _destination);
            }
        }

        /// <summary>
        /// Exports all implementations listed in a <see cref="Selections"/> document as archives.
        /// </summary>
        /// <param name="store">Used to get cached implementations.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public void ExportImplementations(IStore store, ITaskHandler handler)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            foreach (var digest in _selections.Implementations.Select(x => x.ManifestDigest).Distinct())
            {
                string sourcePath = store.GetPath(digest);
                if (sourcePath == null)
                {
                    Log.Warn("Implementation " + digest + " missing from cache");
                    continue;
                }

                using (var generator = ArchiveGenerator.Create(sourcePath, Path.Combine(_destination, digest.Best + ".tbz2"), Archive.MimeTypeTarBzip))
                    handler.RunTask(generator);
            }
        }

        /// <summary>
        /// Deploys a bootstrap file for importing exported feeds and implementations.
        /// </summary>
        public void DeployImportScript()
        {
            typeof(Exporter).CopyEmbeddedToFile("import.cmd", Path.Combine(_destination, "import.cmd"));
        }
    }
}