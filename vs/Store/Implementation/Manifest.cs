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
        private readonly ReadOnlyCollection<ManifestNode> _nodes;
        /// <summary>
        /// A list of all elements in the tree this manifest represents.
        /// </summary>
        public IList<ManifestNode> Nodes { get { return _nodes; } }

        /// <summary>
        /// The format used for <see cref="Manifest.Save"/>, also specifies the algorithm used in <see cref="ManifestFileBase.Hash"/>.
        /// </summary>
        public ManifestFormat Format { get; private set; }
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
        /// <param name="startPath">The top-level directory of the <see cref="Model.Implementation"/>.</param>
        /// <param name="path">The path of the directory to analyze.</param>
        private static void AddToList(IList<ManifestNode> nodes, HashAlgorithm algorithm, string startPath, string path)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            if (string.IsNullOrEmpty(startPath)) throw new ArgumentNullException("startPath");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            foreach (var file in Directory.GetFiles(path))
            {
                var fileName = Path.GetFileName(file);

                // Don't include top-level manifest management files in manifest
                if (startPath == path && (fileName == ".manifest" || fileName == ".xbit")) continue;

                // ToDo: Handle .xbit
                // ToDo: Handle symlinks

                var fileInfo = new FileInfo(file);
                nodes.Add(new ManifestFile(
                    FileHelper.ComputeHash(file, algorithm),
                    FileHelper.UnixTime(fileInfo.LastWriteTimeUtc),
                    fileInfo.Length,
                    fileName));
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                var fileInfo = new DirectoryInfo(directory);
                nodes.Add(new ManifestDirectory(
                    FileHelper.UnixTime(fileInfo.LastWriteTimeUtc),
                    // Remove leading portion of path and use Unix slashes
                    directory.Substring(startPath.Length).Replace('\\', '/')));

                // Recurse into sub-directories
                AddToList(nodes, algorithm, startPath, directory);
            }
        }
        #endregion

        /// <summary>
        /// Generates a manifest using the old format for a directory in the file system.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest.</param>
        /// <returns>A manifest for the directory.</returns>
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        public static Manifest Generate(string path, ManifestFormat format)
        {
            var nodes = new List<ManifestNode>();
            AddToList(nodes, format.HashingMethod, path, path);
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
            using (var writer = new StreamWriter(path))
            {
                foreach (ManifestNode node in Nodes)
                    writer.WriteLine(Format.GenerateEntryForNode(node));
            }
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

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
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
        #endregion
    }
}
