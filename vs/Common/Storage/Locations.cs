using System;
using System.IO;

namespace Common.Storage
{
    /// <summary>
    /// Provides easy access to platform-specific common directories for storing settings and application data.
    /// </summary>
    public static class Locations
    {
        /// <summary>
        /// Returns the directory to store per-user settings that can roam across different machines.
        /// </summary>
        /// <param name="appName">The name of application to add to the end of the path.</param>
        public static string GetUserSettingsDir(string appName)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
                case PlatformID.Unix:
                    // ToDo: Try to use freedesktop.org basedir and then fallback to dot-directory
                    throw new NotImplementedException();
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // ToDo: Just write to user-directory
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the directory to store per-user settings that can not roam across different machines.
        /// </summary>
        /// <param name="appName">The name of application to add to the end of the path.</param>
        public static string GetUserLocalSettingsDir(string appName)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
                case PlatformID.Unix:
                    // ToDo: Try to use freedesktop.org basedir and then fallback to dot-directory
                    throw new NotImplementedException();
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // ToDo: Just write to user-directory
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the directory to store system-wide settings.
        /// </summary>
        /// <param name="appName">The name of application to add to the end of the path.</param>
        public static string GetSystemSettingsDir(string appName)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
                case PlatformID.Unix:
                    // ToDo: Try to use freedesktop.org basedir and then fallback to /etc-directory
                    throw new NotImplementedException();
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // ToDo: Just write to filesystem-root
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the directory to store per-user cache data (will not roam across different machines).
        /// </summary>
        /// <param name="appName">The name of application to add to the end of the path.</param>
        public static string GetUserCacheDir(string appName)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
                case PlatformID.Unix:
                    // ToDo: Try to use freedesktop.org basedir and then fallback to ~/.cache-directory
                    throw new NotImplementedException();
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // ToDo: Just write to user-directory
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the directory to store system-wide cache data.
        /// </summary>
        /// <param name="appName">The name of application to add to the end of the path.</param>
        public static string GetSystemCacheDir(string appName)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
                case PlatformID.Unix:
                    // ToDo: Try to use freedesktop.org basedir and then fallback to /var/cache-directory
                    throw new NotImplementedException();
                case PlatformID.MacOSX:
                    // ToDo: Do whatever you should do on MacOS X
                    throw new NotImplementedException();
                default:
                    // ToDo: Just write to filesystem-root
                    throw new NotImplementedException();
            }
        }
    }
}
