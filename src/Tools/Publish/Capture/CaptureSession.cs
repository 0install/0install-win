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
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations.Archives;
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

        [NotNull]
        private readonly FeedBuilder _feedBuilder;

        /// <summary>
        /// The fully qualified path to the installation directory; leave <see langword="null"/> or empty for auto-detection.
        /// </summary>
        [CanBeNull]
        public string InstallationDir { get; set; }

        private CaptureSession([NotNull] Snapshot snapshotBefore, [NotNull] FeedBuilder feedBuilder)
        {
            _snapshot = snapshotBefore;
            _feedBuilder = feedBuilder;
        }

        /// <summary>
        /// Captures the current system state as a snapshot of the system state before the target application was installed.
        /// </summary>
        /// <param name="feedBuilder">All collected data is stored into this builder. You can perform additional modifications before using <see cref="FeedBuilder.Build"/> to get a feed.</param>
        /// <exception cref="IOException">There was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the registry or the file system was not permitted.</exception>
        [NotNull]
        public static CaptureSession Start([NotNull] FeedBuilder feedBuilder)
        {
            #region Sanity checks
            if (feedBuilder == null) throw new ArgumentNullException("feedBuilder");
            #endregion

            return new CaptureSession(Snapshot.Take(), feedBuilder);
        }

        [CanBeNull]
        private SnapshotDiff _diff;

        /// <summary>
        /// Collects data from the locations indicated by the differences between the <see cref="Start"/> state and the current system state.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        /// <exception cref="InvalidOperationException">No installation directory was detected.</exception>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">There was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the registry or file system was not permitted.</exception>
        public void Diff([NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _diff = new SnapshotDiff(before: _snapshot, after: Snapshot.Take());
            if (string.IsNullOrEmpty(InstallationDir)) InstallationDir = _diff.GetInstallationDir();

            _feedBuilder.ImplementationDirectory = InstallationDir;
            _feedBuilder.DetectCandidates(handler);
        }

        /// <summary>
        /// Finishes the capture process after <see cref="Diff"/> has been called an <see cref="FeedBuilder.MainCandidate"/> has been set.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Diff"/> was not called or <see cref="FeedBuilder.MainCandidate"/> is not set.</exception>
        /// <exception cref="IOException">There was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the registry or file system was not permitted.</exception>
        public void Finish()
        {
            if (_diff == null || InstallationDir == null) throw new InvalidOperationException("Diff() must be called first.");

            _feedBuilder.GenerateCommands();

            var commandMapper = new CommandMapper(InstallationDir, _feedBuilder.Commands);
            _feedBuilder.CapabilityList = GetCapabilityList(commandMapper, _diff);
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
        /// Creates a archive containing the <see cref="InstallationDir"/>.
        /// </summary>
        /// <remarks>Sets <see cref="FeedBuilder.RetrievalMethod"/> and calls <see cref="FeedBuilder.CalculateDigest"/>.</remarks>
        /// <param name="archivePath">The path of the archive file to create.</param>
        /// <param name="archiveUrl">The URL where the archive will be uploaded.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about IO tasks.</param>
        /// <exception cref="InvalidOperationException"><see cref="Diff"/> was not called or <see cref="FeedBuilder.MainCandidate"/> is not set.</exception>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">There was an error reading the installation files or writing the archive.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the file system was not permitted.</exception>
        public void CollectFiles([NotNull] string archivePath, [NotNull] Uri archiveUrl, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(archivePath)) throw new ArgumentNullException("archivePath");
            if (archiveUrl == null) throw new ArgumentNullException("archiveUrl");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (InstallationDir == null) throw new InvalidOperationException("Diff() must be called first.");

            _feedBuilder.ImplementationDirectory = InstallationDir;
            _feedBuilder.CalculateDigest(handler);

            var mimeType = Archive.GuessMimeType(archivePath) ?? Archive.MimeTypeZip;
            using (var generator = ArchiveGenerator.Create(InstallationDir, archivePath, mimeType))
                handler.RunTask(generator);
            _feedBuilder.RetrievalMethod = new Archive {Href = archiveUrl, MimeType = mimeType};
        }

        #region Storage
        /// <summary>
        /// Loads a capture session from a snapshot file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <param name="feedBuilder">All collected data is stored into this builder. You can perform additional modifications before using <see cref="FeedBuilder.Build"/> to get a feed.</param>
        /// <exception cref="IOException">A problem occured while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurred while deserializing the binary data.</exception>
        [NotNull]
        public static CaptureSession Load([NotNull] string path, [NotNull] FeedBuilder feedBuilder)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (feedBuilder == null) throw new ArgumentNullException("feedBuilder");
            #endregion

            return new CaptureSession(BinaryStorage.LoadBinary<Snapshot>(path), feedBuilder);
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
