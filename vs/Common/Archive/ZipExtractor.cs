using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Archive
{
    /// <summary>
    /// Exception class indicating an invalid archive given to ZipExtractor.
    /// </summary>
    /// Currently this is only thrown when an archive contains folder entries
    /// named '..'.
    public class InvalidArchive : Exception
    {
        public InvalidArchive(string message)
            : base(message)
        { }
    }

    /// <summary>
    /// Helper class to perform extraction of Zip archives.
    /// </summary>
    public class ZipExtractor
    {
        private readonly Stream _archive;

        /// <summary>
        /// Constructs a <see cref="ZipExtractor"/> to extract the zip archive
        /// in the supplied stream.
        /// Does not take ownership of the stream.
        /// </summary>
        /// <param name="archive">stream containing the archive's data.</param>
        public ZipExtractor(Stream archive)
        {
            _archive = archive;
        }

        /// <summary>
        /// Perform this extraction, writing the contents into the supplied path.
        /// </summary>
        /// <param name="path">file system path to write to.</param>
        /// <exception cref="InvalidArchive">Thrown if archive is not usable, e.g. it contains any folder named '..'.</exception>
        public void ExtractTo(string path)
        {
            using (var zip = new ZipFile(_archive))
            {
                zip.IsStreamOwner = false;
                string xbitFilePath = Path.Combine(path, ".xbit");

                foreach (ZipEntry entry in zip)
                {
                    RejectArchiveIfNameContains(entry, "..");

                    if (entry.IsDirectory)
                    {
                        ExtractFolderEntry(path, entry);
                    }
                    else if (entry.IsFile)
                    {
                        ExtractFileEntry(path, zip, entry);
                        AddEntryToXbitFileIfNecessary(entry, xbitFilePath);
                    }
                }
            }
        }

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
            if (entry.Name.Contains(name)) throw new InvalidArchive("Invalid entry \n" + entry.Name);
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
    }
}
