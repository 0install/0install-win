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
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.Win32;
using NanoByte.Common.Tasks;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.ContextMenu"/> and <see cref="AccessPoints.ContextMenu"/> on Windows systems.
    /// </summary>
    public static class ContextMenu
    {
        #region Constants
        /// <summary>The HKCU registry key prefix for registering things for all files.</summary>
        public const string RegKeyClassesFilesPrefix = "*";

        /// <summary>The HKCU registry key prefix for registering things for all directories.</summary>
        public const string RegKeyClassesDirectoriesPrefix = "Directory";

        /// <summary>The registry key postfix for registering simple context menu entries.</summary>
        public const string RegKeyContextMenuSimplePostfix = @"shell";

        /// <summary>The registry key postfix for registering extended (COM-based) context menu entries.</summary>
        public const string RegKeyContextMenuExtendedPostfix = @"shellex\ContextMenuHandlers";

        /// <summary>The registry key postfix for registering (COM-based) property sheets.</summary>
        public const string RegKeyPropertySheetsPostfix = @"shellex\PropertySheetHandlers";

        private static string GetPrefix(Store.Model.Capabilities.ContextMenuTarget target)
        {
            switch (target)
            {
                case Store.Model.Capabilities.ContextMenuTarget.Files:
                    return RegKeyClassesFilesPrefix;
                case Store.Model.Capabilities.ContextMenuTarget.Directories:
                default:
                    return RegKeyClassesDirectoriesPrefix;
            }
        }
        #endregion

        #region Apply
        /// <summary>
        /// Adds a context menu entry to the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="contextMenu">The context menu entry to add.</param>
        /// <param name="machineWide">Add the context menu entry machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="contextMenu"/> is invalid.</exception>
        public static void Apply(InterfaceFeed target, Store.Model.Capabilities.ContextMenu contextMenu, bool machineWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (contextMenu == null) throw new ArgumentNullException("contextMenu");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (contextMenu.Verb == null) throw new InvalidDataException("Missing verb");
            if (string.IsNullOrEmpty(contextMenu.Verb.Name)) throw new InvalidDataException("Missing verb name");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var verbKey = hive.CreateSubKey(FileType.RegKeyClasses + @"\" + GetPrefix(contextMenu.Target) + @"\shell\" + contextMenu.Verb.Name))
            {
                string description = contextMenu.Verb.Descriptions.GetBestLanguage(CultureInfo.CurrentUICulture);
                if (description != null) verbKey.SetValue("", description);
                if (contextMenu.Verb.Extended) verbKey.SetValue(FileType.RegValueExtended, "");

                using (var commandKey = verbKey.CreateSubKey("command"))
                    commandKey.SetValue("", FileType.GetLaunchCommandLine(target, contextMenu.Verb, machineWide, handler));
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes a context menu entry from the current system.
        /// </summary>
        /// <param name="contextMenu">The context menu entry to remove.</param>
        /// <param name="machineWide">Remove the context menu entry machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="contextMenu"/> is invalid.</exception>
        public static void Remove(Store.Model.Capabilities.ContextMenu contextMenu, bool machineWide)
        {
            #region Sanity checks
            if (contextMenu == null) throw new ArgumentNullException("contextMenu");
            #endregion

            if (contextMenu.Verb == null) throw new InvalidDataException("Missing verb");
            if (string.IsNullOrEmpty(contextMenu.Verb.Name)) throw new InvalidDataException("Missing verb name");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;
            try
            {
                hive.DeleteSubKeyTree(FileType.RegKeyClasses + @"\" + GetPrefix(contextMenu.Target) + @"\shell\" + contextMenu.Verb.Name);
            }
            catch (ArgumentException)
            {} // Ignore missing registry keys
        }
        #endregion
    }
}
