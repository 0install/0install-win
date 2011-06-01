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
using System.Xml.Serialization;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// An represents changes to the desktop environment's UI which the user explicitly requested.
    /// </summary>
    [XmlType("access-point", Namespace = XmlNamespace)]
    public abstract class AccessPoint : XmlUnknown, ICloneable
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing desktop integration data.
        /// </summary>
        public const string XmlNamespace = "http://0install.de/schema/injector/desktop-integration";
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
