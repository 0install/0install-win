/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using NanoByte.Common.Properties;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Storage
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
        /// <summary>
        /// The directory the application binaries are located in without a trailing directory separator charachter.
        /// </summary>
        public static readonly string InstallBase = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        /// <summary>
        /// The name of the flag file whose existence determines whether <see cref="IsPortable"/> is set to <see langword="true"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public const string PortableFlagName = "_portable";

        private static bool _isPortable = File.Exists(Path.Combine(InstallBase, PortableFlagName));

        /// <summary>
        /// Indicates whether the application is currently operating in portable mode.
        /// </summary>
        /// <remarks>
        ///   <para>Portable mode is activated by placing a file named <see cref="PortableFlagName"/> in <see cref="InstallBase"/>.</para>
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
                return GetEnvironmentVariable("XDG_CONFIG_HOME", WindowsUtils.IsWindows
                    ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    : Path.Combine(HomeDir, ".config"));
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
                return GetEnvironmentVariable("XDG_DATA_HOME", WindowsUtils.IsWindows
                    ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    : Path.Combine(HomeDir, ".local/share"));
            }
        }

        /// <summary>
        /// The directory to store per-user non-essential data (should not roam across different machines).
        /// </summary>
        /// <remarks>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.cache</c>.</remarks>
        public static string UserCacheDir
        {
            get
            {
                return GetEnvironmentVariable("XDG_CACHE_HOME", WindowsUtils.IsWindows
                    ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    : Path.Combine(HomeDir, ".cache"));
            }
        }
        #endregion

        #region Machine-wide directories
        /// <summary>
        /// The directories to store machine-wide settings (can roam across different machines).
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/etc/xdg</c>.</remarks>
        public static string SystemConfigDirs
        {
            get
            {
                return GetEnvironmentVariable("XDG_CONFIG_DIRS", WindowsUtils.IsWindows
                    ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                    : "/etc/xdg");
            }
        }

        /// <summary>
        /// The directories to store machine-wide data files (should not roam across different machines).
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/usr/local/share:/usr/share</c>.</remarks>
        public static string SystemDataDirs
        {
            get
            {
                return GetEnvironmentVariable("XDG_DATA_DIRS", WindowsUtils.IsWindows
                    ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                    : "/usr/local/share" + Path.PathSeparator + "/usr/share");
            }
        }

        /// <summary>
        /// The directory to store machine-wide non-essential data.
        /// </summary>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it is <c>/var/cache</c>.</remarks>
        public static string SystemCacheDir
        {
            get
            {
                return WindowsUtils.IsWindows
                    ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                    : "/var/cache";
            }
        }
        #endregion

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

            string resourceCombined = (resource.Length == 0) ? "" : resource.Aggregate(Path.Combine);
            string path;
            try
            {
                path = (_isPortable
                    ? new[] {_portableBase, "config", resourceCombined}
                    : new[] {UserConfigDir, appName, resourceCombined}).Aggregate(Path.Combine);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception to add context information
                throw new IOException(string.Format(Resources.InvalidConfigDir, UserConfigDir) + "\n" + ex.Message, ex);
            }
            #endregion

            // Ensure the directory part of the path exists
            string dirPath = isFile ? (Path.GetDirectoryName(path) ?? path) : path;
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            return Path.GetFullPath(path);
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

            string resourceCombined = (resource.Length == 0) ? "" : resource.Aggregate(Path.Combine);
            string path;
            if (_isPortable)
            {
                // Check in portable base directory
                path = new[] {_portableBase, "config", resourceCombined}.Aggregate(Path.Combine);
                if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path)))
                    yield return Path.GetFullPath(path);
            }
            else
            {
                // Check in user profile and system directories
                foreach (var dirPath in (UserConfigDir + Path.PathSeparator + SystemConfigDirs).Split(Path.PathSeparator))
                {
                    try
                    {
                        path = new[] {dirPath, appName, resourceCombined}.Aggregate(Path.Combine);
                    }
                        #region Error handling
                    catch (ArgumentException ex)
                    {
                        // Wrap exception to add context information
                        throw new IOException(string.Format(Resources.InvalidConfigDir, dirPath) + "\n" + ex.Message, ex);
                    }
                    #endregion

                    if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path)))
                        yield return Path.GetFullPath(path);
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

            string resourceCombined = (resource.Length == 0) ? "" : resource.Aggregate(Path.Combine);
            string path;
            try
            {
                path = (_isPortable
                    ? new[] {_portableBase, "data", resourceCombined}
                    : new[] {UserDataDir, appName, resourceCombined}).Aggregate(Path.Combine);
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception to add context information
                throw new IOException(string.Format(Resources.InvalidConfigDir, UserDataDir) + "\n" + ex.Message, ex);
            }
            #endregion

            // Ensure the directory part of the path exists
            string dirPath = isFile ? (Path.GetDirectoryName(path) ?? path) : path;
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            return Path.GetFullPath(path);
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

            string resourceCombined = (resource.Length == 0) ? "" : resource.Aggregate(Path.Combine);
            string path;
            if (_isPortable)
            {
                // Check in portable base directory
                path = new[] {_portableBase, "data", resourceCombined}.Aggregate(Path.Combine);
                if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path)))
                    yield return Path.GetFullPath(path);
            }
            else
            {
                // Check in user profile and system directories
                foreach (var dirPath in (UserDataDir + Path.PathSeparator + SystemDataDirs).Split(Path.PathSeparator))
                {
                    try
                    {
                        path = new[] {dirPath, appName, resourceCombined}.Aggregate(Path.Combine);
                    }
                        #region Error handling
                    catch (ArgumentException ex)
                    {
                        // Wrap exception to add context information
                        throw new IOException(string.Format(Resources.InvalidConfigDir, dirPath) + "\n" + ex.Message, ex);
                    }
                    #endregion

                    if ((isFile && File.Exists(path)) || (!isFile && Directory.Exists(path)))
                        yield return Path.GetFullPath(path);
                }
            }
        }
        #endregion

        #region Directories
        /// <summary>
        /// Returns a path for a cache directory (should not roam across different machines).
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="machineWide"><see langword="true"/> if the directory should be machine-wide.</param>
        /// <param name="resource">The directory name of the resource to be stored.</param>
        /// <returns>A fully qualified directory path. The directory is guaranteed to already exist.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static string GetCacheDirPath(string appName, bool machineWide, params string[] resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (resource == null) throw new ArgumentNullException("resource");
            #endregion

            string resourceCombined = (resource.Length == 0) ? "" : resource.Aggregate(Path.Combine);
            string appPath;
            try
            {
                if (machineWide)
                {
                    appPath = Path.Combine(SystemCacheDir, appName);
                    CreateSecureMachineWideDir(appPath);
                }
                else
                {
                    appPath = _isPortable
                        ? Path.Combine(_portableBase, "cache")
                        : Path.Combine(UserCacheDir, appName);
                }
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception to add context information
                throw new IOException(string.Format(Resources.InvalidConfigDir, UserCacheDir) + "\n" + ex.Message, ex);
            }
            #endregion

            string path = Path.Combine(appPath, resourceCombined);

            // Ensure the directory exists
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Returns a path for a directory that can safley be used for desktop integration. It ignores <see cref="IsPortable"/>.
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path.</param>
        /// <param name="machineWide"><see langword="true"/> if the directory should be machine-wide and machine-specific instead of roaming with the user profile.</param>
        /// <param name="resource">The directory name of the resource to be stored.</param>
        /// <returns>A fully qualified directory path. The directory is guaranteed to already exist.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <remarks>If a new directory is created with <paramref name="machineWide"/> set to <see langword="true"/> on Windows, ACLs are set to deny write access for non-Administrator users.</remarks>
        public static string GetIntegrationDirPath(string appName, bool machineWide, params string[] resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (resource == null) throw new ArgumentNullException("resource");
            #endregion

            string resourceCombined = (resource.Length == 0) ? "" : resource.Aggregate(Path.Combine);
            string appPath = Path.Combine(
                Environment.GetFolderPath(machineWide ? Environment.SpecialFolder.CommonApplicationData : Environment.SpecialFolder.ApplicationData),
                appName);
            if (machineWide) CreateSecureMachineWideDir(appPath);
            string path = Path.Combine(appPath, resourceCombined);

            // Ensure the directory exists
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return Path.GetFullPath(path);
        }
        #endregion

        #region ACL Security
        /// <summary>
        /// The name of the flag file whose existence indicates that a directory has been secured with ACLs to prevent non-admins from writing there.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public const string SecuredFlagName = "_secured";

        /// <summary>
        /// ACL that gives normal users read and execute access and admins and the the system full access.
        /// </summary>
        private static readonly DirectorySecurity _secureSharedAcl;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Locations()
        {
            if (WindowsUtils.IsWindowsNT)
            {
                _secureSharedAcl = new DirectorySecurity();
                _secureSharedAcl.SetOwner(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                _secureSharedAcl.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
                _secureSharedAcl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-1-0" /*Everyone*/), FileSystemRights.ReadAndExecute, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                _secureSharedAcl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.ReadAndExecute, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                _secureSharedAcl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                _secureSharedAcl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            }
        }

        /// <summary>
        /// Creates a directory with ACLs that block write-access for regular users.
        /// </summary>
        /// <exception cref="NotAdminException">Thrown if a directory does not exist yet and the user is not an administrator.</exception>
        private static void CreateSecureMachineWideDir(string path)
        {
            if (Directory.Exists(path)) return;

            if (WindowsUtils.IsWindowsNT)
            {
                if (!WindowsUtils.IsAdministrator) throw new NotAdminException();

                Directory.CreateDirectory(path, _secureSharedAcl);
                File.Create(Path.Combine(path, SecuredFlagName));
            }
            else Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Applies ACLs to an existing directory that block write-access for regular users. Does nothing if the directory does not exist.
        /// </summary>
        /// <exception cref="NotAdminException">Thrown if a directory exists and the user is not an administrator.</exception>
        public static void SecureExistingMachineWideDir(string path)
        {
            if (!Directory.Exists(path)) return;

            if (WindowsUtils.IsWindowsNT)
            {
                if (!WindowsUtils.IsAdministrator) throw new NotAdminException();

                var directory = new DirectoryInfo(path);
                directory.ResetAcl();
                directory.SetAccessControl(_secureSharedAcl);
                File.Create(Path.Combine(path, SecuredFlagName));
            }
        }
        #endregion
    }
}
