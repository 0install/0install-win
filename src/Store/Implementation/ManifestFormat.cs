﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
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
        public static ManifestFormat Sha256New { get { return _sha256New; } }

        /// <summary>
        /// All currently supported <see cref="ManifestFormat"/>s listed from best (safest) to worst.
        /// </summary>
        public static readonly ManifestFormat[] All = new[] {_sha256New, _sha256, _sha1New, _sha1};

        /// <summary>
        /// All currently supported and non-deprecated <see cref="ManifestFormat"/>s listed from best (safest) to worst.
        /// </summary>
        public static readonly ManifestFormat[] Recommended = new[] {_sha256New, _sha256, _sha1New};
        #endregion

        #region Factory methods
        /// <summary>
        /// Selects the correct <see cref="ManifestFormat"/> based on the digest prefix.
        /// </summary>
        /// <param name="id">The digest id to extract the prefix from or only the prefix.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is no known algorithm prefix.</exception>
        public static ManifestFormat FromPrefix(string id)
        {
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
        public abstract string Prefix { get; }

        /// <summary>
        /// The separator placed between the <see cref="Prefix"/> and the actual digest.
        /// </summary>
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
        public abstract string GenerateEntryForNode(ManifestNode node);

        /// <summary>
        /// Parses a file entry (line) back into a <see cref="ManifestDirectory"/>.
        /// </summary>
        internal abstract ManifestDirectory ReadDirectoryNodeFromEntry(string entry);

        /// <summary>
        /// Creates a recursive list of all filesystem entries in a certain directory sorted according to the format specifications.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <returns>An array of filesystem entries.</returns>
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the directory is not permitted.</exception>
        public abstract FileSystemInfo[] GetSortedDirectoryEntries(string path);
        #endregion

        #region Digest methods
        public string DigestManifest(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return SerializeManifestDigest(GetHashAlgorithm().ComputeHash(stream));
        }

        public string DigestContent(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return SerializeContentDigest(GetHashAlgorithm().ComputeHash(stream));
        }

        public string DigestContent(byte[] data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            return SerializeContentDigest(GetHashAlgorithm().ComputeHash(data));
        }

        /// <summary>
        /// Retreives a new instance of the hashing algorithm used for generating digests.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Generates a new instance each time to allow for concurrent usage")]
        protected abstract HashAlgorithm GetHashAlgorithm();

        protected virtual string SerializeContentDigest(byte[] hash)
        {
            return StringUtils.Base16Encode(hash);
        }

        protected virtual string SerializeManifestDigest(byte[] hash)
        {
            return StringUtils.Base16Encode(hash);
        }
        #endregion

        //--------------------//

        #region Inner classes: Old Format
        /// <summary>
        /// An abstract base class for <see cref="ManifestFormat"/>s using the old manifest format.
        /// </summary>
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
                var result = new C5.LinkedList<FileSystemInfo>();
                foreach (string entry in entries)
                {
                    if (Directory.Exists(entry))
                    {
                        // Recurse into sub-direcories
                        result.Add(new DirectoryInfo(entry));
                        result.AddAll(GetSortedDirectoryEntries(entry));
                    }
                        // Simply list files
                    else result.Add(new FileInfo(entry));
                }
                return result.ToArray();
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the old manifest format and <see cref="SHA1"/> digests.
        /// </summary>
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
                var result = new C5.LinkedList<FileSystemInfo>();
                foreach (string file in files)
                {
                    // Simply list files
                    result.Add(new FileInfo(file));
                }
                foreach (string directory in directories)
                {
                    // Recurse into sub-direcories
                    result.Add(new DirectoryInfo(directory));
                    result.AddAll(GetSortedDirectoryEntries(directory));
                }
                return result.ToArray();
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA1"/> digests.
        /// </summary>
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
                return StringUtils.Base32Encode(hash);
            }
        }
        #endregion
    }
}
