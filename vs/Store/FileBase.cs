namespace ZeroInstall.Store
{
    /// <summary>
    /// An immutable abstract base class for file-entries in a <see cref="Manifest"/>.
    /// </summary>
    public abstract class FileBase : ManifestNode
    {
        #region Properties
        /// <summary>
        /// The hash of the content of the file calculated using the selected digest algorithm.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// The time this file was last modified in the number of seconds since the epoch.
        /// </summary>
        public long ModifiedTime { get; private set; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// The name of the file without the containing directory.
        /// </summary>
        public string FileName { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new file entry.
        /// </summary>
        /// <param name="hash">The hash of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified in the number of seconds since the epoch.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="fileName">The name of the file without the containing directory.</param>
        protected FileBase(string hash, long modifiedTime, long size, string fileName)
        {
            Hash = hash;
            ModifiedTime = modifiedTime;
            Size = size;
            FileName = fileName;
        }
        #endregion

        //--------------------//

        #region Compare
        protected bool Equals(FileBase other)
        {
            if (other == null) return false;
            if (other == this) return true;
            return other.Hash == Hash && other.ModifiedTime == ModifiedTime && other.Size == Size && other.FileName == FileName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Hash != null ? Hash.GetHashCode() : 0);
                result = (result * 397) ^ ModifiedTime.GetHashCode();
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
