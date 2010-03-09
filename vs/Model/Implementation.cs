using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A specific (executable) implementation of an <see cref="Interface"/>.
    /// </summary>
    public sealed class Implementation : ImplementationBase
    {
        #region Properties
        /// <summary>
        /// A string to be appended to the version. The purpose of this is to allow complex version numbers (such as "1.0-rc2").
        /// </summary>
        [Category("Release"), Description("A string to be appended to the version. The purpose of this is to allow complex version numbers (such as \"1.0-rc2\").")]
        [XmlAttribute("version-modifier")]
        public string VersionModifier { get; set; }

        /// <summary>
        /// A unique identifier for this implementation.
        /// </summary>
        /// <remarks>For example, when the user marks a particular version as buggy this identifier is used to keep track of it, and saving and restoring selections uses it.</remarks>
        [Category("Identity"), Description("A unique identifier for this implementation.")]
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// If the feed file is a local file (the interface 'uri' starts with /) then the local-path attribute may contain the pathname of a local directory (either an absolute path or a path relative to the directory containing the feed file).
        /// </summary>
        [Category("Identity"), Description("If the feed file is a local file (the interface 'uri' starts with /) then the local-path attribute may contain the pathname of a local directory (either an absolute path or a path relative to the directory containing the feed file).")]
        [XmlAttribute("local-path")]
        public string LocalPath { get; set; }

        /// <summary>
        /// Digests of the .manifest file using various hashing algorithms.
        /// </summary>
        [Category("Identity"), Description("Digests of the .manifest file using various hashing algorithms.")]
        [XmlElement("manifest-digest")]
        public ManifestDigest ManifestDigest { get; set; }

        #region Retrieval methods
        private readonly Set<Archive> _archives = new Set<Archive>();
        /// <summary>
        /// A list of <see cref="Archive"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of archives as retrieval methods.")]
        [XmlElement("archive")]
        public Set<Archive> Archives { get { return _archives; } }

        private readonly Collection<Recipe> _recipes = new Collection<Recipe>();
        /// <summary>
        /// A list of <see cref="Recipe"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of recipes as retrieval methods.")]
        [XmlElement("recipe")]
        public Collection<Recipe> Recipes { get { return _recipes; } }
        #endregion

        #endregion
    }
}
