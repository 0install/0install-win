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
using Microsoft.Win32;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Capabilities.UrlProtocol"/> and <see cref="AccessPoints.UrlProtocol"/> on Windows systems.
    /// </summary>
    public static class UrlProtocol
    {
        #region Constants
        /// <summary>The registry value name used to indicate that a programatic identifier is actually a ULR protocol handler.</summary>
        public const string ProtocolIndicator = "URL Protocol";

        /// <summary>The HKCU registry key where Windows Vista and newer store URL protocol associations.</summary>
        public const string RegKeyUserVistaUrlAssoc = @"Software\Microsoft\Windows\Shell\ Associations\UrlAssociations";
        #endregion

        #region Apply
        /// <summary>
        /// Applies an <see cref="Capabilities.UrlProtocol"/> to the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="capability">The capability to be applied.</param>
        /// <param name="defaults">Flag indicating that file association, etc. should become default handlers for their respective types.</param>
        /// <param name="systemWide">Apply the configuration system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Apply(InterfaceFeed target, Capabilities.UrlProtocol capability, bool defaults, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var classesKey = hive.OpenSubKey(FileType.RegKeyClasses, true))
            {
                using (var progIDKey = classesKey.CreateSubKey(capability.ID))
                {
                    //
                }
            }
        }
        #endregion
    }
}
