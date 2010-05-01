using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Common.Helpers;
using ZeroInstall.Store.Properties;
using IO=System.IO;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// The manifest lists every file, directory and symlink in the tree, and contains the digest of each file's content.
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
        /// Loads a manifest file using the new format into memory.
        /// </summary>
        /// <param name="path">The path of the file to load.</param>
        /// <returns>A <see cref="Manifest"/> object containing the parsed content of the file.</returns>
        /// <exception cref="IO.IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="ArgumentException">Thrown if the file specified is not a valid manifest file.</exception>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public static Manifest Load(string path)
        {
            var nodes = new List<ManifestNode>();
            
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("F")) nodes.Add(File.FromString(line));
                    else if (line.StartsWith("X")) nodes.Add(ExecutableFile.FromString(line));
                    else if (line.StartsWith("S")) nodes.Add(Symlink.FromString(line));
                    else if (line.StartsWith("D")) nodes.Add(Directory.FromString(line));
                    else throw new ArgumentException(Resources.InvalidLinesInManifest, "path");
                }
            }

            return new Manifest(nodes);
        }

        /// <summary>
        /// Stores a manifest in a file using the new format.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public void Save(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (ManifestNode node in _nodes)
                    writer.WriteLine(node.ToString());
            }
        }

        /// <summary>
        /// Stores a manifest in a file using the old format.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <remarks>
        /// The exact format is specified here: http://0install.net/manifest-spec.html
        /// </remarks>
        public void SaveOld(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                // ToDo: Perform lexicografic sort
                foreach (ManifestNode node in _nodes)
                    writer.WriteLine(node.ToStringOld());
            }
        }
        #endregion

        #region Generate
        /// <summary>
        /// Recursively adds <see cref="ManifestNode"/>s representing objects in a directory in the file system to a list.
        /// </summary>
        /// <param name="nodes">The list to add new <see cref="ManifestNode"/>s to.</param>
        /// <param name="startPath">The top-level directory of the <see cref="Model.Implementation"/>.</param>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        private static void AddToList(IList<ManifestNode> nodes, string startPath, string path, HashAlgorithm algorithm)
        {
            foreach (var file in IO.Directory.GetFiles(path))
            {
                var fileName = Path.GetFileName(file);

                // Don't include top-level manifest management files in manifest
                if (startPath == path && (fileName == ".manifest" || fileName == ".xbit")) continue;

                // ToDo: Handle .xbit
                // ToDo: Handle symlinks

                var fileInfo = new FileInfo(file);
                nodes.Add(new File(
                    FileHelper.ComputeHash(file, algorithm),
                    FileHelper.UnixTime(fileInfo.LastWriteTimeUtc),
                    fileInfo.Length,
                    fileName));
            }

            foreach (var directory in IO.Directory.GetDirectories(path))
            {
                var fileInfo = new DirectoryInfo(directory);
                nodes.Add(new Directory(
                    FileHelper.UnixTime(fileInfo.LastWriteTimeUtc),
                    // Remove leading portion of path and use Unix slashes
                    directory.Substring(startPath.Length).Replace('\\', '/')));

                // Recurse into sub-directories
                AddToList(nodes, startPath, directory, algorithm);
            }
        }

        /// <summary>
        /// Generates a manifest for a directory in the file system.
        /// </summary>
        /// <param name="path">The path of the directory to analyze.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>A <see cref="Manifest"/> for the directory.</returns>
        /// <exception cref="IO.IOException">Thrown if the directory could not be processed.</exception>
        public static Manifest Generate(string path, HashAlgorithm algorithm)
        {
            var nodes = new List<ManifestNode>();

            AddToList(nodes, path, path, algorithm);

            return new Manifest(nodes);
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
            return obj.GetType() == typeof(Manifest) && Equals((Manifest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397;
                foreach (ManifestNode node in _nodes)
                    result = (result * 397) ^ node.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
