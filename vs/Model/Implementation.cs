using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

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
        // ToDo: Prevent double entries
        private readonly Collection<Archive> _archives = new Collection<Archive>();
        /// <summary>
        /// A list of <see cref="Archive"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of archives as retrieval methods.")]
        [XmlElement("archive")]
        public Collection<Archive> Archives { get { return _archives; } }

        private readonly Collection<Recipe> _recipes = new Collection<Recipe>();
        /// <summary>
        /// A list of <see cref="Recipe"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of recipes as retrieval methods.")]
        [XmlElement("recipe")]
        public Collection<Recipe> Recipes { get { return _recipes; } }
        #endregion

        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Replaces <see cref="Stability.Unset"/> with <see cref="Stability.Testing"/>.
        /// Transfers legacy entries from <see cref="ID"/> to <see cref="LocalPath"/> and <see cref="ManifestDigest"/>.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the interface again since it will lose some of its structure.</remarks>
        public void Simplify()
        {
            // Default stability rating to testing
            if (Stability == Stability.Unset) Stability = Stability.Testing;

            if (string.IsNullOrEmpty(ID)) return;

            const string sha1Prefix = "sha1=";
            const string sha1NewPrefix = "sha1new=";
            const string sha256Prefix = "sha256=";

            // Fill in values (only if missing) using legacy entries (indentified by prefixes)
            if (string.IsNullOrEmpty(LocalPath) && (ID.StartsWith(".") || ID.StartsWith("/"))) LocalPath = ID;
            else if (string.IsNullOrEmpty(ManifestDigest.Sha1) && ID.StartsWith(sha1Prefix)) ManifestDigest.Sha1 = ID.Substring(sha1Prefix.Length);
            else if (string.IsNullOrEmpty(ManifestDigest.Sha1New) && ID.StartsWith(sha1NewPrefix)) ManifestDigest.Sha1New = ID.Substring(sha1NewPrefix.Length);
            else if (string.IsNullOrEmpty(ManifestDigest.Sha256) && ID.StartsWith(sha256Prefix)) ManifestDigest.Sha256 = ID.Substring(sha256Prefix.Length);
        }
        #endregion
    }
}
