using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Archive
{

    public class InvalidArchive : Exception
    {
        public InvalidArchive(string message)
            : base(message)
        { }
    }

    public class ZipExtractor
    {
        private readonly Stream _archive;

        public ZipExtractor(Stream archive)
        {
            _archive = archive;
        }

        public void ExtractTo(string path)
        {
            using (var zip = new ZipFile(_archive))
            {
                zip.IsStreamOwner = false;
                string xbitFile = Path.Combine(path, ".xbit");

                foreach (ZipEntry entry in zip)
                {
                    if (entry.Name.Contains("..")) throw new InvalidArchive("Invalid entry \n" + entry.Name);

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(Path.Combine(path, entry.Name));
                    }
                    else if (entry.IsFile)
                    {
                        string targetPath = Path.Combine(path, entry.Name);
                        new FileInfo(targetPath).Directory.Create();

                        long size = entry.Size;
                        long bytesRead = 0;
                        var entryInputStream = zip.GetInputStream(entry);
                        using (var output = File.Create(targetPath))
                        {
                            while (bytesRead < size)
                            {
                                output.WriteByte((byte)entryInputStream.ReadByte());
                                ++bytesRead;
                            }
                        }
                        File.SetLastWriteTimeUtc(targetPath, entry.DateTime);

                        if (IsXbitSet(entry))
                        {
                            using (var xbitWriter = File.AppendText(xbitFile))
                            {
                                xbitWriter.Write("/");
                                xbitWriter.Write(entry.Name);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether an <see cref="ZipEntry"/> was packed on a Unix-system with the executable flag set to true.
        /// </summary>
        private static bool IsXbitSet(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;
            const int userExecuteFlag = 0x0040 << 16;
            return ((entry.ExternalFileAttributes & userExecuteFlag) == userExecuteFlag);
        }
    }
}
