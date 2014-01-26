﻿/*
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
using System.IO;
using System.Net;
using Common.Tasks;

namespace ZeroInstall.DesktopIntegration.Unix
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.UrlProtocol"/> and <see cref="AccessPoints.UrlProtocol"/> on GNOME systems.
    /// </summary>
    public static class UrlProtocol
    {
        #region Register
        /// <summary>
        /// Registers a URL protocol in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="urlProtocol">The URL protocol to register.</param>
        /// <param name="machineWide">Register the URL protocol machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="accessPoint">Indicates that the handler shall become the default handler for the protocol.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="urlProtocol"/> is invalid.</exception>
        public static void Register(InterfaceFeed target, Store.Model.Capabilities.UrlProtocol urlProtocol, bool machineWide, ITaskHandler handler, bool accessPoint = false)
        {
            #region Sanity checks
            if (urlProtocol == null) throw new ArgumentNullException("urlProtocol");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(urlProtocol.ID)) throw new InvalidDataException("Missing ID");

            // TODO: Implement
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a URL protocol in the current system.
        /// </summary>
        /// <param name="urlProtocol">The URL protocol to remove.</param>
        /// <param name="machineWide">Unregister the URL protocol machine-wide instead of just for the current user.</param>
        /// <param name="accessPoint">Indicates that the handler was the default handler for the protocol.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="urlProtocol"/> is invalid.</exception>
        public static void Unregister(Store.Model.Capabilities.UrlProtocol urlProtocol, bool machineWide, bool accessPoint = false)
        {
            #region Sanity checks
            if (urlProtocol == null) throw new ArgumentNullException("urlProtocol");
            #endregion

            if (string.IsNullOrEmpty(urlProtocol.ID)) throw new InvalidDataException("Missing ID");

            // TODO: Implement
        }
        #endregion
    }
}
