using System;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// A immutable directory-entry in a <see cref="Manifest"/>.
    /// </summary>
    public sealed class Directory : ManifestNode, IEquatable<Directory>
    {
        #region Constants
        /// <summary>
        /// The character at the beginning of a line that identifies this type of node.
        /// </summary>
        public const char NodeChar = 'D';
        #endregion

        #region Properties
        /// <summary>
        /// The time this directory was last modified in the number of seconds since the epoch.
        /// </summary>
        /// <remarks>Obsolete, retained for compatibility with older implementations.</remarks>
        public long ModifiedTime { get; private set; }

        /// <summary>
        /// The complete path of this directory relative to the tree root as a Unix-Path beginning with a slash.
        /// </summary>
        public string FullPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new direcory-entry.
        /// </summary>
        /// <param name="modifiedTime">The time this directory was last modified in the number of seconds since the epoch.</param>
        /// <param name="fullPath">The complete path of this directory relative to the tree root as a Unix-Path beginning with a slash.</param>
        public Directory(long modifiedTime, string fullPath)
        {
            ModifiedTime = modifiedTime;
            FullPath = fullPath;
        }
        #endregion

        //--------------------//

        #region Compare
        public bool Equals(Directory other)
        {
            if (other == null) return false;
            if (other == this) return true;
            return other.ModifiedTime == ModifiedTime && Equals(other.FullPath, FullPath);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(Directory) && Equals((Directory)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ModifiedTime.GetHashCode() * 397) ^ (FullPath != null ? FullPath.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Directory left, Directory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Directory left, Directory right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
