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
        /// <summary>
        /// The default value for <see cref="Capabilities.Verb.Arguments"/>.
        /// </summary>
        public const string DefaultArguments = "\"%1\"";

        /// <summary>The HKCU/HKLM registry key backing HKCR.</summary>
        public const string RegKeyClass = @"SOFTWARE\Classes";

        /// <summary>The HKLM registry key for registering applications as candidates for default programs.</summary>
        public const string RegKeyMachineRegisteredApplications = @"SOFTWARE\RegisteredApplications";
        #endregion

        #region Apply
        /// <summary>
        /// Applies an <see cref="Capabilities.FileType"/> and optionally <see cref="AccessPoints.FileType"/> to the current Windows system.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application being integrated.</param>
        /// <param name="feed">The of the application to get additional information (e.g. icons) from.</param>
        /// <param name="fileType">The capability to be applied.</param>
        /// <param name="accessPoint">Flag indicating that an according <see cref="AccessPoints.FileType"/> was also set (i.e. the file association should become the default handler for the type).</param>
        /// <param name="global">Flag indicating to apply the configuration system-wide instead of just for the current user.</param>
        public static void Apply(string interfaceID, Feed feed, Capabilities.FileType fileType, bool accessPoint, bool global)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            if (fileType == null) throw new ArgumentNullException("fileType");
            #endregion

            var hive = global ? Registry.LocalMachine : Registry.CurrentUser;
            using (var classesKey = hive.OpenSubKey(RegKeyClass, true))
            {
                using (var progIDKey = classesKey.CreateSubKey(fileType.ID))
                {
                    if (fileType.Description != null) progIDKey.SetValue("", fileType.Description);

                    // ToDo: Set icon

                    using (var shellKey = progIDKey.CreateSubKey("shell"))
                    {
                        foreach (var verb in fileType.Verbs)
                        {
                            using (var verbKey = shellKey.CreateSubKey(verb.Name))
                            using (var commandKey = verbKey.CreateSubKey("command"))
                            {
                                string launchCommand = "\"" + Path.Combine(Locations.InstallBase, feed.NeedsTerminal ? "0install.exe" : "0install-win.exe") + "\" run ";
                                if (!string.IsNullOrEmpty(verb.Command))
                                    launchCommand += "--command=" + StringUtils.EscapeWhitespace(verb.Command) + " ";
                                launchCommand += StringUtils.EscapeWhitespace(interfaceID) + " " + (string.IsNullOrEmpty(verb.Arguments) ? DefaultArguments : verb.Arguments);
                                commandKey.SetValue("", launchCommand);
                            }
                        }
                    }
                }

                foreach (var extension in fileType.Extensions)
                {
                    using (var extensionKey = classesKey.CreateSubKey(extension.Value))
                    {
                        if (extension.MimeType != null) extensionKey.SetValue("Content Type", extension.MimeType);
                        if (extension.PerceivedType != null) extensionKey.SetValue("PerceivedType", extension.PerceivedType);

                        using (var openWithKey = extensionKey.CreateSubKey("OpenWithProgIDs"))
                            openWithKey.SetValue(fileType.ID, "");

                        if(accessPoint) extensionKey.SetValue("", fileType.ID);
                    }
                }
            }
        }
        #endregion
    }
}
