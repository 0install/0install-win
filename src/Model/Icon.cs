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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using ZeroInstall.Model.Design;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An icon for an interface.
    /// </summary>
    [Serializable]
    [XmlType("icon", Namespace = Feed.XmlNamespace)]
    public class Icon : XmlUnknown, ICloneable, IEquatable<Icon>
    {
        #region Constants
        /// <summary>
        /// The <see cref="MimeType"/> value for PNG icons.
        /// </summary>
        public const string MimeTypePng = "image/png";

        /// <summary>
        /// The <see cref="MimeType"/> value for Windows ICO icons.
        /// </summary>
        public const string MimeTypeIco = "image/vnd.microsoft.icon";
        #endregion

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
        [XmlAttribute("href"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string LocationString { get { return (Location == null ? null : Location.ToString()); } set { Location = (value == null ? null : new Uri(value)); } }

        /// <summary>
        /// The MIME type of the icon. This value is case-insensitive.
        /// </summary>
        [Description("The MIME type of the icon. This value is case-insensitive.")]
        [XmlAttribute("type")]
        [TypeConverter(typeof(IconMimeTypeConverter))]
        public string MimeType { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty icon.
        /// </summary>
        public Icon()
        {}

        /// <summary>
        /// Creates a new icon with pre-set values.
        /// </summary>
        /// <param name="location">The URL used to locate the icon.</param>
        /// <param name="mimeType">The MIME type of the icon.</param>
        public Icon(Uri location, string mimeType)
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

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Icon"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Icon"/>.</returns>
        public Icon Clone()
        {
            return new Icon { UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Location = Location, MimeType = MimeType};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Icon other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Location == Location && other.MimeType == MimeType;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Icon && Equals((Icon)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (Location != null) result = (result * 397) ^ Location.GetHashCode();
                if (MimeType != null) result = (result * 397) ^ MimeType.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
