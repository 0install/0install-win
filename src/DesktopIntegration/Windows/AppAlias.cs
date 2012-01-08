﻿/*
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
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.DesktopIntegration.Properties;

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

        #region Create
        /// <summary>
        /// Creates an application alias in the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="command">The command within <paramref name="target"/> the alias shall point to; may be <see langword="null"/>.</param>
        /// <param name="aliasName">The name of the alias to be created.</param>
        /// <param name="systemWide">Create the alias system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Create(InterfaceFeed target, string command, string aliasName, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException("aliasName");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(aliasName) || aliasName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, aliasName));

            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration", "aliases");
            string stubFilePath = Path.Combine(stubDirPath, aliasName + ".exe");

            StubBuilder.BuildRunStub(stubFilePath, target, command, handler);
            AddToPath(stubDirPath, systemWide);
            AddToAppPaths(aliasName + ".exe", stubFilePath, systemWide);
        }

        /// <summary>
        /// Adds a directory to the system's search path.
        /// </summary>
        /// <param name="directory">The directory to add to the search path.</param>
        /// <param name="systemWide"><see langword="true"/> to use the system-wide path variable; <see langword="false"/> for the per-user variant.</param>
        private static void AddToPath(string directory, bool systemWide)
        {
            var variableTarget = systemWide ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
            string existingValue = Environment.GetEnvironmentVariable("PATH", variableTarget);
            if (existingValue == null || !existingValue.Contains(directory))
            {
                Environment.SetEnvironmentVariable("PATH", existingValue + Path.PathSeparator + directory, variableTarget);
                WindowsUtils.NotifyEnvironmentChanged();
            }
        }

        /// <summary>
        /// Adds an EXE to the AppPath registry key.
        /// </summary>
        /// <param name="exeName">The name of the EXE file to add (including the file ending).</param>
        /// <param name="exePath">The full path to the EXE file.</param>
        /// <param name="systemWide"><see langword="true"/> to use the system-wide registry key; <see langword="false"/> for the per-user variant.</param>
        private static void AddToAppPaths(string exeName, string exePath, bool systemWide)
        {
            // Only Windows 7 and newer support per-user AppPaths
            if (!systemWide && !WindowsUtils.IsWindows7) return;

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var appPathsKey = hive.CreateSubKey(RegKeyAppPaths))
            using (var exeKey = appPathsKey.CreateSubKey(exeName))
                exeKey.SetValue("", exePath);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes an application alias from the current Windows system. 
        /// </summary>
        /// <param name="aliasName">The name of the alias to be removed.</param>
        /// <param name="systemWide">The alias was created system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Remove(string aliasName, bool systemWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException("aliasName");
            #endregion

            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration", "aliases");
            string stubFilePath = Path.Combine(stubDirPath, aliasName + ".exe");

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var appPathsKey = hive.OpenSubKey(RegKeyAppPaths, true))
            {
                if (appPathsKey != null)
                {
                    if (((ICollection<string>)appPathsKey.GetSubKeyNames()).Contains(aliasName + ".exe"))
                        appPathsKey.DeleteSubKey(aliasName + ".exe");
                }
            }

            if (File.Exists(stubFilePath)) File.Delete(stubFilePath);
        }
        #endregion
    }
}
