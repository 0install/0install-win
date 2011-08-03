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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Common;
using Common.Tasks;
using Common.Utils;
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

        /// <summary>The registry value name for application user model IDs (used by the Windows 7 taksbar).</summary>
        public const string RegValueAppUserModelID = "AppUserModelID";

        /// <summary>The registry value name for MIME type storage.</summary>
        public const string RegValueContentType = "Content Type";

        /// <summary>The registry value name for perceived type storage.</summary>
        public const string RegValuePerceivedType = "PerceivedType";

        /// <summary>The registry value name for the flag indicating a menue entry should only appear when the SHIFT key is pressed.</summary>
        public const string RegValueExtended = "extended";

        /// <summary>The registry subkey containing <see cref="Capabilities.FileType"/> references.</summary>
        public const string RegSubKeyIcon = "DefaultIcon";

        /// <summary>The registry subkey containing "open with" ProgID references.</summary>
        public const string RegSubKeyOpenWith = "OpenWithProgIDs";

        /// <summary>The registry subkey below <see cref="RegKeyClasses"/> that contains MIME type to extension mapping.</summary>
        public const string RegSubKeyMimeType = @"MIME\Database\Content Type";

        /// <summary>The registry value name for a MIME type extension association.</summary>
        public const string RegValueExtension = "Extension";
        #endregion

        #region Register
        /// <summary>
        /// Registers a file type in the current Windows system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="fileType">The file type to register.</param>
        /// <param name="setDefault">Indicates that the file associations shall become default handlers for their respective types.</param>
        /// <param name="systemWide">Register the file type system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="fileType"/> is invalid.</exception>
        public static void Register(InterfaceFeed target, Capabilities.FileType fileType, bool setDefault, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (fileType == null) throw new ArgumentNullException("fileType");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(fileType.ID)) throw new InvalidDataException("Missing ID");

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;

            // Register ProgID
            using (var progIDKey = hive.CreateSubKey(RegKeyClasses + @"\" + fileType.ID))
                RegisterVerbCapability(progIDKey, target, fileType, systemWide, handler);

            using (var classesKey = hive.OpenSubKey(RegKeyClasses, true))
            {
                foreach (var extension in fileType.Extensions)
                {
                    if (string.IsNullOrEmpty(extension.Value)) continue;

                    // Register extensions
                    using (var extensionKey = classesKey.CreateSubKey(extension.Value))
                    {
                        if (!string.IsNullOrEmpty(extension.MimeType)) extensionKey.SetValue(RegValueContentType, extension.MimeType);
                        if (!string.IsNullOrEmpty(extension.PerceivedType)) extensionKey.SetValue(RegValuePerceivedType, extension.PerceivedType);

                        using (var openWithKey = extensionKey.CreateSubKey(RegSubKeyOpenWith))
                            openWithKey.SetValue(fileType.ID, "");

                        if (setDefault) extensionKey.SetValue("", fileType.ID);
                    }

                    // Register MIME types
                    if (!string.IsNullOrEmpty(extension.MimeType))
                    {
                        using (var mimeKey = classesKey.CreateSubKey(RegSubKeyMimeType + @"\" + extension.MimeType))
                            mimeKey.SetValue(RegValueExtension, extension.Value);
                    }
                }
            }
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a file type in the current Windows system.
        /// </summary>
        /// <param name="fileType">The file type to remove.</param>
        /// <param name="systemWide">Unregister the file type system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="fileType"/> is invalid.</exception>
        public static void Unregister(Capabilities.FileType fileType, bool systemWide)
        {
            #region Sanity checks
            if (fileType == null) throw new ArgumentNullException("fileType");
            #endregion

            if (string.IsNullOrEmpty(fileType.ID)) throw new InvalidDataException("Missing ID");

            var hive = systemWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var classesKey = hive.OpenSubKey(RegKeyClasses, true))
            {
                foreach (var extension in fileType.Extensions)
                {
                    if (string.IsNullOrEmpty(extension.Value)) continue;

                    // Unegister MIME types
                    if (!string.IsNullOrEmpty(extension.MimeType))
                    {
                        // ToDo
                    }

                    // Unregister extensions
                    using (var extensionKey = classesKey.CreateSubKey(extension.Value))
                    {
                        using (var openWithKey = extensionKey.CreateSubKey(RegSubKeyOpenWith))
                            openWithKey.DeleteValue(fileType.ID, false);

                        // ToDo: Restore previous default
                    }
                }

                // Remove ProgID
                try { classesKey.DeleteSubKeyTree(fileType.ID); }
                catch (ArgumentException) {} // Ignore missing registry keys
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Registers a <see cref="Capabilities.VerbCapability"/> in a registry key.
        /// </summary>
        /// <param name="registryKey">The registry key to write the new data to.</param>
        /// <param name="target">The application being integrated.</param>
        /// <param name="capability">The capability to register.</param>
        /// <param name="systemWide">Assume <paramref name="registryKey"/> is effective system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="capability"/> is invalid.</exception>
        internal static void RegisterVerbCapability(RegistryKey registryKey, InterfaceFeed target, Capabilities.VerbCapability capability, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (capability is Capabilities.UrlProtocol) registryKey.SetValue(UrlProtocol.ProtocolIndicator, "");

            string description = capability.Descriptions.GetBestLanguage(CultureInfo.CurrentCulture);
            if (description != null) registryKey.SetValue("", description);

            // Write verb command information
            using (var shellKey = registryKey.CreateSubKey("shell"))
            {
                foreach (var verb in capability.Verbs)
                {
                    using (var verbKey = shellKey.CreateSubKey(verb.Name))
                    {
                        string verbDescription = verb.Descriptions.GetBestLanguage(CultureInfo.CurrentCulture);
                        if (verbDescription != null) verbKey.SetValue("", verbDescription);
                        if (verb.Extended) verbKey.SetValue(RegValueExtended, "");

                        using (var commandKey = verbKey.CreateSubKey("command"))
                            commandKey.SetValue("", GetLaunchCommandLine(target, verb, systemWide, handler));

                        // Prevent conflicts with existing entries
                        shellKey.DeleteSubKey("ddeexec", false);
                    }
                }
            }

            // Set specific icon if available, fall back to referencing the icon embedded in the stub EXE
            string iconPath;
            try { iconPath = IconProvider.GetIconPath(capability.GetIcon(Icon.MimeTypeIco), systemWide, handler); }
            catch (KeyNotFoundException) { iconPath = StubProvider.GetRunStub(target, null, systemWide, handler); }
            using (var iconKey = registryKey.CreateSubKey(RegSubKeyIcon))
                iconKey.SetValue("", iconPath + ",0");
        }

        /// <summary>
        /// Generates a command-line string for launching a <see cref="Capabilities.Verb"/>.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="verb">The verb to get to launch command for.</param>
        /// <param name="systemWide">Store the stub in a system-wide directory instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        internal static string GetLaunchCommandLine(InterfaceFeed target, Capabilities.Verb verb, bool systemWide, ITaskHandler handler)
        {
            string launchCommand = "\"" + StubProvider.GetRunStub(target, verb.Command, systemWide, handler) + "\"";
            if (!string.IsNullOrEmpty(verb.Arguments)) launchCommand += " " + verb.Arguments;
            return launchCommand;
        }
        #endregion
    }
}
