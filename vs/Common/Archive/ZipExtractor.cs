using System;
using System.Collections.Generic;
using System.IO;
using Common.Properties;
using ICSharpCode.SharpZipLib.Zip;
using Common.Helpers;

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
        public override void Extract(string target, string subDir)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            try
            {
                string xbitFilePath = Path.Combine(target, ".xbit");

                foreach (ZipEntry entry in _zip)
                {
                    RejectArchiveIfNameContains(entry, "..");

                    // Only extract objects within the selected sub-directory
                    if (!string.IsNullOrEmpty(subDir) && !entry.Name.StartsWith(subDir)) continue;

                    if (entry.IsDirectory)
                    {
                        ExtractFolderEntry(target, entry);
                    }
                    else if (entry.IsFile)
                    {
                        ExtractFileEntry(target, _zip, entry);
                        AddEntryToXbitFileIfNecessary(entry, xbitFilePath);
                    }
                }
            }
            catch (ZipException ex)
            {
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
        }
        #endregion

        #region Helpers
        private static void ExtractFileEntry(string path, ZipFile zip, ZipEntry entry)
        {
            string targetPath = Path.Combine(path, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            DecompressAndWriteFile(zip, entry, targetPath);
            File.SetLastWriteTimeUtc(targetPath, entry.DateTime);
        }
        
        private static void DecompressAndWriteFile(ZipFile zip, ZipEntry entry, string targetPath)
        {
            long bytesRead = 0;
            var entryInputStream = zip.GetInputStream(entry);
            using (var output = File.Create(targetPath))
            {
                while (bytesRead < entry.Size)
                {
                    output.WriteByte((byte)entryInputStream.ReadByte());
                    ++bytesRead;
                }
            }
        }

        private static void AddEntryToXbitFileIfNecessary(ZipEntry entry, string xbitFile)
        {

            if (IsXbitSet(entry))
            {
                using (var xbitWriter = File.AppendText(xbitFile))
                {
                    xbitWriter.Write("/");
                    xbitWriter.Write(entry.Name);
                }
            }
        }

        private static void RejectArchiveIfNameContains(ZipEntry entry, string name)
        {
            if (entry.Name.Contains(name)) throw new IOException(Resources.InvalidEntry + entry.Name);
        }

        private static void ExtractFolderEntry(string path, ZipEntry entry)
        {
            Directory.CreateDirectory(Path.Combine(path, entry.Name));
            Directory.SetLastWriteTimeUtc(Path.Combine(path, entry.Name), entry.DateTime);
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
