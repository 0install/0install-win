/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common.Storage;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Manages <see cref="IStore"/> implementation directories.
    /// </summary>
    public static class StoreConfig
    {
        /// <summary>
        /// Returns a list of paths for implementation directories as defined by configuration files including the default locations.
        /// </summary>
        /// <param name="serviceMode"><c>true</c> to exclude the default location in the user profile, e.g., for system services.</param>
        /// <remarks>Mutliple configuration files apply cumulatively. I.e., directories from both the user config and the system config are used.</remarks>
        /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reads data from a config file with no caching")]
        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetImplementationDirs(bool serviceMode = false)
        {
            if (!serviceMode)
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

            // Add configured cache locations
            foreach (string configFile in Locations.GetLoadConfigPaths("0install.net", true, "injector", "implementation-dirs"))
            {
                foreach (var path in GetImplementationDirs(configFile))
                    yield return path;
            }
        }

        /// <summary>
        /// Returns a list of custom implementation directories in the current user configuration.
        /// </summary>
        /// <exception cref="IOException">There was a problem accessin a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        [NotNull, ItemNotNull]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "May throw exceptions")]
        public static IEnumerable<string> GetUserImplementationDirs()
        {
            return GetImplementationDirs(GetUserConfigFile());
        }

        /// <summary>
        /// Sets the list of custom implementation directories in the current user configuration.
        /// </summary>
        /// <param name="paths">The list of implementation directories to set.</param>
        /// <exception cref="IOException">There was a problem writing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        public static void SetUserImplementationDirs([NotNull, ItemNotNull, InstantHandle] IEnumerable<string> paths)
        {
            SetImplementationDirs(GetUserConfigFile(), paths);
        }

        private static string GetUserConfigFile()
        {
            return Locations.GetSaveConfigPath("0install.net", true, "injector", "implementation-dirs");
        }

        /// <summary>
        /// Returns a list of custom implementation directories in the current machine-wide configuration.
        /// </summary>
        /// <exception cref="IOException">There was a problem accessin a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        [NotNull, ItemNotNull]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "May throw exceptions")]
        public static IEnumerable<string> GetMachineWideImplementationDirs()
        {
            return GetImplementationDirs(GetMachineWideConfigFile());
        }

        /// <summary>
        /// Sets the list of custom implementation directories in the current machine-wide configuration.
        /// </summary>
        /// <param name="paths">The list of implementation directories to set.</param>
        /// <exception cref="IOException">There was a problem writing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        public static void SetMachineWideImplementationDirs([NotNull, ItemNotNull, InstantHandle] IEnumerable<string> paths)
        {
            SetImplementationDirs(GetMachineWideConfigFile(), paths);
        }

        private static string GetMachineWideConfigFile()
        {
            return Locations.GetSaveSystemConfigPath("0install.net", true, "injector", "implementation-dirs");
        }

        /// <summary>
        /// Returns a list of implementation directories in a specific configuration file.
        /// </summary>
        /// <param name="configPath">The path of the configuration file to read.</param>
        /// <exception cref="IOException">There was a problem accessing <paramref name="configPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to <paramref name="configPath"/> was not permitted.</exception>
        [NotNull, ItemNotNull]
        private static IEnumerable<string> GetImplementationDirs([NotNull] string configPath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(configPath)) throw new ArgumentNullException("configPath");
            #endregion

            if (!File.Exists(configPath)) yield break;

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
        /// Sets the list of implementation directories in a specific configuration file.
        /// </summary>
        /// <param name="configPath">The path of the configuration file to write.</param>
        /// <param name="paths">The list of implementation directories to set.</param>
        /// <exception cref="IOException">There was a problem writing <paramref name="configPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to <paramref name="configPath"/> was not permitted.</exception>
        private static void SetImplementationDirs([NotNull] string configPath, [NotNull, ItemNotNull, InstantHandle] IEnumerable<string> paths)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(configPath)) throw new ArgumentNullException("configPath");
            if (paths == null) throw new ArgumentNullException("paths");
            #endregion

            using (var atomic = new AtomicWrite(configPath))
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
