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
using ZeroInstall.Model;
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

        #region Register
        /// <summary>
        /// Registers a URL protocol in the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="urlProtocol">The URL protocol to register.</param>
        /// <param name="setDefault">Indicates that the handler shall become default handler for the protocol.</param>
        /// <param name="systemWide">Register the URL protocol system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Register(InterfaceFeed target, Capabilities.UrlProtocol urlProtocol, bool setDefault, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (urlProtocol == null) throw new ArgumentNullException("urlProtocol");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(urlProtocol.Prefix))
            {
                if (!setDefault) return;
            }
            else
            {
                // ToDo
            }

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var classesKey = hive.OpenSubKey(FileType.RegKeyClasses, true))
            {
                using (var progIDKey = classesKey.CreateSubKey(urlProtocol.ID))
                {
                    if (urlProtocol.Description != null) progIDKey.SetValue("", urlProtocol.Description);

                    // Find the first suitable icon specified by the capability, then fall back to the feed
                    var suitableIcons = urlProtocol.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco);
                    if (suitableIcons.IsEmpty) suitableIcons = target.Feed.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco && icon.Location != null);
                    if (!suitableIcons.IsEmpty)
                    {
                        using (var iconKey = progIDKey.CreateSubKey(FileType.RegSubKeyIcon))
                            iconKey.SetValue("", IconProvider.GetIcon(suitableIcons.First, systemWide, handler) + ",0");
                    }

                    using (var shellKey = progIDKey.CreateSubKey("shell"))
                    {
                        foreach (var verb in urlProtocol.Verbs)
                        {
                            using (var verbKey = shellKey.CreateSubKey(verb.Name))
                            using (var commandKey = verbKey.CreateSubKey("command"))
                            {
                                string launchCommand = "\"" + StubProvider.GetRunStub(target, verb.Command, systemWide, handler) + "\"";
                                if (!string.IsNullOrEmpty(verb.Arguments)) launchCommand += " " + verb.Arguments;
                                commandKey.SetValue("", launchCommand);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a URL protocol in the current Windows system.
        /// </summary>
        /// <param name="urlProtocol">The URL protocol to remove.</param>
        /// <param name="systemWide">Unregister the URL protocol system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static void Unregister(Capabilities.UrlProtocol urlProtocol, bool systemWide)
        {
            #region Sanity checks
            if (urlProtocol == null) throw new ArgumentNullException("urlProtocol");
            #endregion

            // ToDo: Implement
        }
        #endregion
    }
}
