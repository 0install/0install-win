using System;
using System.Globalization;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An immutable symlink-entry in a <see cref="Manifest"/>.
    /// </summary>
    public sealed class Symlink : ManifestNode, IEquatable<Symlink>
    {
        #region Properties
        /// <summary>
        /// The hash of the link target path.
        /// </summary>
        public string Hash { get; set; }
        
        /// <summary>
        /// The length of the link target path.
        /// </summary>
        public long Size { get; set; }

        private string _symlinkName;
        /// <summary>
        /// The name of the symlink without the containing directory.
        /// </summary>
        public string SymlinkName
        {
            get { return _symlinkName; }
            private set
            {
                if (value.Contains("\n")) throw new ArgumentException(Resources.NewlineInName, "value");
                _symlinkName = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new symlink-entry.
        /// </summary>
        /// <param name="hash">The hash of the link target path.</param>
        /// <param name="size">The length of the link target path.</param>
        /// <param name="symlinkName">The name of the symlink without the containing directory.</param>
        internal Symlink(string hash, long size, string symlinkName)
        {
            Hash = hash;
            Size = size;
            SymlinkName = symlinkName;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the string representation of this node for the manifest format.
        /// </summary>
        /// <returns><code>"S", space, hash, space, size, space, symlink name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "S {0} {1} {2}", Hash, Size, SymlinkName);
        }

        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToString"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        /// <exception cref="ArgumentException">Thrown if the number of space-separated parts in the <paramref name="line"/> are incorrect.</exception>
        internal static Symlink FromString(string line)
        {
            const int numberOfParts = 4;
            string[] parts = line.Split(new[] { ' ' }, numberOfParts);
            if (parts.Length != numberOfParts) throw new ArgumentException(Resources.InvalidNumberOfLineParts, "line");
            return new Symlink(parts[1], long.Parse(parts[2]), parts[3]);
        }
        #endregion

        #region Equality
        public bool Equals(Symlink other)
        {
            if (ReferenceEquals(null, other)) return false;

            return Equals(other.Hash, Hash) && other.Size == Size && Equals(other.SymlinkName, SymlinkName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
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
        #endregion
    }
}
