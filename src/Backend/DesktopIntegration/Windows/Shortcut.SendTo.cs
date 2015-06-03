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
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.DesktopIntegration.Windows
{
    public static partial class Shortcut
    {
        /// <summary>
        /// Creates a new Windows shortcut in the "Send to" menu.
        /// </summary>
        /// <param name="sendTo">Information about the shortcut to be created.</param>
        /// <param name="target">The target the shortcut shall point to.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        public static void Create(SendTo sendTo, FeedTarget target, ITaskHandler handler)
        {
            #region Sanity checks
            if (sendTo == null) throw new ArgumentNullException("sendTo");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string filePath = GetSendToPath(sendTo.Name);
            Create(filePath, target, sendTo.Command, handler, machineWide: false);
        }

        /// <summary>
        /// Removes a Windows shortcut from the "Send to" menu.
        /// </summary>
        /// <param name="sendTo">Information about the shortcut to be removed.</param>
        public static void Remove(SendTo sendTo)
        {
            #region Sanity checks
            if (sendTo == null) throw new ArgumentNullException("sendTo");
            #endregion

            string filePath = GetSendToPath(sendTo.Name);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        private static string GetSendToPath(string name)
        {
            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, name));

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), name + ".lnk");
        }
    }
}
