/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Common;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// A manifest lists every file, directory and symlink in the tree and contains a hash of each file's content.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    public sealed class Manifest : IEquatable<Manifest>
    {
        #region Properties
        /// <summary>
        /// The format of the manifest (which file details are listed, which hash method is used, etc.).
        /// </summary>
        public ManifestFormat Format { get; private set; }

        private readonly C5.IList<ManifestNode> _nodes;
        /// <summary>
        /// A list of all elements in the tree this manifest represents.
        /// </summary>
// ReSharper disable ReturnTypeCanBeEnumerable.Global
        public IList<ManifestNode> Nodes { get { return _nodes; } }
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
                {
                    _totalSize = 0;
                    foreach (var node in _nodes)
                    {
                        var fileNode = node as ManifestFileBase;
                        if (fileNode != null) _totalSize += fileNode.Size;
                    }
                }

                return _totalSize;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new manifest.
        /// </summary>
        /// <param name="nodes">A list of all elements in the tree this manifest represents.</param>
        /// <param name="format">The format used for <see cref="Save(Stream)"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Hash"/>.</param>
        internal Manifest(C5.IList<ManifestNode> nodes, ManifestFormat format)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            Format = format;

            // Make the collection immutable
            _nodes = new C5.GuardedList<ManifestNode>(nodes);
        }
        #endregion

        //--------------------//
        
        #region Storage
        /// <summary>
        /// Writes the manifest to a file and calculates its hash.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <returns>The manifest digest (format=hash).</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public string Save(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = File.Create(path))
                Save(stream);

            // Caclulate the hash of the completed manifest file
            return Format.Prefix + FileUtils.ComputeHash(path, Format.HashAlgorithm);
        }

        /// <summary>
        /// Writes the manifest to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The manifest digest (format=hash).</returns>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public void Save(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            // Use UTF-8 without BOM and Unix-stlye line breaks to ensure correct hash values
            var writer = new StreamWriter(stream, new UTF8Encoding(false)) {NewLine = "\n"};

            // Write one line for each node
            foreach (ManifestNode node in Nodes)
                writer.WriteLine(Format.GenerateEntryForNode(node));

            writer.Flush();
        }

        /// <summary>
        /// Parses a manifest file stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <param name="format">The format of the file and the format of the created <see cref="Manifest"/>. Comprises the hash algorithm used and the file's format.</param>
        /// <returns>A set of <see cref="ManifestNode"/>s containing the parsed content of the file.</returns>
        /// <exception cref="FormatException">Thrown if the file specified is not a valid manifest file.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static Manifest Load(Stream stream, ManifestFormat format)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            var nodes = new C5.ArrayList<ManifestNode>();

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

            return new Manifest(nodes, format);
        }

        /// <summary>
        /// Parses a manifest file.
        /// </summary>
        /// <param name="path">The path of the file to load.</param>
        /// <param name="format">The format of the file and the format of the created <see cref="Manifest"/>. Comprises the hash algorithm used and the file's format.</param>
        /// <returns>A set of <see cref="ManifestNode"/>s containing the parsed content of the file.</returns>
        /// <exception cref="FormatException">Thrown if the file specified is not a valid manifest file.</exception>
        /// <exception cref="IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static Manifest Load(string path, ManifestFormat format)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            using (var stream = File.OpenRead(path))
                return Load(stream, format);
        }
        #endregion

        #region Comfort methods
        /// <summary>
        /// Generates a manifest for a directory in the filesystem.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest (which file details are listed, which hash method is used, etc.).</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>A manifest for the directory.</returns>
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        public static Manifest Generate(string path, ManifestFormat format, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (format == null) throw new ArgumentNullException("format");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var generator = new ManifestGenerator(path, format);

            // Defer task to handler
            handler.RunTask(generator);

            return generator.Result;
        }

        /// <summary>
        /// Generates a manifest for a directory in the filesystem and writes the manifest to a file named ".manifest" in that directory.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest (which file details are listed, which hash method is used, etc.).</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>The manifest digest (format=hash).</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static string CreateDotFile(string path, ManifestFormat format, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            return Generate(path, format, handler).Save(Path.Combine(path, ".manifest"));
        }

        /// <summary>
        /// Generates a <see cref="ManifestDigest"/> object for a directory containing digests for all <see cref="ManifestFormat.Recommended"/>.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>The combined manifest digest structure.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        public static ManifestDigest CreateDigest(string path, ITaskHandler handler)
        {
            var digest = new ManifestDigest();

            // Generate manifest for each available format...
            foreach (var format in ManifestFormat.Recommended)
                // ... and add the resulting digest to the return value
                ManifestDigest.ParseID(Generate(path, format, handler).CalculateDigest(), ref digest);

            return digest;
        }

        /// <summary>
        /// Calculates the digest for the manifest in-memory.
        /// </summary>
        /// <returns>The manifest digest (format=hash).</returns>
        public string CalculateDigest()
        {
            using (var stream = new MemoryStream())
            {
                Save(stream);

                stream.Position = 0;
                return Format.Prefix + FileUtils.ComputeHash(stream, Format.HashAlgorithm);
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the manifest in the same text representation format used by <see cref="Save(System.IO.Stream)"/>.
        /// </summary>
        public override string ToString()
        {
            // Use the same format as the file
            var output = new StringBuilder();
            foreach (ManifestNode node in _nodes)
                output.Append(Format.GenerateEntryForNode(node) + "\n");
            return output.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Manifest other)
        {
            if (other == null) return false;

            if (_nodes.Count != other._nodes.Count) return false;
            for (int i = 0; i < _nodes.Count; i++)
            {
                // If any node pair does not match, the manifests are not equal
                if (!Equals(_nodes[i], other._nodes[i])) return false;
            }

            // If the for-loop ran through, all node pairs are identical and the manifests are equal
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Manifest) && Equals(obj as Manifest);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Format != null ? Format.GetHashCode() : 0);
                result = (result * 397) ^ _nodes.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
