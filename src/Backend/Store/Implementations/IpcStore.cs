/*
 * Copyright 2010-2014 Bastian Eicher
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Provides transparent access to an <see cref="IStore"/> running in another process (the Store Service).
    /// </summary>
    public partial class IpcStore : IStore
    {
        /// <inheritdoc/>
        public StoreKind Kind { get { return StoreKind.Service; } }

        /// <inheritdoc/>
        public string DirectoryPath
        {
            get
            {
                try
                {
                    return GetServiceProxy().DirectoryPath;
                }
                    #region Error handling
                catch (RemotingException)
                {
                    // Ignore remoting errors in case service is offline
                    return null;
                }
                #endregion
            }
        }

        //--------------------//

        /// <summary>
        /// Always returns empty list. Use a non-IPC <see cref="IStore"/> for this method instead.
        /// </summary>
        /// <remarks>Using the store service for this is unnecessary since it only requires read access to the file system.</remarks>
        public IEnumerable<ManifestDigest> ListAll()
        {
            return Enumerable.Empty<ManifestDigest>();
        }

        /// <summary>
        /// Always returns empty list. Use a non-IPC <see cref="IStore"/> for this method instead.
        /// </summary>
        /// <remarks>Using the store service for this is unnecessary since it only requires read access to the file system.</remarks>
        public IEnumerable<string> ListAllTemp()
        {
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Always returns <see langword="false"/>. Use a non-IPC <see cref="IStore"/> for this method instead.
        /// </summary>
        /// <remarks>Using the store service for this is unnecessary since it only requires read access to the file system.</remarks>
        public bool Contains(ManifestDigest manifestDigest)
        {
            return false;
        }

        /// <summary>
        /// Always returns <see langword="false"/>. Use a non-IPC <see cref="IStore"/> for this method instead.
        /// </summary>
        /// <remarks>Using the store service for this is unnecessary since it only requires read access to the file system.</remarks>
        public bool Contains(string directory)
        {
            return false;
        }

        /// <inheritdoc/>
        public void Flush()
        {
            // No internal caching
        }

        /// <summary>
        /// Always returns <see langword="null"/>. Use a non-IPC <see cref="IStore"/> for this method instead.
        /// </summary>
        /// <remarks>Using the store service for this is unnecessary since it only requires read access to the file system.</remarks>
        public string GetPath(ManifestDigest manifestDigest)
        {
            return null;
        }

        /// <inheritdoc/>
        public string AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            try
            {
                return GetServiceProxy().AddDirectory(path, manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        public string AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            try
            {
                return GetServiceProxy().AddArchives(archiveInfos, manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        public bool Remove(ManifestDigest manifestDigest)
        {
            try
            {
                return GetServiceProxy().Remove(manifestDigest);
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return false;
            }
            #endregion
        }

        /// <inheritdoc/>
        public long Optimise(ITaskHandler handler)
        {
            try
            {
                return GetServiceProxy().Optimise(handler);
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return 0;
            }
            #endregion
        }

        /// <summary>
        /// Does nothing. Should be handled by an <see cref="DirectoryStore"/> directly instead of using the service.
        /// </summary>
        public void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {}

        //--------------------//

        /// <summary>
        /// Returns a fixed string.
        /// </summary>
        public override string ToString()
        {
            // NOTE: Do not touch DirectoryPath here to avoid potentially expensive IPC
            return "Connection to Store Service (if available)";
        }
    }
}
