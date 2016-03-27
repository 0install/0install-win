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
using ZeroInstall.Store.Model.Capabilities;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Builds simple <see cref="Feed"/>s containing a single <see cref="Implementation"/>.
    /// </summary>
    public class FeedBuilder : IDisposable
    {
        #region Directories
        private TemporaryDirectory _temporaryDirectory;

        /// <summary>
        /// A temporary directory to prepare files for <see cref="ImplementationDirectory"/>. Not used by the <see cref="FeedBuilder"/> itself.
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

        /// <summary>
        /// Set the directory to search for <see cref="Candidates"/> and to generate the <see cref="ManifestDigest"/> from.
        /// Is usually a subdirectory of or equal to <see cref="TemporaryDirectory"/>.
        /// </summary>
        public string ImplementationDirectory { get; set; }
        #endregion

        #region Candidates
        private readonly List<Candidate> _candidates = new List<Candidate>();

        /// <summary>
        /// Lists auto-detected candidates for <see cref="EntryPoint"/>s.
        /// </summary>
        public IEnumerable<Candidate> Candidates { get { return _candidates; } }

        /// <summary>
        /// Set the main entry point. Provides meta-data and startup instructions for the application.
        /// Should be one of the auto-detected <see cref="Candidates"/>.
        /// </summary>
        public Candidate MainCandidate { get; set; }

        /// <summary>
        /// Detects <see cref="Candidates"/> in the <see cref="ImplementationDirectory"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        /// <exception cref="InvalidOperationException"><see cref="ImplementationDirectory"/> is <c>null</c> or empty.</exception>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">There was a problem generating the manifest or detectng the executables.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to temporary files was not permitted.</exception>
        public void DetectCandidates(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            if (string.IsNullOrEmpty(ImplementationDirectory)) throw new InvalidOperationException("Implementation directory is not set.");
            #endregion

            _candidates.Clear();

            handler.RunTask(new SimpleTask(Resources.DetectingCandidates,
                () => _candidates.AddRange(Detection.ListCandidates(new DirectoryInfo(ImplementationDirectory)))));

            MainCandidate = _candidates.FirstOrDefault();
        }
        #endregion

        #region Commands
        private readonly List<Command> _commands = new List<Command>();

        /// <summary>
        /// Lists the <see cref="Command"/> derived from <see cref="Candidates"/> and <see cref="MainCandidate"/>.
        /// </summary>
        [NotNull]
        public List<Command> Commands { get { return _commands; } }

        private readonly List<EntryPoint> _entryPoints = new List<EntryPoint>();

        /// <summary>
        /// Lists the <see cref="EntryPoint"/>s accompanying <see cref="Commands"/>.
        /// </summary>
        [NotNull]
        public List<EntryPoint> EntryPoints { get { return _entryPoints; } }

        /// <summary>
        /// Generates <see cref="Commands"/> and <see cref="EntryPoints"/> bases on <see cref="Candidates"/> and <see cref="MainCandidate"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="MainCandidate"/> is <c>null</c>.</exception>
        public void GenerateCommands()
        {
            if (MainCandidate == null) throw new InvalidOperationException(Resources.EntryPointNotFound);

            Commands.Clear();
            EntryPoints.Clear();

            var mainCommand = MainCandidate.CreateCommand();
            mainCommand.Name = Command.NameRun;
            Commands.Add(mainCommand);

            EntryPoints.Add(new EntryPoint
            {
                Command = Command.NameRun,
                BinaryName = Path.GetFileNameWithoutExtension(MainCandidate.RelativePath)
            });

            foreach (var candidate in _candidates.Except(MainCandidate))
            {
                var command = candidate.CreateCommand();
                Commands.Add(command);

                var entryPoint = new EntryPoint
                {
                    Command = command.Name,
                    BinaryName = Path.GetFileNameWithoutExtension(candidate.RelativePath),
                    NeedsTerminal = candidate.NeedsTerminal
                };
                if (!string.IsNullOrEmpty(candidate.Name)) entryPoint.Names.Add(candidate.Name);
                if (!string.IsNullOrEmpty(candidate.Summary)) entryPoint.Summaries.Add(candidate.Summary);
                EntryPoints.Add(entryPoint);
            }
        }
        #endregion

        #region Manifest digest
        /// <summary>
        /// The value used for <see cref="ImplementationBase.ManifestDigest"/>.
        /// </summary>
        public ManifestDigest ManifestDigest { get; private set; }

        /// <summary>
        /// Calculates the <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        /// <exception cref="InvalidOperationException"><see cref="ImplementationDirectory"/> is <c>null</c> or empty.</exception>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">There was a problem generating the manifest.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to temporary files was not permitted.</exception>
        public void CalculateDigest(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            if (string.IsNullOrEmpty(ImplementationDirectory)) throw new InvalidOperationException("Implementation directory is not set.");
            #endregion

            var newDigest = new ManifestDigest();

            // Generate manifest for each available format...
            foreach (var generator in ManifestFormat.All.Select(format => new ManifestGenerator(ImplementationDirectory, format)))
            {
                // ... and add the resulting digest to the return value
                handler.RunTask(generator);
                newDigest.ParseID(generator.Manifest.CalculateDigest());
            }

            ManifestDigest = newDigest;
        }
        #endregion

        #region Feed
        /// <summary>
        /// Set to configure <see cref="Feed.Uri"/>.
        /// </summary>
        public FeedUri Uri { get; set; }

        private readonly List<Icon> _icons = new List<Icon>();

        /// <summary>
        /// Set to configure <see cref="Feed.Icons"/>.
        /// </summary>
        public ICollection<Icon> Icons { get { return _icons; } }

        /// <summary>
        /// Set to configure <see cref="Implementation.RetrievalMethods"/>.
        /// </summary>
        [CanBeNull]
        public RetrievalMethod RetrievalMethod { get; set; }

        /// <summary>
        /// Set to configure <see cref="Feed.CapabilityLists"/>.
        /// </summary>
        public CapabilityList CapabilityList { get; set; }

        /// <summary>
        /// Set to configure <see cref="SignedFeed.SecretKey"/>.
        /// </summary>
        public OpenPgpSecretKey SecretKey { get; set; }

        /// <summary>
        /// Generates a feed as described by the properties.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="MainCandidate"/> is <c>null</c>.</exception>
        public SignedFeed Build()
        {
            #region Sanity checks
            if (MainCandidate == null) throw new InvalidOperationException("MainCandidate is not set.");
            #endregion

            var implementation = new Implementation
            {
                ID = @"sha1new=" + ManifestDigest.Sha1New,
                ManifestDigest = ManifestDigest,
                Version = MainCandidate.Version,
                Architecture = MainCandidate.Architecture
            };
            implementation.Commands.AddRange(Commands);
            if (RetrievalMethod != null) implementation.RetrievalMethods.Add(RetrievalMethod);

            var feed = new Feed
            {
                Name = MainCandidate.Name,
                Uri = Uri,
                Summaries = {MainCandidate.Summary},
                NeedsTerminal = MainCandidate.NeedsTerminal,
                Elements = {implementation}
            };
            feed.Icons.AddRange(_icons);
            if (!string.IsNullOrEmpty(MainCandidate.Category)) feed.Categories.Add(new Category {Name = MainCandidate.Category});
            feed.EntryPoints.AddRange(EntryPoints);
            if (CapabilityList != null && CapabilityList.Entries.Count != 0) feed.CapabilityLists.Add(CapabilityList);

            return new SignedFeed(feed, SecretKey);
        }
        #endregion
    }
}
