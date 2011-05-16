/*
 * Copyright 2011 Bastian Eicher
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
namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// A capability tells the desktop environment what an application can do and in which fashion this can be represented to the user. It does not change the behaviour of existing UI elements.
    /// </summary>
    [XmlType("integration", Namespace = XmlNamespace)]
    public abstract class Capability : ICloneable
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
        /// Creates a deep copy of this <see cref="Capability"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Capability"/>.</returns>
        public abstract Capability CloneCapability();

        /// <summary>
        /// Creates a deep copy of this <see cref="Capability"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Capability"/>.</returns>
        public object Clone()
        {
            return CloneCapability();
        }
        #endregion
    }
}
