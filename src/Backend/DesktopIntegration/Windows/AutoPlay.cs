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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.AutoPlay"/> and <see cref="AccessPoints.AutoPlay"/> on Windows systems.
    /// </summary>
    public static class AutoPlay
    {
        #region Constants
        /// <summary>The HKCU/HKLM registry key for storing AutoPlay handlers.</summary>
        public const string RegKeyHandlers = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers";

        /// <summary>The HKCU/HKLM registry key for storing AutoPlay handler associations.</summary>
        public const string RegKeyAssocs = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\EventHandlers";

        /// <summary>The HKCU registry key for storing user-selected AutoPlay handlers.</summary>
        public const string RegKeyChosenAssocs = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\UserChosenExecuteHandlers";

        /// <summary>The registry value name for storing the programatic identifier to invoke.</summary>
        public const string RegValueProgID = "InvokeProgID";

        /// <summary>The registry value name for storing the name of the verb to invoke.</summary>
        public const string RegValueVerb = "InvokeVerb";

        /// <summary>The registry value name for storing the name of the application providing the AutoPlay action.</summary>
        public const string RegValueProvider = "Provider";

        /// <summary>The registry value name for storing the description of the AutoPlay action.</summary>
        public const string RegValueDescription = "Action";

        /// <summary>The registry value name for storing the icon for the AutoPlay action.</summary>
        public const string RegValueIcon = "DefaultIcon";
        #endregion

        #region Register
        /// <summary>
        /// Adds an AutoPlay handler registration to the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="autoPlay">The AutoPlay handler information to be applied.</param>
        /// <param name="machineWide">Register the handler machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="accessPoint">Indicates that the handler should become the default handler for all <see cref="Store.Model.Capabilities.AutoPlay.Events"/>.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="autoPlay"/> is invalid.</exception>
        public static void Register(InterfaceFeed target, [NotNull] Store.Model.Capabilities.AutoPlay autoPlay, bool machineWide, [NotNull] ITaskHandler handler, bool accessPoint = false)
        {
            #region Sanity checks
            if (autoPlay == null) throw new ArgumentNullException("autoPlay");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(autoPlay.ID)) throw new InvalidDataException("Missing ID");
            if (autoPlay.Verb == null) throw new InvalidDataException("Missing verb");
            if (string.IsNullOrEmpty(autoPlay.Verb.Name)) throw new InvalidDataException("Missing verb name");
            if (string.IsNullOrEmpty(autoPlay.Provider)) throw new InvalidDataException("Missing provider");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;

            using (var commandKey = hive.CreateSubKey(FileType.RegKeyClasses + @"\" + FileType.RegKeyPrefix + ".AutoPlay" + autoPlay.ID + @"\shell\" + autoPlay.Verb.Name + @"\command"))
                commandKey.SetValue("", FileType.GetLaunchCommandLine(target, autoPlay.Verb, machineWide, handler));

            using (var handlerKey = hive.CreateSubKey(RegKeyHandlers + @"\" + FileType.RegKeyPrefix + autoPlay.ID))
            {
                // Add flag to remember whether created for capability or access point
                handlerKey.SetValue(accessPoint ? FileType.PurposeFlagAccessPoint : FileType.PurposeFlagCapability, "");

                handlerKey.SetValue(RegValueProgID, FileType.RegKeyPrefix + ".AutoPlay" + autoPlay.ID);
                handlerKey.SetValue(RegValueVerb, autoPlay.Verb.Name);
                handlerKey.SetValue(RegValueProvider, autoPlay.Provider);
                handlerKey.SetValue(RegValueDescription, autoPlay.Descriptions.GetBestLanguage(CultureInfo.CurrentUICulture) ?? autoPlay.Verb.Name);

                var icon = autoPlay.GetIcon(Icon.MimeTypeIco) ?? target.Feed.GetIcon(Icon.MimeTypeIco, autoPlay.Verb.Command);
                if (icon != null)
                    handlerKey.SetValue(RegValueIcon, IconProvider.GetIconPath(icon, handler, machineWide) + ",0");
            }

            foreach (var autoPlayEvent in autoPlay.Events.Except(x => string.IsNullOrEmpty(x.Name)))
            {
                using (var eventKey = hive.CreateSubKey(RegKeyAssocs + @"\" + autoPlayEvent.Name))
                    eventKey.SetValue(FileType.RegKeyPrefix + autoPlay.ID, "");

                if (accessPoint)
                {
                    using (var chosenEventKey = hive.CreateSubKey(RegKeyChosenAssocs + @"\" + autoPlayEvent.Name))
                        chosenEventKey.SetValue("", FileType.RegKeyPrefix + autoPlay.ID);
                }
            }
        }
        #endregion

        #region Unregister
        /// <summary>
        /// Removes an AutoPlay handler registration from the current system.
        /// </summary>
        /// <param name="autoPlay">The AutoPlay handler information to be removed.</param>
        /// <param name="machineWide">Remove the handler machine-wide instead of just for the current user.</param>
        /// <param name="accessPoint">Indicates that the handler should was the default handler for all <see cref="Store.Model.Capabilities.AutoPlay.Events"/>.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="autoPlay"/> is invalid.</exception>
        public static void Unregister([NotNull] Store.Model.Capabilities.AutoPlay autoPlay, bool machineWide, bool accessPoint = false)
        {
            #region Sanity checks
            if (autoPlay == null) throw new ArgumentNullException("autoPlay");
            #endregion

            if (string.IsNullOrEmpty(autoPlay.ID)) throw new InvalidDataException("Missing ID");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;

            if (accessPoint)
            {
                // TODO: Restore previous default
            }

            try
            {
                // Remove appropriate purpose flag and check if there are others
                bool otherFlags;
                using (var handlerKey = hive.OpenSubKey(RegKeyHandlers + @"\" + FileType.RegKeyPrefix + autoPlay.ID, writable: true))
                {
                    if (handlerKey == null) otherFlags = false;
                    else
                    {
                        handlerKey.DeleteValue(accessPoint ? FileType.PurposeFlagAccessPoint : FileType.PurposeFlagCapability, throwOnMissingValue: false);
                        otherFlags = handlerKey.GetValueNames().Any(name => name.StartsWith(FileType.PurposeFlagPrefix));
                    }
                }

                // Delete handler key and ProgID if there are no other references
                if (!otherFlags)
                {
                    foreach (var autoPlayEvent in autoPlay.Events.Except(x => string.IsNullOrEmpty(x.Name)))
                    {
                        using (var eventKey = hive.CreateSubKey(RegKeyAssocs + @"\" + autoPlayEvent.Name))
                            eventKey.DeleteValue(FileType.RegKeyPrefix + autoPlay.ID, throwOnMissingValue: false);
                    }

                    hive.DeleteSubKey(RegKeyHandlers + @"\" + FileType.RegKeyPrefix + autoPlay.ID, throwOnMissingSubKey: false);
                    hive.DeleteSubKeyTree(FileType.RegKeyClasses + @"\" + FileType.RegKeyPrefix + ".AutoPlay" + autoPlay.ID);
                }
            }
            catch (ArgumentException)
            {} // Ignore missing registry keys
        }
        #endregion
    }
}
