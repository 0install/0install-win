using ICSharpCode.SharpZipLib.Zip;

namespace Common.Storage
{
    /// <summary>
    /// Represents a file in a content archive
    /// </summary>
    internal struct ContentArchiveEntry
    {
        #region Variables
        private readonly ZipFile _zipFile;
        private readonly ZipEntry _zipEntry;
        #endregion

        #region Properties
        /// <summary>
        /// The archive containing the file
        /// </summary>
        public ZipFile ZipFile { get { return _zipFile; } }

        /// <summary>
        /// The actual content file
        /// </summary>
        public ZipEntry ZipEntry { get { return _zipEntry; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new content file representation
        /// </summary>
        /// <param name="zipFile">The archive containing the file</param>
        /// <param name="zipEntry">The actual content file</param>
        public ContentArchiveEntry(ZipFile zipFile, ZipEntry zipEntry)
        {
            _zipFile = zipFile;
            _zipEntry = zipEntry;
        }
        #endregion
    }
}