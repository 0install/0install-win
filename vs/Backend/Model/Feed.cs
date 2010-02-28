using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Backend.Model
{
    /// <summary>
    /// An additional feed for an <see cref="Interface"/>.
    /// </summary>
    public sealed class Feed : ReleaseBase
    {
        #region Properties
        /// <summary>
        /// The URL used to locate the feed.
        /// </summary>
        [Description("The URL used to locate the feed.")]
        [XmlIgnore]
        public Uri Source
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        [XmlAttribute("src"), Browsable(false)]
        public String SourceString
        {
            get { return (Source == null ? null : Source.ToString()); }
            set { Source = new Uri(value); }
        }
        #endregion
    }
}
