/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using Common;
using Common.Tasks;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Helper methods for <see cref="IStore"/>s accessed via IPC.
    /// </summary>
    public static class RemoteStores
    {
        /// <summary>
        /// Wrapper for <see cref="IStore.ListAll"/>, handling remoting exceptions.
        /// </summary>
        public static IEnumerable<ManifestDigest> ListAllSafe(this IStore store)
        {
            try
            {
                return store.ListAll();
            }
                #region Error handling
            catch (UnauthorizedAccessException)
            {
                // Ignore authorization errors since listing is not a critical task
                return new ManifestDigest[0];
            }
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return new ManifestDigest[0];
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.ListAllTemp"/>, handling remoting exceptions.
        /// </summary>
        public static IEnumerable<string> ListAllTempSafe(this IStore store)
        {
            try
            {
                return store.ListAllTemp();
            }
                #region Error handling
            catch (UnauthorizedAccessException)
            {
                // Ignore authorization errors since listing is not a critical task
                return new string[0];
            }
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return new string[0];
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.Contains(ManifestDigest)"/>, handling remoting exceptions.
        /// </summary>
        public static bool ContainsSafe(this IStore store, ManifestDigest manifestDigest)
        {
            try
            {
                return store.Contains(manifestDigest);
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return false;
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.Contains(string)"/>, handling remoting exceptions.
        /// </summary>
        public static bool ContainsSafe(this IStore store, string directory)
        {
            try
            {
                return store.Contains(directory);
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return false;
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.GetPath"/>, handling remoting exceptions.
        /// </summary>
        public static string GetPathSafe(this IStore store, ManifestDigest manifestDigest)
        {
            try
            {
                return store.GetPath(manifestDigest);
            }
                #region Error handling
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return null;
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.Remove"/>, handling remoting exceptions.
        /// </summary>
        public static bool RemoveSafe(this IStore store, ManifestDigest manifestDigest)
        {
            try
            {
                if (store.Contains(manifestDigest))
                {
                    store.Remove(manifestDigest);
                    return true;
                }
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
            }
            #endregion

            return false;
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.Optimise"/>, handling remoting exceptions.
        /// </summary>
        public static void OptimiseSafe(this IStore store, ITaskHandler handler)
        {
            try
            {
                store.Optimise(handler);
            }
                #region Sanity checks
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
            }
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.Verify"/>, handling remoting exceptions.
        /// </summary>
        public static void VerifySafe(this IStore store, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            try
            {
                store.Verify(manifestDigest, handler);
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
            }
            #endregion
        }

        /// <summary>
        /// Wrapper for <see cref="IStore.Audit"/>, handling remoting exceptions.
        /// </summary>
        public static IEnumerable<DigestMismatchException> AuditSafe(this IStore store, ITaskHandler handler)
        {
            try
            {
                return store.Audit(handler);
            }
                #region Error handling
            catch (RemotingException)
            {
                // Ignore remoting errors in case service is offline
                return new DigestMismatchException[0];
            }
            #endregion
        }
    }
}
