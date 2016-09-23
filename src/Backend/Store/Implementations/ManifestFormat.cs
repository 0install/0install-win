using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Abstract class to encapsulate the differences between the different formats that can be used to save and load <see cref="Manifest"/>s.
    /// </summary>
    /// <remarks>
    /// Comprises: The digest method used and the format specification used to serialize and deserialize manifests.
    /// </remarks>
    [Serializable]
    public abstract class ManifestFormat
    {
        #region Static
        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha1New"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public static ManifestFormat Sha1New { get; } = new Sha1NewFormat();

        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha256"/>.
        /// </summary>
        public static ManifestFormat Sha256 { get; } = new Sha256Format();

        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha256New"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public static ManifestFormat Sha256New { get; } = new Sha256NewFormat();

        /// <summary>
        /// All currently supported <see cref="ManifestFormat"/>s listed from best (safest) to worst.
        /// </summary>
        public static readonly ManifestFormat[] All = {Sha256New, Sha256, Sha1New};

        /// <summary>
        /// Selects the correct <see cref="ManifestFormat"/> based on the digest prefix.
        /// </summary>
        /// <param name="id">The digest id to extract the prefix from or only the prefix.</param>
        /// <exception cref="ArgumentException"><paramref name="id"/> is no known algorithm prefix.</exception>
        [NotNull]
        public static ManifestFormat FromPrefix([NotNull] string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            #endregion

            if (id.StartsWith(Sha256New.Prefix)) return Sha256New;
            if (id.StartsWith(Sha256.Prefix)) return Sha256;
            if (id.StartsWith(Sha1New.Prefix)) return Sha1New;
            throw new ArgumentException(Resources.NoKnownDigestMethod);
        }
        #endregion

        #region Instance
        /// <summary>
        /// The prefix used to identify the format (e.g. "sha256").
        /// </summary>
        [NotNull]
        public abstract string Prefix { get; }

        /// <summary>
        /// The separator placed between the <see cref="Prefix"/> and the actual digest.
        /// </summary>
        [NotNull]
        public virtual string Separator => "=";

        /// <inheritdoc/>
        public override string ToString() => Prefix;

        /// <summary>
        /// Generates the digest of a implementation file as used within the manifest file.
        /// </summary>
        /// <param name="stream">The content of the implementation file.</param>
        /// <returns>A string representation of the digest.</returns>
        [NotNull]
        public string DigestContent([NotNull] Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            #endregion

            return SerializeContentDigest(GetHashAlgorithm().ComputeHash(stream));
        }

        /// <summary>
        /// Generates the digest of a implementation file as used within the manifest file.
        /// </summary>
        /// <param name="data">The content of the implementation file as a byte array.</param>
        /// <returns>A string representation of the digest.</returns>
        [NotNull]
        public string DigestContent([NotNull] byte[] data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException(nameof(data));
            #endregion

            return SerializeContentDigest(GetHashAlgorithm().ComputeHash(data));
        }

        /// <summary>
        /// Generates the digest of a manifest file as used for the implementation directory name.
        /// </summary>
        /// <param name="stream">The content of the manifest file.</param>
        /// <returns>A string representation of the digest.</returns>
        [NotNull]
        public string DigestManifest([NotNull] Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            #endregion

            return SerializeManifestDigest(GetHashAlgorithm().ComputeHash(stream));
        }

        /// <summary>
        /// Retreives a new instance of the hashing algorithm used for generating digests.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Generates a new instance each time to allow for concurrent usage")]
        [NotNull]
        protected abstract HashAlgorithm GetHashAlgorithm();

        /// <summary>
        /// Serializes a hash as digest of an implementation file as used within the manifest file.
        /// </summary>
        protected virtual string SerializeContentDigest([NotNull] byte[] hash) => hash.Base16Encode();

        /// <summary>
        /// Serializes a hash as a digest of a manifest file as used for the implementation directory name.
        /// </summary>
        [NotNull]
        protected virtual string SerializeManifestDigest([NotNull] byte[] hash) => hash.Base16Encode();
        #endregion

        #region Inner classes
        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA1"/> digests.
        /// </summary>
        [Serializable]
        private class Sha1NewFormat : ManifestFormat
        {
            public override string Prefix => "sha1new";

            protected override HashAlgorithm GetHashAlgorithm() => SHA1.Create();
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA256"/> digests.
        /// </summary>
        [Serializable]
        private class Sha256Format : ManifestFormat
        {
            public override string Prefix => "sha256";

            protected override HashAlgorithm GetHashAlgorithm() => SHA256.Create();
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA256"/> digests.
        /// </summary>
        [Serializable]
        private class Sha256NewFormat : ManifestFormat
        {
            public override string Prefix => "sha256new";

            public override string Separator => "_";

            protected override HashAlgorithm GetHashAlgorithm() => SHA256.Create();

            protected override string SerializeManifestDigest(byte[] hash) => hash.Base32Encode();
        }
        #endregion
    }
}
