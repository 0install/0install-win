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
using System.Xml;
using System.Xml.Serialization;
using ZeroInstall.Model.Design;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An icon for an interface.
    /// </summary>
    [TypeConverter(typeof(IconConverter))]
    [Serializable]
    [XmlType("icon", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
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
        [XmlAttribute("href"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        /// <summary>
        /// Contains any unknown additional XML attributes.
        /// </summary>
        [XmlAnyAttribute]
        public XmlAttribute[] UnknownAttributes;
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
        /// <summary>
        /// Returns the icon in the form "Icon: Location (MimeType)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("Icon: {0} ({1})", Location, MimeType);
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
                return ((LocationString ?? "").GetHashCode() * 397) ^ (MimeType ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
