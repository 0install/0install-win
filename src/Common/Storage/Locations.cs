/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Common.Properties;
using Common.Utils;

namespace Common.Storage
{
    /// <summary>
    /// Provides easy access to platform-specific common directories for storing settings and application data.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Environment.SpecialFolder"/> on Windows and the freedesktop.org basedir spec (XDG) on Linux.
    /// See http://freedesktop.org/wiki/Standards/basedir-spec
    /// </remarks>
    public static class Locations
    {
        #region Helpers
        /// <summary>
        /// Returns the value of an environment variable or a default value if it isn't set.
        /// </summary>
        /// <param name="variable">The name of the environment variable to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the environment variable was not set.</param>
        /// <returns>The value of the environment variable or <paramref name="defaultValue"/>.</returns>
        private static string GetEnvironmentVariable(string variable, string defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(variable);
            return (string.IsNullOrEmpty(value)) ? defaultValue : value;
        }
        #endregion

        #region Variables
        /// <summary>
        /// The directory the application binaries are located in without a trailing directory separator charachter.
        /// </summary>
        public static readonly string InstallBase = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        #endregion

        #region Properties
        private static bool _isPortable = File.Exists(Path.Combine(InstallBase, "_portable"));

        /// <summary>
        /// Indicates whether the application is currently operating in portable mode.
        /// </summary>
        /// <remarks>
        ///   <para>Portable mode is activated by placing a file named "_portable" in <see cref="InstallBase"/>.</para>
        ///   <para>When portable mode is active files are stored and loaded from <see cref="PortableBase"/> instead of the user profile and sysem directories.</para>
        /// </remarks>
        public static bool IsPortable { get { return _isPortable; } set { _isPortable = value; } }

        private static string _portableBase = InstallBase;

        /// <summary>
        /// The directory used for storing files if <see cref="IsPortable"/> is <see langword="true"/>. Defaults to <see cref="InstallBase"/>.
        /// </summary>
        public static string PortableBase { get { return _portableBase; } set { _portableBase = value; } }

