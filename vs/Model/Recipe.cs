using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A recipe is a list of
    /// </summary>
    public sealed class Recipe : RetrievalMethod
    {
        #region Properties

        #region Steps
        private readonly Set<Archive> _archives = new Set<Archive>();
        /// <summary>
        /// An ordered list of archives to extract.
        /// </summary>
        [Description("An ordered list of archives to extract.")]
        [XmlElement("archive")]
        public Set<Archive> Archives { get { return _archives; } }
        #endregion

        #endregion
    }
}
