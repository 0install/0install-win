﻿/*
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
using System.Collections.Generic;
using System.IO;
using System.Security;
using Common.Collections;
using Common.Utils;
using ZeroInstall.Capture.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents a file system directory storing information from a capturing session.
    /// </summary>
    public partial class CaptureDir
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
        /// <param name="installationDir">The fully qualified path to the installation directory; leave <see langword="null"/> for auto-detection.</param>
        /// <param name="mainExe">The relative path to the main EXE; leave <see langword="null"/> for auto-detection.</param>
        /// <param name="getFiles">Indicates whether to collect installation files in addition to registry data.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry or file system was not permitted.</exception>
        public void Collect(string installationDir, string mainExe, bool getFiles)
        {
            #region Sanity checks
            if (SnapshotPre == null) throw new InvalidOperationException("Pre-installation snapshot missing.");
            if (SnapshotPost == null) throw new InvalidOperationException("Post-installation snapshot missing.");
            #endregion

            var snapshotDiff = Snapshot.Diff(SnapshotPre, SnapshotPost);

            if (string.IsNullOrEmpty(installationDir)) installationDir = GetInstallationDir(snapshotDiff);
            var commands = GetCommands(installationDir, mainExe);

            string appName = null, appDescription = null;
            var capabilities = new CapabilityList {Architecture = new Architecture(OS.Windows, Cpu.All)};
            try
            {
                var commandProvider = new CommandProvider(installationDir, commands);

                CollectDefaultPrograms(snapshotDiff, commandProvider, capabilities, ref appName);

                var appRegistration = GetAppRegistration(snapshotDiff, commandProvider, capabilities, ref appName, ref appDescription);
                capabilities.Entries.Add(appRegistration);
                if (appRegistration == null)
                { // Only collect URL protocols if there wasn't already an application registration that covered them
                    CollectProtocolAssocs(snapshotDiff.ProtocolAssocs, commandProvider, capabilities);
                }

                CollectFileTypes(snapshotDiff, commandProvider, capabilities);
                CollectContextMenus(snapshotDiff, commandProvider, capabilities);
                CollectAutoPlays(snapshotDiff, commandProvider, capabilities);
                CollectComServers(snapshotDiff.ClassIDs, commandProvider, capabilities);
                CollectGames(snapshotDiff.Games, commandProvider, capabilities);
            }
                #region Error handling
            catch (SecurityException ex)
            {
                // Wrap exception since only certain exception types are allowed in tasks
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion

            Implementation implementation = (getFiles && !string.IsNullOrEmpty(installationDir)) ? GetImplementation(installationDir) : null;
            BuildFeed(appName, appDescription, capabilities, commands, implementation).Save(Path.Combine(DirectoryPath, "feed.xml"));
        }
        #endregion

        #region Build Feed
        /// <summary>
        /// Creates a local path <see cref="Implementation"/> from the installation directory.
        /// </summary>
        /// <param name="installationDir">The fully qualified path to the installation directory; may be <see langword="null"/>.</param>
        private Implementation GetImplementation(string installationDir)
        {
            string implementationDir = Path.Combine(DirectoryPath, "implementation");
            if (Directory.Exists(implementationDir)) Directory.Delete(implementationDir, true);

            // ToDo: Use callback logic to report progress
            FileUtils.CopyDirectory(installationDir, implementationDir, true, false);

            return new Implementation
            {
                LocalPath = "implementation",
                ID = "local",
                Architecture = new Architecture(OS.Windows, Cpu.All),
                Version = new ImplementationVersion("1.0")
            };
        }

        /// <summary>
        /// Creates a barebone feed representing the application's capabilities.
        /// </summary>
        /// <param name="appName">The name of the application as displayed to the user.</param>
        /// <param name="appDescription">A user-friendly description of the application.</param>
        /// <param name="capabilities">A list of capabilities that were detected.</param>
        /// <param name="commands">A list of commands that can be uses to start the application.</param>
        /// <param name="implementation">An implementation to add to the main group; may be <see langword="null"/>.</param>
        private static Feed BuildFeed(string appName, string appDescription, CapabilityList capabilities, IEnumerable<Command> commands, Implementation implementation)
        {
            #region Sanity checks
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commands == null) throw new ArgumentNullException("commands");
            #endregion

            var group = new Group();
            group.Commands.AddAll(commands);
            if (implementation != null) group.Elements.Add(implementation);

            var feed = new Feed
            {
                Name = appName ?? "Application name",
                Summaries = {new LocalizableString("Short summary")},
                Descriptions = {new LocalizableString(appDescription ?? "Application description")},
                Elements = {group},
                CapabilityLists = {capabilities}
            };
            foreach (var command in commands)
            {
                feed.EntryPoints.Add(new EntryPoint
                {
                    Command = command.Name,
                    // Trim away leading directories and trailing file ending
                    BinaryName = StringUtils.GetLeftPartAtLastOccurrence(StringUtils.GetRightPartAtLastOccurrence(command.Path, '/'), '.'),
                    // Trim away leading namespaces
                    Names = {StringUtils.GetRightPartAtLastOccurrence(command.Name, '.')}
                });
            }
            return feed;
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
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing snapshot data.</exception>
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
