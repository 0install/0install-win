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
using Common.Storage;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;
using AccessPoints = ZeroInstall.DesktopIntegration.Model;

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
        #endregion

        #region Apply
        /// <summary>
        /// Applies a <see cref="Capabilities.FileType"/> to the current Windows system.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application being integrated.</param>
        /// <param name="feed">The feed of the application to get additional information (e.g. icons) from.</param>
        /// <param name="capability">The capability to be applied.</param>
        /// <param name="defaults">Flag indicating that file association, etc. should become default handlers for their respective types.</param>
        /// <param name="global">Apply the configuration system-wide instead of just for the current user.</param>
        public static void Apply(string interfaceID, Feed feed, Capabilities.FileType capability, bool defaults, bool global)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            if (capability == null) throw new ArgumentNullException("capability");
            #endregion

            var hive = global ? Registry.LocalMachine : Registry.CurrentUser;
            using (var classesKey = hive.OpenSubKey(RegKeyClasses, true))
            {
                using (var progIDKey = classesKey.CreateSubKey(capability.ID))
                {
                    if (capability.Description != null) progIDKey.SetValue("", capability.Description);

                    // ToDo: Set icon

                    using (var shellKey = progIDKey.CreateSubKey("shell"))
                    {
                        foreach (var verb in capability.Verbs)
                        {
                            using (var verbKey = shellKey.CreateSubKey(verb.Name))
                            using (var commandKey = verbKey.CreateSubKey("command"))
                            {
                                string launchCommand = "\"" + Path.Combine(Locations.InstallBase, feed.NeedsTerminal ? "0install.exe" : "0install-win.exe") + "\" run ";
                                if (!string.IsNullOrEmpty(verb.Command))
                                    launchCommand += "--command=" + StringUtils.EscapeWhitespace(verb.Command) + " ";
                                launchCommand += StringUtils.EscapeWhitespace(interfaceID) + " " + verb.Arguments;
                                commandKey.SetValue("", launchCommand);
                            }
                        }
                    }
                }

                foreach (var extension in capability.Extensions)
                {
                    using (var extensionKey = classesKey.CreateSubKey(extension.Value))
                    {
                        if (extension.MimeType != null) extensionKey.SetValue("Content Type", extension.MimeType);
                        if (extension.PerceivedType != null) extensionKey.SetValue("PerceivedType", extension.PerceivedType);

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
