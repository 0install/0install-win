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
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Store;

namespace ZeroInstall.DesktopIntegration.Unix
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.FileType"/> and <see cref="AccessPoints.FileType"/> on FreeDesktop.org systems.
    /// </summary>
    public static class FileType
    {
        #region Register
        /// <summary>
        /// Registers a file type in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="fileType">The file type to register.</param>
        /// <param name="machineWide">Register the file type machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="accessPoint">Indicates that the file associations shall become default handlers for their respective types.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="fileType"/> is invalid.</exception>
        public static void Register(FeedTarget target, [NotNull] Store.Model.Capabilities.FileType fileType, bool machineWide, [NotNull] ITaskHandler handler, bool accessPoint = false)
        {
            #region Sanity checks
            if (fileType == null) throw new ArgumentNullException(nameof(fileType));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            if (string.IsNullOrEmpty(fileType.ID)) throw new InvalidDataException("Missing ID");

            // TODO: Implement
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a file type in the current system.
        /// </summary>
        /// <param name="fileType">The file type to remove.</param>
        /// <param name="machineWide">Unregister the file type machine-wide instead of just for the current user.</param>
        /// <param name="accessPoint">Indicates that the file associations were default handlers for their respective types.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="fileType"/> is invalid.</exception>
        public static void Unregister([NotNull] Store.Model.Capabilities.FileType fileType, bool machineWide, bool accessPoint = false)
        {
            #region Sanity checks
            if (fileType == null) throw new ArgumentNullException(nameof(fileType));
            #endregion

            if (string.IsNullOrEmpty(fileType.ID)) throw new InvalidDataException("Missing ID");

            // TODO: Implement
        }
        #endregion
    }
}
