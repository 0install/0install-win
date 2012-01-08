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
using Common.Tasks;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Capabilities.GamesExplorer"/> on Windows systems.
    /// </summary>
    public static class GamesExplorer
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key for registering applications in the Windows Games Explorer.</summary>
        public const string RegKeyMachineGames = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameUX\Games";
        #endregion

        #region Register
        /// <summary>
        /// Registers a game in the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="gamesExplorer">The game to be registered.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="systemWide">Register the game system-wide instead of just for the current user.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="gamesExplorer"/> is invalid.</exception>
        public static void Register(InterfaceFeed target, Capabilities.GamesExplorer gamesExplorer, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (gamesExplorer == null) throw new ArgumentNullException("gamesExplorer");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(gamesExplorer.ID)) throw new InvalidDataException("Missing ID");

            // ToDo: Implement
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a game in the current Windows system.
        /// </summary>
        /// <param name="gamesExplorer">The game to be unregistered.</param>
        /// <param name="systemWide">Unegister the game system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="gamesExplorer"/> is invalid.</exception>
        public static void Unregister(Capabilities.GamesExplorer gamesExplorer, bool systemWide)
        {
            #region Sanity checks
            if (gamesExplorer == null) throw new ArgumentNullException("gamesExplorer");
            #endregion

            if (string.IsNullOrEmpty(gamesExplorer.ID)) throw new InvalidDataException("Missing ID");

            // ToDo: Implement
        }
        #endregion
    }
}
