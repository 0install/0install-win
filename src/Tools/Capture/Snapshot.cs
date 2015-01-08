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
using System.Linq;
using System.Security;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using ZeroInstall.Capture.Properties;
using ZeroInstall.DesktopIntegration.Windows;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents the systems state at a point in time. This is used to determine changes.
    /// </summary>
    [Serializable]
    public class Snapshot
    {
        #region Variables
        /// <summary>A list of associations of services with clients (e.g. web browsers, mail readers, ...).</summary>
        public ComparableTuple<string>[] ServiceAssocs;

        /// <summary>A list of applications registered as AutoPlay handlers.</summary>
        public string[] AutoPlayHandlersUser, AutoPlayHandlersMachine;

        /// <summary>A list of associations of an AutoPlay events with an AutoPlay handlers.</summary>
        public ComparableTuple<string>[] AutoPlayAssocsUser, AutoPlayAssocsMachine;

        /// <summary>A list of associations of file extensions with programatic identifiers.</summary>
        public ComparableTuple<string>[] FileAssocs;

        /// <summary>A list of protocol associations for well-known protocols (e.g. HTTP, FTP, ...).</summary>
        public ComparableTuple<string>[] ProtocolAssocs;

        /// <summary>A list of programatic indentifiers.</summary>
        public string[] ProgIDs;

        /// <summary>A list of COM class IDs.</summary>
        public string[] ClassIDs;

        /// <summary>A list of applications registered as candidates for default programs.</summary>
        public string[] RegisteredApplications;

        /// <summary>A list of context menu entries for all files.</summary>
        public string[] ContextMenuFiles;

        /// <summary>A list of context menu entries for executable files.</summary>
        public string[] ContextMenuExecutableFiles;

        /// <summary>A list of context menu entries for all directories.</summary>
        public string[] ContextMenuDirectories;

        /// <summary>A list of context menu entries for all filesystem objects (files and directories).</summary>
        public string[] ContextMenuAll;

        /// <summary>A list of program installation directories.</summary>
        public string[] ProgramsDirs;
        #endregion

        //--------------------//

        #region Take snapshot
        /// <summary>
        /// Takes a snapshot of the current system state.
        /// </summary>
        /// <returns>The newly created snapshot.</returns>
        /// <exception cref="IOException">There was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry or file system was not permitted.</exception>
        /// <exception cref="PlatformNotSupportedException">This method is called while running on a platform for which capturing is not supported.</exception>
        public static Snapshot Take()
        {
            if (!WindowsUtils.IsWindows) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            var snapshot = new Snapshot();

            try
            {
                TakeRegistry(snapshot);
            }
                #region Error handling
            catch (SecurityException ex)
            {
                // Wrap exception since only certain exception types are allowed in tasks
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion

            TakeFileSystem(snapshot);

            return snapshot;
        }
        #endregion

        #region Registry
        /// <summary>
        /// Stores information about the current state of the registry in a snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to store the data in.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static void TakeRegistry(Snapshot snapshot)
        {
            snapshot.ServiceAssocs = GetServiceAssocs();
            snapshot.AutoPlayHandlersUser = RegUtils.GetSubKeyNames(Registry.CurrentUser, AutoPlay.RegKeyHandlers);
            snapshot.AutoPlayHandlersMachine = RegUtils.GetSubKeyNames(Registry.LocalMachine, AutoPlay.RegKeyHandlers);
            snapshot.AutoPlayAssocsUser = GetAutoPlayAssocs(Registry.CurrentUser);
            snapshot.AutoPlayAssocsMachine = GetAutoPlayAssocs(Registry.LocalMachine);
            GetFileAssocData(out snapshot.FileAssocs, out snapshot.ProgIDs);
            snapshot.ProtocolAssocs = GetProtocolAssoc();
            snapshot.ClassIDs = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ComServer.RegKeyClassesIDs);
            snapshot.RegisteredApplications = RegUtils.GetValueNames(Registry.LocalMachine, AppRegistration.RegKeyMachineRegisteredApplications);

            snapshot.ContextMenuFiles = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFiles + "\\" + ContextMenu.RegKeyPostfix);
            snapshot.ContextMenuExecutableFiles = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesExecutableFiles + "\\" + ContextMenu.RegKeyPostfix);
            snapshot.ContextMenuDirectories = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesDirectories + "\\" + ContextMenu.RegKeyPostfix);
            snapshot.ContextMenuAll = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAll + "\\" + ContextMenu.RegKeyPostfix);
        }

        /// <summary>
        /// Retrieves a list of service associations from the registry.
        /// </summary>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static ComparableTuple<string>[] GetServiceAssocs()
        {
            using (var clientsKey = Registry.LocalMachine.OpenSubKey(DefaultProgram.RegKeyMachineClients))
            {
                if (clientsKey == null) return new ComparableTuple<string>[0];

                return (
                    from serviceName in clientsKey.GetSubKeyNames()
                    // ReSharper disable AccessToDisposedClosure
                    from clientName in RegUtils.GetSubKeyNames(clientsKey, serviceName)
                    // ReSharper restore AccessToDisposedClosure
                    select new ComparableTuple<string>(serviceName, clientName)).ToArray();
            }
        }

        /// <summary>
        /// Retrieves a list of file assocations and programatic indentifiers the registry.
        /// </summary>
        private static void GetFileAssocData(out ComparableTuple<string>[] fileAssocs, out string[] progIDs)
        {
            var fileAssocsList = new List<ComparableTuple<string>>();
            var progIDsList = new List<string>();

            foreach (string keyName in Registry.ClassesRoot.GetSubKeyNames())
            {
                if (keyName.StartsWith("."))
                {
                    using (var assocKey = Registry.ClassesRoot.OpenSubKey(keyName))
                    {
                        // Get the main ProgID
                        fileAssocsList.Add(new ComparableTuple<string>(keyName, assocKey.GetValue("", "").ToString()));

                        // Get additional ProgIDs
                        fileAssocsList.AddRange(RegUtils.GetValueNames(assocKey, FileType.RegSubKeyOpenWith).Select(progID => new ComparableTuple<string>(keyName, progID)));
                    }
                }
                else progIDsList.Add(keyName);
            }

            fileAssocs = fileAssocsList.ToArray();
            progIDs = progIDsList.ToArray();
        }

        /// <summary>
        /// Retrieves a list of protocol associations for well-known protocols (e.g. HTTP, FTP, ...).
        /// </summary>
        private static ComparableTuple<string>[] GetProtocolAssoc()
        {
            return (
                from protocol in new[] {"ftp", "gopher", "http", "https"}
                let command = Registry.GetValue(@"HKEY_CLASSES_ROOT\" + protocol + @"\shell\open\command", "", "") as string
                where !string.IsNullOrEmpty(command)
                select new ComparableTuple<string>(protocol, command)).ToArray();
        }

        /// <summary>
        /// Retrieves a list of AutoPlay associations from the registry.
        /// </summary>
        /// <param name="hive">The registry hive to search in (usually HKCU or HKLM).</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static ComparableTuple<string>[] GetAutoPlayAssocs(RegistryKey hive)
        {
            using (var eventsKey = hive.OpenSubKey(AutoPlay.RegKeyAssocs))
            {
                if (eventsKey == null) return new ComparableTuple<string>[0];

                return (
                    from eventName in eventsKey.GetSubKeyNames()
                    from handlerName in RegUtils.GetValueNames(eventsKey, eventName)
                    select new ComparableTuple<string>(eventName, handlerName)).ToArray();
            }
        }
        #endregion

        #region File system
        /// <summary>
        /// Stores information about the current state of the file system in a snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to store the data in.</param>
        /// <exception cref="IOException">There was an error accessing the file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file system was not permitted.</exception>
        private static void TakeFileSystem(Snapshot snapshot)
        {
            // Locate installation directories
            string programFiles32Bit = WindowsUtils.Is64BitProcess
                ? Environment.GetEnvironmentVariable("ProgramFiles(x86)")
                : Environment.GetEnvironmentVariable("ProgramFiles");
            string programFiles64Bit = WindowsUtils.Is64BitProcess
                ? Environment.GetEnvironmentVariable("ProgramFiles")
                : null;

            // Build a list of all installation directorie
            var programDirs = new List<string>();
            if (string.IsNullOrEmpty(programFiles32Bit)) Log.Warn(Resources.MissingProgramFiles32Bit);
            else programDirs.AddRange(Directory.GetDirectories(programFiles32Bit));
            if (!string.IsNullOrEmpty(programFiles64Bit))
                programDirs.AddRange(Directory.GetDirectories(programFiles64Bit));
            snapshot.ProgramsDirs = programDirs.ToArray();
        }
        #endregion

        #region Diff
        /// <summary>
        /// Determines which elements have been added to the system between two snapshots.
        /// </summary>
        /// <param name="oldSnapshot">The first snapshot taken.</param>
        /// <param name="newSnapshot">The second snapshot taken.</param>
        /// <returns>A snapshot containing all elements that are present in <paramref name="newSnapshot"/> but not in <paramref name="oldSnapshot"/>.</returns>
        /// <remarks>Assumes that all internal arrays are sorted alphabetically.</remarks>
        public static Snapshot Diff(Snapshot oldSnapshot, Snapshot newSnapshot)
        {
            #region Sanity checks
            if (oldSnapshot == null) throw new ArgumentNullException("oldSnapshot");
            if (newSnapshot == null) throw new ArgumentNullException("newSnapshot");
            #endregion

            return new Snapshot
            {
                ServiceAssocs = newSnapshot.ServiceAssocs.GetAddedElements(oldSnapshot.ServiceAssocs),
                AutoPlayHandlersUser = newSnapshot.AutoPlayHandlersUser.GetAddedElements(oldSnapshot.AutoPlayHandlersUser),
                AutoPlayHandlersMachine = newSnapshot.AutoPlayHandlersMachine.GetAddedElements(oldSnapshot.AutoPlayHandlersMachine),
                AutoPlayAssocsUser = newSnapshot.AutoPlayAssocsUser.GetAddedElements(oldSnapshot.AutoPlayAssocsUser),
                AutoPlayAssocsMachine = newSnapshot.AutoPlayAssocsMachine.GetAddedElements(oldSnapshot.AutoPlayAssocsMachine),
                FileAssocs = newSnapshot.FileAssocs.GetAddedElements(oldSnapshot.FileAssocs),
                ProtocolAssocs = newSnapshot.ProtocolAssocs.GetAddedElements(oldSnapshot.ProtocolAssocs),
                ProgIDs = newSnapshot.ProgIDs.GetAddedElements(oldSnapshot.ProgIDs, StringComparer.OrdinalIgnoreCase),
                ClassIDs = newSnapshot.ClassIDs.GetAddedElements(oldSnapshot.ClassIDs, StringComparer.OrdinalIgnoreCase),
                RegisteredApplications = newSnapshot.RegisteredApplications.GetAddedElements(oldSnapshot.RegisteredApplications),
                ContextMenuFiles = newSnapshot.ContextMenuFiles.GetAddedElements(oldSnapshot.ContextMenuFiles),
                ContextMenuDirectories = newSnapshot.ContextMenuDirectories.GetAddedElements(oldSnapshot.ContextMenuDirectories),
                ProgramsDirs = newSnapshot.ProgramsDirs.GetAddedElements(oldSnapshot.ProgramsDirs, StringComparer.OrdinalIgnoreCase)
            };
        }
        #endregion
    }
}
