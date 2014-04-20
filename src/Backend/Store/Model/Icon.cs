/*
 * Copyright 2010-2014 Bastian Eicher
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
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// An icon representing the application. Used in the Catalog GUI as well as for desktop icons, menu entries, etc..
    /// </summary>
    /// <seealso cref="Feed.Icons"/>
    /// <seealso cref="EntryPoint.Icons"/>
    [Description("An icon representing the application. Used in the Catalog GUI as well as for desktop icons, menu entries, etc..")]
    [Serializable]
    [XmlRoot("icon", Namespace = Feed.XmlNamespace), XmlType("icon", Namespace = Feed.XmlNamespace)]
    public class Icon : FeedElement, ICloneable, IEquatable<Icon>
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

        /// <summary>
        /// All known <see cref="MimeType"/> values for icons.
        /// </summary>
        public static readonly string[] KnownMimeTypes = {MimeTypePng, MimeTypeIco};
        #endregion

        #region Properties
        /// <summary>
        /// The URL used to locate the icon.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public Uri Href { get; set; }

        /// <summary>Used for XML serialization and PropertyGrid.</summary>
        /// <seealso cref="Href"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [DisplayName(@"Href"), Description("The URL used to locate the icon.")]
        [XmlAttribute("href"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string HrefString { get { return (Href == null ? null : Href.ToString()); } set { Href = (string.IsNullOrEmpty(value) ? null : new Uri(value)); } }

        /// <summary>
        /// The MIME type of the icon. This value is case-insensitive.
        /// </summary>
        [Description("The MIME type of the icon. This value is case-insensitive.")]
        [XmlAttribute("type"), DefaultValue("")]
        [TypeConverter(typeof(IconMimeTypeConverter))]
        public string MimeType { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the icon in the form "Location (MimeType)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Href, MimeType);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Icon"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Icon"/>.</returns>
        public Icon Clone()
        {
            return new Icon {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Href = Href, MimeType = MimeType};
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
            return base.Equals(other) && other.Href == Href && other.MimeType == MimeType;
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
                if (Href != null) result = (result * 397) ^ Href.GetHashCode();
                if (MimeType != null) result = (result * 397) ^ MimeType.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
