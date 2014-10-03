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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Utility class for getting Windows icon files. Provides persistent local paths.
    /// </summary>
    public static class IconProvider
    {
        /// <summary>How long to keep reusing existing icons before redeploying them.</summary>
        private static readonly TimeSpan _freshness = new TimeSpan(0, 20, 0); // 20 minutes

        /// <summary>
        /// Retrieves a Windows icon via the <see cref="IIconCache"/> and stores a permanent copy of it.
        /// </summary>
        /// <param name="icon">The icon to retrieve.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply the configuration machine-wide instead of just for the current user.</param>
        /// <returns>The path to the icon file.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        public static string GetIconPath(Icon icon, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (icon == null) throw new ArgumentNullException("icon");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string iconDirPath = Locations.GetIntegrationDirPath("0install.net", machineWide, "desktop-integration", "icons");
            string iconFilePath = Path.Combine(iconDirPath, icon.Href.ToString().Hash(SHA256.Create()) + ".ico");

            // Return an existing icon or get a new one from the cache
            if (!File.Exists(iconFilePath) || (DateTime.UtcNow - File.GetLastWriteTimeUtc(iconFilePath) > _freshness))
                File.Copy(IconCacheProvider.GetInstance().GetIcon(icon.Href, handler), iconFilePath, overwrite: true);
            return iconFilePath;
        }
    }
}
