/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Net;
using Common;
using Common.Tasks;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for creating and removing Windows "add/remove programs" uninstall entries.
    /// </summary>
    public static class UninstallEntry
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key for uninstall entries.</summary>
        public const string RegKeyClasses = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        #endregion

        #region Create
        /// <summary>
        /// Creates an uninstall entry for an application.
        /// </summary>
        /// <param name="name">The name of the application the entry represents.</param>
        /// <param name="target">The application the entry represents.</param>
        /// <param name="systemWide">Create the entry system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Create(string name, InterfaceFeed target, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implement
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes an uninstall entry for an application.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application the entry represents.</param>
        /// <param name="systemWide">Remove the entry system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Remove(string interfaceID, bool systemWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            // ToDo: Implement
        }
        #endregion
    }
}
