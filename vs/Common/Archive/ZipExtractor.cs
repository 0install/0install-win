using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Archive
{
    
    public class ZipExtractor
    {
        private readonly Stream _archive;

        public ZipExtractor(Stream archive)
        {
            _archive = archive;
        }

        public void ExtractTo(string path)
        {
            using (var zip = new ZipInputStream(_archive))
            {
                zip.IsStreamOwner = false;

                ZipEntry entry;
                while ((entry = zip.GetNextEntry()) != null)
                {
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
                        using (var output = File.Create(targetPath))
                        {
                            while (bytesRead < size)
                            {
                                output.WriteByte(Convert.ToByte(zip.ReadByte()));
                                ++bytesRead;
                            }
                        }
                    }
                }
            }
        }
    }
}
