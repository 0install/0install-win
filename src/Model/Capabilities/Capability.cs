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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// A capability tells the desktop environment what an application can do and in which fashion this can be represented to the user. It does not change the behaviour of existing UI elements.
    /// </summary>
    [XmlType("capability", Namespace = XmlNamespace)]
    public abstract class Capability : XmlUnknown, ICloneable
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing application capabilities.
        /// </summary>
        public const string XmlNamespace = "http://0install.de/schema/injector/capabilities";
        #endregion

        #region Properties
        /// <summary>
        /// Indicates whether this capability can be registered only machine-wide and not per-user.
        /// </summary>
        [XmlIgnore]
        public abstract bool GlobalOnly { get; }

        /// <summary>
        /// An ID that uniquely identifies this capability within an interface.
        /// </summary>
        /// <remarks>In case of conflicts the first capability listed with a specific ID will take precedence.</remarks>
        [Description("An ID that uniquely identifies this capability within an interface.")]
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// Identifiers from a namespace global to all <see cref="Capability"/>s.
        /// Collisions in this namespace indicate that the concered <see cref="Capability"/>s are in conflict cannot be registered on a single system at the same time.
        /// </summary>
        /// <remarks>These identifiers are not guaranteed to stay the same between versions. They should not be stored in files but instead always generated on demand.</remarks>
        public abstract IEnumerable<string> ConflictIDs { get; }
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

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(Capability other)
        {
            if (other == null) return false;

            return other.ID == ID;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (ID ?? "").GetHashCode();
        }
        #endregion
    }
}
