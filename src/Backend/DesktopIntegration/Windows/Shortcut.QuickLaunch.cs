/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Linq;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.DesktopIntegration.Windows
{
    public static partial class Shortcut
    {
        /// <summary>
        /// Creates a new Windows shortcut in the quick launch bar.
        /// </summary>
        /// <param name="quickLaunch">Information about the shortcut to be created.</param>
        /// <param name="target">The target the shortcut shall point to.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        public static void Create(QuickLaunch quickLaunch, FeedTarget target, ITaskHandler handler)
        {
            #region Sanity checks
            if (quickLaunch == null) throw new ArgumentNullException("quickLaunch");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string filePath = GetQuickLaunchPath(quickLaunch.Name);
            Create(filePath, target, quickLaunch.Command, handler);
        }

        /// <summary>
        /// Removes a Windows shortcut from the quick launch bar.
        /// </summary>
        /// <param name="quickLaunch">Information about the shortcut to be removed.</param>
        public static void Remove(QuickLaunch quickLaunch)
        {
            #region Sanity checks
            if (quickLaunch == null) throw new ArgumentNullException("quickLaunch");
            #endregion

            string filePath = GetQuickLaunchPath(quickLaunch.Name);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        private static string GetQuickLaunchPath(string name)
        {
            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, name));

            return new[] {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Internet Explorer", "Quick Launch", name + ".lnk"}.Aggregate(Path.Combine);
        }
    }
}
