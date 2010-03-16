using System;
using System.IO;

namespace Common.Storage
{
    /// <summary>
    /// Provides easy access to platform-specific common directories for storing settings and application data.
    /// </summary>
    public static class Locations
    {
        #region Per user
        /// <summary>
        /// Returns the directory to store per-user settings that can roam across different machines.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the path.</param>
        /// <returns>On Windows this is <c>%appdata%</c>, on Linux it usually is <c>~/.config</c>.</returns>
        public static string GetUserSettingsDir(string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            // Mono will automatically use XDG specification on non-Windows platforms
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        }

        /// <summary>
        /// Returns the directory to store per-user data files that can not roam across different machines.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the path.</param>
        /// <returns>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.local/share</c>.</returns>
        public static string GetUserDataDir(string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            // Mono will automatically use XDG specification on non-Windows platforms
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
        }

        /// <summary>
        /// Returns the directory to store per-user (non-essential) cache data.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the path.</param>
        /// <returns>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.cache</c>.</returns>
        public static string GetUserCacheDir(string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                default:
                    // Try to use XDG specification
                    string cacheDir = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");

                    // Fall back to default directory in case of failure
                    if (string.IsNullOrEmpty(cacheDir))
                        cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".cache");

                    return Path.Combine(cacheDir, appName);
            }
        }
        #endregion

        #region System-wide
        /// <summary>
        /// Returns the directory to store system-wide settings that can roam across different machines.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the path.</param>
        /// <returns>On Windows this is <c>CommonApplicationData</c>, on Linux it is <c>/etc/xdg</c>.</returns>
        public static string GetSystemSettingsDir(string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), appName);
                case PlatformID.Unix:
                    return Path.Combine("/etc/xdg", appName);
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // Just write to filesystem root
                    return Path.Combine("/config", appName);
            }
        }

        /// <summary>
        /// Returns the directory to store system-wide data files that can not roam across different machines.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the path.</param>
        /// <returns>On Windows this is <c>CommonApplicationData</c>, on Linux it is <c>/usr/local/share</c>.</returns>
        public static string GetSystemDataDir(string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), appName);
                case PlatformID.Unix:
                    return Path.Combine("/usr/local/share", appName);
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // Just write to filesystem root
                    return Path.Combine("/data", appName);
            }
        }

        /// <summary>
        /// Returns the directory to store system-wide cache data.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the path.</param>
        /// <returns>On Windows this is <c>CommonApplicationData</c>, on Linux it is <c>/var/cache</c>.</returns>
        public static string GetSystemCacheDir(string appName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            #endregion

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), appName);
                case PlatformID.Unix:
                    return Path.Combine("/var/cache", appName);
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // Just write to filesystem root
                    return Path.Combine("/cache", appName);
            }
        }
        #endregion

        #region Find
        /// <summary>
        /// Searches all directories for storing settings that can roam across different machines for a specific file.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the directory paths.</param>
        /// <param name="fileName">The name of the file to look for.</param>
        /// <returns>The full path to the file if it was found; <see langword="null"/> otherwise.</returns>
        public static string FindSettingsFile(string appName, string fileName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            #endregion

            // Check in user profile
            string path = Path.Combine(GetUserSettingsDir(appName), fileName);
            if (File.Exists(path)) return path;

            // Check in XDG locations
            string xdgVariable = Environment.GetEnvironmentVariable("XDG_CONFIG_DIRS");
            if (!string.IsNullOrEmpty(xdgVariable))
            {
                foreach (string directory in xdgVariable.Split(':'))
                {
                    path = Path.Combine(directory, fileName);
                    if (File.Exists(path)) return path;
                }
            }

            // Check system-wide
            path = Path.Combine(GetSystemSettingsDir(appName), fileName);
            if (File.Exists(path)) return path;

            return null;
        }

        /// <summary>
        /// Searches all directories for storing data files that can not roam across different machines for a specific file.
        /// </summary>
        /// <param name="appName">The name of the application to add to the end of the directory paths.</param>
        /// <param name="fileName">The name of the file to look for.</param>
        /// <returns>The full path to the file if it was found; <see langword="null"/> otherwise.</returns>
        public static string FindDataFile(string appName, string fileName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            #endregion

            // Check in user profile
            string path = Path.Combine(GetUserDataDir(appName), fileName);
            if (File.Exists(path)) return path;

            // Check in XDG locations
            string xdgVariable = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
            if (!string.IsNullOrEmpty(xdgVariable))
            {
                foreach (string directory in xdgVariable.Split(':'))
                {
                    path = Path.Combine(directory, fileName);
                    if (File.Exists(path)) return path;
                }
            }

            // Check system-wide
            path = Path.Combine(GetSystemDataDir(appName), fileName);
            if (File.Exists(path)) return path;

            return null;
        }
        #endregion
    }
}
