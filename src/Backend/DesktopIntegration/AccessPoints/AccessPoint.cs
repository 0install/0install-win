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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// An access point represents changes to the desktop environment's UI which the user explicitly requested.
    /// </summary>
    [XmlType("access-point", Namespace = AppList.XmlNamespace)]
    public abstract class AccessPoint : XmlUnknown, ICloneable
    {
        #region Conflict ID
        /// <summary>
        /// Retrieves identifiers from a namespace global to all <see cref="AccessPoint"/>s.
        /// Collisions in this namespace indicate that the concered <see cref="AccessPoint"/>s are in conflict cannot be applied on a single system at the same time.
        /// </summary>
        /// <param name="appEntry">The application entry containing this access point.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <remarks>These identifiers are not guaranteed to stay the same between versions. They should not be stored in files but instead always generated on demand.</remarks>
        public abstract IEnumerable<string> GetConflictIDs(AppEntry appEntry);
        #endregion

        #region Apply
        /// <summary>
        /// Applies this access point to the current machine.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply the configuration machine-wide instead of just for the current user.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the access point's data or a referenced <see cref="Store.Model.Capabilities.Capability"/>'s data are invalid.</exception>
        public abstract void Apply(AppEntry appEntry, Feed feed, ITaskHandler handler, bool machineWide);

        /// <summary>
        /// Unapplies this access point on the current machine.
        /// </summary>
        /// <param name="appEntry">The application entry containing this access point.</param>
        /// <param name="machineWide">Apply the configuration machine-wide instead of just for the current user.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public abstract void Unapply(AppEntry appEntry, bool machineWide);
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPoint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPoint"/>.</returns>
        public abstract AccessPoint Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
