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

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="ZeroInstall.DesktopIntegration.Model.AppPath"/> on Windows systems.
    /// </summary>
    public static class AppPath
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key for storing application lookup paths.</summary>
        public const string RegKeyAppPath = @"Software\Microsoft\Windows\CurrentVersion\App Paths";

        /// <summary>The HKCR registry key for storing additional application launching behavior.</summary>
        public const string RegKeyClassesApplications = @"Applications";
        #endregion
    }
}
