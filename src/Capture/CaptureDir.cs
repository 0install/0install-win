/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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
using System.Security;
using Microsoft.Win32;
using ZeroInstall.Capture.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents a file system directory storing information from a capturing session.
    /// </summary>
    public class CaptureDir
    {
        #region Constants
        private const string SnapshotPreFileName = "pre.snapshot", SnapshotPostFileName = "post.snapshot";
        #endregion

        #region Properties
        /// <summary>
        /// The directory containing the data from a capture session.
        /// </summary>
        public string DirectoryPath { get; private set; }

        /// <summary>
        /// A snapshot of the system state taken before the target application was installed.
        /// </summary>
        public Snapshot SnapshotPre { get; private set; }

        /// <summary>
        /// A snapshot of the system state taken after the target application was installed.
        /// </summary>
        public Snapshot SnapshotPost { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new capture directory.
        /// </summary>
        /// <param name="path">A directory path. The directory will be created if it doesn't exist yet.</param>
        private CaptureDir(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            DirectoryPath = path;
        }
        #endregion

        //--------------------//

        #region Snapshot operations
        /// <summary>
        /// Captures the current system state as a snapshot of the system state before the target application was installed.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry or the file system was not permitted.</exception>
        public void TakeSnapshotPre()
        {
            SnapshotPre = Snapshot.Take();
            SnapshotPre.Save(Path.Combine(DirectoryPath, SnapshotPreFileName));
        }

        /// <summary>
        /// Captures the current system state as a snapshot of the system state after the target application was installed.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry or the file system was not permitted.</exception>
        public void TakeSnapshotPost()
        {
            SnapshotPost = Snapshot.Take();
            SnapshotPost.Save(Path.Combine(DirectoryPath, SnapshotPostFileName));
        }
        #endregion

        #region Collect
        /// <summary>
        /// Collects data from the locations indicated by the differences between <see cref="SnapshotPre"/> and <see cref="SnapshotPost"/>.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry or file system was not permitted.</exception>
        public void Collect()
        {
            #region Sanity checks
            if (SnapshotPre == null) throw new InvalidOperationException("Pre-installation snapshot missing.");
            if (SnapshotPost == null) throw new InvalidOperationException("Post-installation snapshot missing.");
            #endregion

            var snapshotDiff = Snapshot.Diff(SnapshotPre, SnapshotPost);
            var capabilities = new CapabilityList {Architecture = new Architecture(OS.Windows, Cpu.All)};

            try
            {
                CollectFileTypes(snapshotDiff, capabilities);
                CollectAutoPlays(snapshotDiff, capabilities);
            }
            #region Error handling
            catch (SecurityException ex)
            {
                // Wrap exception since only certain exception types are allowed in tasks
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion

            // ToDo: Collect more Feed information
            var feed = new Feed {CapabilityLists = {capabilities}};
            feed.Save(Path.Combine(DirectoryPath, "feed.xml"));
        }
        #endregion

        #region Collect file types
        /// <summary>
        /// Collects data about file types indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectFileTypes(Snapshot snapshotDiff, CapabilityList capabilities)
        {
            foreach (string progID in snapshotDiff.ProgIDs)
            {
                if (string.IsNullOrEmpty(progID)) continue;
                var fileType = GetFileType(progID, snapshotDiff);
                if (fileType != null)  capabilities.Entries.Add(fileType);
            }
        }

        /// <summary>
        /// Retreives data about a specific file type from a snapshot diff.
        /// </summary>
        /// <param name="progID">The programatic identifier of the file type.</param>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static FileType GetFileType(string progID, Snapshot snapshotDiff)
        {
            FileType fileType;
            using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
            {
                if (progIDKey == null) return null;

                fileType = new FileType
                {
                    ID = progID,
                    Description = progIDKey.GetValue("", "").ToString()
                };

                foreach (string verb in progIDKey.GetSubKeyNames())
                {
                    fileType.Verbs.Add(new FileTypeVerb
                    {
                        Name = verb,
                        // ToDo: Extract Command and Arguments
                    });
                }
            }

            foreach (var fileAssoc in snapshotDiff.FileAssocs)
            {
                if (fileAssoc.Value != progID || string.IsNullOrEmpty(fileAssoc.Key)) continue;

                using (var assocKey = Registry.ClassesRoot.OpenSubKey(fileAssoc.Key))
                {
                    if (assocKey == null) continue;

                    fileType.Extensions.Add(new FileTypeExtension
                    {
                        Value = fileAssoc.Key,
                        MimeType = assocKey.GetValue("Content Type", "").ToString(),
                        PerceivedType = assocKey.GetValue("PerceivedType", "").ToString()
                    });
                }
            }

            return fileType;
        }
        #endregion

        #region Collect AutoPlay
        /// <summary>
        /// Collects data about AutoPlay handlers indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectAutoPlays(Snapshot snapshotDiff, CapabilityList capabilities)
        {
            foreach (string handler in snapshotDiff.AutoPlayHandlers)
            {
                if (string.IsNullOrEmpty(handler)) continue;
                var autoPlay = GetAutoPlay(handler, snapshotDiff);
                if (autoPlay != null) capabilities.Entries.Add(autoPlay);
            }
        }

        /// <summary>
        /// Retreives data about a AutoPlay handler type from a snapshot diff.
        /// </summary>
        /// <param name="handler">The internal name of the AutoPlay handler.</param>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static AutoPlay GetAutoPlay(string handler, Snapshot snapshotDiff)
        {
            using (var handlerKey = Registry.LocalMachine.OpenSubKey(DesktopIntegration.Windows.AutoPlay.RegKeyMachineHandlers + @"\" + handler))
            {
                if (handlerKey == null) return null;

                var autoPlay = new AutoPlay
                {
                    ID = handler,
                    Provider = handlerKey.GetValue("Provider", "").ToString(),
                    Description = handlerKey.GetValue("Description", "").ToString(),
                    FileTypeID = handlerKey.GetValue("InvokeProgID", "").ToString(),
                    FileTypeIDVerb = handlerKey.GetValue("InvokeVerb", "").ToString()
                };

                foreach (var autoPlayAssoc in snapshotDiff.AutoPlayAssocs)
                    if (autoPlayAssoc.Value == handler) autoPlay.Events.Add(new AutoPlayEvent {Name = autoPlayAssoc.Key});

                return autoPlay;
            }
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Creates a new empty capture directory.
        /// </summary>
        /// <param name="path">The directory to store the data from a capture session.</param>
        /// <returns>An object for accessing the newly created capture directory.</returns>
        /// <exception cref="IOException">Thrown if the directory already exists and is not empty or if the directory could not be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static CaptureDir Create(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            try
            {
                path = Path.GetFullPath(path);

                // Make sure directory is empty
                if (Directory.Exists(path))
                {
                    if (Directory.GetFileSystemEntries(path).Length > 0)
                        throw new IOException(string.Format(Resources.DirectoryNotEmpty, path));
                }
                else Directory.CreateDirectory(path);

                // Add flag file to capturing directory
                File.Create(Path.Combine(path, "_capture"));
            }
            #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            catch (NotSupportedException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion

            return new CaptureDir(path);
        }

        /// <summary>
        /// Opens an existing capture directory.
        /// </summary>
        /// <param name="path">The directory containing the data from a capture session.</param>
        /// <returns>An object for accessing the capture directory.</returns>
        /// <exception cref="IOException">Thrown if the directory already exists and is not empty or if the directory could not be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing snapshot data.</exception>
        public static CaptureDir Open(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            try
            {
                path = Path.GetFullPath(path);

                // Make sure directory is already a capture directory
                if (!File.Exists(Path.Combine(path, "_capture")))
                    throw new IOException(string.Format(Resources.NotCaptureDirectory, path));
            }
            #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            catch (NotSupportedException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion

            var captureDir = new CaptureDir(path);

            // Load existing snapshot data
            string snapshotPrePath = Path.Combine(path, SnapshotPreFileName);
            if (File.Exists(snapshotPrePath))
                captureDir.SnapshotPre = Snapshot.Load(Path.Combine(path, SnapshotPreFileName));

            string snapshotPostPath = Path.Combine(path, SnapshotPostFileName);
            if (File.Exists(snapshotPostPath))
                captureDir.SnapshotPost = Snapshot.Load(Path.Combine(path, SnapshotPostFileName));

            return captureDir;
        }
        #endregion
    }
}
