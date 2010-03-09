using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A recipe is a list of
    /// </summary>
    public sealed class Recipe : RetrievalMethod
    {
        #region Properties

        #region Steps
        // ToDo: Prevent double entries
        private readonly Collection<Archive> _archives = new Collection<Archive>();
        /// <summary>
        /// An ordered list of archives to extract.
        /// </summary>
        [Description("An ordered list of archives to extract.")]
        [XmlElement("archive")]
        public Collection<Archive> Archives { get { return _archives; } }
        #endregion

        #endregion
    }
}
