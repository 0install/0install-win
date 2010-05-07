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

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// A manifest lists every file, directory and symlink in the tree and contains a hash of each file's content.
    /// </summary>
    /// <remarks>This class and the derived classes are immutable.</remarks>
    public abstract class Manifest : IEquatable<Manifest>
    {
        #region Properties
        private readonly ReadOnlyCollection<ManifestNode> _nodes;
        /// <summary>
        /// A list of all elements in the tree this manifest represents.
        /// </summary>
        public IList<ManifestNode> Nodes { get { return _nodes; } }

        /// <summary>
        /// The hash algorithm used for <see cref="ManifestFileBase.Hash"/> and <see cref="Save"/>.
        /// </summary>
        public HashAlgorithm HashAlgorithm { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new manifest.
        /// </summary>
        /// <param name="nodes">A list of all elements in the tree this manifest represents.</param>
        /// <param name="hashAlgorithm">The hash algorithm used for <see cref="ManifestFileBase.Hash"/> and <see cref="Save"/>.</param>
        protected Manifest(IList<ManifestNode> nodes, HashAlgorithm hashAlgorithm)
        {
            #region Sanity checks
            if (nodes == null) throw new ArgumentNullException("nodes");
            if (hashAlgorithm == null) throw new ArgumentNullException("hashAlgorithm");
            #endregion

            HashAlgorithm = hashAlgorithm;

            // Make the collection immutable
            _nodes = new ReadOnlyCollection<ManifestNode>(nodes);
        }
        #endregion

        //--------------------//

        #region Generate
        /// <summary>
        /// Recursively adds <see cref="ManifestNode"/>s representing objects in a directory in the file system to a list.
        /// </summary>
        /// <param name="nodes">The list to add new <see cref="ManifestNode"/>s to.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <param name="startPath">The top-level directory of the <see cref="Model.Implementation"/>.</param>
        /// <param name="path">The path of the directory to analyze.</param>
        protected static void AddToList(IList<ManifestNode> nodes, HashAlgorithm algorithm, string startPath, string path)
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

        #region Storage
        /// <summary>
        /// Writes the manifest to a file and calculates its hash.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <returns>The hash value of the file.</returns>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public abstract string Save(string path);
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

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397 ^ HashAlgorithm.GetHashCode();
                foreach (ManifestNode node in _nodes)
                    result = (result * 397) ^ node.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
