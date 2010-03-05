using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.MyApps
{
    /// <summary>
    /// An entry for the <see cref="AppList"/> list.
    /// </summary>
    public struct AppEntry
    {
        #region Properties
        /// <summary>
        /// The URI used to identify the interface and locate the feed.
        /// </summary>
        [Description("The URI used to identify the interface and locate the feed.")]
        [XmlIgnore]
        public Uri Interface
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false)]
        public String InterfaceString
        {
            get { return (Interface == null ? null : Interface.ToString()); }
            set { Interface = new Uri(value); }
        }
        #endregion
    }
}


