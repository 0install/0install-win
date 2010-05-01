/*
 * Copyright 2010 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An icon for an <see cref="Interface"/>.
    /// </summary>
    [TypeConverter(typeof(IconConverter))]
    public struct Icon : IEquatable<Icon>
    {
        #region Properties
        /// <summary>
        /// The URL used to locate the icon.
        /// </summary>
        [Description("The URL used to locate the icon.")]
        [XmlIgnore]
        public Uri Location { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Location"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("href"), Browsable(false)]
        public String LocationString
        {
            get { return (Location == null ? null : Location.ToString()); }
            set { Location = (value == null ? null : new Uri(value)); }
        }

        /// <summary>
        /// The MIME type of the icon.
        /// </summary>
        [Description("The MIME type of the icon.")]
        [XmlAttribute("type")]
        public String MimeType { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new icon sturcture with pre-set values.
        /// </summary>
        /// <param name="location">The URL used to locate the icon.</param>
        /// <param name="mimeType">The MIME type of the icon.</param>
        public Icon(Uri location, string mimeType) : this()
        {
            Location = location;
            MimeType = mimeType;
        }

        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("{0} ({1})", Location, MimeType);
        }
        #endregion

        #region Equality
        public bool Equals(Icon other)
        {
            return other.LocationString == LocationString && other.MimeType == MimeType;
        }

        public static bool operator ==(Icon left, Icon right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Icon left, Icon right)
        {
            return !left.Equals(right);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(Icon) && Equals((Icon)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LocationString != null ? LocationString.GetHashCode() : 0) * 397) ^ (MimeType != null ? MimeType.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
