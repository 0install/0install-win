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
using Common.Download;
using Common.Storage;
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
        private static readonly Fetcher _defaultFetcher = new Fetcher(StoreProvider.DefaultStore);
        /// <summary>
        /// A singleton-instance of the <see cref="Fetcher"/> using <see cref="StoreProvider.DefaultStore"/>.
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
                    var tempArchive = new FileInfo(Path.GetTempFileName());
                    new DownloadFile(archive.Location, tempArchive.FullName).RunSync();
                    
                    using (var archiveStream = tempArchive.OpenRead())
                    {
                        archiveStream.Seek(archive.StartOffset, SeekOrigin.Begin);
                        
                        HandleZip(archiveStream);
                    }
                }
            }
        }

        private void HandleZip(Stream archive)
        {
            var stream = new ZipInputStream(archive);

            using (var extractFolder = new TemporaryDirectory())
            {
                while (stream.Available == 1)
                {
                    ZipEntry entry = stream.GetNextEntry();
                    string extractedEntry = Path.Combine(extractFolder.Path, entry.Name);
                    if (entry.IsDirectory) Directory.CreateDirectory(extractedEntry);
                    else if (entry.IsFile)
                    {
                        var data = new byte[entry.Size];
                        File.WriteAllBytes(extractedEntry, data);
                    }
                    else throw new NotSupportedException("Not supported archive entry");
                }
            }
        }
    }
}
