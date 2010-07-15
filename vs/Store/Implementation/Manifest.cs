/*
 * Copyright 2010 Bastian Eicher
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
using System.Security.Cryptography;
using System.Text;
using Common.Helpers;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// A manifest lists every file, directory and symlink in the tree and contains a hash of each file's content.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public sealed class Manifest : IEquatable<Manifest>
    {
        #region Properties
        /// <summary>
        /// The format used for <see cref="Save(Stream)"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Hash"/>.
        /// </summary>
        public ManifestFormat Format { get; private set; }

        private readonly C5.IList<ManifestNode> _nodes;
        /// <summary>
        /// A list of all elements in the tree this manifest represents.
        /// </summary>
        public IList<ManifestNode> Nodes { get { return _nodes; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new manifest.
        /// </summary>
        /// <param name="nodes">A list of all elements in the tree this manifest represents.</param>
        /// <param name="format">The format used for <see cref="Save(Stream)"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Hash"/>.</param>
        private Manifest(C5.IList<ManifestNode> nodes, ManifestFormat format)
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
        
        #region Factory methods
        /// <summary>
        /// Generates a manifest for a directory in the filesystem.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest.</param>
        /// <param name="manifestProgress">Callback to track the progress of generating the manifest (hashing files); may be <see langword="null"/>.</param>
        /// <returns>A manifest for the directory.</returns>
        /// <exception cref="ArgumentException">Thrown if one of the filenames of the files in <paramref name="path"/> contains a newline character.</exception> 
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the directory is not permitted.</exception>
        public static Manifest Generate(string path, ManifestFormat format, ProgressCallback manifestProgress)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            var externalXBits = GetExternalXBits(path);

            // Get the complete (recursive) content of the directory sorted according to the format specification
            var entries = format.GetSortedDirectoryEntries(path);
            
            // Iterate through the directory listing to build a list of manifets entries
            var nodes = new C5.ArrayList<ManifestNode>(entries.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                var file = entries[i] as FileInfo;
                if (file != null)
                {
                    // Don't include manifest management files in manifest
                    if (file.Name == ".manifest" || file.Name == ".xbit") continue;

                    nodes.Add(GetFileNode(file, format.HashAlgorithm, externalXBits));
                }
                else
                {
                    var directory = entries[i] as DirectoryInfo;
                    if (directory != null) nodes.Add(GetDirectoryNode(directory, path));
                }

                // Report back the progess
                if (manifestProgress != null) manifestProgress(i + 1 / (float)entries.Length, entries[i].Name);
            }

            return new Manifest(nodes, format);
        }

        /// <summary>
        /// Executable bits must be stored in an external file (named <code>.xbit</code>) on some platforms (e.g. Windows) because the filesystem attributes can't.
        /// </summary>
        /// <param name="path">The path of the directory containing a <code>.xbit</code> file.</param>
        /// <returns>A list of fully qualified paths of files that are named in the <code>.xbit</code> file.</returns>
        private static ICollection<string> GetExternalXBits(string path)
        {
            var externalXBits = new C5.HashSet<string>();
            string xBitPath = Path.Combine(path, ".xbit");
            if (File.Exists(xBitPath))
            {
                using (StreamReader xbitFile = File.OpenText(xBitPath))
                {
                    // Each line in the file signals an executable file
                    while (!xbitFile.EndOfStream)
                    {
                        string currentLine = xbitFile.ReadLine();
                        if (currentLine.StartsWith("/"))
                        {
                            // Trim away the first slash and then replace Unix-style slashes
                            string relativePath = StringHelper.UnifySlashes(currentLine.Substring(1));
                            externalXBits.Add(Path.Combine(path, relativePath));
                        }
                    }
                }
            }
            return externalXBits;
        }

        /// <summary>
        /// Creates a manifest node for a file.
        /// </summary>
        /// <param name="file">The file object to create a node for.</param>
        /// <param name="hashAlgorithm">The algorithm to use to calculate the hash of the file's content.</param>
        /// <param name="externalXBits">A list of fully qualified paths of files that are named in the <code>.xbit</code> file.</param>
        /// <returns>The node for the list.</returns>
        private static ManifestFileBase GetFileNode(FileInfo file, HashAlgorithm hashAlgorithm, ICollection<string> externalXBits)
        {
            // ToDo: Handle symlinks

            // ToDo: Handle executable bits in filesystem itself
            if (externalXBits.Contains(file.FullName))
                return new ManifestExecutableFile(FileHelper.ComputeHash(file.FullName, hashAlgorithm), FileHelper.UnixTime(file.LastWriteTimeUtc), file.Length, file.Name);

            return new ManifestFile(FileHelper.ComputeHash(file.FullName, hashAlgorithm), FileHelper.UnixTime(file.LastWriteTimeUtc), file.Length, file.Name);
        }

        /// <summary>
        /// Creates a manifest node for a directory.
        /// </summary>
        /// <param name="directory">The directory object to create a node for.</param>
        /// <param name="rootPath">The fully qualified path of the root directory the manifest is being generated for.</param>
        /// <returns>Thenode for the list.</returns>
        private static ManifestDirectory GetDirectoryNode(DirectoryInfo directory, string rootPath)
        {
            return new ManifestDirectory(
                FileHelper.UnixTime(directory.LastWriteTime),
                // Remove leading portion of path and use Unix slashes
                directory.FullName.Substring(rootPath.Length).Replace(Path.DirectorySeparatorChar, '/'));
        }
        #endregion

        //--------------------//
        
        #region Storage
        /// <summary>
        /// Writes the manifest to a file and calculates its hash.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <returns>The manifest digest (format=hash).</returns>
        /// <exception cref="IOException">Thrown if the file couldn't be created.</exception>
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
            return Format.Prefix + FileHelper.ComputeHash(path, Format.HashAlgorithm);
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

            // Default encoding (UTF8 without BOM) and Unix-stlye linebreaks to ensure correct hash values
            var writer = new StreamWriter(stream) {NewLine = "\n"};

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
                string line = reader.ReadLine();
                if (line.StartsWith("F")) nodes.Add(ManifestFile.FromString(line));
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
        /// Generates a manifest for a directory in the filesystem and writes the manifest to a file named ".manifest" in that directory.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest.</param>
        /// <param name="manifestProgress">Callback to track the progress of generating the manifest (hashing files); may be <see langword="null"/>.</param>
        /// <returns>The manifest digest (format=hash).</returns>
        /// <exception cref="IOException">Thrown if the file couldn't be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static string CreateDotFile(string path, ManifestFormat format, ProgressCallback manifestProgress)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            return Generate(path, format, manifestProgress).Save(Path.Combine(path, ".manifest"));
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
                return Format.Prefix + FileHelper.ComputeHash(stream, Format.HashAlgorithm);
            }
        }

        /// <summary>
        /// Generates a <see cref="ManifestDigest"/> object for a directory containing digests for all available <see cref="ManifestFormat"/>s.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="manifestProgress">Callback to track the progress of generating the manifest (hashing files); may be <see langword="null"/>.</param>
        /// <returns>The combined manifest digest structure.</returns>
        /// <exception cref="IOException">Thrown if the file couldn't be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public static ManifestDigest CreateDigest(string path, ProgressCallback manifestProgress)
        {
            var digest = new ManifestDigest();

            // Generate manifest for each available format...
            foreach (var format in ManifestFormat.All)
                // ... and add the resulting digest to the return value
                ManifestDigest.ParseID(Generate(path, format, manifestProgress).CalculateDigest(), ref digest);

            return digest;
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            // Use the same format as the file
            var output = new StringBuilder();
            foreach (ManifestNode node in _nodes)
                output.AppendLine(node.ToString());
            return output.ToString();
        }
        #endregion

        #region Equality
        public bool Equals(Manifest other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (_nodes.Count != other._nodes.Count) return false;
            for (int i = 0; i < _nodes.Count; i++)
            {
                // If any node pair does not match, the manifests are not equal
                if (!Equals(_nodes[i], other._nodes[i])) return false;
            }

            // If the for-loop ran through, all node pairs are identical and the manifests are equal
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Manifest) && Equals(obj as Manifest);
        }

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
