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
        [XmlAttribute("not-before")]
        public string NotBeforeVersion { get; set; }

        /// <summary>
        /// This version and all later versions are unsuitable.
        /// </summary>
        [Description("This version and all later versions are unsuitable.")]
        [XmlAttribute("before")]
        public string BeforeVersion { get; set; }
        #endregion
    }
}
