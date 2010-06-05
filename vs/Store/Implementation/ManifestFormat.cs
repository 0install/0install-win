using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Abstract class to encapsulate the differences between the different formats that can be used to save and load <see cref="Manifest"/>s.
    /// </summary>
    /// <remarks>
    /// Comprises: The hashing method used and the format specification used to serialize and deserialize manifests.
    /// </remarks>
    public abstract class ManifestFormat
    {
        #region Singleton properties
        private static readonly ManifestFormat _sha1Old = new Sha1OldFormat();
        /// <summary>
        /// The <see cref="ManifestFormat"/> to use for <see cref="ManifestDigest.Sha1Old"/>.
        /// </summary>
        public static ManifestFormat Sha1Old { get { return _sha1Old; } }

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

        /// <summary>
        /// All currently supported <see cref="ManifestFormat"/>s listed from best to worst.
        /// </summary>
        public static readonly IEnumerable<ManifestFormat> All = new[] { _sha256, _sha1New, _sha1Old };
        #endregion

        #region Factory methods
        /// <summary>
        /// Selects the correct <see cref="ManifestFormat"/> based on the digest prefix.
        /// </summary>
        public static ManifestFormat FromPrefix(string prefix)
        {
            switch (prefix)
            {
                case ManifestDigest.Sha1OldPrefix: return Sha1Old;
                case ManifestDigest.Sha1NewPrefix: return Sha1New;
                case ManifestDigest.Sha256Prefix: return Sha256;
                default: throw new ArgumentException(Resources.NoKnownDigestMethod, "prefix");
            }
        }
        #endregion

        //--------------------//

        #region Abstract properties
        /// <summary>
        /// The hashing algorithm used for <see cref="ManifestFileBase.Hash"/> and the <see cref="Manifest"/> hash itself.
        /// </summary>
        public abstract HashAlgorithm HashAlgorithm { get; }

        /// <summary>
        /// The prefix used to identify the format (e.g. "sha256").
        /// </summary>
        public abstract string Prefix { get; }
        #endregion

        #region Abstract methods
        /// <summary>
        /// Generates a file entry (line) for a specific <see cref="ManifestNode"/>.
        /// </summary>
        public abstract string GenerateEntryForNode(ManifestNode node);

        /// <summary>
        /// Parses a file entry (line) back into a <see cref="ManifestNode"/>.
        /// </summary>
        internal abstract ManifestDirectory ReadDirectoryNodeFromEntry(string entry);

        /// <summary>
        /// Creates a recursive list of all file system entries in a certain directory sorted according to the format specifications.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <returns>A list of file system entries.</returns>
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the directory is not permitted.</exception>
        public abstract IEnumerable<FileSystemInfo> GetSortedDirectoryEntries(string path);
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
                return node.ToStringOld();
            }

            internal override ManifestDirectory ReadDirectoryNodeFromEntry(string entry)
            {
                return ManifestDirectory.FromStringOld(entry);
            }

            public override IEnumerable<FileSystemInfo> GetSortedDirectoryEntries(string path)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
                #endregion

                // Get a combined list of files and directories
                var entries = Directory.GetFileSystemEntries(path);

                // Sort the list lexicographically
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
                return result;
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the old manifest format and <see cref="SHA1"/> hashes.
        /// </summary>
        private class Sha1OldFormat : OldFormat
        {
            private static readonly HashAlgorithm _algorithm = SHA1.Create();
            public override HashAlgorithm HashAlgorithm { get { return _algorithm; } }

            public override string Prefix { get { return "sha1="; } }
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
                return node.ToString();
            }

            internal override ManifestDirectory ReadDirectoryNodeFromEntry(string entry)
            {
                return ManifestDirectory.FromString(entry);
            }

            public override IEnumerable<FileSystemInfo> GetSortedDirectoryEntries(string path)
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
                #endregion

                // Get separated lists for files and directories
                var files = Directory.GetFiles(path);
                var directories = Directory.GetDirectories(path);

                // Sort the lists lexicographically
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
                return result;
            }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA1"/> hashes.
        /// </summary>
        private class Sha1NewFormat : NewFormat
        {
            private static readonly HashAlgorithm _algorithm = SHA1.Create();
            public override HashAlgorithm HashAlgorithm { get { return _algorithm; } }

            public override string Prefix { get { return "sha1new="; } }
        }

        /// <summary>
        /// A specific <see cref="ManifestFormat"/>s using the new manifest format and <see cref="SHA256"/> hashes.
        /// </summary>
        private class Sha256Format : NewFormat
        {
            private static readonly HashAlgorithm _algorithm = SHA256.Create();
            public override HashAlgorithm HashAlgorithm { get { return _algorithm; } }

            public override string Prefix { get { return "sha256="; } }
        }
        #endregion
    }
}
