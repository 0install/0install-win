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

using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// Abstract base class for capabilities that can be applied as default handlers for something at the user's request.
    /// </summary>
    [XmlType("default-capability", Namespace = CapabilityList.XmlNamespace)]
    public abstract class DefaultCapability : Capability
    {
        /// <summary>
        /// When set to <c>true</c> this capability is not applied as a default handler without explicit confirmation from the user.
        /// </summary>
        /// <remarks>Use this to exclude exotic capabilities from default integration categories.</remarks>
        [Description("When set to true do not apply this capability is not applied as a default handler without explicit confirmation from the user. Use this to exclude exotic capabilities from default integration categories.")]
        [XmlAttribute("explicit-only"), DefaultValue(false)]
        public bool ExplicitOnly { get; set; }

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(DefaultCapability other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.ExplicitOnly == ExplicitOnly;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ ExplicitOnly.GetHashCode();
            }
        }
        #endregion
    }
}
