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
using System.Collections.Generic;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;

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
        /// Creates an application alias in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="command">The command within <paramref name="target"/> the alias shall point to; can be <see langword="null"/>.</param>
        /// <param name="aliasName">The name of the alias to be created.</param>
        /// <param name="machineWide">Create the alias machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        public static void Create(FeedTarget target, [CanBeNull] string command, [NotNull] string aliasName, bool machineWide, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException("aliasName");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(aliasName) || aliasName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, aliasName));

            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", machineWide, "desktop-integration", "aliases");
            string stubFilePath = Path.Combine(stubDirPath, aliasName + ".exe");

            target.BuildRunStub(stubFilePath, handler, needsTerminal: true, command: command);
            AddToPath(stubDirPath, machineWide);
            AddToAppPaths(aliasName + ".exe", stubFilePath, machineWide);
        }

        /// <summary>
        /// Adds a directory to the system's search path.
        /// </summary>
        /// <param name="directory">The directory to add to the search path.</param>
        /// <param name="machineWide"><see langword="true"/> to use the machine-wide path variable; <see langword="false"/> for the per-user variant.</param>
        private static void AddToPath(string directory, bool machineWide)
        {
            var variableTarget = machineWide ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
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
        /// <param name="machineWide"><see langword="true"/> to use the machine-wide registry key; <see langword="false"/> for the per-user variant.</param>
        private static void AddToAppPaths(string exeName, string exePath, bool machineWide)
        {
            // Only Windows 7 and newer support per-user AppPaths
            if (!machineWide && !WindowsUtils.IsWindows7) return;

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var appPathsKey = hive.CreateSubKey(RegKeyAppPaths))
            {
                if (appPathsKey == null) throw new IOException("Registry access failed.");
                using (var exeKey = appPathsKey.CreateSubKey(exeName))
                {
                    if (exeKey == null) throw new IOException("Registry access failed.");
                    exeKey.SetValue("", exePath);
                }
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes an application alias from the current system. 
        /// </summary>
        /// <param name="aliasName">The name of the alias to be removed.</param>
        /// <param name="machineWide">The alias was created machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        public static void Remove(string aliasName, bool machineWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException("aliasName");
            #endregion

            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", machineWide, "desktop-integration", "aliases");
            string stubFilePath = Path.Combine(stubDirPath, aliasName + ".exe");

            RemoveFromAppPaths(aliasName + ".exe", machineWide);

            if (File.Exists(stubFilePath)) File.Delete(stubFilePath);
        }

        /// <summary>
        /// Removes an EXE from the AppPath registry key.
        /// </summary>
        /// <param name="exeName">The name of the EXE file to add (including the file ending).</param>
        /// <param name="machineWide"><see langword="true"/> to use the machine-wide registry key; <see langword="false"/> for the per-user variant.</param>
        private static void RemoveFromAppPaths(string exeName, bool machineWide)
        {
            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var appPathsKey = hive.OpenSubKey(RegKeyAppPaths, writable: true))
            {
                if (appPathsKey != null)
                {
                    if (((ICollection<string>)appPathsKey.GetSubKeyNames()).Contains(exeName))
                        appPathsKey.DeleteSubKey(exeName);
                }
            }
        }
        #endregion
    }
}
