using System;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An immutable symlink-entry in a <see cref="Manifest"/>.
    /// </summary>
    public sealed class Symlink : ManifestNode, IEquatable<Symlink>
    {
        #region Constants
        /// <summary>
        /// The character at the beginning of a line that identifies this type of node.
        /// </summary>
        public const char NodeChar = 'S';
        #endregion

        #region Properties
        /// <summary>
        /// The hash of the link target path.
        /// </summary>
        public string Hash { get; set; }
        
        /// <summary>
        /// The length of the link target path.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// The name of the symlink without the containing directory.
        /// </summary>
        public string SymlinkName { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new symlink-entry.
        /// </summary>
        /// <param name="hash">The hash of the link target path.</param>
        /// <param name="size">The length of the link target path.</param>
        /// <param name="symlinkName">The name of the symlink without the containing directory.</param>
        public Symlink(string hash, long size, string symlinkName)
        {
            Hash = hash;
            Size = size;
            SymlinkName = symlinkName;
        }
        #endregion

        //--------------------//

        #region Compare
        public bool Equals(Symlink other)
        {
            if (other == null) return false;
            if (other == this) return true;
            return Equals(other.Hash, Hash) && other.Size == Size && Equals(other.SymlinkName, SymlinkName);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj.GetType() == typeof(Symlink) && Equals((Symlink)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Hash != null ? Hash.GetHashCode() : 0);
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ (SymlinkName != null ? SymlinkName.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(Symlink left, Symlink right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Symlink left, Symlink right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
