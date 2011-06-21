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
    /// Contains control logic for applying <see cref="Capabilities.DefaultProgram"/> and <see cref="AccessPoints.DefaultProgram"/> on Windows systems.
    /// </summary>
    public static class DefaultProgram
    {
        #region Constants
        /// <summary>The HKLM registry key for registering applications as clients for specific services.</summary>
        public const string RegKeyMachineClients = @"SOFTWARE\Clients";

        /// <summary>The registry value name for localized name storage.</summary>
        public const string RegValueLocalizedName = "LocalizedString";

        /// <summary>The name of the registry subkeys containing information about application installation commands and status.</summary>
        public const string RegSubKeyInstallInfo = "InstallInfo";

        /// <summary>The registry value name below <see cref="RegSubKeyInstallInfo"/> for the command to set an application as the default program.</summary>
        public const string RegValueReinstallCommand = "ReinstallCommand";

        /// <summary>The registry value name below <see cref="RegSubKeyInstallInfo"/> for the command to create icons/shortcuts to the application.</summary>
        public const string RegValueShowIconsCommand = "ShowIconsCommand";

        /// <summary>The registry value name below <see cref="RegSubKeyInstallInfo"/> for the command to remove icons/shortcuts to the application.</summary>
        public const string RegValueHideIconsCommand = "HideIconsCommand";

        /// <summary>The registry value name below <see cref="RegSubKeyInstallInfo"/> for storing whether the application's icons are currently visible.</summary>
        public const string RegValueIconsVisible = "IconsVisible";
        #endregion

        #region Register
        /// <summary>
        /// Registers an application as a candidate for a default program for some service in the current Windows system. This can only be applied system-wide, not per user.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="defaultProgram">The default program information to be registered.</param>
        /// <param name="setDefault">Indicates that the program should be set as the current default for the service it provides.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Register(InterfaceFeed target, Capabilities.DefaultProgram defaultProgram, bool setDefault, ITaskHandler handler)
        {
            #region Sanity checks
            if (defaultProgram == null) throw new ArgumentNullException("defaultProgram");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // ToDo: Implement
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters an application as a candidate for a default program in the current Windows system. This can only be applied system-wide, not per user.
        /// </summary>
        /// <param name="defaultProgram">The default program information to be removed.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Unregister(Capabilities.DefaultProgram defaultProgram)
        {
            #region Sanity checks
            if (defaultProgram == null) throw new ArgumentNullException("defaultProgram");
            #endregion

            // ToDo: Implement
        }
        #endregion
    }
}
