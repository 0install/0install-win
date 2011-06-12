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
using AccessPoints = ZeroInstall.DesktopIntegration.Model;

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
        #endregion

        // ToDo: Generate InstallInfo entries
    }
}
