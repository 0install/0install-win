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
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using ZeroInstall.DesktopIntegration.Windows;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish.Capture
{
    /// <summary>
    /// Represents the systems state at a point in time. This is used to determine changes.
    /// </summary>
    [Serializable]
    public class Snapshot
    {
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
            snapshot.TakeRegistry();
            snapshot.TakeFileSystem();
            return snapshot;
        }

        #region Registry
        /// <summary>
        /// Stores information about the current state of the registry in a snapshot.
        /// </summary>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        private void TakeRegistry()
        {
            ServiceAssocs = GetServiceAssocs();
            AutoPlayHandlersUser = RegUtils.GetSubKeyNames(Registry.CurrentUser, AutoPlay.RegKeyHandlers);
            AutoPlayHandlersMachine = RegUtils.GetSubKeyNames(Registry.LocalMachine, AutoPlay.RegKeyHandlers);
            AutoPlayAssocsUser = GetAutoPlayAssocs(Registry.CurrentUser);
            AutoPlayAssocsMachine = GetAutoPlayAssocs(Registry.LocalMachine);
            GetFileAssocData(out FileAssocs, out ProgIDs);
            ProtocolAssocs = GetProtocolAssoc();
            ClassIDs = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ComServer.RegKeyClassesIDs);
            RegisteredApplications = RegUtils.GetValueNames(Registry.LocalMachine, AppRegistration.RegKeyMachineRegisteredApplications);

            ContextMenuFiles = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFiles + "\\" + ContextMenu.RegKeyPostfix);
            ContextMenuExecutableFiles = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesExecutableFiles + "\\" + ContextMenu.RegKeyPostfix);
            ContextMenuDirectories = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesDirectories + "\\" + ContextMenu.RegKeyPostfix);
            ContextMenuAll = RegUtils.GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAll + "\\" + ContextMenu.RegKeyPostfix);
        }

        /// <summary>
        /// Retrieves a list of service associations from the registry.
        /// </summary>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
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
                        if (assocKey == null) continue;

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
                let command = RegistryUtils.GetString(@"HKEY_CLASSES_ROOT\" + protocol + @"\shell\open\command", valueName: null)
                where !string.IsNullOrEmpty(command)
                select new ComparableTuple<string>(protocol, command)).ToArray();
        }

        /// <summary>
        /// Retrieves a list of AutoPlay associations from the registry.
        /// </summary>
        /// <param name="hive">The registry hive to search in (usually HKCU or HKLM).</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
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
        /// <exception cref="IOException">There was an error accessing the file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file system was not permitted.</exception>
        private void TakeFileSystem()
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
            ProgramsDirs = programDirs.ToArray();
        }
        #endregion
    }
}
