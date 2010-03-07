using System;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An immutable executable file-entry in a <see cref="Manifest"/>.
    /// </summary>
    public sealed class ExecutableFile : FileBase, IEquatable<ExecutableFile>
    {
        #region Constants
        /// <summary>
        /// The character at the beginning of a line that identifies this type of node.
        /// </summary>
        public const char NodeChar = 'X';
        #endregion

        #region Contsructor
        /// <summary>
        /// Creates a new executable file entry.
        /// </summary>
        /// <param name="hash">The hash of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified in the number of seconds since the epoch.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="fileName">The name of the file without the containing directory.</param>
        public ExecutableFile(string hash, long modifiedTime, long size, string fileName) : base(hash, modifiedTime, size, fileName)
        {}
        #endregion

        //--------------------//

        #region Compare
        public bool Equals(ExecutableFile other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(ExecutableFile) && Equals((ExecutableFile)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ExecutableFile left, ExecutableFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExecutableFile left, ExecutableFile right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
