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
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Capabilities.AutoPlay"/> and <see cref="AccessPoints.AutoPlay"/> on Windows systems.
    /// </summary>
    public static class AutoPlay
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key for storing AutoPlay handlers.</summary>
        public const string RegKeyHandlers = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers";

        /// <summary>The HKCU/HKLM registry key for storing AutoPlay handler associations.</summary>
        public const string RegKeyAssocs = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\EventHandlers";

        /// <summary>The HKCU registry key for storing user-selected AutoPlay handlers.</summary>
        public const string RegKeyUserAssocs = @"SOFTWAREy\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\UserChosenExecuteHandlers";

        /// <summary>The registry value name for storing the description of the AutoPlay action.</summary>
        public const string RegValueDescription = "Action";

        /// <summary>The registry value name for storing the name of the application providing the AutoPlay action.</summary>
        public const string RegValueProvider = "Provider";

        /// <summary>The registry value name for storing the programatic identifier to invoke.</summary>
        public const string RegValueProgID = "InvokeProgID";

        /// <summary>The registry value name for storing the name of the verb to invoke.</summary>
        public const string RegValueVerb = "InvokeVerb";
        #endregion

        #region Register
        /// <summary>
        /// Adds an AutoPlay handler registration to the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="autoPlay">The AutoPlay handler information to be applied.</param>
        /// <param name="setDefault">Indicates that the handler should become the default handler for all <see cref="Capabilities.AutoPlay.Events"/>.</param>
        /// <param name="systemWide">Register the handler system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Register(InterfaceFeed target, Capabilities.AutoPlay autoPlay, bool setDefault, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (autoPlay == null) throw new ArgumentNullException("autoPlay");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implement
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Removes an AutoPlay handler registration from the current Windows system.
        /// </summary>
        /// <param name="autoPlay">The AutoPlay handler information to be removed.</param>
        /// <param name="systemWide">Remove the handler system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Unregister(Capabilities.AutoPlay autoPlay, bool systemWide)
        {
            #region Sanity checks
            if (autoPlay == null) throw new ArgumentNullException("autoPlay");
            #endregion

            // ToDo: Implement
        }
        #endregion
    }
}
