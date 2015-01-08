/*
 * Copyright 2010-2014 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// A manifest lists every file, directory and symlink in the tree and contains a digest of each file's content.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [Serializable]
    public sealed class Manifest : IEquatable<Manifest>, IEnumerable<ManifestNode>
    {
        #region Properties
        /// <summary>
        /// The format of the manifest (which file details are listed, which digest method is used, etc.).
        /// </summary>
        public ManifestFormat Format { get; private set; }

        private readonly ManifestNode[] _nodes;

        // ReSharper restore ReturnTypeCanBeEnumerable.Global

        private long _totalSize = -1;

        /// <summary>
        /// The combined size of all files listed in the manifest in bytes.
        /// </summary>
        public long TotalSize
        {
            get
            {
                // Only calculate the total size if it hasn't been cached yet
                if (_totalSize == -1)
                    _totalSize = _nodes.OfType<ManifestFileBase>().Sum(node => node.Size);

                return _totalSize;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new manifest.
        /// </summary>
        /// <param name="format">The format used for <see cref="Save(Stream)"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Digest"/>.</param>
        /// <param name="nodes">A list of all elements in the tree this manifest represents.</param>
        public Manifest([NotNull] ManifestFormat format, [NotNull, ItemNotNull, InstantHandle] IEnumerable<ManifestNode> nodes)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            Format = format;
            _nodes = nodes.ToArray(); // Make the collection immutable
        }

        /// <summary>
        /// Creates a new manifest.
        /// </summary>
        /// <param name="format">The format used for <see cref="Save(Stream)"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Digest"/>.</param>
        /// <param name="nodes">A list of all elements in the tree this manifest represents.</param>
        public Manifest([NotNull] ManifestFormat format, [NotNull, ItemNotNull, InstantHandle] params ManifestNode[] nodes)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            Format = format;
            _nodes = nodes;
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Writes the manifest to a file and calculates its digest.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <returns>The manifest digest.</returns>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        [NotNull]
        public string Save([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = File.Create(path))
                Save(stream);

            // Caclulate the digest of the completed manifest file
            using (var stream = File.OpenRead(path))
                return Format.Prefix + Format.Separator + Format.DigestManifest(stream);
        }

        /// <summary>
        /// Writes the manifest to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The manifest digest.</returns>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public void Save([NotNull] Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            // Use UTF-8 without BOM and Unix-stlye line breaks to ensure correct digest values
            var writer = new StreamWriter(stream, encoding: FeedUtils.Encoding) {NewLine = "\n"};

            // Write one line for each node
            foreach (ManifestNode node in _nodes)
                writer.WriteLine(Format.GenerateEntryForNode(node));

            writer.Flush();
        }

        /// <summary>
        /// Parses a manifest file stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <param name="format">The format of the file and the format of the created <see cref="Manifest"/>. Comprises the digest method used and the file's format.</param>
        /// <returns>A set of <see cref="ManifestNode"/>s containing the parsed content of the file.</returns>
        /// <exception cref="FormatException">The file specified is not a valid manifest file.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        [NotNull]
        public static Manifest Load([NotNull] Stream stream, [NotNull] ManifestFormat format)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            var nodes = new List<ManifestNode>();

            var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                // Parse each line as a node
                string line = reader.ReadLine() ?? "";
                if (line.StartsWith("F")) nodes.Add(ManifestNormalFile.FromString(line));
                else if (line.StartsWith("X")) nodes.Add(ManifestExecutableFile.FromString(line));
                else if (line.StartsWith("S")) nodes.Add(ManifestSymlink.FromString(line));
                else if (line.StartsWith("D")) nodes.Add(format.ReadDirectoryNodeFromEntry(line));
                else throw new FormatException(Resources.InvalidLinesInManifest);
            }

            return new Manifest(format, nodes);
        }

        /// <summary>
        /// Parses a manifest file.
        /// </summary>
        /// <param name="path">The path of the file to load.</param>
        /// <param name="format">The format of the file and the format of the created <see cref="Manifest"/>. Comprises the digest method used and the file's format.</param>
        /// <returns>A set of <see cref="ManifestNode"/>s containing the parsed content of the file.</returns>
        /// <exception cref="FormatException">The file specified is not a valid manifest file.</exception>
        /// <exception cref="IOException">The manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        [NotNull]
        public static Manifest Load([NotNull] string path, [NotNull] ManifestFormat format)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            using (var stream = File.OpenRead(path))
                return Load(stream, format);
        }

        /// <summary>
        /// Calculates the digest for the manifest in-memory.
        /// </summary>
        /// <returns>The manifest digest.</returns>
        public string CalculateDigest()
        {
            using (var stream = new MemoryStream())
            {
                Save(stream);

                stream.Position = 0;
                return Format.Prefix + Format.Separator + Format.DigestManifest(stream);
            }
        }
        #endregion

        //--------------------//

        #region Enumeration
        IEnumerator<ManifestNode> IEnumerable<ManifestNode>.GetEnumerator()
        {
            return ((IEnumerable<ManifestNode>)_nodes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        /// <summary>
        /// Retreives a specific <see cref="ManifestNode"/>.
        /// </summary>
        /// <param name="i">The index of the node to retreive.</param>
        public ManifestNode this[int i] { get { return _nodes[i]; } }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the manifest in the same text representation format used by <see cref="Save(System.IO.Stream)"/>.
        /// </summary>
        public override string ToString()
        {
            // Use the same format as the file
            var output = new StringBuilder();
            foreach (var node in _nodes)
                output.AppendLine(Format.GenerateEntryForNode(node));
            return output.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Manifest other)
        {
            if (other == null) return false;

            if (_nodes.Length != other._nodes.Length) return false;

            // If any node pair does not match, the manifests are not equal
            for (int i = 0; i < _nodes.Length; i++)
                if (!Equals(_nodes[i], other._nodes[i])) return false;

            // If the for-loop ran through, all node pairs are identical and the manifests are equal
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Manifest && Equals((Manifest)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Format != null ? Format.GetHashCode() : 0);
                foreach (ManifestNode node in _nodes)
                    result = (result * 397) ^ node.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
