using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ZeroInstall.Store
{
    /// <summary>
    /// The manifest lists every file, directory and symlink in the tree, and gives the digest of each file's content.
    /// This class is immutable.
    /// </summary>
    public sealed class Manifest : IEquatable<Manifest>
    {
        #region Properties
        private readonly ReadOnlyCollection<ManifestNode> _nodes;
        /// <summary>
        /// A list of all elements in the tree this manifest represents.
        /// </summary>
        /// <remarks></remarks>
        public IList<ManifestNode> Nodes { get { return _nodes; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new manifest
        /// </summary>
        /// <param name="nodes">A list of all elements in the tree this manifest represents.</param>
        private Manifest(IList<ManifestNode> nodes)
        {
            // Make the collection immutable
            _nodes = new ReadOnlyCollection<ManifestNode>(nodes);
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a manifest file into memory.
        /// </summary>
        /// <param name="path">The path of the file to load.</param>
        /// <returns>A <see cref="Manifest"/> object containing the parsed content of the file.</returns>
        /// <exception cref="IOException">Thrown if the file specified is not a vaild manifest file.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static Manifest Load(string path)
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores a manifest in a file.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public void Save(string path)
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion

        #region Generate
        /// <summary>
        /// Generates a manifest for a directory in the file system.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <returns>A <see cref="Manifest"/> for the directory.</returns>
        /// <exception cref="IOException">Thrown if the directory could not be processed.</exception>
        public static Manifest Generate(string path)
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }
        #endregion

        //--------------------//

        #region Compare
        public bool Equals(Manifest other)
        {
            if (other == null) return false;
            if (other == this) return true;

            if (other._nodes.Count != _nodes.Count) return false;
            for (int i = 0; i < _nodes.Count; i++)
            {
                // If any node pair does not match, the manifests are not equal
                if (!Equals(other._nodes[i], _nodes[i])) return false;
            }

            // If the for-loop ran through, all node pairs are identical and the manifests are equal
            return false;
        }
        #endregion
    }
}
