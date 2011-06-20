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
using System.Collections.Generic;
using System.IO;
using System.Net;
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using Microsoft.Win32;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="AccessPoints.AppAlias"/> on Windows systems.
    /// </summary>
    public static class AppAlias
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key for storing application lookup paths.</summary>
        public const string RegKeyAppPaths = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
        #endregion

        #region Apply
        /// <summary>
        /// Applies an <see cref="AccessPoints.AppAlias"/> to the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="appAlias">The access point to be applied.</param>
        /// <param name="systemWide">Create the alias system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Apply(InterfaceFeed target, AccessPoints.AppAlias appAlias, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appAlias == null) throw new ArgumentNullException("appAlias");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration", "aliases");
            string stubFilePath = Path.Combine(stubDirPath, appAlias.Name + ".exe");
            StubProvider.BuildRunStub(stubFilePath, target, appAlias.Command, handler);

            var variableTarget = systemWide ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
            string existingValue = Environment.GetEnvironmentVariable("PATH", variableTarget);
            if (existingValue == null || !existingValue.Contains(stubDirPath))
                Environment.SetEnvironmentVariable("PATH", existingValue + Path.PathSeparator + stubDirPath, variableTarget);

            // Only Windows 7 and newer support per-user AppPaths
            if (systemWide || WindowsUtils.IsWindows7)
            {
                var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
                using (var appPathsKey = hive.CreateSubKey(RegKeyAppPaths))
                {
                    using (var exeKey = appPathsKey.CreateSubKey(appAlias.Name + ".exe"))
                        exeKey.SetValue("", stubFilePath);
                }
            }
        }
        #endregion

        #region Unapply
        /// <summary>
        /// Removes an <see cref="AccessPoints.AppAlias"/> from the current Windows system. 
        /// </summary>
        /// <param name="appAlias">The access point to be removed.</param>
        /// <param name="systemWide">The alias was created system-wide instead of just for the current user.</param>
        public static void Unapply(AccessPoints.AppAlias appAlias, bool systemWide)
        {
            #region Sanity checks
            if (appAlias == null) throw new ArgumentNullException("appAlias");
            #endregion

            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration", "aliases");
            string stubFilePath = Path.Combine(stubDirPath, appAlias.Name + ".exe");

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var appPathsKey = hive.OpenSubKey(RegKeyAppPaths, true))
            {
                if (appPathsKey != null)
                {
                    if (((ICollection<string>)appPathsKey.GetSubKeyNames()).Contains(appAlias.Name + ".exe"))
                        appPathsKey.DeleteSubKey(appAlias.Name + ".exe");
                }
            }

            if (File.Exists(stubFilePath)) File.Delete(stubFilePath);
        }
        #endregion
    }
}
