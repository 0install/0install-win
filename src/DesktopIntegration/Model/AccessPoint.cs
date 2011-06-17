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
using System.Xml.Serialization;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// An access point represents changes to the desktop environment's UI which the user explicitly requested.
    /// </summary>
    [XmlType("access-point", Namespace = AppList.XmlNamespace)]
    public abstract class AccessPoint : XmlUnknown, ICloneable
    {
        #region Apply
        /// <summary>
        /// Applies this access point to the current machine.
        /// </summary>
        /// <param name="appEntry">The application entry containing this access point.</param>
        /// <param name="feed">The feed of the application to get additional information (e.g. icons) from.</param>
        /// <param name="systemWide">Apply the configuration system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public abstract void Apply(AppEntry appEntry, Feed feed, bool systemWide);

        /// <summary>
        /// Unapplies this access point on the current machine.
        /// </summary>
        /// <param name="appEntry">The application entry containing this access point.</param>
        /// <param name="systemWide">Apply the configuration system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public abstract void Unapply(AppEntry appEntry, bool systemWide);
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPoint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPoint"/>.</returns>
        public abstract AccessPoint CloneAccessPoint();

        /// <summary>
        /// Creates a deep copy of this <see cref="AccessPoint"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="AccessPoint"/>.</returns>
        public object Clone()
        {
            return CloneAccessPoint();
        }
        #endregion
    }
}
