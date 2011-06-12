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

using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Capabilities.AppRegistration"/> on Windows systems.
    /// </summary>
    public static class AppRegistration
    {
        #region Constants
        /// <summary>The HKLM registry key for registering applications as candidates for default programs.</summary>
        public const string RegKeyMachineRegisteredApplications = @"SOFTWARE\RegisteredApplications";

        /// <summary>The registry value name for the application name.</summary>
        public const string RegValueAppName = "ApplicationName";

        /// <summary>The registry value name for the application description.</summary>
        public const string RegValueAppDescription = "ApplicationDescription";

        /// <summary>The registry value name for the application icon.</summary>
        public const string RegValueAppIcon = "ApplicationIcon";

        /// <summary>The registry subkey containing <see cref="Capabilities.FileType"/> references.</summary>
        public const string RegSubKeyFileAssocs = "FileAssociations";

        /// <summary>The registry subkey containing <see cref="Capabilities.UrlProtocol"/> references.</summary>
        public const string RegSubKeyUrlAssocs = "URLAssociations";

        /// <summary>The registry subkey containing <see cref="Capabilities.DefaultProgram"/> references.</summary>
        public const string RegSubKeyStartMenu = "StartMenu";
        #endregion
    }
}
