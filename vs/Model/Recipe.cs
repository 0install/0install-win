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
        
        //--------------------//

        #region Simplify
        /// <summary>
        /// Call <see cref="ISimplifyable.Simplify"/> on all contained <see cref="RetrievalStep"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Interface"/> again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            foreach (var archive in Archives) archive.Simplify();
        }
        #endregion

        //--------------------//

        // ToDo: Implement ToString and Equals
    }
}
