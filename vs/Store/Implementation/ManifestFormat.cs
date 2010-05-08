using System.Security.Cryptography;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Abstract class to encapsulate the differences between the different formats that can be used to save and load <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>
    /// Comprises: The hashing method used and the format specification used to serialize and deserialize manifests.
    /// </remarks>
    public abstract class ManifestFormat
    {
        #region Singleton properties
        private static readonly ManifestFormat _sha1Old = new Sha1OldFormat();
        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestMethod.Sha1Old"/>.
        /// </summary>
        public static ManifestFormat Sha1Old { get { return _sha1Old; } }

        private static readonly ManifestFormat _sha1New = new Sha1NewFormat();
        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestMethod.Sha1New"/>.
        /// </summary>
        public static ManifestFormat Sha1New { get { return _sha1New; } }

        private static readonly ManifestFormat _sha256 = new Sha256Format();
        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestMethod.Sha256"/>.
        /// </summary>
        public static ManifestFormat Sha256 { get { return _sha256; } }
        #endregion

        #region Properties
        /// <summary>
        /// The hashing algorithm used for <see cref="ManifestFileBase.Hash"/> and the <see cref="Manifest"/> hash itself.
        /// </summary>
        internal abstract HashAlgorithm HashingMethod { get; }
        #endregion

        #region Serialization methods
        /// <summary>
        /// Used to generate a file entry (line) for a specific <see cref="ManifestNode"/>.
        /// </summary>
        internal abstract string GenerateEntryForNode(ManifestNode node);

        /// <summary>
        /// Used to parse a file entry (line) back into a <see cref="ManifestNode"/>.
        /// </summary>
        internal abstract ManifestNode ReadDirectoryNodeFromString(string data);
        #endregion

        #region Inner classes
        /// <summary>
        /// An abstract base class for <see cref="ManifestFormat"/>s using the old manifest format.
        /// </summary>
        private abstract class OldFormat : ManifestFormat
        {
            internal override string GenerateEntryForNode(ManifestNode node)
            {
                return node.ToStringOld();
            }

            internal override ManifestNode ReadDirectoryNodeFromString(string data)
            {
                return ManifestDirectory.FromStringOld(data);
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the old manifest format and <see cref="SHA1"/> hashes.
        /// </summary>
        private class Sha1OldFormat : OldFormat
        {
            private static readonly HashAlgorithm _algorithm = SHA1.Create();
            internal override HashAlgorithm HashingMethod { get { return _algorithm; } }
        }

        /// <summary>
        /// An abstract base class for <see cref="ManifestFormat"/>s using the new manifest format.
        /// </summary>
        private abstract class NewFormat : ManifestFormat
        {
            internal override string GenerateEntryForNode(ManifestNode node)
            {
                return node.ToString();
            }

            internal override ManifestNode ReadDirectoryNodeFromString(string data)
            {
                return ManifestDirectory.FromString(data);
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA1"/> hashes.
        /// </summary>
        private class Sha1NewFormat : NewFormat
        {
            private static readonly HashAlgorithm _algorithm = SHA1.Create();
            internal override HashAlgorithm HashingMethod { get { return _algorithm; } }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA256"/> hashes.
        /// </summary>
        private class Sha256Format : NewFormat
        {
            private static readonly HashAlgorithm _algorithm = SHA256.Create();
            internal override HashAlgorithm HashingMethod { get { return _algorithm; } }
        }
        #endregion
    }

}
