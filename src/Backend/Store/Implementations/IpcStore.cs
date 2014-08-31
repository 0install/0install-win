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
        #region List all
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
        #endregion

        #region Contains
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
            try
            {
                GetServiceProxy().Flush();
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
            }
            #endregion
        }
        #endregion

        #region Get path
        /// <summary>
        /// Always returns <see langword="null"/>. Use a non-IPC <see cref="IStore"/> for this method instead.
        /// </summary>
        /// <remarks>Using the store service for this is unnecessary since it only requires read access to the file system.</remarks>
        public string GetPath(ManifestDigest manifestDigest)
        {
            return null;
        }
        #endregion

        #region Add
        /// <inheritdoc/>
        public void AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            try
            {
                GetServiceProxy().AddDirectory(path, manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new IOException(ex.Message, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        public void AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            try
            {
                GetServiceProxy().AddArchives(archiveInfos, manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new IOException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public void Remove(ManifestDigest manifestDigest)
        {
            try
            {
                GetServiceProxy().Remove(manifestDigest);
            }
                #region Error handling
            catch (RemotingException ex)
            {
                throw new ImplementationNotFoundException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        #region Optimise
        /// <summary>
        /// Does nothing. Should be handled by an administrator directly instead of using the service.
        /// </summary>
        public long Optimise(ITaskHandler handler)
        {
            return 0;
        }
        #endregion

        #region Verify
        /// <inheritdoc/>
        public void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {
            try
            {
                GetServiceProxy().Verify(manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
            }
            #endregion
        }
        #endregion
    }
}
