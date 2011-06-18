/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common;
using Common.Tasks;
using Common.Utils;
using Microsoft.Win32;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="AccessPoints.AppPath"/> on Windows systems.
    /// </summary>
    public static class AppPath
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key for storing application lookup paths.</summary>
        public const string RegKeyAppPaths = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
        #endregion

        #region Apply
        /// <summary>
        /// Applies an <see cref="AccessPoints.AppPath"/> to the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="appPath">The access point to be applied.</param>
        /// <param name="systemWide">Apply the configuration system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Apply(InterfaceFeed target, AccessPoints.AppPath appPath, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appPath == null) throw new ArgumentNullException("appPath");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Only Windows 7 and newer support per-user AppPaths
            if (!systemWide && !WindowsUtils.IsWindows7)
            {
                // ToDo: Fall back to classic PATH
                return;
            }

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var appPathsKey = hive.OpenSubKey(RegKeyAppPaths, true))
            {
                using (var exeKey = appPathsKey.CreateSubKey(appPath.Name))
                {
                    string stubPath = FileUtils.PathCombine(
                        Environment.GetFolderPath(systemWide ? Environment.SpecialFolder.CommonApplicationData : Environment.SpecialFolder.LocalApplicationData),
                        "0install.net", "aliases", appPath.Name + ".exe");
                    
                    // ToDo: Get icon
                    StubProvider.BuildRunStub(stubPath, target, appPath.Command, handler);
                    exeKey.SetValue("", stubPath);
                }
            }
        }
        #endregion
    }
}
