/*
 * Copyright 2010-2013 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents version restrictions for a specific sub-implementation in a selection process.
    /// </summary>
    [XmlRoot("version-for", Namespace = Feed.XmlNamespace), XmlType("version-for", Namespace = Feed.XmlNamespace)]
    public class VersionFor : ICloneable, IEquatable<VersionFor>
    {
        #region Properties
        /// <summary>
        /// The URI or local path (must be absolute) of the interface to set the restriction for.
        /// </summary>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        [Description("The URI or local path (must be absolute) of the interface to set the restriction for.")]
        [XmlAttribute("interface")]
        public string InterfaceID { get; set; }

        /// <summary>
        /// The range of versions of the implementation that can be chosen. <see langword="null"/> for no limit.
        /// </summary>
        [Description("The range of versions of the implementation that can be chosen. null for no limit.")]
        [XmlIgnore]
        public VersionRange Versions { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Versions"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string VersionsString { get { return (Versions == null) ? null : Versions.ToString(); } set { Versions = string.IsNullOrEmpty(value) ? null : new VersionRange(value); } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="VersionFor"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="VersionFor"/>.</returns>
        public VersionFor Clone()
        {
            return new VersionFor {InterfaceID = InterfaceID, Versions = Versions};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the version restriction in the form "InterfaceID: Versions". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}: {1}", InterfaceID, Versions);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(VersionFor other)
        {
            if (other == null) return false;
            if (InterfaceID != other.InterfaceID) return false;
            if (Versions != other.Versions) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(VersionFor) && Equals((VersionFor)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (InterfaceID != null ? InterfaceID.GetHashCode() : 0);
                // ToDo: result = (result * 397) ^ _languages.GetSequencedHashCode();
                result = (result * 397) ^ (Versions != null ? Versions.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
