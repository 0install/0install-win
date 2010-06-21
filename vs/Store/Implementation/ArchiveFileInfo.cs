namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// A parameter structure containing information about a requested archive extraction.
    /// </summary>
    /// <see cref="IStore.AddArchive"/>
    /// <see cref="IStore.AddMultipleArchives"/>
    public struct ArchiveFileInfo
    {
        /// <summary>
        /// The file to be extracted.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The MIME type of archive format of the file; <see langword="null"/> to guess.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// The sub-directory in the archive to be extracted; <see langword="null"/> for entire archive.
        /// </summary>
        public string SubDir { get; set; }

        /// <summary>
        /// The number of bytes at the beginning of the file which should be ignored.
        /// </summary>
        public long StartOffset { get; set; }
    }
}
