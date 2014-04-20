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
using System.IO;
using System.Linq;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.EntryPoints;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Builds basic single-<see cref="Implementation"/> <see cref="Feed"/>s.
    /// </summary>
    public class FeedBuilder : IDisposable
    {
        #region Dependencies
        private readonly ITaskHandler _handler;

        /// <summary>
        /// Creates a new feed builder.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        public FeedBuilder(ITaskHandler handler)
        {
            _handler = handler;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Used for <see cref="Feed.Uri"/>.
        /// </summary>
        public Uri Uri { get; set; }

        private readonly List<Icon> _icons = new List<Icon>();

        /// <summary>
        /// Used for <see cref="Feed.Icons"/>.
        /// </summary>
        public ICollection<Icon> Icons { get { return _icons; } }

        /// <summary>
        /// Provides meta-data and startup instructions for the application.
        /// </summary>
        public Candidate Candidate { get; set; }

        /// <summary>
        /// A retrieval method for the single <see cref="Implementation"/> in the <see cref="Feed"/>.
        /// </summary>
        public RetrievalMethod RetrievalMethod { get; set; }

        private TemporaryDirectory _temporaryDirectory;

        /// <summary>
        /// A temporary directory (usually used as the <see cref="ImplementationDirectory"/>). Not used by the <see cref="FeedBuilder"/> itself.
        /// </summary>
        /// <remarks>Setting a new value will automatically <see cref="IDisposable.Dispose"/> the previous one.</remarks>
        public TemporaryDirectory TemporaryDirectory
        {
            get { return _temporaryDirectory; }
            set
            {
                if (_temporaryDirectory != null) _temporaryDirectory.Dispose();
                _temporaryDirectory = value;
            }
        }

        private string _workingDirectory;

        /// <summary>
        /// The final implementation directory as it would be created by the <see cref="RetrievalMethod"/>.
        /// </summary>
        /// <remarks>Setting this value causes a new <see cref="ManifestDigest"/> to be calculated.</remarks>
        public string ImplementationDirectory
        {
            get { return _workingDirectory; }
            set
            {
                _workingDirectory = value;
                ManifestDigest = CalculateDigest(value);
            }
        }

        /// <summary>
        /// Calculates the <see cref="ManifestDigest"/> of a specific directory.
        /// </summary>
        private ManifestDigest CalculateDigest(string path)
        {
            var digest = new ManifestDigest();

            // Generate manifest for each available format...
            foreach (var generator in ManifestFormat.Recommended.Select(format => new ManifestGenerator(path, format)))
            {
                // ... and add the resulting digest to the return value
                _handler.RunTask(generator);
                ManifestDigest.ParseID(generator.Result.CalculateDigest(), ref digest);
            }

            return digest;
        }

        /// <summary>
        /// Used for <see cref="ImplementationBase.ManifestDigest"/>.
        /// </summary>
        public ManifestDigest ManifestDigest { get; private set; }

        /// <summary>
        /// Used for <see cref="SignedFeed.SecretKey"/>.
        /// </summary>
        public OpenPgpSecretKey SecretKey { get; set; }
        #endregion

        //--------------------//

        #region Build
        /// <summary>
        /// Generates the feed described by the properties.
        /// </summary>
        public SignedFeed Build()
        {
            var feed = new Feed
            {
                Name = Candidate.Name,
                Uri = Uri,
                Summaries = {Candidate.Summary},
                NeedsTerminal = Candidate.NeedsTerminal,
                Elements =
                {
                    new Group
                    {
                        Architecture = Candidate.Architecture,
                        Commands = {Candidate.Command},
                        Elements =
                        {
                            new Implementation
                            {
                                ID = "sha1new=" + ManifestDigest.Sha1New,
                                ManifestDigest = ManifestDigest,
                                Version = Candidate.Version,
                                RetrievalMethods = {RetrievalMethod}
                            }
                        }
                    }
                },
                EntryPoints =
                {
                    new EntryPoint
                    {
                        Command = Command.NameRun,
                        Names = {Candidate.Name},
                        BinaryName = Path.GetFileNameWithoutExtension(Candidate.RelativePath)
                    }
                }
            };
            feed.Icons.AddRange(_icons);

            return new SignedFeed(feed, SecretKey);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Deletes the <see cref="TemporaryDirectory"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Do not trigger via GC
            if (!disposing) return;

            if (_temporaryDirectory != null) _temporaryDirectory.Dispose();
        }
        #endregion
    }
}
