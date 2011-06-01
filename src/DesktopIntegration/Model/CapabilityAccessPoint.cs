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

using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// An access points that builds upon a <see cref="ZeroInstall.Model.Capabilities.Capability"/>.
    /// </summary>
    [XmlType("capablity-access-point", Namespace = XmlNamespace)]
    public abstract class CapabilityAccessPoint : AccessPoint
    {
        #region Properties
        /// <summary>
        /// The ID of the <see cref="Capability"/> being referenced.
        /// </summary>
        [Description("The ID of the Capability being referenced.")]
        [XmlAttribute("capability")]
        public string Capability { get; set; }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(CapabilityAccessPoint other)
        {
            if (other == null) return false;

            return other.Capability == Capability;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Capability ?? "").GetHashCode();
        }
        #endregion
    }
}