        #region Per-user directories
        /// <summary>
        /// The home directory of the current user.
        /// </summary>
        public static string HomeDir { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); } }

        /// <summary>
        /// The directory to store per-user settings (can roam across different machines).
        /// </summary>
        /// <remarks>On Windows this is <c>%appdata%</c>, on Linux it usually is <c>~/.config</c>.</remarks>
        public static string UserConfigDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CONFIG_HOME", Path.Combine(HomeDir, ".config"));

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                }
            }
        }

        /// <summary>
        /// The directory to store per-user data files (should not roam across different machines).
        /// </summary>
        /// <remarks>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.local/share</c>.</remarks>
        public static string UserDataDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_DATA_HOME", Path.Combine(HomeDir, ".local/share"));

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
            }
        }

        /// <summary>
        /// The directory to store per-user non-essential data (should not roam across different machines).
        /// </summary>
        /// <remarks>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.cache</c>.</remarks>
        public static string CacheDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CACHE_HOME", Path.Combine(HomeDir, ".cache"));

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
            }
        }
        #endregion

        #region System-wide directories
        /// <summary>
        /// The directories to store system-wide settings (can roam across different machines).
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/etc/xdg</c>.</remarks>
        public static string SystemConfigDirs
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CONFIG_DIRS", "/etc/xdg");

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                }
            }
        }

        /// <summary>
        /// The directories to store system-wide data files (should not roam across different machines).
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/usr/local/share:/usr/share</c>.</remarks>
        public static string SystemDataDirs
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_DATA_DIRS", "/usr/local/share:/usr/share");

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                }
            }
        }
        #endregion

        #endregion

        //--------------------//

        #region Paths
        /// <summary>
        /// Returns a path for storing a configuration resource (can roam across different machines).
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="isFile"><see langword="true"/> if the last part of <paramref name="resource"/> refers to a file instead of a directory.</param>
        /// <param name="resource">The path elements (directory and/or file names) of the resource to be stored.</param>
        /// <returns>A fully qualified path to use to store the resource. Directories are guaranteed to already exist; files are not.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static string GetSaveConfigPath(string appName, bool isFile, params string[] resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (resource == null) throw new ArgumentNullException("resource");
            #endregion

            string resourceCombined = FileUtils.PathCombine(resource);
            string path;
            try
            {
                path = _isPortable
                    ? FileUtils.PathCombine(_portableBase, "config", resourceCombined)
                    : FileUtils.PathCombine(UserConfigDir, appName, resourceCombined);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception to add context information
                throw new IOException(string.Format(Resources.InvalidConfigDir, UserConfigDir) + "\n" + ex.Message, ex);
            }
            #endregion

            // Ensure the directory part of the path exists
            path = Path.GetFullPath(path);
            string dirPath = isFile ? (Path.GetDirectoryName(path) ?? path) : path;
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            return path;
        }

        /// <summary>
        /// Returns a list of paths for loading a configuration resource (can roam across different machines).
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="isFile"><see langword="true"/> if the last part of <paramref name="resource"/> refers to a file instead of a directory.</param>
        /// <param name="resource">The path elements (directory and/or file names) of the resource to be loaded.</param>
        /// <returns>
        /// A list of fully qualified paths to use to load the resource sorted by decreasing importance.
        /// This list will always reflect the current state in the filesystem and can not be modified! It may be empty.
        /// </returns>
        public static IEnumerable<string> GetLoadConfigPaths(string appName, bool isFile, params string[] resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (resource == null) throw new ArgumentNullException("resource");
            #endregion

            string resourceCombined = FileUtils.PathCombine(resource);
            string path;
            if (_isPortable)
            {
                // Check in portable base directory
                path = FileUtils.PathCombine(_portableBase, "config", resourceCombined);
                path = Path.GetFullPath(path);
                if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path))) yield return path;
            }
            else
            {
                // Check in user profile
                try
                {
                    path = FileUtils.PathCombine(UserConfigDir, appName, resourceCombined);
                }
                    #region Error handling
                catch (ArgumentException ex)
                {
                    // Wrap exception to add context information
                    throw new IOException(string.Format(Resources.InvalidConfigDir, UserConfigDir) + "\n" + ex.Message, ex);
                }
                #endregion

                path = Path.GetFullPath(path);
                if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path))) yield return path;

                // Check in system directories
                foreach (var dirPath in SystemConfigDirs.Split(Path.PathSeparator))
                {
                    try
                    {
                        path = FileUtils.PathCombine(dirPath, appName, resourceCombined);
                    }
                        #region Error handling
                    catch (ArgumentException ex)
                    {
                        // Wrap exception to add context information
                        throw new IOException(string.Format(Resources.InvalidConfigDir, dirPath) + "\n" + ex.Message, ex);
                    }
                    #endregion

                    path = Path.GetFullPath(path);
                    if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path))) yield return path;
                }
            }
        }

        /// <summary>
        /// Returns a path for storing a data resource (should not roam across different machines).
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="isFile"><see langword="true"/> if the last part of <paramref name="resource"/> refers to a file instead of a directory.</param>
        /// <param name="resource">The path elements (directory and/or file names) of the resource to be stored.</param>
        /// <returns>A fully qualified path to use to store the resource. Directories are guaranteed to already exist; files are not.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static string GetSaveDataPath(string appName, bool isFile, params string[] resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (resource == null) throw new ArgumentNullException("resource");
            #endregion

            string resourceCombined = FileUtils.PathCombine(resource);
            string path;
            try
            {
                path = _isPortable
                    ? FileUtils.PathCombine(_portableBase, "data", resourceCombined)
                    : FileUtils.PathCombine(UserDataDir, appName, resourceCombined);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception to add context information
                throw new IOException(string.Format(Resources.InvalidConfigDir, UserDataDir) + "\n" + ex.Message, ex);
            }
            #endregion

            // Ensure the directory part of the path exists
            path = Path.GetFullPath(path);
            string dirPath = isFile ? (Path.GetDirectoryName(path) ?? path) : path;
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            return path;
        }

        /// <summary>
        /// Returns a list of paths for loading a data resource (should not roam across different machines).
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="isFile"><see langword="true"/> if the last part of <paramref name="resource"/> refers to a file instead of a directory.</param>
        /// <param name="resource">The path elements (directory and/or file names) of the resource to be loaded.</param>
        /// <returns>
        /// A list of fully qualified paths to use to load the resource sorted by decreasing importance.
        /// This list will always reflect the current state in the filesystem and can not be modified! It may be empty.
        /// </returns>
        public static IEnumerable<string> GetLoadDataPaths(string appName, bool isFile, params string[] resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (resource == null) throw new ArgumentNullException("resource");
            #endregion

            string resourceCombined = FileUtils.PathCombine(resource);
            string path;
            if (_isPortable)
            {
                // Check in portable base directory
                path = FileUtils.PathCombine(_portableBase, "data", resourceCombined);path = Path.GetFullPath(path);
                path = Path.GetFullPath(path);
                if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path))) yield return path;
            }
            else
            {
                // Check in user profile
                try
                {
                    path = FileUtils.PathCombine(UserDataDir, appName, resourceCombined);
                }
                    #region Error handling
                catch (ArgumentException ex)
                {
                    // Wrap exception to add context information
                    throw new IOException(string.Format(Resources.InvalidConfigDir, UserDataDir) + "\n" + ex.Message, ex);
                }
                #endregion

                path = Path.GetFullPath(path);
                if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path))) yield return path;

                // Check in system directories
                foreach (var dirPath in SystemDataDirs.Split(Path.PathSeparator))
                {
                    try
                    {
                        path = FileUtils.PathCombine(dirPath, appName, resourceCombined);
                    }
                        #region Error handling
                    catch (ArgumentException ex)
                    {
                        // Wrap exception to add context information
                        throw new IOException(string.Format(Resources.InvalidConfigDir, dirPath) + "\n" + ex.Message, ex);
                    }
                    #endregion

                    path = Path.GetFullPath(path);
                    if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path))) yield return path;
                }
            }
        }
        #endregion

        #region Directories
        /// <summary>
        /// Returns a path for a cache directory (should not roam across different machines).
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="resource">The directory name of the resource to be stored.</param>
        /// <returns>A fully qualified directory path. The directory is guaranteed to already exist.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static string GetCacheDirPath(string appName, params string[] resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (resource == null) throw new ArgumentNullException("resource");
            #endregion

            string resourceCombined = FileUtils.PathCombine(resource);
            string path;
            try
            {
                path = _isPortable
                    ? FileUtils.PathCombine(_portableBase, "cache", resourceCombined)
                    : FileUtils.PathCombine(CacheDir, appName, resourceCombined);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception to add context information
                throw new IOException(string.Format(Resources.InvalidConfigDir, CacheDir) + "\n" + ex.Message, ex);
            }
            #endregion

            // Ensure the directory exists
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.GetFullPath(path);
            return path;
        }

        /// <summary>
        /// Returns a path for a directory that can safley be used for desktop integration. It ignores <see cref="IsPortable"/>.
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path.</param>
        /// <param name="systemWide"><see langword="true"/> if the directory should be system-wide and machine-specific instead of roaming with the user profile.</param>
        /// <param name="resource">The directory name of the resource to be stored.</param>
        /// <returns>A fully qualified directory path. The directory is guaranteed to already exist.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <remarks>If a new directory is created with <paramref name="systemWide"/> set to <see langword="true"/> on Windows, ACLs are set to deny write access for non-Administrator users.</remarks>
        public static string GetIntegrationDirPath(string appName, bool systemWide, params string[] resource)
        {
            string resourceCombined = FileUtils.PathCombine(resource);
            string path = FileUtils.PathCombine(
                Environment.GetFolderPath(systemWide ? Environment.SpecialFolder.CommonApplicationData : Environment.SpecialFolder.ApplicationData),
                appName, resourceCombined);

            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                if (WindowsUtils.IsWindowsNT && systemWide)
                {
                    // Set ACLs for new directory to: Admins/System = Full access, Users/Everyone = Read+Execute
                    var security = new DirectorySecurity();
                    security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.ReadAndExecute, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-1-0" /*Everyone*/), FileSystemRights.ReadAndExecute, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    directory.Create(security);
                }
                else directory.Create();
            }

            path = Path.GetFullPath(path);
            return path;
        }
        #endregion
    }
}
