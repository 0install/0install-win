/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.IO;

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

        #region Directories

        #region Per-user
        /// <summary>
        /// The home directory of the current user.
        /// </summary>
        public static string HomeDir
        { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); } }

        /// <summary>
        /// The directory to store per-user settings that can roam across different machines.
        /// </summary>
        /// <remarks>On Windows this is <c>%appdata%</c>, on Linux it usually is <c>~/.config</c>.</remarks>
        public static string UserSettingsDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CONFIG_HOME", Path.Combine(HomeDir, ".config"));

                    case PlatformID.MacOSX:
                        // ToDo: Do whatever you should do on MacOS X
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// The directory to store per-user data files that can not roam across different machines.
        /// </summary>
        /// <remarks>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.local/share</c>.</remarks>
        public static string UserDataDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_DATA_HOME", Path.Combine(HomeDir, ".local/share"));

                    case PlatformID.MacOSX:
                        // ToDo: Do whatever you should do on MacOS X
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// The directory to store per-user (non-essential) cache data.
        /// </summary>
        /// <remarks>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.cache</c>.</remarks>
        public static string UserCacheDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CACHE_HOME", Path.Combine(HomeDir, ".cache"));

                    case PlatformID.MacOSX:
                        // ToDo: Do whatever you should do on MacOS X
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        #region System-wide
        /// <summary>
        /// The directories to store system-wide settings that can roam across different machines.
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/etc/xdg</c>.</remarks>
        public static string SystemSettingsDirs
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CONFIG_DIRS", "/etc/xdg");

                    case PlatformID.MacOSX:
                        // ToDo: Do whatever you should do on MacOS X
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// The directories to store system-wide data files that can not roam across different machines.
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/usr/local/share:/usr/share</c>.</remarks>
        public static string SystemDataDirs
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_DATA_DIRS", "/usr/local/share:/usr/share");

                    case PlatformID.MacOSX:
                        // ToDo: Do whatever you should do on MacOS X
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// The directories to store system-wide cache data.
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/var/cache</c>.</remarks>
        public static string SystemCacheDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CACHE_DIRS", "/var/cache");

                    case PlatformID.MacOSX:
                        // ToDo: Do whatever you should do on MacOS X
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        #endregion
    }
}
