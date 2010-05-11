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
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Common.Helpers;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// A manifest lists every file, directory and symlink in the tree and contains a hash of each file's content.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    public sealed class Manifest : IEquatable<Manifest>
    {
        #region Properties
        /// <summary>
        /// The format used for <see cref="Manifest.Save"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Hash"/>.
        /// </summary>
        public ManifestFormat Format { get; private set; }

        private readonly ReadOnlyCollection<ManifestNode> _nodes;
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
        /// <param name="format">The format used for <see cref="Manifest.Save"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Hash"/>.</param>
        private Manifest(IList<ManifestNode> nodes, ManifestFormat format)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            Format = format;

            // Make the collection immutable
            _nodes = new ReadOnlyCollection<ManifestNode>(nodes);
        }
        #endregion
        
        #region Factory methods

        #region Helper
        /// <summary>
        /// Recursively adds <see cref="ManifestNode"/>s representing objects in a directory in the file system to a list.
        /// </summary>
        /// <param name="nodes">The list to add new <see cref="ManifestNode"/>s to.</param>
        /// <param name="algorithm">The hashing algorithm to use for <see cref="ManifestFileBase.Hash"/>.</param>
        /// <param name="externalXbits">A list of files that are to be treated as if they had the executable flag set.</param>
        /// <param name="startPath">The top-level directory of the <see cref="Model.Implementation"/>.</param>
        /// <param name="path">The path of the directory to analyze.</param>
        private static void AddToList(IList<ManifestNode> nodes, HashAlgorithm algorithm, ICollection<string> externalXbits, string startPath, string path)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            if (externalXbits == null) throw new ArgumentNullException("externalXbits");
            if (string.IsNullOrEmpty(startPath)) throw new ArgumentNullException("startPath");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Find all files (including executable and symlinks)
            foreach (var file in Directory.GetFiles(path))
            {
                var fileName = Path.GetFileName(file);

                // Don't include top-level manifest management files in manifest
                if (startPath == path && (fileName == ".manifest" || fileName == ".xbit")) continue;
                
                // ToDo: Handle symlinks

                // ToDo: Handle executable bits in filesystem itself
                var fileInfo = new FileInfo(file);
                if (externalXbits.Contains(file))
                    nodes.Add(new ManifestExecutableFile(FileHelper.ComputeHash(file, algorithm), FileHelper.UnixTime(fileInfo.LastWriteTimeUtc), fileInfo.Length, fileName));
                else
                    nodes.Add(new ManifestFile(FileHelper.ComputeHash(file, algorithm), FileHelper.UnixTime(fileInfo.LastWriteTimeUtc), fileInfo.Length, fileName));
            }

            // Find all directories
            foreach (var directory in Directory.GetDirectories(path))
            {
                // Get directory information
                var dirInfo = new DirectoryInfo(directory);
                nodes.Add(new ManifestDirectory(
                    FileHelper.UnixTime(dirInfo.LastWriteTimeUtc),
                    // Remove leading portion of path and use Unix slashes
                    directory.Substring(startPath.Length).Replace(Path.DirectorySeparatorChar, '/')));

                // Recurse into sub-directories
                AddToList(nodes, algorithm, externalXbits, startPath, directory);
            }
        }
        #endregion

        /// <summary>
        /// Generates a manifest for a directory in the file system.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest.</param>
        /// <returns>A manifest for the directory.</returns>
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        public static Manifest Generate(string path, ManifestFormat format)
        {
            #region Parse external X-Bits file
            // Executable bits must be stored externally on some platforms (e.g. Windows) because the file system attributes can't
            var externalXbits = new C5.HashSet<string>();
            string xbitPath = Path.Combine(path, ".xbit");
            if (File.Exists(xbitPath))
            {
                // ToDo: Check encoding and linebreak style
                using (StreamReader xbitFile = File.OpenText(xbitPath))
                {
                    // Each line in the file signals an executable file
                    while (!xbitFile.EndOfStream)
                    {
                        string currentLine = xbitFile.ReadLine();
                        if (currentLine.StartsWith("/"))
                        {
                            // Trim away the first slash and then replace Unix-style slashes
                            string relativePath = StringHelper.UnifySlashes(currentLine.Substring(1));
                            externalXbits.Add(Path.Combine(path, relativePath));
                        }
                    }
                }
            }
            #endregion

            var nodes = new List<ManifestNode>();
            AddToList(nodes, format.HashingMethod, externalXbits, path, path);
            return new Manifest(nodes, format);
        }
        #endregion

        //--------------------//
        
        #region Storage
        /// <summary>
        /// Writes the manifest to a file using the and calculates its hash.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <returns>The manifest digest (format=hash).</returns>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public string Save(string path)
        {
            // ToDo: Check encoding and linebreak style
            using (var writer = new StreamWriter(path))
            {
                // Write one line for each node
                foreach (ManifestNode node in Nodes)
                    writer.WriteLine(Format.GenerateEntryForNode(node));
            }

            // Caclulate the hash of the completed manifest file
            return Format.Prefix + FileHelper.ComputeHash(path, Format.HashingMethod);
        }

        /// <summary>
        /// Parses a manifest file.
        /// </summary>
        /// <param name="path">The path of the file to load.</param>
        /// <param name="format">The format of the file and the format of the created <see cref="Manifest"/>. Comprises the hash algorithm used and the file's format.</param>
        /// <returns>A set of <see cref="ManifestNode"/>s containing the parsed content of the file.</returns>
        /// <exception cref="IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="FormatException">Thrown if the file specified is not a valid manifest file.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static Manifest Load(string path, ManifestFormat format)
        {
            var nodes = new List<ManifestNode>();

            // ToDo: Check encoding and linebreak style
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    // Parse each line as a node
                    string line = reader.ReadLine();
                    if (line.StartsWith("F")) nodes.Add(ManifestFile.FromString(line));
                    else if (line.StartsWith("X")) nodes.Add(ManifestExecutableFile.FromString(line));
                    else if (line.StartsWith("S")) nodes.Add(ManifestSymlink.FromString(line));
                    else if (line.StartsWith("D")) nodes.Add(format.ReadDirectoryNodeFromString(line));
                    else throw new FormatException(Resources.InvalidLinesInManifest);
                }
            }

            return new Manifest(nodes, format);
        }
        #endregion

        #region Comfort methods
        /// <summary>
        /// Generates a manifest for a directory in the file system and writes the manifest to a file named ".manifest" in that directory.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest.</param>
        /// <returns>The manifest digest (format=hash).</returns>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static string CreateDotFile(string path, ManifestFormat format)
        {
            return Generate(path, format).Save(Path.Combine(path, ".manifest"));
        }

        /// <summary>
        /// Calculates the hash value for the manifest in-memory.
        /// </summary>
        /// <returns>The manifest digest (format=hash).</returns>
        public string CalculateHash()
        {
            using (var stream = new MemoryStream())
            {
                // ToDo: Check encoding and linebreak style
                var writer = new StreamWriter(stream);
                foreach (ManifestNode node in Nodes)
                    writer.WriteLine(Format.GenerateEntryForNode(node));
                writer.Flush();

                stream.Position = 0;
                return Format.Prefix + FileHelper.ComputeHash(stream, Format.HashingMethod);
            }
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
                foreach (var node in _nodes) result = (result * 397) ^ (node != null ? node.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
