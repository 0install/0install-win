/*
 * Copyright 2010-2015 Bastian Eicher
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
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Publish.Capture
{
    /// <summary>
    /// Manages the process of taking two <see cref="Snapshot"/>s and comparing them to generate a <see cref="Feed"/>.
    /// </summary>
    public class CaptureSession
    {
        [NotNull]
        private readonly Snapshot _snapshot;

        /// <summary>
        /// The fully qualified path to the installation directory; leave <see langword="null"/> for auto-detection.
        /// </summary>
        [CanBeNull]
        public string InstallationDir { get; set; }

        /// <summary>
        /// The relative path to the main EXE; leave <see langword="null"/> for auto-detection.
        /// </summary>
        [CanBeNull]
        public string MainExe { get; set; }

        private CaptureSession([NotNull] Snapshot snapshotBefore)
        {
            _snapshot = snapshotBefore;
        }

        /// <summary>
        /// Captures the current system state as a snapshot of the system state before the target application was installed.
        /// </summary>
        /// <exception cref="IOException">There was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the registry or the file system was not permitted.</exception>
        [NotNull]
        public static CaptureSession Start()
        {
            return new CaptureSession(Snapshot.Take());
        }

        /// <summary>
        /// Collects data from the locations indicated by the differences between <see cref="_snapshot"/> and the current system state.
        /// </summary>
        /// <param name="feedBuilder">All collected data is stored into this builder. You can perform additional modifications before using <see cref="FeedBuilder.Build"/> to get a feed.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        /// <exception cref="IOException">There was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the registry or file system was not permitted.</exception>
        public void Finish([NotNull] FeedBuilder feedBuilder, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (feedBuilder == null) throw new ArgumentNullException("feedBuilder");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var diff = new SnapshotDiff(before: _snapshot, after: Snapshot.Take());

            if (string.IsNullOrEmpty(InstallationDir)) InstallationDir = diff.GetInstallationDir();

            feedBuilder.ImplementationDirectory = InstallationDir;

            feedBuilder.DetectCandidates(handler);
            try
            {
                feedBuilder.MainCandidate = string.IsNullOrEmpty(MainExe)
                    ? feedBuilder.Candidates.First()
                    : feedBuilder.Candidates.First(x => StringUtils.EqualsIgnoreCase(FileUtils.UnifySlashes(x.RelativePath), MainExe));
            }
                #region Error handling
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException(Resources.EntryPointNotFound);
            }
            #endregion

            feedBuilder.GenerateCommands();

            var commandMapper = new CommandMapper(InstallationDir, feedBuilder.Commands);
            feedBuilder.CapabilityList = GetCapabilityList(commandMapper, diff);
        }

        /// <summary>
        /// Collects data from the locations indicated by the differences between <see cref="_snapshot"/> and the current system state.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        /// <returns>All collected data is stored into this feed.</returns>
        /// <exception cref="IOException">There was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the registry or file system was not permitted.</exception>
        [NotNull, Pure]
        public SignedFeed Finish([NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var feedBuilder = new FeedBuilder();
            Finish(feedBuilder, handler);
            return feedBuilder.Build();
        }

        [NotNull]
        private static CapabilityList GetCapabilityList(CommandMapper commandMapper, SnapshotDiff diff)
        {
            var capabilities = new CapabilityList {OS = OS.Windows};
            string appName = null, appDescription = null;

            diff.CollectFileTypes(commandMapper, capabilities);
            diff.CollectContextMenus(commandMapper, capabilities);
            diff.CollectAutoPlays(commandMapper, capabilities);
            diff.CollectDefaultPrograms(commandMapper, capabilities, ref appName);

            var appRegistration = diff.GetAppRegistration(commandMapper, capabilities, ref appName, ref appDescription);
            if (appRegistration != null) capabilities.Entries.Add(appRegistration);
            else
            { // Only collect URL protocols if there wasn't already an application registration that covered them
                diff.CollectProtocolAssocs(commandMapper, capabilities);
            }

            return capabilities;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CollectFiles()
        {
            throw new NotImplementedException();
        }

        #region Storage
        /// <summary>
        /// Loads a capture session from a snapshot file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <exception cref="IOException">A problem occured while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurred while deserializing the binary data.</exception>
        [NotNull]
        public static CaptureSession Load([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return new CaptureSession(BinaryStorage.LoadBinary<Snapshot>(path));
        }

        /// <summary>
        /// Saves the capture session to a snapshot file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">A problem occured while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        public void Save([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            _snapshot.SaveBinary(path);
        }
        #endregion
    }
}
