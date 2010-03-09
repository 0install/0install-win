using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// An icon for an <see cref="Interface"/>.
    /// </summary>
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
            set { Location = new Uri(value); }
        }

        /// <summary>
        /// The MIME type of the icon.
        /// </summary>
        [Description("The MIME type of the icon.")]
        [XmlAttribute("type")]
        public String MimeType { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("{0} ({1})", Location, MimeType);
        }
        #endregion

        #region Compare
        public bool Equals(Icon other)
        {
            return other.LocationString == LocationString && other.MimeType == MimeType;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj.GetType() == typeof(Icon) && Equals((Icon)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LocationString != null ? LocationString.GetHashCode() : 0) * 397) ^ (MimeType != null ? MimeType.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Icon left, Icon right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Icon left, Icon right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
