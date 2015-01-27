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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Creates <see cref="IStore"/> instances.
    /// </summary>
    public static class StoreFactory
    {
        /// <summary>
        /// Creates an <see cref="IStore"/> instance that uses the default cache locations.
        /// </summary>
        /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file or one of the stores was not permitted.</exception>
        [NotNull]
        public static IStore CreateDefault()
        {
            return new CompositeStore(GetStores());
        }

        /// <summary>
        /// Returns a list of <see cref="IStore"/>s representing all local cache directories.
        /// </summary>
        /// <exception cref="IOException">A directory could not be created or if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating a directory was not permitted.</exception>
        [NotNull, ItemNotNull]
        private static IEnumerable<IStore> GetStores()
        {
            var stores = new List<IStore>();

            // Directories
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
                    throw new IOException(string.Format(Resources.ProblemAccessingStore, path), ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Wrap exception to add context information
                    throw new UnauthorizedAccessException(string.Format(Resources.ProblemAccessingStore, path), ex);
                }
                #endregion
            }

            // Store service
            if (WindowsUtils.IsWindowsNT && !Locations.IsPortable) stores.Add(new IpcStore());

            return stores;
        }

        /// <summary>
        /// Returns a list of paths for implementation directories / stores as defined by configuration files including the default locations.
        /// </summary>
        /// <param name="excludeUserProfile"><see langword="true"/> to exclude the default location in the user profile (e.g. for system services).</param>
        /// <remarks>Mutliple configuration files apply cumulatively. I.e., directories from both the user config and the system config are used.</remarks>
        /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetImplementationDirs(bool excludeUserProfile = false)
        {
            if (!excludeUserProfile)
            {
                // Add the user cache to have a reliable fallback location for storage
                yield return Locations.GetCacheDirPath("0install.net", machineWide: false, resource: "implementations");
            }

            // Add the system cache when not in portable mode
            if (!Locations.IsPortable)
            {
                string systemCache;
                try
                {
                    systemCache = Locations.GetCacheDirPath("0install.net", machineWide: true, resource: "implementations");
                }
                catch (UnauthorizedAccessException)
                { // Standard users cannot create machine-wide directories, only use them if they already exist
                    systemCache = null;
                }
                if (systemCache != null) yield return systemCache;
            }

            // Add custom cache locations
            foreach (string configFile in Locations.GetLoadConfigPaths("0install.net", true, "injector", "implementation-dirs"))
            {
                foreach (var path in GetCustomImplementationDirs(configFile))
                    yield return path;
            }
        }

        /// <summary>
        /// Returns a list of implementation directory paths in a specific configuration file.
        /// </summary>
        /// <param name="configPath">The path of the configuration file to read.</param>
        /// <exception cref="IOException">There was a problem accessing<paramref name="configPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to <paramref name="configPath"/> was not permitted.</exception>
        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetCustomImplementationDirs([NotNull] string configPath)
        {
            foreach (string path in
                from line in File.ReadAllLines(configPath, Encoding.UTF8)
                where !line.StartsWith("#") && !string.IsNullOrEmpty(line)
                select Environment.ExpandEnvironmentVariables(line))
            {
                string result = path;
                try
                {
                    if (!Path.IsPathRooted(path))
                    { // Allow relative paths only for portable installations
                        if (Locations.IsPortable) result = Path.Combine(Locations.PortableBase, path);
                        else throw new IOException(string.Format(Resources.NonRootedPathInConfig, path, configPath));
                    }
                }
                    #region Error handling
                catch (ArgumentException ex)
                {
                    // Wrap exception to add context information
                    throw new IOException(string.Format(Resources.ProblemAccessingStoreEx, path, configPath), ex);
                }
                #endregion

                yield return result;
            }
        }

        /// <summary>
        /// Sets the list of implementation directory paths in the current user configuration.
        /// </summary>
        /// <param name="paths">The list of implementation directory paths to set.</param>
        /// <exception cref="IOException">There was a problem writing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        public static void SetUserCustomImplementationDirs([NotNull, ItemNotNull, InstantHandle] IEnumerable<string> paths)
        {
            #region Sanity checks
            if (paths == null) throw new ArgumentNullException("paths");
            #endregion

            using (var atomic = new AtomicWrite(Locations.GetSaveConfigPath("0install.net", true, "injector", "implementation-dirs")))
            {
                using (var configFile = new StreamWriter(atomic.WritePath, append: false, encoding: FeedUtils.Encoding) {NewLine = "\n"})
                {
                    foreach (var path in paths)
                        configFile.WriteLine(path);
                }
                atomic.Commit();
            }
        }
    }
}
