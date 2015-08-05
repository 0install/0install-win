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
        /// Creates a new Windows shortcut in the "Startup" menu.
        /// </summary>
        /// <param name="autoStart">Information about the shortcut to be created.</param>
        /// <param name="target">The target the shortcut shall point to.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Create the shortcut machine-wide instead of just for the current user.</param>
        public static void Create(AutoStart autoStart, FeedTarget target, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (autoStart == null) throw new ArgumentNullException("autoStart");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string filePath = GetStartupPath(autoStart.Name, machineWide);
            Create(filePath, target.GetRunStub(autoStart.Command, handler));
        }

        /// <summary>
        /// Removes a Windows shortcut from the "Startup" menu.
        /// </summary>
        /// <param name="autoStart">Information about the shortcut to be removed.</param>
        /// <param name="machineWide">The shortcut was created machine-wide instead of just for the current user.</param>
        public static void Remove(AutoStart autoStart, bool machineWide)
        {
            #region Sanity checks
            if (autoStart == null) throw new ArgumentNullException("autoStart");
            #endregion

            string filePath = GetStartupPath(autoStart.Name, machineWide);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        private static string GetStartupPath(string name, bool machineWide)
        {
            CheckName(name);

            const Environment.SpecialFolder commonStartup = (Environment.SpecialFolder)0x0018;
            string startupDir = Environment.GetFolderPath(machineWide ? commonStartup : Environment.SpecialFolder.Startup);
            return Path.Combine(startupDir, name + ".lnk");
        }
    }
}
