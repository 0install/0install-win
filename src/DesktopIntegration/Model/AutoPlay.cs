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

namespace ZeroInstall.DesktopIntegration.Model
{
    /// <summary>
    /// Makes an application the default AutoPlay handler for a specific event.
    /// </summary>
    /// <seealso cref="ZeroInstall.Model.Capabilities.AutoPlay"/>
    [XmlType("auto-play", Namespace = XmlNamespace)]
    public class AutoPlay : CapabilityAccessPoint, IEquatable<AutoPlay>
    {
        #region Conversion
        /// <summary>
        /// Returns the access point in the form "AutoPlay". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("AutoPlay");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint CloneAccessPoint()
        {
            return new AutoPlay {Capability = Capability};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AutoPlay other)
        {
            if (other == null) return false;

            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(AutoPlay) && Equals((AutoPlay)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
