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
    /// Contains control logic for applying <see cref="Capabilities.ContextMenu"/> and <see cref="AccessPoints.ContextMenu"/> on Windows systems.
    /// </summary>
    public static class ContextMenu
    {
        #region Constants
        /// <summary>The HKCU registry key prefix for registering things for all files.</summary>
        public const string RegKeyClassesFilesPrefix = "*";

        /// <summary>The HKCU registry key prefix for registering things for all filesytem objects (files and directories).</summary>
        public const string RegKeyClassesAllPrefix = "AllFilesystemObjects";

        /// <summary>The registry key postfix for registering simple context menu entries.</summary>
        public const string RegKeyContextMenuSimplePostfix = @"shell";

        /// <summary>The registry key postfix for registering extended (COM-based) context menu entries.</summary>
        public const string RegKeyContextMenuExtendedPostfix = @"shellex\ContextMenuHandlers";

        /// <summary>The registry key postfix for registering (COM-based) property sheets.</summary>
        public const string RegKeyPropertySheetsPostfix = @"shellex\PropertySheetHandlers";
        #endregion
    }
}
