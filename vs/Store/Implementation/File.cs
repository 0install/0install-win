using System;
using System.Globalization;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An immutable non-executable file-entry in a <see cref="Manifest"/>.
    /// </summary>
    public sealed class File : FileBase, IEquatable<File>
    {
        #region Constructor
        /// <summary>
        /// Creates a new non-executable file entry.
        /// </summary>
        /// <param name="hash">The hash of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified in the number of seconds since the epoch.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="fileName">The name of the file without the containing directory.</param>
        public File(string hash, long modifiedTime, long size, string fileName) : base(hash, modifiedTime, size, fileName)
        {}
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the string representation of this node for the manifest format.
        /// </summary>
        /// <returns><code>"F", space, hash, space, mtime, space, size, space, file name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "F {0} {1} {2} {3}\n", Hash, ModifiedTime, Size, FileName);
        }
        #endregion

        #region Compare
        public bool Equals(File other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(File) && Equals((File)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(File left, File right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(File left, File right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
