using System;
using System.Globalization;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An immutable directory-entry in a <see cref="Manifest"/>.
    /// </summary>
    public sealed class Directory : ManifestNode, IEquatable<Directory>
    {
        #region Properties
        /// <summary>
        /// The time this directory was last modified in the number of seconds since the epoch.
        /// </summary>
        /// <remarks>Outdated, retained for compatibility with old manifest format.</remarks>
        public long ModifiedTime { get; private set; }

        private string _fullPath;
        /// <summary>
        /// The name of the symlink without the containing directory.
        /// </summary>
        public string FullPath
        {
            get { return _fullPath; }
            private set
            {
                if (value.Contains("\n")) throw new ArgumentException(Resources.NewlineInName, "value");
                _fullPath = value;
            }
        }
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

        #region Conversion
        /// <summary>
        /// Returns the string representation of this node for the new manifest format.
        /// </summary>
        /// <returns><code>"D", space, full path name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "D {0}\n", FullPath);
        }

        /// <summary>
        /// Returns the string representation of this node for the old manifest format.
        /// </summary>
        /// <returns><code>"D", space, mtime, space, full path name, newline</code></returns>
        public override string ToStringOld()
        {
            return string.Format(CultureInfo.InvariantCulture, "D {0} {1}\n", ModifiedTime, FullPath);
        }
        #endregion

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
