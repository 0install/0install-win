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
using System.Collections.Generic;
using System.IO;
using System.Security;
using Common;
using Common.Collections;
using Common.Utils;
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
        /// <param name="files">Indicates whether to collect installation files in addition to registry data.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry or file system was not permitted.</exception>
        public void Collect(bool files)
        {
            #region Sanity checks
            if (SnapshotPre == null) throw new InvalidOperationException("Pre-installation snapshot missing.");
            if (SnapshotPost == null) throw new InvalidOperationException("Post-installation snapshot missing.");
            #endregion

            var snapshotDiff = Snapshot.Diff(SnapshotPre, SnapshotPost);

            // ToDo: Allow manual override
            string installationDir = GetInstallationDir(snapshotDiff);
            var commands = GetCommands(installationDir);

            // ToDo: snapshotDiff.ProtocolAssocs
            // ToDo: snapshotDiff.*ContextMenuSimple
            // ToDo: snapshotDiff.RegisteredApplications (also pull additional URL protcols from here)
            // ToDo: snapshotDiff.Games

            var capabilities = new CapabilityList {Architecture = new Architecture(OS.Windows, Cpu.All)};
            try
            {
                CollectFileTypes(snapshotDiff, capabilities, commands);
                CollectAutoPlays(snapshotDiff, capabilities, commands);
            }
            #region Error handling
            catch (SecurityException ex)
            {
                // Wrap exception since only certain exception types are allowed in tasks
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion

            Implementation implementation = (files && !string.IsNullOrEmpty(installationDir)) ? GetImplementation(installationDir) : null;
            BuildFeed(capabilities, commands, implementation).Save(Path.Combine(DirectoryPath, "feed.xml"));
        }
        #endregion

        #region Installation directory
        /// <summary>
        /// Locates the directory into which the new application was installed.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        private static string GetInstallationDir(Snapshot snapshotDiff)
        {
            string installationDir;
            if (snapshotDiff.ProgramsDirs.Length == 0)
            {
                Log.Warn(Resources.NoInstallationDirDetected);
                installationDir = null;
            }
            else
            {
                if (snapshotDiff.ProgramsDirs.Length > 1)
                    Log.Warn(Resources.MultipleInstallationDirsDetected);

                installationDir = snapshotDiff.ProgramsDirs[0];
                Log.Info(string.Format(Resources.UsingInstallationDir, installationDir));
            }
            return installationDir;
        }

        /// <summary>
        /// Detects all EXE files within the installation directory and returns them as <see cref="Command"/>s.
        /// </summary>
        /// <param name="installationDir">The fully qualified path to the installation directory; may be <see langword="null"/>.</param>
        private static IEnumerable<Command> GetCommands(string installationDir)
        {
            if (installationDir == null) return new Command[0];

            bool firstExe = true;
            var commands = new C5.LinkedList<Command>();
            foreach (string absolutePath in Directory.GetFiles(installationDir, "*.exe", SearchOption.AllDirectories))
            {
                // Ignore unisnstaller
                // ToDo: Better heuristic
                if (absolutePath.Contains("uninstall")) continue;

                // ToDo: Better filter
                string relativePath = absolutePath.Replace(installationDir + Path.DirectorySeparatorChar, "");

                // Assume first detected EXE is the main entry point
                // ToDo: Better heuristic
                commands.Add(new Command {Name = firstExe ? "run" : relativePath.Replace(".exe", "").Replace(Path.DirectorySeparatorChar, '.'), Path = relativePath});
                firstExe = false;
            }
            return commands;
        }
        #endregion

        #region Collect file types
        /// <summary>
        /// Collects data about file types and also URL protocol handlers indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <param name="commands">A list of commands that can be uses to start the application.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectFileTypes(Snapshot snapshotDiff, CapabilityList capabilities, IEnumerable<Command> commands)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commands == null) throw new ArgumentNullException("commands");
            #endregion

            foreach (string progID in snapshotDiff.ProgIDs)
            {
                if (string.IsNullOrEmpty(progID)) continue;
                var fileType = GetFileType(progID, snapshotDiff, commands);
                if (fileType != null) capabilities.Entries.Add(fileType);
            }
        }

        /// <summary>
        /// Retreives data about a specific file type or URL protocol from a snapshot diff.
        /// </summary>
        /// <param name="progID">The programatic identifier of the file type.</param>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commands">A list of commands that can be uses to start the application.</param>
        /// <returns>Data about the file type or <see paramref="null"/> if no file type for this <paramref name="progID"/> was detected.</returns>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static VerbCapability GetFileType(string progID, Snapshot snapshotDiff, IEnumerable<Command> commands)
        {
            using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
            {
                if (progIDKey == null) return null;

                VerbCapability capability;
                if (progIDKey.GetValue(DesktopIntegration.Windows.UrlProtocol.ProtocolFlag) == null)
                { // Normal file type
                    var fileType = new FileType
                    {
                        ID = progID,
                        Description = progIDKey.GetValue("", "").ToString()
                    };

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

                    // Only return file types that have extensions associated with them
                    if (fileType.Extensions.IsEmpty) return null;

                    capability = fileType;
                }
                else
                { // URL protocol handler
                    capability = new UrlProtocol
                    {
                        ID = progID,
                        Description = progIDKey.GetValue("", "").ToString(),
                        Prefix = progID
                    };
                }

                using (var shellKey = progIDKey.OpenSubKey("shell"))
                {
                    if (shellKey == null) return null;

                    foreach (string verbName in shellKey.GetSubKeyNames())
                    {
                        var verb = GetVerb(progID, verbName, commands);
                        if (verb != default(Verb)) capability.Verbs.Add(verb);
                    }
                }

                return capability;
            }
        }

        /// <summary>
        /// Retreives data about a verb (an executable command) from the registry.
        /// </summary>
        /// <param name="progID">The programatic indentifier the verb is associated with.</param>
        /// <param name="verb">The internal name of the verb.</param>
        /// <param name="commands">A list of commands that can be uses to start the application.</param>
        private static Verb GetVerb(string progID, string verb, IEnumerable<Command> commands)
        {
            using (var verbKey = Registry.ClassesRoot.OpenSubKey(progID + @"\shell\" + verb))
            {
                if (verbKey == null) return default(Verb);

                string description = verbKey.GetValue("", "").ToString();
                string commandString;
                using (var commandKey = verbKey.OpenSubKey("command"))
                {
                    if (commandKey == null) return default(Verb);
                    commandString = commandKey.GetValue("", "").ToString();
                }

                foreach (var command in commands)
                {
                    // ToDo: Make more elegant
                    if (commandString.Contains(command.Path))
                    {
                        // Isolate arguments
                        string arguments = StringUtils.GetRightPartAtFirstOccurrence(commandString, command.Path + "\" ");
                        if (arguments == DesktopIntegration.Windows.FileType.DefaultArguments) arguments = null;

                        return new Verb
                        {
                            Name = verb,
                            Description = description,
                            Command = command.Name,
                            Arguments = arguments
                        };
                    }
                }
                return default(Verb);
            }
        }
        #endregion

        #region Collect AutoPlay
        /// <summary>
        /// Collects data about AutoPlay handlers indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <param name="commands">A list of commands that can be uses to start the application.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectAutoPlays(Snapshot snapshotDiff, CapabilityList capabilities, IEnumerable<Command> commands)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            #endregion

            foreach (string handler in snapshotDiff.AutoPlayHandlersUser)
            {
                var autoPlay = GetAutoPlay(handler, Registry.CurrentUser, snapshotDiff.AutoPlayAssocsUser, commands);
                if (autoPlay != null) capabilities.Entries.Add(autoPlay);
            }

            foreach (string handler in snapshotDiff.AutoPlayHandlersMachine)
            {
                var autoPlay = GetAutoPlay(handler, Registry.LocalMachine, snapshotDiff.AutoPlayAssocsMachine, commands);
                if (autoPlay != null) capabilities.Entries.Add(autoPlay);
            }
        }

        /// <summary>
        /// Retreives data about a AutoPlay handler type from a snapshot diff.
        /// </summary>
        /// <param name="handler">The internal name of the AutoPlay handler.</param>
        /// <param name="hive">The registry hive to search in (usually HKCU or HKLM).</param>
        /// <param name="autoPlayAssocs">A list of associations of an AutoPlay events with an AutoPlay handlers</param>
        /// <param name="commands">A list of commands that can be uses to start the application.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static Capability GetAutoPlay(string handler, RegistryKey hive, IEnumerable<ComparableTuple<string>> autoPlayAssocs, IEnumerable<Command> commands)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            if (hive == null) throw new ArgumentNullException("hive");
            if (autoPlayAssocs == null) throw new ArgumentNullException("autoPlayAssocs");
            #endregion

            using (var handlerKey = hive.OpenSubKey(DesktopIntegration.Windows.AutoPlay.RegKeyHandlers + @"\" + handler))
            {
                if (handlerKey == null) return null;

                string progID = handlerKey.GetValue("InvokeProgID", "").ToString();
                string verbName = handlerKey.GetValue("InvokeVerb", "").ToString();
                var autoPlay = new AutoPlay
                {
                    ID = handler,
                    Provider = handlerKey.GetValue("Provider", "").ToString(),
                    Description = handlerKey.GetValue("Description", "").ToString(),
                    ProgID = progID,
                    Verb = GetVerb(progID, verbName, commands)
                };

                foreach (var autoPlayAssoc in autoPlayAssocs)
                    if (autoPlayAssoc.Value == handler) autoPlay.Events.Add(new AutoPlayEvent {Name = autoPlayAssoc.Key});

                return autoPlay;
            }
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
            Log.Info("Copying installation files...");
            FileUtils.CopyDirectory(installationDir, implementationDir, true, false);
            Log.Info("Done.");

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
        /// <param name="capabilities">A list of capabilities that were detected.</param>
        /// <param name="commands">A list of commands that can be uses to start the application.</param>
        /// <param name="implementation">An implementation to add to the main group; may be <see langword="null"/>.</param>
        private static Feed BuildFeed(CapabilityList capabilities, IEnumerable<Command> commands, Implementation implementation)
        {
            #region Sanity checks
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commands == null) throw new ArgumentNullException("commands");
            #endregion

            var group = new Group();
            group.Commands.AddAll(commands);
            if (implementation != null) group.Elements.Add(implementation);

            return new Feed
            {
                // ToDo: Auto-detect values from Default Program registration
                Name = "Application name",
                Summaries = {new LocalizableString("Application summary")},
                Descriptions = {new LocalizableString("Application description")},
                Elements = {group},
                CapabilityLists = {capabilities}
            };
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
