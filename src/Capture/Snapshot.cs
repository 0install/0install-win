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
using Common;
using Common.Storage;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Capture.Properties;
using ZeroInstall.DesktopIntegration.Windows;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents the systems state at a point in time. Used to determine changes.
    /// </summary>
    [Serializable]
    public class Snapshot
    {
        #region Variables
        /// <summary>A per-user list of EXE files that have lookup paths defined.</summary>
        private string[] _appPathsUser;

        /// <summary>A machine-wide list of EXE files that have lookup paths defined.</summary>
        private string[] _appPathsMachine;

        /// <summary>A list of applications defining additional launching behavior.</summary>
        private string[] _applications;

        /// <summary>A list of appliactions registered as candidates for default programs.</summary>
        private string[] _registeredApplications;

        /// <summary>A list of applications registered as clients for specific services.</summary>
        private ClientList[] _clients;

        /// <summary>A list of file assocations.</summary>
        private FileAssoc[] _fileAssocs;

        /// <summary>A list of simple context menu entries for all file types.</summary>
        private string[] _filesContextMenuSimple;

        /// <summary>A list of extended (COM-based) context menu entries for all file types.</summary>
        private string[] _filesContextMenuExtended;

        /// <summary>A list of (COM-based) property sheets for all file types.</summary>
        private string[] _filesPropertySheets;

        /// <summary>A list of simple context menu entries for all filesystem objects (files and directories).</summary>
        private string[] _allContextMenuSimple;

        /// <summary>A list of extended (COM-based) context menu entries for all filesystem objects (files and directories).</summary>
        private string[] _allContextMenuExtended;

        /// <summary>A list of (COM-based) property sheets for all file-system entries.</summary>
        private string[] _allPropertySheets;

        /// <summary>A list of programatic indentifiers.</summary>
        private string[] _progIDs;

        /// <summary>A list of program installation directories.</summary>
        private string[] _programsDirs;

        /// <summary>A list of applications registered as AutoPlay handlers.</summary>
        private string[] _autoPlayHandlers;

        /// <summary>A list of applications registered in the Windows Games Explorer.</summary>
        private string[] _games;
        #endregion

        //--------------------//

        #region Take snapshot
        /// <summary>
        /// Takes a snapshot of the current system state.
        /// </summary>
        /// <returns>The newly created snapshot.</returns>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry or file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry or file system was not permitted.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called while running on a platform for which capturing is not supported.</exception>
        public static Snapshot Take()
        {
            if (!WindowsUtils.IsWindows) throw new PlatformNotSupportedException(Resources.OnlyAvailableOnWindows);

            var snapshot = new Snapshot();

            try { TakeRegistry(snapshot); }
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
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void TakeRegistry(Snapshot snapshot)
        {
            snapshot._appPathsUser = GetSubKeyNames(Registry.CurrentUser, AppPath.RegKeyAppPath);
            snapshot._appPathsMachine = GetSubKeyNames(Registry.LocalMachine, AppPath.RegKeyAppPath);
            snapshot._applications = GetSubKeyNames(Registry.ClassesRoot, AppPath.RegKeyClassesApplications);
            snapshot._registeredApplications = GetValueNames(Registry.LocalMachine, DefaultProgram.RegKeyMachineRegisteredApplications);
            snapshot._clients = GetClientLists();
            GetFileAssocData(out snapshot._fileAssocs, out snapshot._progIDs);

            snapshot._filesContextMenuSimple = GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFilesPrefix + ContextMenu.RegKeyContextMenuSimplePostfix);
            snapshot._filesContextMenuExtended = GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFilesPrefix + ContextMenu.RegKeyContextMenuExtendedPostfix);
            snapshot._filesPropertySheets = GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesFilesPrefix + ContextMenu.RegKeyPropertySheetsPostfix);

            snapshot._allContextMenuSimple = GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAllPrefix + ContextMenu.RegKeyContextMenuSimplePostfix);
            snapshot._allContextMenuExtended = GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAllPrefix + ContextMenu.RegKeyContextMenuExtendedPostfix);
            snapshot._allPropertySheets = GetSubKeyNames(Registry.ClassesRoot, ContextMenu.RegKeyClassesAllPrefix + ContextMenu.RegKeyPropertySheetsPostfix);

            snapshot._autoPlayHandlers = GetSubKeyNames(Registry.LocalMachine, AutoPlay.RegKeyMachineHandlers);
            snapshot._games = GetSubKeyNames(Registry.LocalMachine, GamesExplorer.RegKeyMachineGames);
        }

        /// <summary>
        /// Retreives a list of <see cref="ClientList"/>s from the registry.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static ClientList[] GetClientLists()
        {
            using (var clientsKey = Registry.LocalMachine.OpenSubKey(DefaultProgram.RegKeyMachineClients))
            {
                if (clientsKey != null)
                {
                    string[] serviceNames = clientsKey.GetSubKeyNames();
                    var clients = new ClientList[serviceNames.Length];
                    for (int i = 0; i < serviceNames.Length; i++)
                        clients[i] = new ClientList(serviceNames[i], clientsKey.OpenSubKey(serviceNames[i]).GetSubKeyNames());
                    return clients;
                }
            }
            return null;
        }

        /// <summary>
        /// Retreives a list of <see cref="FileAssoc"/>s and programatic indentifiers the registry.
        /// </summary>
        private static void GetFileAssocData(out FileAssoc[] fileAssocs, out string[] progIDs)
        {
            var fileAssocsList = new C5.LinkedList<FileAssoc>();
            var progIDsList = new C5.LinkedList<string>();

            foreach (string keyName in Registry.ClassesRoot.GetSubKeyNames())
            {
                if (keyName.StartsWith("."))
                {
                    using (var assocKey = Registry.ClassesRoot.OpenSubKey(keyName))
                    {
                        if (assocKey == null) continue;
                        string mainProgID = (assocKey.GetValue("") ?? "").ToString();
                        string[] openWithProgIDs = GetSubKeyNames(assocKey, "OpenWithProgIDs");
                        fileAssocsList.Add(new FileAssoc(keyName, mainProgID, openWithProgIDs));
                    }
                }
                else progIDsList.Add(keyName);
            }

            fileAssocs = fileAssocsList.ToArray();
            progIDs = progIDsList.ToArray();
        }

        private static string[] GetValueNames(RegistryKey root, string key)
        {
            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey == null ? null : contextMenuExtendedKey.GetValueNames();
        }

        private static string[] GetSubKeyNames(RegistryKey root, string key)
        {
            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey == null ? null : contextMenuExtendedKey.GetSubKeyNames();
        }
        #endregion

        #region File system
        /// <summary>
        /// Stores information about the current state of the file system in a snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to store the data in.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the file system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file system was not permitted.</exception>
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
            var programDirs = new C5.ArrayList<string>();
            if (string.IsNullOrEmpty(programFiles32Bit)) Log.Warn(Resources.MissingProgramFiles32Bit);
            else programDirs.AddAll(Directory.GetDirectories(programFiles32Bit));
            if (!string.IsNullOrEmpty(programFiles64Bit))
                programDirs.AddAll(Directory.GetDirectories(programFiles64Bit));
            snapshot._programsDirs = programDirs.ToArray();
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="Snapshot"/> from a binary file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Snapshot"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the binary data.</exception>
        public static Snapshot Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return BinaryStorage.Load<Snapshot>(path);
        }

        /// <summary>
        /// Loads a <see cref="Snapshot"/> from a stream containing a binary file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Snapshot"/>.</returns>
        public static Snapshot Load(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return BinaryStorage.Load<Snapshot>(stream);
        }

        /// <summary>
        /// Saves this <see cref="Snapshot"/> to a binary file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            BinaryStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="Snapshot"/> to a stream as a binary file.
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            BinaryStorage.Save(stream, this);
        }
        #endregion
    }
}
