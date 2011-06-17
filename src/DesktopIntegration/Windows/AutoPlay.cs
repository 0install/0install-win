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
    }
}
