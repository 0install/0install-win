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
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Model;
using AccessPoints = ZeroInstall.DesktopIntegration.Model;

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
        /// <param name="interfaceID">The interface ID of the application being integrated.</param>
        /// <param name="feed">The of the application to get additional information (e.g. icons) from.</param>
        /// <param name="appPath">The access point to be applied.</param>
        /// <param name="global">Flag indicating to apply the configuration system-wide instead of just for the current user.</param>
        public static void Apply(string interfaceID, Feed feed, AccessPoints.AppPath appPath, bool global)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            if (appPath == null) throw new ArgumentNullException("appPath");
            #endregion

            // Only Windows 7 and newer support per-user AppPaths
            if (!global && !WindowsUtils.IsWindows7)
            {
                // ToDo: Fall back to classic PATH
                return;
            }

            var hive = global ? Registry.LocalMachine : Registry.CurrentUser;
            using (var appPathsKey = hive.OpenSubKey(RegKeyAppPaths, true))
            {
                using (var exeKey = appPathsKey.CreateSubKey(appPath.Name))
                {
                    string stubPath = FileUtils.PathCombine(
                        Environment.GetFolderPath(global ? Environment.SpecialFolder.CommonApplicationData : Environment.SpecialFolder.LocalApplicationData),
                        "0install.net", "aliases", appPath.Name + ".exe");
                    
                    // ToDo: Get icon
                    StubBuilder.BuildRunStub(stubPath, interfaceID, feed.Name, null, appPath.Command, !feed.NeedsTerminal);
                    exeKey.SetValue("", stubPath);
                }
            }
        }
        #endregion
    }
}
