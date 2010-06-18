using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Archive
{
    /// <summary>
    /// Extracts ZIP files.
    /// </summary>
    public class ZipExtractor : Extractor
    {
        #region Constructor
        /// <summary>
        /// Prepares to extract a ZIP archive contained in a stream.
        /// </summary>
        /// <param name="archive">The stream containing the archive's data.</param>
        /// <param name="subDir">The sub-directory within the archive to extract; may be <see langword="null"/>.</param>
        public ZipExtractor(Stream archive, string subDir) : base(archive, subDir)
        {}
        #endregion

        //--------------------//

        #region Extraction methods
        public override void Extract(string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            using (var zip = new ZipFile(Stream))
            {
                zip.IsStreamOwner = false;
                string xbitFilePath = Path.Combine(target, ".xbit");

                foreach (ZipEntry entry in zip)
                {
                    RejectArchiveIfNameContains(entry, "..");

                    if (entry.IsDirectory)
                    {
                        ExtractFolderEntry(target, entry);
                    }
                    else if (entry.IsFile)
                    {
                        ExtractFileEntry(target, zip, entry);
                        AddEntryToXbitFileIfNecessary(entry, xbitFilePath);
                    }
                }
            }
        }
        #endregion

        #region Helpers
        private static void ExtractFileEntry(string path, ZipFile zip, ZipEntry entry)
        {
            string targetPath = Path.Combine(path, entry.Name);
            ProvideParentFolder(targetPath);

            DecompressAndWriteFile(zip, entry, targetPath);
            File.SetLastWriteTimeUtc(targetPath, entry.DateTime);
        }

        private static void ProvideParentFolder(string targetPath)
        {
            new FileInfo(targetPath).Directory.Create();
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
            if (entry.Name.Contains(name)) throw new IOException("Invalid entry: " + entry.Name);
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
