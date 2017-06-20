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

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NanoByte.Common;

namespace ZeroInstall.Store.Model.Preferences
{
    /// <summary>
    /// Stores user-specific preferences for an <see cref="Implementation"/>.
    /// </summary>
    [XmlType("implementation-preferences", Namespace = Feed.XmlNamespace)]
    public sealed class ImplementationPreferences : XmlUnknown, ICloneable<ImplementationPreferences>, IEquatable<ImplementationPreferences>
    {
        /// <summary>
        /// A unique identifier for the implementation. Coressponds to <see cref="ImplementationBase.ID"/>.
        /// </summary>
        [Description("A unique identifier for the implementation.")]
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// A user-specified override for <see cref="Element.Stability"/> specified in the feed.
        /// </summary>
        [Description("A user-specified override for the implementation stability specified in the feed.")]
        [XmlAttribute("user-stability"), DefaultValue(typeof(Stability), "Unset")]
        public Stability UserStability { get; set; } = Stability.Unset;

        /// <summary>
        /// Indicates whether this configuration object stores no information other than the <see cref="ID"/> and is thus superflous.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public bool IsSuperflous => (UserStability == Stability.Unset);

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ImplementationPreferences"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ImplementationPreferences"/>.</returns>
        public ImplementationPreferences Clone() => new ImplementationPreferences {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID, UserStability = UserStability};
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the preferences in the form "ImplementationPreferences: ID". Not safe for parsing!
        /// </summary>
        public override string ToString() => $"ImplementationPreferences: {ID}";
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ImplementationPreferences other)
        {
            if (other == null) return false;
            return base.Equals(other) && ID == other.ID && UserStability == other.UserStability;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is ImplementationPreferences && Equals((ImplementationPreferences)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ ID?.GetHashCode() ?? 0;
                result = (result * 397) ^ (int)UserStability;
                return result;
            }
        }
        #endregion
    }
}
