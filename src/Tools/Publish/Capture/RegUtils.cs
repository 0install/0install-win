using System;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace ZeroInstall.Publish.Capture
{
    /// <summary>
    /// Provides convenience helper methods for registry access.
    /// </summary>
    internal static class RegUtils
    {
        /// <summary>
        /// Retrieves the names of all values within a specific subkey of a registry root.
        /// </summary>
        /// <param name="root">The root key to look within.</param>
        /// <param name="key">The path of the subkey below <paramref name="root"/>.</param>
        /// <returns>A list of value names; an empty array if the key does not exist.</returns>
        [NotNull, ItemNotNull]
        public static string[] GetValueNames([NotNull] RegistryKey root, [NotNull] string key)
        {
            #region Sanity checks
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            #endregion

            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey?.GetValueNames() ?? new string[0];
        }

        /// <summary>
        /// Retrieves the names of all subkeys within a specific subkey of a registry root.
        /// </summary>
        /// <param name="root">The root key to look within.</param>
        /// <param name="key">The path of the subkey below <paramref name="root"/>.</param>
        /// <returns>A list of key names; an empty array if the key does not exist.</returns>
        [NotNull, ItemNotNull]
        public static string[] GetSubKeyNames([NotNull] RegistryKey root, [NotNull] string key)
        {
            #region Sanity checks
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            #endregion

            using (var contextMenuExtendedKey = root.OpenSubKey(key))
                return contextMenuExtendedKey?.GetSubKeyNames() ?? new string[0];
        }
    }
}
