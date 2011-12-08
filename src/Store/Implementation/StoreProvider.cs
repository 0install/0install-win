/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Text;
using Common.Storage;
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
            return new CompositeStore(GetStores());
        }

        /// <summary>
        /// Returns a list of <see cref="IStore"/>s representing all local cache directories and the <see cref="ServiceStore"/>.
        /// </summary>
        /// <exception cref="IOException">Thrown if a directory could not be created or if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory was not permitted.</exception>
        private static IEnumerable<IStore> GetStores()
        {
            var stores = new C5.LinkedList<IStore>();
            foreach (var path in GetImplementationDirs())
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
            stores.Add(new ServiceStore());

            return stores;
        }

        /// <summary>
        /// Returns a list of custom paths for implementation directories / stores as defined by configuration files.
        /// </summary>
        /// <exception cref="IOException">Thrown if there was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to a configuration file was not permitted.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        private static IEnumerable<string> GetImplementationDirs()
        {
            // Always add the user cache to have a reliable fallback location for storage
            yield return Locations.GetCacheDirPath("0install.net", "implementations");

            // ToDo: Add default shared location if the configs are empty
            foreach (string configFile in Locations.GetLoadConfigPaths("0install.net", true, "injector", "implementation-dirs"))
            {
                foreach (string line in File.ReadAllLines(configFile, Encoding.UTF8))
                {
                    if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;
                    string path = Environment.ExpandEnvironmentVariables(line);

                    try
                    {
                        if (!Path.IsPathRooted(path))
                        { // Allow relative paths only for portable installations
                            if (Locations.IsPortable) path = Path.Combine(Locations.PortableBase, path);
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

                    yield return path;
                }
            }
        }
    }
}
