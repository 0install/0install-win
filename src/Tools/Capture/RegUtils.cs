using System;
using System.IO;
using System.Security;
using Microsoft.Win32;
using NanoByte.Common.Utils;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Provides convenience helper methods for registry access.
    /// </summary>
    internal static class RegUtils
    {
        #region Registry
        /// <summary>
        /// Retrieves the names of all values within a specific subkey of a registry root.
        /// </summary>
        /// <param name="root">The root key to look within.</param>
        /// <param name="key">The path of the subkey below <paramref name="root"/>.</param>
        /// <returns>A list of value names; an empty array if the key does not exist.</returns>
        public static string[] GetValueNames(RegistryKey root, string key)
        {
            #region Sanity checks
            if (root == null) throw new ArgumentNullException("root");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            #endregion

            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey == null ? new string[0] : contextMenuExtendedKey.GetValueNames();
        }

        /// <summary>
        /// Retrieves the names of all subkeys within a specific subkey of a registry root.
        /// </summary>
        /// <param name="root">The root key to look within.</param>
        /// <param name="key">The path of the subkey below <paramref name="root"/>.</param>
        /// <returns>A list of key names; an empty array if the key does not exist.</returns>
        public static string[] GetSubKeyNames(RegistryKey root, string key)
        {
            #region Sanity checks
            if (root == null) throw new ArgumentNullException("root");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            #endregion

            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey == null ? new string[0] : contextMenuExtendedKey.GetSubKeyNames();
        }

        /// <summary>
        /// Opens a HKEY_LOCAL_MACHINE key in the registry for reading, first trying to find the 64-bit version of it, then falling back to the 32-bit version.
        /// </summary>
        /// <param name="keyPath">The path to the key below HKEY_LOCAL_MACHINE.</param>
        /// <param name="x64">Indicates whether a 64-bit key was opened.</param>
        /// <returns>The opened registry key or <see langword="null"/> if it could not found.</returns>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if access to the registry was not permitted.</exception>
        public static RegistryKey OpenHklmKey(string keyPath, out bool x64)
        {
            RegistryKey result;
            x64 = false;
            // TODO: Use Is64BitOperatingSystem and native APIs
            if (WindowsUtils.Is64BitProcess)
            {
                result = Registry.LocalMachine.OpenSubKey(@"WOW6432Node\" + keyPath);
                if (result == null) result = Registry.LocalMachine.OpenSubKey(keyPath);
                else x64 = true;
            }
            else result = Registry.LocalMachine.OpenSubKey(keyPath);

            return result;
        }
        #endregion
    }
}
