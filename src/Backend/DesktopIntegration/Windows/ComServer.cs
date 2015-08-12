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

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.ComServer"/> on Windows systems.
    /// </summary>
    public static class ComServer
    {
        #region Constants
        /// <summary>The HKCR registry key for storing COM class IDs.</summary>
        public const string RegKeyClassesIDs = @"CLSID";
        #endregion

        #region Register
        /// <summary>
        /// Registers a COM server in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="comServer">The COM server to be registered.</param>
        /// <param name="machineWide">Register the COM server machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="comServer"/> is invalid.</exception>
        public static void Register(FeedTarget target, [NotNull] Store.Model.Capabilities.ComServer comServer, bool machineWide, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (comServer == null) throw new ArgumentNullException(nameof(comServer));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            if (string.IsNullOrEmpty(comServer.ID)) throw new InvalidDataException("Missing ID");

            // TODO: Implement
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a COM server in the current system.
        /// </summary>
        /// <param name="comServer">The COM server to be unregistered.</param>
        /// <param name="machineWide">Unregister the COM server machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="comServer"/> is invalid.</exception>
        public static void Unregister([NotNull] Store.Model.Capabilities.ComServer comServer, bool machineWide)
        {
            #region Sanity checks
            if (comServer == null) throw new ArgumentNullException(nameof(comServer));
            #endregion

            if (string.IsNullOrEmpty(comServer.ID)) throw new InvalidDataException("Missing ID");

            // TODO: Implement
        }
        #endregion
    }
}
