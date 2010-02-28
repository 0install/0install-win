using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Backend.Model
{
    /// <summary>
    /// Restrict the set of versions from which the injector may choose an <see cref="Implementation"/>. 
    /// </summary>
    public struct Constraint
    {
        #region Properties
        /// <summary>
        /// This is the lowest-numbered version that can be chosen.
        /// </summary>
        [Description("This is the lowest-numbered version that can be chosen.")]
        [XmlIgnore]
        public Version NotBeforeVersion { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="NotBeforeVersion"/>
        [XmlAttribute("not-before"), Browsable(false)]
        public string NotBeforeVersionString
        {
            get { return (NotBeforeVersion == null ? null : NotBeforeVersion.ToString()); }
            set { NotBeforeVersion = new Version((value)); }
        }

        /// <summary>
        /// This version and all later versions are unsuitable.
        /// </summary>
        [Description("This version and all later versions are unsuitable.")]
        [XmlIgnore]
        public Version BeforeVersion { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="BeforeVersion"/>
        [XmlAttribute("before"), Browsable(false)]
        public string BeforeVersionString
        {
            get { return (BeforeVersion == null ? null : BeforeVersion.ToString()); }
            set { BeforeVersion = new Version((value)); }
        }
        #endregion
    }
}
