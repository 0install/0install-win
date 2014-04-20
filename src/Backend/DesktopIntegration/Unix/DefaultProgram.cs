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
using System.IO;
using System.Net;
using NanoByte.Common.Tasks;

namespace ZeroInstall.DesktopIntegration.Unix
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.DefaultProgram"/> and <see cref="AccessPoints.DefaultProgram"/> on GNOME or KDE systems.
    /// </summary>
    public static class DefaultProgram
    {
        #region Register
        /// <summary>
        /// Registers an application as a candidate for a default program for some service in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="defaultProgram">The default program information to be registered.</param>
        /// <param name="machineWide">Apply the registration machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="accessPoint">Indicates that the program should be set as the current default for the service it provides.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="defaultProgram"/> is invalid.</exception>
        public static void Register(InterfaceFeed target, Store.Model.Capabilities.DefaultProgram defaultProgram, bool machineWide, ITaskHandler handler, bool accessPoint = false)
        {
            #region Sanity checks
            if (defaultProgram == null) throw new ArgumentNullException("defaultProgram");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(defaultProgram.ID)) throw new InvalidDataException("Missing ID");
            if (string.IsNullOrEmpty(defaultProgram.Service)) throw new InvalidDataException("Missing Service");

            // TODO: Implement
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters an application as a candidate for a default program in the current system. This can only be applied machine-wide, not per user.
        /// </summary>
        /// <param name="defaultProgram">The default program information to be removed.</param>
        /// <param name="machineWide">Apply the registration machine-wide instead of just for the current user.</param>
        /// <param name="accessPoint">Indicates that the program was set as the current default for the service it provides.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="defaultProgram"/> is invalid.</exception>
        public static void Unregister(Store.Model.Capabilities.DefaultProgram defaultProgram, bool machineWide, bool accessPoint = false)
        {
            #region Sanity checks
            if (defaultProgram == null) throw new ArgumentNullException("defaultProgram");
            #endregion

            if (string.IsNullOrEmpty(defaultProgram.ID)) throw new InvalidDataException("Missing ID");
            if (string.IsNullOrEmpty(defaultProgram.Service)) throw new InvalidDataException("Missing Service");

            // TODO: Implement
        }
        #endregion
    }
}
