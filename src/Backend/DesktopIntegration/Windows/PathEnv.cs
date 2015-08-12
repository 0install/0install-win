/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Manages the PATH environment variable.
    /// </summary>
    public static class PathEnv
    {
        /// <summary>
        /// Adds a directory to the search PATH.
        /// </summary>
        /// <param name="directory">The directory to add to the search PATH.</param>
        /// <param name="machineWide"><c>true</c> to use the machine-wide PATH variable; <c>false</c> for the per-user variant.</param>
        public static void AddDir([NotNull] string directory, bool machineWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException(nameof(directory));
            #endregion

            var currentPath = Get(machineWide);
            if (!currentPath.Contains(directory)) Set(currentPath.Append(directory), machineWide);
        }

        /// <summary>
        /// Removes a directory from the search PATH.
        /// </summary>
        /// <param name="directory">The directory to remove from the search PATH.</param>
        /// <param name="machineWide"><c>true</c> to use the machine-wide PATH variable; <c>false</c> for the per-user variant.</param>
        public static void RemoveDir([NotNull] string directory, bool machineWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException(nameof(directory));
            #endregion

            var currentPath = Get(machineWide);
            Set(currentPath.Except(directory).ToArray(), machineWide);
        }

        /// <summary>
        /// Returns the current search PATH.
        /// </summary>
        /// <param name="machineWide"><c>true</c> to use the machine-wide PATH variable; <c>false</c> for the per-user variant.</param>
        /// <returns>The individual directories listed in the search path.</returns>
        [NotNull]
        public static string[] Get(bool machineWide)
        {
            string value = Environment.GetEnvironmentVariable(
                variable: "Path",
                target: machineWide ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User);
            return string.IsNullOrEmpty(value) ? new string[0] : value.Split(Path.PathSeparator);
        }

        /// <summary>
        /// Sets the current search PATH.
        /// </summary>
        /// <param name="directories">The individual directories to list in the search PATH.</param>
        /// <param name="machineWide"><c>true</c> to use the machine-wide PATH variable; <c>false</c> for the per-user variant.</param>
        public static void Set([NotNull, ItemNotNull] string[] directories, bool machineWide)
        {
            #region Sanity checks
            if (directories == null) throw new ArgumentNullException(nameof(directories));
            #endregion

            Environment.SetEnvironmentVariable(
                variable: "Path",
                value: StringUtils.Join(Path.PathSeparator.ToString(), directories),
                target: machineWide ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User);
            WindowsUtils.NotifyEnvironmentChanged();
        }
    }
}