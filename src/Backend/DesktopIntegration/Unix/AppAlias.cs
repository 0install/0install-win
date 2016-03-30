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
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.DesktopIntegration.Unix
{
    /// <summary>
    /// Contains control logic for applying <see cref="AccessPoints.AppAlias"/> on Unix systems.
    /// </summary>
    public static class AppAlias
    {
        #region Create
        /// <summary>
        /// Creates an application alias in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="command">The command within <paramref name="target"/> the alias shall point to; can be <c>null</c>.</param>
        /// <param name="aliasName">The name of the alias to be created.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Create the alias machine-wide instead of just for the current user.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        public static void Create(FeedTarget target, [CanBeNull] string command, [NotNull] string aliasName, [NotNull] ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException("aliasName");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(aliasName) || aliasName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, aliasName));

            // TODO: Find directory in search PATH

            // TODO: Write file
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes an application alias from the current system.
        /// </summary>
        /// <param name="aliasName">The name of the alias to be removed.</param>
        /// <param name="machineWide">The alias was created machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem is not permitted.</exception>
        public static void Remove(string aliasName, bool machineWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException("aliasName");
            #endregion

            // TODO: Find directory in search PATH

            // TODO: Delete file
        }
        #endregion
    }
}
