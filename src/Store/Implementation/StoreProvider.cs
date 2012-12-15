/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Creates <see cref="IStore"/> instances.
    /// </summary>
    public static class StoreProvider
    {
        /// <summary>
        /// Creates an <see cref="IStore"/> instance that uses the default cache locations.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to a configuration file or one of the stores was not permitted.</exception>
        public static IStore CreateDefault()
        {
            return new CompositeStore(GetStores().ToArray());
        }

        /// <summary>
        /// Returns a list of <see cref="IStore"/>s representing all local cache directories.
        /// </summary>
        /// <exception cref="IOException">Thrown if a directory could not be created or if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory was not permitted.</exception>
        private static IEnumerable<IStore> GetStores()
        {
            var stores = new List<IStore>();

            // Directories
            foreach (var path in GetImplementationDirs(true))
            {
                try
                {
                    stores.Add(new DirectoryStore(path));
                }
                    #region Error handling
                catch (IOException ex)
                {
                    // Wrap exception to add context information
                    throw new IOException(string.Format(Resources.ProblemAccessingStore, path) + "\n" + ex.Message, ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Wrap exception to add context information
                    throw new UnauthorizedAccessException(string.Format(Resources.ProblemAccessingStore, path) + "\n" + ex.Message, ex);
                }
                #endregion
            }

            // Store service
            if (WindowsUtils.IsWindowsNT && !Locations.IsPortable)
            {
                try
                {
                    stores.Add(IpcStoreProvider.ServiceProxy);
                }
                    #region Error handling
                catch (RemotingException)
                {
                    // Ignore remoting errors in case service is offline
                }
                #endregion
            }

            return stores;
        }

        /// <summary>
        /// Returns a list of paths for implementation directories / stores as defined by configuration files including the default locations.
        /// </summary>
        /// <param name="includeUserProfile"><see langword="true"/> to include the default location in the user profile. Use <see langword="false"/> for system services.</param>
        /// <exception cref="IOException">Thrown if there was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to a configuration file was not permitted.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        public static IEnumerable<string> GetImplementationDirs(bool includeUserProfile)
        {
            if (includeUserProfile)
            {
                // Add the user cache to have a reliable fallback location for storage
                yield return Locations.GetCacheDirPath("0install.net", false, "implementations");
            }

            // Add custom cache locations
            foreach (string configFile in Locations.GetLoadConfigPaths("0install.net", true, "injector", "implementation-dirs"))
            {
                foreach (string path in
                    from line in File.ReadAllLines(configFile, Encoding.UTF8)
                    where !line.StartsWith("#") && !string.IsNullOrEmpty(line)
                    select Environment.ExpandEnvironmentVariables(line))
                {
                    string result = path;
                    try
                    {
                        if (!Path.IsPathRooted(path))
                        { // Allow relative paths only for portable installations
                            if (Locations.IsPortable) result = Path.Combine(Locations.PortableBase, path);
                            else throw new IOException(string.Format(Resources.NonRootedPathInConfig, path, configFile));
                        }
                    }
                        #region Error handling
                    catch (ArgumentException ex)
                    {
                        // Wrap exception to add context information
                        throw new IOException(string.Format(Resources.ProblemAccessingStoreEx, path, configFile) + "\n" + ex.Message, ex);
                    }
                    #endregion

                    yield return result;
                }
            }

            // Add the system cache when not in portable mode
            if (!Locations.IsPortable)
            {
                string systemCache;
                try
                {
                    systemCache = Locations.GetCacheDirPath("0install.net", true, "implementations");
                }
                catch (UnauthorizedAccessException)
                { // Standard users cannot create machine-wide directories, only use them if they already exist
                    systemCache = null;
                }
                if (systemCache != null) yield return systemCache;
            }
        }
    }
}
