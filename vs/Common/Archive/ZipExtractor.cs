using System;
using System.Collections.Generic;
using System.IO;
using Common.Helpers;
using Common.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Archive
{
    /// <summary>
    /// Provides methods for extracting a ZIP archive.
    /// </summary>
    public class ZipExtractor : Extractor
    {
        #region Variables
        private ZipFile _zip;
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract a ZIP archive contained in a stream.
        /// </summary>
        /// <param name="archive">The stream containing the archive's data.</param>
        /// <param name="startOffset">The number of bytes at the beginning of the stream which should be ignored.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public ZipExtractor(Stream archive, long startOffset) : base(archive, startOffset)
        {
            try
            {
                _zip = new ZipFile(Stream) { IsStreamOwner = false };
            }
            catch (ZipException ex)
            {
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
        }
        #endregion

        //--------------------//

        #region Content
        public override IEnumerable<string> ListContent()
        {
            var contentList = new List<string>((int)_zip.Count);
            try
            {
                foreach (ZipEntry entry in _zip)
                    contentList.Add(StringHelper.UnifySlashes(entry.Name));
            }
            catch (ZipException ex)
            {
                throw new IOException(Resources.ArchiveInvalid, ex);
            }

            return contentList;
        }

        public override IEnumerable<string> ListDirectories()
        {
            var directoryList = new List<string>((int)_zip.Count);
            try
            {
                foreach (ZipEntry entry in _zip)
                    if (entry.IsDirectory) directoryList.Add(StringHelper.UnifySlashes(entry.Name));
            }
            catch (ZipException ex)
            {
                throw new IOException(Resources.ArchiveInvalid, ex);
            }

            return directoryList;
        }
        #endregion

        #region Extraction
        public override void Extract(string target, string subDir, ProgressCallback extractionProgress)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            try
            {
                int i = 0;
                foreach (ZipEntry entry in _zip)
                {
                    string entryName = GetSubEntryName(entry.Name, subDir);
                    if (string.IsNullOrEmpty(entryName)) continue;

                    if (entry.IsDirectory) CreateDirectory(target, entryName, entry.DateTime);
                    else if (entry.IsFile)
                    {
                        using (var stream = _zip.GetInputStream(entry))
                            WriteFile(target, entryName, entry.DateTime, stream, entry.Size, IsXbitSet(entry));

                        // Report back the progess
                        if (extractionProgress != null) extractionProgress(++i / (float)_zip.Count, entryName);
                    }
                }
            }
            catch (ZipException ex)
            {
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
        }

        /// <summary>
        /// Determines whether an <see cref="ZipEntry"/> was packed on a Unix-system with the executable flag set to true.
        /// </summary>
        private static bool IsXbitSet(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;
            const int userExecuteFlag = 0x0040 << 16;
            return ((entry.ExternalFileAttributes & userExecuteFlag) != 0);
        }
        #endregion
    }
}
