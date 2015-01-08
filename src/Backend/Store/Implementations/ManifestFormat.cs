using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
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
        #region Singleton properties
        private static readonly ManifestFormat _sha1 = new Sha1Format();

        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha1"/>.
        /// </summary>
        public static ManifestFormat Sha1 { get { return _sha1; } }

        private static readonly ManifestFormat _sha1New = new Sha1NewFormat();

        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha1New"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public static ManifestFormat Sha1New { get { return _sha1New; } }

        private static readonly ManifestFormat _sha256 = new Sha256Format();

        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha256"/>.
        /// </summary>
        public static ManifestFormat Sha256 { get { return _sha256; } }

        private static readonly ManifestFormat _sha256New = new Sha256NewFormat();

        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha256New"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public static ManifestFormat Sha256New { get { return _sha256New; } }

        /// <summary>
        /// All currently supported <see cref="ManifestFormat"/>s listed from best (safest) to worst.
        /// </summary>
        public static readonly ManifestFormat[] All = {_sha256New, _sha256, _sha1New, _sha1};

        /// <summary>
        /// All currently supported and non-deprecated <see cref="ManifestFormat"/>s listed from best (safest) to worst.
        /// </summary>
        public static readonly ManifestFormat[] Recommended = {_sha256New, _sha256, _sha1New};
        #endregion

        #region Factory methods
        /// <summary>
        /// Selects the correct <see cref="ManifestFormat"/> based on the digest prefix.
        /// </summary>
        /// <param name="id">The digest id to extract the prefix from or only the prefix.</param>
        /// <exception cref="ArgumentException"><paramref name="id"/> is no known algorithm prefix.</exception>
        [NotNull]
        public static ManifestFormat FromPrefix([NotNull] string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            if (id.StartsWith(Sha256New.Prefix)) return Sha256New;
            if (id.StartsWith(Sha256.Prefix)) return Sha256;
            if (id.StartsWith(Sha1New.Prefix)) return Sha1New;
            if (id.StartsWith(Sha1.Prefix)) return Sha1;
            throw new ArgumentException(Resources.NoKnownDigestMethod);
        }
        #endregion

        //--------------------//

        #region Properties
        /// <summary>
        /// The prefix used to identify the format (e.g. "sha256").
        /// </summary>
        [NotNull]
        public abstract string Prefix { get; }

        /// <summary>
        /// The separator placed between the <see cref="Prefix"/> and the actual digest.
        /// </summary>
        [NotNull]
        public virtual string Separator { get { return "="; } }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Prefix;
        }
        #endregion

        #region Builder methods
        /// <summary>
        /// Generates a file entry (line) for a specific <see cref="ManifestNode"/>.
        /// </summary>
        [NotNull]
        public abstract string GenerateEntryForNode([NotNull] ManifestNode node);

        /// <summary>
        /// Parses a file entry (line) back into a <see cref="ManifestDirectory"/>.
        /// </summary>
        [NotNull]
        internal abstract ManifestDirectory ReadDirectoryNodeFromEntry([NotNull] string entry);

        /// <summary>
        /// Creates a recursive list of all filesystem entries in a certain directory sorted according to the format specifications.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <returns>An array of filesystem entries.</returns>
        /// <exception cref="IOException">The directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the directory is not permitted.</exception>
        [NotNull, ItemNotNull]
        public abstract FileSystemInfo[] GetSortedDirectoryEntries([NotNull] string path);
        #endregion

        #region Digest methods
        /// <summary>
        /// Generates the digest of a implementation file as used within the manifest file.
        /// </summary>
        /// <param name="stream">The content of the implementation file.</param>
        /// <returns>A string representation of the digest.</returns>
        public string DigestContent([NotNull] Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return SerializeContentDigest(GetHashAlgorithm().ComputeHash(stream));
        }

        /// <summary>
        /// Generates the digest of a implementation file as used within the manifest file.
        /// </summary>
        /// <param name="data">The content of the implementation file as a byte array.</param>
        /// <returns>A string representation of the digest.</returns>
        public string DigestContent([NotNull] byte[] data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            return SerializeContentDigest(GetHashAlgorithm().ComputeHash(data));
        }

        /// <summary>
        /// Generates the digest of a manifest file as used for the implementation directory name.
        /// </summary>
        /// <param name="stream">The content of the manifest file.</param>
        /// <returns>A string representation of the digest.</returns>
        public string DigestManifest([NotNull] Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return SerializeManifestDigest(GetHashAlgorithm().ComputeHash(stream));
        }

        /// <summary>
        /// Retreives a new instance of the hashing algorithm used for generating digests.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Generates a new instance each time to allow for concurrent usage")]
        protected abstract HashAlgorithm GetHashAlgorithm();

        /// <summary>
        /// Serializes a hash as digest of an implementation file as used within the manifest file.
        /// </summary>
        protected virtual string SerializeContentDigest([NotNull] byte[] hash)
        {
            return hash.Base16Encode();
        }

        /// <summary>
        /// Serializes a hash as a digest of a manifest file as used for the implementation directory name.
        /// </summary>
        protected virtual string SerializeManifestDigest([NotNull] byte[] hash)
        {
            return hash.Base16Encode();
        }
        #endregion

        //--------------------//

        #region Inner classes: Old Format
        /// <summary>
        /// An abstract base class for <see cref="ManifestFormat"/>s using the old manifest format.
        /// </summary>
        [Serializable]
        private abstract class OldFormat : ManifestFormat
        {
            public override string GenerateEntryForNode(ManifestNode node)
            {
                #region Sanity checks
                if (node == null) throw new ArgumentNullException("node");
                #endregion

                return node.ToStringOld();
            }

            internal override ManifestDirectory ReadDirectoryNodeFromEntry(string entry)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(entry)) throw new ArgumentNullException("entry");
                #endregion

                return ManifestDirectory.FromStringOld(entry);
            }

            public override FileSystemInfo[] GetSortedDirectoryEntries(string path)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
                #endregion

                // Get a combined list of files and directories
                var entries = Directory.GetFileSystemEntries(path);

                // C-sort the list
                Array.Sort(entries, StringComparer.Ordinal);

                // Create the combined result list (files and sub-diretories mixed)
                var result = new List<FileSystemInfo>();
                foreach (string entry in entries)
                {
                    if (Directory.Exists(entry))
                    {
                        result.Add(new DirectoryInfo(entry));

                        // Recurse into sub-direcories (but do not follow symlinks)
                        if (!FileUtils.IsSymlink(entry)) result.AddRange(GetSortedDirectoryEntries(entry));
                    }
                    else result.Add(new FileInfo(entry));
                }
                return result.ToArray();
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the old manifest format and <see cref="SHA1"/> digests.
        /// </summary>
        [Serializable]
        private class Sha1Format : OldFormat
        {
            public override string Prefix { get { return "sha1"; } }

            protected override HashAlgorithm GetHashAlgorithm()
            {
                return SHA1.Create();
            }
        }
        #endregion

        #region Inner classes: New Format
        /// <summary>
        /// An abstract base class for <see cref="ManifestFormat"/>s using the new manifest format.
        /// </summary>
        [Serializable]
        private abstract class NewFormat : ManifestFormat
        {
            public override string GenerateEntryForNode(ManifestNode node)
            {
                #region Sanity checks
                if (node == null) throw new ArgumentNullException("node");
                #endregion

                return node.ToString();
            }

            internal override ManifestDirectory ReadDirectoryNodeFromEntry(string entry)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(entry)) throw new ArgumentNullException("entry");
                #endregion

                return ManifestDirectory.FromString(entry);
            }

            public override FileSystemInfo[] GetSortedDirectoryEntries(string path)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
                #endregion

                // Get separated lists for files and directories
                var files = Directory.GetFiles(path);
                var directories = Directory.GetDirectories(path);

                // C-sort the lists
                Array.Sort(files, StringComparer.Ordinal);
                Array.Sort(directories, StringComparer.Ordinal);

                // Create the combined result list (files first, then sub-diretories)
                var result = new List<FileSystemInfo>(files.Select(file => new FileInfo(file)).Cast<FileSystemInfo>());
                foreach (string directory in directories)
                {
                    result.Add(new DirectoryInfo(directory));

                    // Recurse into sub-direcories (but do not follow symlinks)
                    if (!FileUtils.IsSymlink(directory)) result.AddRange(GetSortedDirectoryEntries(directory));
                }
                return result.ToArray();
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA1"/> digests.
        /// </summary>
        [Serializable]
        private class Sha1NewFormat : NewFormat
        {
            public override string Prefix { get { return "sha1new"; } }

            protected override HashAlgorithm GetHashAlgorithm()
            {
                return SHA1.Create();
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA256"/> digests.
        /// </summary>
        [Serializable]
        private class Sha256Format : NewFormat
        {
            public override string Prefix { get { return "sha256"; } }

            protected override HashAlgorithm GetHashAlgorithm()
            {
                return SHA256.Create();
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA256"/> digests.
        /// </summary>
        [Serializable]
        private class Sha256NewFormat : NewFormat
        {
            public override string Prefix { get { return "sha256new"; } }

            public override string Separator { get { return "_"; } }

            protected override HashAlgorithm GetHashAlgorithm()
            {
                return SHA256.Create();
            }

            protected override string SerializeManifestDigest(byte[] hash)
            {
                return hash.Base32Encode();
            }
        }
        #endregion
    }
}
