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
    /// Contains control logic for applying <see cref="Capabilities.FileType"/> and <see cref="AccessPoints.FileType"/> on Windows systems.
    /// </summary>
    public static class FileType
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key backing HKCR.</summary>
        public const string RegKeyClasses = @"SOFTWARE\Classes";

        /// <summary>The registry value name for friendly type name storage.</summary>
        public const string RegValueFriendlyName = "FriendlyTypeName";

        /// <summary>The registry value name for MIME type storage.</summary>
        public const string RegValueContentType = "Content Type";

        /// <summary>The registry value name for perceived type storage.</summary>
        public const string RegValuePerceivedType = "PerceivedType";

        /// <summary>The registry subkey containing <see cref="Capabilities.FileType"/> references.</summary>
        public const string RegSubKeyIcon = "DefaultIcon";
        #endregion

        #region Apply
        /// <summary>
        /// Applies a <see cref="Capabilities.FileType"/> to the current Windows system.
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
        public static void Apply(InterfaceFeed target, Capabilities.FileType capability, bool defaults, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            #endregion

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var classesKey = hive.OpenSubKey(RegKeyClasses, true))
            {
                using (var progIDKey = classesKey.CreateSubKey(capability.ID))
                {
                    if (capability.Description != null) progIDKey.SetValue("", capability.Description);

                    // Find the first suitable icon specified by the capability, then fall back to the feed
                    var suitableIcons = capability.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco);
                    if (suitableIcons.IsEmpty) suitableIcons = target.Feed.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco && icon.Location != null);
                    if (!suitableIcons.IsEmpty)
                    {
                        using (var iconKey = progIDKey.CreateSubKey(RegSubKeyIcon))
                            iconKey.SetValue("", IconProvider.GetIcon(suitableIcons.First, systemWide, handler) + ",0");
                    }

                    using (var shellKey = progIDKey.CreateSubKey("shell"))
                    {
                        foreach (var verb in capability.Verbs)
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

                foreach (var extension in capability.Extensions)
                {
                    using (var extensionKey = classesKey.CreateSubKey(extension.Value))
                    {
                        if (extension.MimeType != null) extensionKey.SetValue(RegValueContentType, extension.MimeType);
                        if (extension.PerceivedType != null) extensionKey.SetValue(RegValuePerceivedType, extension.PerceivedType);

                        using (var openWithKey = extensionKey.CreateSubKey("OpenWithProgIDs"))
                            openWithKey.SetValue(capability.ID, "");

                        if(defaults) extensionKey.SetValue("", capability.ID);
                    }
                }
            }
        }
        #endregion
    }
}
