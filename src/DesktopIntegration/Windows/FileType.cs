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

        /// <summary>The registry value name for the flag indicating a menue entry should only appear when the SHIFT key is pressed.</summary>
        public const string RegValueExtendedFlag = "extended";

        /// <summary>The registry subkey containing <see cref="Capabilities.FileType"/> references.</summary>
        public const string RegSubKeyIcon = "DefaultIcon";

        /// <summary>The registry subkey containing "open with" ProgID references.</summary>
        public const string RegSubKeyOpenWith = "OpenWithProgIDs";
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

            using (var progIDKey = hive.CreateSubKey(RegKeyClasses + @"\" + fileType.ID))
                RegisterVerbCapability(progIDKey, target, fileType, systemWide, ModelUtils.Escape(target.Feed.Name), handler);

            using (var classesKey = hive.OpenSubKey(RegKeyClasses, true))
            {
                foreach (var extension in fileType.Extensions)
                {
                    if (string.IsNullOrEmpty(extension.Value)) continue;

                    using (var extensionKey = classesKey.CreateSubKey(extension.Value))
                    {
                        if (extension.MimeType != null) extensionKey.SetValue(RegValueContentType, extension.MimeType);
                        if (extension.PerceivedType != null) extensionKey.SetValue(RegValuePerceivedType, extension.PerceivedType);

                        using (var openWithKey = extensionKey.CreateSubKey(RegSubKeyOpenWith))
                            openWithKey.SetValue(fileType.ID, "");

                        if(setDefault) extensionKey.SetValue("", fileType.ID);
                    }
                }
            }
        }

        /// <summary>
        /// Registers a <see cref="Capabilities.VerbCapability"/> in a registry key.
        /// </summary>
        /// <param name="registryKey">The registry key to write the new data to.</param>
        /// <param name="target">The application being integrated.</param>
        /// <param name="capability">The capability to register.</param>
        /// <param name="systemWide">Assume <paramref name="registryKey"/> is effective system-wide instead of just for the current user.</param>
        /// <param name="exeName">The name any generated stub EXEs should have without the file ending (e.g. "MyApplication").</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the data in <paramref name="capability"/> is invalid.</exception>
        internal static void RegisterVerbCapability(RegistryKey registryKey, InterfaceFeed target, Capabilities.VerbCapability capability, bool systemWide, string exeName, ITaskHandler handler)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(capability.ID)) throw new InvalidDataException("Missing ID");

            if (capability.Description != null) registryKey.SetValue("", capability.Description);

            if (capability is Capabilities.UrlProtocol) registryKey.SetValue(UrlProtocol.ProtocolIndicator, "");

            // Find the first suitable icon specified by the capability, then fall back to the feed
            var suitableIcons = capability.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco);
            if (suitableIcons.IsEmpty) suitableIcons = target.Feed.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco && icon.Location != null);
            if (!suitableIcons.IsEmpty)
            {
                using (var iconKey = registryKey.CreateSubKey(RegSubKeyIcon))
                    iconKey.SetValue("", IconProvider.GetIcon(suitableIcons.First, systemWide, handler) + ",0");
            }

            using (var shellKey = registryKey.CreateSubKey("shell"))
            {
                foreach (var verb in capability.Verbs)
                {
                    using (var verbKey = shellKey.CreateSubKey(verb.Name))
                    {
                        if (verb.Description != null) verbKey.SetValue("", verb.Description);
                        if (verb.Extended) verbKey.SetValue(RegValueExtendedFlag, "");

                        using (var commandKey = verbKey.CreateSubKey("command"))
                            commandKey.SetValue("", GetLaunchCommandLine(target, verb, systemWide, exeName, handler));
                    }
                }
            }
        }

        /// <summary>
        /// Generates a command-line string for launching a <see cref="Capabilities.Verb"/>.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="verb">The verb to get to launch command for.</param>
        /// <param name="systemWide">Store the stub in a system-wide directory instead of just for the current user.</param>
        /// <param name="exeName">The name any generated stub EXEs should have without the file ending (e.g. "MyApplication").</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        internal static string GetLaunchCommandLine(InterfaceFeed target, Capabilities.Verb verb, bool systemWide, string exeName, ITaskHandler handler)
        {
            string launchCommand = "\"" + StubProvider.GetRunStub(target, verb.Command, systemWide, exeName, handler) + "\"";
            if (!string.IsNullOrEmpty(verb.Arguments)) launchCommand += " " + verb.Arguments;
            return launchCommand;
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

            // ToDo: Implement
        }
        #endregion
    }
}
