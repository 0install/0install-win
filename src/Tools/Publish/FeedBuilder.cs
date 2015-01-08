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
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.EntryPoints;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store;
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
        #region Properties
        /// <summary>
        /// Used for <see cref="Feed.Uri"/>.
        /// </summary>
        public FeedUri Uri { get; set; }

        private readonly List<Icon> _icons = new List<Icon>();

        /// <summary>
        /// Used for <see cref="Feed.Icons"/>.
        /// </summary>
        public ICollection<Icon> Icons { get { return _icons; } }

        private readonly List<Candidate> _candidates = new List<Candidate>();

        /// <summary>
        /// Auto-detected candidates for entry points.
        /// </summary>
        public IEnumerable<Candidate> Candidates { get { return _candidates; } }

        /// <summary>
        /// The main entry point. Provides meta-data and startup instructions for the application.
        /// Should be one of the auto-detected <see cref="Candidates"/>.
        /// </summary>
        public Candidate MainCandidate { get; set; }

        /// <summary>
        /// A retrieval method for the single <see cref="Implementation"/> in the <see cref="Feed"/>.
        /// </summary>
        public RetrievalMethod RetrievalMethod { get; set; }

        private TemporaryDirectory _temporaryDirectory;

        /// <summary>
        /// A temporary directory to prepare files for <see cref="SetImplementationDirectory"/>. Not used by the <see cref="FeedBuilder"/> itself.
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

        #region Implementation directory
        /// <summary>
        /// Sets the final implementation directory as it would be created by the <see cref="RetrievalMethod"/>.
        /// Calculates the <see cref="ManifestDigest"/> and auto-detects <see cref="Candidates"/>.
        /// </summary>
        /// <param name="implementationDirectory">The implementation directory. Should be a subdirectory of <see cref="TemporaryDirectory"/>.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">There was a problem generating the manifest or detectng the executables.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to temporary files was not permitted.</exception>
        public void SetImplementationDirectory([NotNull] string implementationDirectory, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(implementationDirectory)) throw new ArgumentNullException("implementationDirectory");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var newDigest = new ManifestDigest();
            // Generate manifest for each available format...
            foreach (var generator in ManifestFormat.Recommended.Select(format => new ManifestGenerator(implementationDirectory, format)))
            {
                // ... and add the resulting digest to the return value
                handler.RunTask(generator);
                newDigest.ParseID(generator.Result.CalculateDigest());
            }
            ManifestDigest = newDigest;

            _candidates.Clear();
            handler.RunTask(new SimpleTask(Resources.DetectingCandidates,
                () => _candidates.AddRange(Detection.ListCandidates(new DirectoryInfo(implementationDirectory)))));
            MainCandidate = _candidates.FirstOrDefault();
        }
        #endregion

        #region Build
        /// <summary>
        /// Generates the feed described by the properties.
        /// </summary>
        public SignedFeed Build()
        {
            #region Sanity checks
            if (MainCandidate == null) throw new InvalidOperationException("MainCandidate is not set.");
            #endregion

            var implementation =
                new Implementation
                {
                    ID = @"sha1new=" + ManifestDigest.Sha1New,
                    ManifestDigest = ManifestDigest,
                    Version = MainCandidate.Version,
                    Architecture = MainCandidate.Architecture,
                    Commands = {MainCandidate.CreateCommand()},
                    RetrievalMethods = {RetrievalMethod}
                };
            var feed = new Feed
            {
                Name = MainCandidate.Name,
                Uri = Uri,
                Summaries = {MainCandidate.Summary},
                NeedsTerminal = MainCandidate.NeedsTerminal,
                Elements = {implementation},
                EntryPoints =
                {
                    new EntryPoint
                    {
                        Command = Command.NameRun,
                        Names = {MainCandidate.Name},
                        BinaryName = Path.GetFileNameWithoutExtension(MainCandidate.RelativePath)
                    }
                }
            };
            feed.Icons.AddRange(_icons);

            foreach (var candidate in _candidates.Except(MainCandidate)
                .Distinct(x => x.CreateCommand().Name))
            {
                var command = candidate.CreateCommand();
                implementation.Commands.Add(command);
                feed.EntryPoints.Add(new EntryPoint
                {
                    Command = command.Name,
                    Names = {candidate.Name},
                    Summaries = {candidate.Summary},
                    BinaryName = Path.GetFileNameWithoutExtension(candidate.RelativePath),
                    NeedsTerminal = candidate.NeedsTerminal
                });
            }
            implementation.Commands[0].Name = Command.NameRun;

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
