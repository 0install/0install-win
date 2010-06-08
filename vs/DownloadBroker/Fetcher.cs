/*
 * Copyright 2010 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Net;
using Common.Helpers;
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.DownloadBroker
{
    /// <summary>
    /// Manages one or more <see cref="FetcherRequest"/>s and keeps clients informed of the progress.
    /// </summary>
    public class Fetcher
    {
        #region Singleton Properties
        private static readonly Fetcher _defaultFetcher = new Fetcher(StoreProvider.Default);
        /// <summary>
        /// A singleton-instance of the <see cref="Fetcher"/> using <see cref="StoreProvider.Default"/>.
        /// </summary>
        public static Fetcher Default { get { return _defaultFetcher; } }
        #endregion

        #region Properties
        /// <summary>
        /// The location to store the downloaded and unpacked <see cref="Implementation"/>s in.
        /// </summary>
        public IStore Store { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download request. Use <see cref="Default"/> whenever possible instead.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Implementation"/>s in.</param>
        public Fetcher(IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            Store = store;
        }
        #endregion
        
        //--------------------//

        /// <summary>
        /// Execute a complete request and block until it is done.
        /// </summary>
        public void RunSync(FetcherRequest fetcherRequest)
        {
            foreach (var implementation in fetcherRequest.Implementations)
            {
                foreach (var archive in implementation.Archives)
                {
                    byte[] data;
                    using (var webClient = new WebClient())
                        data = webClient.DownloadData(archive.Location);

                    using (var archiveBuffer = new MemoryStream(data, (int)archive.StartOffset, data.Length - (int)archive.StartOffset))
                    {
                        string extracted = ExtractZip(archiveBuffer);
                        Store.Add(extracted, implementation.ManifestDigest);
                    }
                }
            }
        }

        private static string ExtractZip(Stream archive)
        {
            string extractFolder = FileHelper.GetTempDirectory();
            string xbitFile = Path.Combine(extractFolder, ".xbit");

            using (var zip = new ZipFile(archive))
            {
                foreach (ZipEntry entry in zip)
                {
                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(Path.Combine(extractFolder, entry.Name));
                    }
                    else if (entry.IsFile)
                    {
                        string currentFile = Path.Combine(extractFolder, entry.Name);
                        Directory.CreateDirectory(Path.GetDirectoryName(currentFile));
                        var binaryEntry = new BinaryReader(zip.GetInputStream(entry));
                        File.WriteAllBytes(currentFile, binaryEntry.ReadBytes((int)entry.Size));
                        File.SetLastWriteTimeUtc(currentFile, entry.DateTime);

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
            return extractFolder;
        }

        /// <summary>
        /// Determines whether an <see cref="ZipEntry"/> was packed on a Unix-system with the executable flag set to true.
        /// </summary>
        public static bool IsXbitSet(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;
            const int userExecuteFlag = 0x0040 << 16;
            return ((entry.ExternalFileAttributes & userExecuteFlag) == userExecuteFlag);
        }
    }
}
