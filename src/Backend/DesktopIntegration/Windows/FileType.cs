/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.FileType"/> and <see cref="AccessPoints.FileType"/> on Windows systems.
    /// </summary>
    public static class FileType
    {
        #region Constants
        /// <summary>Prepended before any programatic identifiers used by Zero Install in the registry. This prevents conflicts with non-Zero Install installations.</summary>
        internal const string RegKeyPrefix = "ZeroInstall.";

        /// <summary>Prepended before any registry purpose flags. Purpose flags indicate what a registry key was created for and whether it is still required.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public const string PurposeFlagPrefix = "ZeroInstall.";

        /// <summary>Indicates a registry key is required by a capability.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public const string PurposeFlagCapability = PurposeFlagPrefix + "Capability";

        /// <summary>Indicates a registry key is required by an access point.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public const string PurposeFlagAccessPoint = PurposeFlagPrefix + "AccessPoint";

        /// <summary>The HKCU/HKLM registry key backing HKCR.</summary>
        public const string RegKeyClasses = @"SOFTWARE\Classes";

        /// <summary>The HKCU/HKLM registry key backing HKCR.</summary>
        public const string RegKeyOverrides = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts";

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

        /// <summary>The registry subkey containing <see cref="Store.Model.Capabilities.FileType"/> references.</summary>
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
        /// Registers a file type in the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="fileType">The file type to register.</param>
        /// <param name="machineWide">Register the file type machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="accessPoint">Indicates that the file associations shall become default handlers for their respective types.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="fileType"/> is invalid.</exception>
        public static void Register(FeedTarget target, [NotNull] Store.Model.Capabilities.FileType fileType, bool machineWide, [NotNull] ITaskHandler handler, bool accessPoint = false)
        {
            #region Sanity checks
            if (fileType == null) throw new ArgumentNullException("fileType");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(fileType.ID)) throw new InvalidDataException("Missing ID");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;

            // Register ProgID
            using (var progIDKey = hive.CreateSubKeyChecked(RegKeyClasses + @"\" + RegKeyPrefix + fileType.ID))
            {
                // Add flag to remember whether created for capability or access point
                progIDKey.SetValue(accessPoint ? PurposeFlagAccessPoint : PurposeFlagCapability, "");

                RegisterVerbCapability(progIDKey, target, fileType, machineWide, handler);
            }

            using (var classesKey = hive.OpenSubKey(RegKeyClasses, writable: true))
            {
                foreach (var extension in fileType.Extensions.Except(x => string.IsNullOrEmpty(x.Value)))
                {
                    // Register extensions
                    using (var extensionKey = classesKey.CreateSubKeyChecked(extension.Value))
                    {
                        if (!string.IsNullOrEmpty(extension.MimeType)) extensionKey.SetValue(RegValueContentType, extension.MimeType);
                        if (!string.IsNullOrEmpty(extension.PerceivedType)) extensionKey.SetValue(RegValuePerceivedType, extension.PerceivedType);

                        using (var openWithKey = extensionKey.CreateSubKeyChecked(RegSubKeyOpenWith))
                            openWithKey.SetValue(RegKeyPrefix + fileType.ID, "");

                        if (accessPoint)
                        {
                            if (!machineWide && WindowsUtils.IsWindowsVista)
                            { // Windows Vista and later store per-user file extension overrides
                                using (var overridesKey = hive.OpenSubKeyChecked(RegKeyOverrides, writable: true))
                                using (var extensionOverrideKey = overridesKey.CreateSubKeyChecked(extension.Value))
                                {
                                    // Only mess with this part of the registry when necessary
                                    bool alreadySet;
                                    using (var userChoiceKey = extensionOverrideKey.OpenSubKey("UserChoice", writable: false))
                                    {
                                        if (userChoiceKey == null) alreadySet = false;
                                        else alreadySet = ((userChoiceKey.GetValue("Progid") ?? "").ToString() == RegKeyPrefix + fileType.ID);
                                    }

                                    if (!alreadySet)
                                    {
                                        // Must delete and recreate instead of direct modification due to wicked ACLs
                                        extensionOverrideKey.DeleteSubKey("UserChoice", throwOnMissingSubKey: false);

                                        using (var userChoiceKey = extensionOverrideKey.CreateSubKeyChecked("UserChoice"))
                                            userChoiceKey.SetValue("Progid", RegKeyPrefix + fileType.ID);
                                    }
                                }
                            }
                            else extensionKey.SetValue("", RegKeyPrefix + fileType.ID);
                        }
                    }

                    // Register MIME types
                    if (!string.IsNullOrEmpty(extension.MimeType))
                    {
                        using (var mimeKey = classesKey.CreateSubKeyChecked(RegSubKeyMimeType + @"\" + extension.MimeType))
                            mimeKey.SetValue(RegValueExtension, extension.Value);
                    }
                }
            }
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Unregisters a file type in the current system.
        /// </summary>
        /// <param name="fileType">The file type to remove.</param>
        /// <param name="machineWide">Unregister the file type machine-wide instead of just for the current user.</param>
        /// <param name="accessPoint">Indicates that the file associations were default handlers for their respective types.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="fileType"/> is invalid.</exception>
        public static void Unregister([NotNull] Store.Model.Capabilities.FileType fileType, bool machineWide, bool accessPoint = false)
        {
            #region Sanity checks
            if (fileType == null) throw new ArgumentNullException("fileType");
            #endregion

            if (string.IsNullOrEmpty(fileType.ID)) throw new InvalidDataException("Missing ID");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;
            using (var classesKey = hive.OpenSubKeyChecked(RegKeyClasses, writable: true))
            {
                foreach (var extension in fileType.Extensions.Except(extension => string.IsNullOrEmpty(extension.Value)))
                {
                    // Unegister MIME types
                    if (!string.IsNullOrEmpty(extension.MimeType))
                    {
                        using (var extensionKey = classesKey.CreateSubKeyChecked(extension.Value))
                        {
                            // TODO: Restore previous default
                            //extensionKey.SetValue("", fileType.PreviousID);
                        }

                        //if (!machineWide && WindowsUtils.IsWindowsVista && !WindowsUtils.IsWindows8)
                        //{
                        //    // Windows Vista and later store per-user file extension overrides, Windows 8 blocks programmatic modification with hash values
                        //    using (var overridesKey = hive.OpenSubKey(RegKeyOverrides, true))
                        //    using (var extensionOverrideKey = overridesKey.CreateSubKeyChecked(extension.Value))
                        //    {
                        //        // Must delete and recreate instead of direct modification due to wicked ACLs
                        //        extensionOverrideKey.DeleteSubKey("UserChoice", throwOnMissingSubKey: false);
                        //        using (var userChoiceKey = extensionOverrideKey.CreateSubKeyChecked("UserChoice"))
                        //            userChoiceKey.SetValue("Progid", fileType.PreviousID);
                        //    }
                        //}
                    }

                    // Unregister extensions
                    using (var extensionKey = classesKey.OpenSubKey(extension.Value, writable: true))
                    {
                        if (extensionKey != null)
                        {
                            using (var openWithKey = extensionKey.OpenSubKey(RegSubKeyOpenWith, writable: true))
                            {
                                if (openWithKey != null)
                                    openWithKey.DeleteValue(RegKeyPrefix + fileType.ID, throwOnMissingValue: false);
                            }
                        }

                        if (accessPoint)
                        {
                            // TODO: Restore previous default
                        }
                    }
                }

                // Remove appropriate purpose flag and check if there are others
                bool otherFlags;
                using (var progIDKey = classesKey.OpenSubKey(RegKeyPrefix + fileType.ID, writable: true))
                {
                    if (progIDKey == null) otherFlags = false;
                    else
                    {
                        progIDKey.DeleteValue(accessPoint ? PurposeFlagAccessPoint : PurposeFlagCapability, throwOnMissingValue: false);
                        otherFlags = progIDKey.GetValueNames().Any(name => name.StartsWith(PurposeFlagPrefix));
                    }
                }

                // Delete ProgID if there are no other references
                if (!otherFlags)
                {
                    try
                    {
                        classesKey.DeleteSubKeyTree(RegKeyPrefix + fileType.ID);
                    }

                        #region Error handling
                    catch (ArgumentException)
                    {
                        // Ignore missing registry keys
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Registers a <see cref="Store.Model.Capabilities.VerbCapability"/> in a registry key.
        /// </summary>
        /// <param name="registryKey">The registry key to write the new data to.</param>
        /// <param name="target">The application being integrated.</param>
        /// <param name="capability">The capability to register.</param>
        /// <param name="machineWide">Assume <paramref name="registryKey"/> is effective machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="capability"/> is invalid.</exception>
        internal static void RegisterVerbCapability(RegistryKey registryKey, FeedTarget target, Store.Model.Capabilities.VerbCapability capability, bool machineWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (capability == null) throw new ArgumentNullException("capability");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (capability is Store.Model.Capabilities.UrlProtocol) registryKey.SetValue(UrlProtocol.ProtocolIndicator, "");

            string description = capability.Descriptions.GetBestLanguage(CultureInfo.CurrentUICulture);
            if (description != null) registryKey.SetValue("", description);

            // Write verb command information
            using (var shellKey = registryKey.CreateSubKeyChecked("shell"))
            {
                foreach (var verb in capability.Verbs)
                {
                    using (var verbKey = shellKey.CreateSubKeyChecked(verb.Name))
                    {
                        string verbDescription = verb.Descriptions.GetBestLanguage(CultureInfo.CurrentUICulture);
                        if (verbDescription != null) verbKey.SetValue("", verbDescription);
                        if (verb.Extended) verbKey.SetValue(RegValueExtended, "");

                        using (var commandKey = verbKey.CreateSubKeyChecked("command"))
                            commandKey.SetValue("", GetLaunchCommandLine(target, verb, machineWide, handler));

                        // Prevent conflicts with existing entries
                        shellKey.DeleteSubKey("ddeexec", throwOnMissingSubKey: false);
                    }
                }
            }

            // Set specific icon if available, fall back to referencing the icon embedded in the stub EXE
            var icon = capability.GetIcon(Icon.MimeTypeIco) ?? target.Feed.GetIcon(Icon.MimeTypeIco);
            if (icon != null)
            {
                using (var iconKey = registryKey.CreateSubKeyChecked(RegSubKeyIcon))
                    iconKey.SetValue("", IconProvider.GetIconPath(icon, handler, machineWide) + ",0");
            }
        }

        /// <summary>
        /// Generates a command-line string for launching a <see cref="Store.Model.Capabilities.Verb"/>.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="verb">The verb to get to launch command for.</param>
        /// <param name="machineWide">Store the stub in a machine-wide directory instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="InvalidOperationException">Write access to the filesystem is not permitted.</exception>
        internal static string GetLaunchCommandLine(FeedTarget target, Store.Model.Capabilities.Verb verb, bool machineWide, ITaskHandler handler)
        {
            try
            {
                string launchCommand = "\"" + target.GetRunStub(verb.Command, handler, machineWide) + "\"";
                if (!string.IsNullOrEmpty(verb.Arguments)) launchCommand += " " + verb.Arguments;
                return launchCommand;
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion
        }
        #endregion
    }
}
