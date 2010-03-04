using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Backend.Model
{
    /// <summary>
    /// An icon for an <see cref="Interface"/>.
    /// </summary>
    public struct Icon
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

        public override string ToString()
        {
            return this.LocationString + " (" + MimeType + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj is Icon)
            {
                Icon icon = (Icon)obj;
                return LocationString == icon.LocationString;
            }
            else
            {
                return false;
            }
        }
    }
}
