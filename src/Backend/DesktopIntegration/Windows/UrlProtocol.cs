/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using System.Net;
using Microsoft.Win32;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.UrlProtocol"/> and <see cref="AccessPoints.UrlProtocol"/> on Windows systems.
    /// </summary>
    public static class UrlProtocol
    {
        #region Constants
        /// <summary>The registry value name used to indicate that a programatic identifier is actually a ULR protocol handler.</summary>
        public const string ProtocolIndicator = "URL Protocol";

        /// <summary>The HKCU registry key where Windows Vista and newer store URL protocol associations.</summary>
        public const string RegKeyUserVistaUrlAssoc = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations";
        #endregion

        #region Register
        /// <summary>
        /// Registers a URL protocol in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="urlProtocol">The URL protocol to register.</param>
        /// <param name="machineWide">Register the URL protocol machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="accessPoint">Indicates that the handler shall become the default handler for the protocol.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="urlProtocol"/> is invalid.</exception>
        public static void Register(InterfaceFeed target, Store.Model.Capabilities.UrlProtocol urlProtocol, bool machineWide, ITaskHandler handler, bool accessPoint = false)
        {
            #region Sanity checks
            if (urlProtocol == null) throw new ArgumentNullException("urlProtocol");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(urlProtocol.ID)) throw new InvalidDataException("Missing ID");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;

            if (urlProtocol.KnownPrefixes.Count == 0)
            {
                if (accessPoint)
                { // Can only be registered invasively by registering protocol ProgID (will replace existing and become default)
                    using (var progIDKey = hive.CreateSubKey(FileType.RegKeyClasses + @"\" + urlProtocol.ID))
                        FileType.RegisterVerbCapability(progIDKey, target, urlProtocol, machineWide, handler);
                }
            }
            else
            { // Can be registered non-invasively by registering custom ProgID (without becoming default)
                using (var progIDKey = hive.CreateSubKey(FileType.RegKeyClasses + @"\" + FileType.RegKeyPrefix + urlProtocol.ID))
                {
                    // Add flag to remember whether created for capability or access point
                    progIDKey.SetValue(accessPoint ? FileType.PurposeFlagAccessPoint : FileType.PurposeFlagCapability, "");

                    FileType.RegisterVerbCapability(progIDKey, target, urlProtocol, machineWide, handler);
                }

                if (accessPoint)
                {
                    foreach (var prefix in urlProtocol.KnownPrefixes)
                    {
                        if (WindowsUtils.IsWindowsVista && !machineWide)
                        {
                            using (var someKey = Registry.CurrentUser.CreateSubKey(RegKeyUserVistaUrlAssoc + @"\" + prefix.Value + @"\UserChoice"))
                                someKey.SetValue("ProgID", FileType.RegKeyPrefix + urlProtocol.ID);
                        }
                        else
                        {
                            // Setting default invasively by registering protocol ProgID
                            using (var progIDKey = hive.CreateSubKey(FileType.RegKeyClasses + @"\" + prefix.Value))
                                FileType.RegisterVerbCapability(progIDKey, target, urlProtocol, machineWide, handler);
                        }
                    }
                }
            }
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a URL protocol in the current system.
        /// </summary>
        /// <param name="urlProtocol">The URL protocol to remove.</param>
        /// <param name="machineWide">Unregister the URL protocol machine-wide instead of just for the current user.</param>
        /// <param name="accessPoint">Indicates that the handler was the default handler for the protocol.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="urlProtocol"/> is invalid.</exception>
        public static void Unregister(Store.Model.Capabilities.UrlProtocol urlProtocol, bool machineWide, bool accessPoint = false)
        {
            #region Sanity checks
            if (urlProtocol == null) throw new ArgumentNullException("urlProtocol");
            #endregion

            if (string.IsNullOrEmpty(urlProtocol.ID)) throw new InvalidDataException("Missing ID");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;

            if (urlProtocol.KnownPrefixes.Count == 0)
            {
                if (accessPoint)
                { // Was registered invasively by registering protocol ProgID
                    try
                    {
                        hive.DeleteSubKeyTree(FileType.RegKeyClasses + @"\" + urlProtocol.ID);
                    }
                    catch (ArgumentException)
                    {} // Ignore missing registry keys
                }
            }
            else
            { // Was registered non-invasively by registering custom ProgID
                if (accessPoint)
                {
                    foreach (var prefix in urlProtocol.KnownPrefixes)
                    {
                        // TODO: Restore previous default
                    }
                }

                // Remove appropriate purpose flag and check if there are others
                bool otherFlags;
                using (var progIDKey = hive.OpenSubKey(FileType.RegKeyClasses + @"\" + FileType.RegKeyPrefix + urlProtocol.ID, writable: true))
                {
                    if (progIDKey == null) otherFlags = false;
                    else
                    {
                        progIDKey.DeleteValue(accessPoint ? FileType.PurposeFlagAccessPoint : FileType.PurposeFlagCapability, throwOnMissingValue: false);
                        otherFlags = progIDKey.GetValueNames().Any(name => name.StartsWith(FileType.PurposeFlagPrefix));
                    }
                }

                // Delete ProgID if there are no other references
                if (!otherFlags)
                    hive.DeleteSubKeyTree(FileType.RegKeyClasses + @"\" + FileType.RegKeyPrefix + urlProtocol.ID);
            }
        }
        #endregion
    }
}
