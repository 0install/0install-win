/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common.Tasks;
using ZeroInstall.Store;

namespace ZeroInstall.DesktopIntegration.Unix
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.ContextMenu"/> and <see cref="AccessPoints.ContextMenu"/> on GNOME systems.
    /// </summary>
    public static class ContextMenu
    {
        #region Apply
        /// <summary>
        /// Adds a context menu entry to the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="contextMenu">The context menu entry to add.</param>
        /// <param name="machineWide">Add the context menu entry machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="contextMenu"/> is invalid.</exception>
        public static void Apply(FeedTarget target, Store.Model.Capabilities.ContextMenu contextMenu, bool machineWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (contextMenu == null) throw new ArgumentNullException(nameof(contextMenu));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            if (contextMenu.Verb == null) throw new InvalidDataException("Missing verb");
            if (string.IsNullOrEmpty(contextMenu.Verb.Name)) throw new InvalidDataException("Missing verb name");

            // TODO: Implement
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes a context menu entry from the current system.
        /// </summary>
        /// <param name="contextMenu">The context menu entry to remove.</param>
        /// <param name="machineWide">Remove the context menu entry machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="contextMenu"/> is invalid.</exception>
        public static void Remove(Store.Model.Capabilities.ContextMenu contextMenu, bool machineWide)
        {
            #region Sanity checks
            if (contextMenu == null) throw new ArgumentNullException(nameof(contextMenu));
            #endregion

            if (contextMenu.Verb == null) throw new InvalidDataException("Missing verb");
            if (string.IsNullOrEmpty(contextMenu.Verb.Name)) throw new InvalidDataException("Missing verb name");

            // TODO: Implement
        }
        #endregion
    }
}
