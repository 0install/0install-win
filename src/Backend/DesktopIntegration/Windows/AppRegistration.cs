/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Contains control logic for applying <see cref="Store.Model.Capabilities.AppRegistration"/> on Windows systems.
    /// </summary>
    public static class AppRegistration
    {
        #region Constants
        /// <summary>Prepended before <see cref="Store.Model.Capabilities.AppRegistration.CapabilityRegPath"/>. This prevents conflicts with non-Zero Install installations.</summary>
        private const string CapabilityPrefix = @"SOFTWARE\Zero Install\Applications\";

        /// <summary>The HKLM registry key for registering applications as candidates for default programs.</summary>
        public const string RegKeyMachineRegisteredApplications = @"SOFTWARE\RegisteredApplications";

        /// <summary>The registry value name for the application name.</summary>
        public const string RegValueAppName = "ApplicationName";

        /// <summary>The registry value name for the application description.</summary>
        public const string RegValueAppDescription = "ApplicationDescription";

        /// <summary>The registry value name for the application icon.</summary>
        public const string RegValueAppIcon = "ApplicationIcon";

        /// <summary>The registry subkey containing <see cref="Store.Model.Capabilities.FileType"/> references.</summary>
        public const string RegSubKeyFileAssocs = "FileAssociations";

        /// <summary>The registry subkey containing <see cref="Store.Model.Capabilities.UrlProtocol"/> references.</summary>
        public const string RegSubKeyUrlAssocs = "URLAssociations";

        /// <summary>The registry subkey containing <see cref="Store.Model.Capabilities.DefaultProgram"/> references.</summary>
        public const string RegSubKeyStartMenu = "StartMenu";
        #endregion

        #region Apply
        /// <summary>
        /// Applies application registration to the current system.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="appRegistration">The registration information to be applied.</param>
        /// <param name="verbCapabilities">The capabilities that the application is to be registered with.</param>
        /// <param name="machineWide">Apply the registration machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">A problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="appRegistration"/> or <paramref name="verbCapabilities"/> is invalid.</exception>
        public static void Register(FeedTarget target, [NotNull] Store.Model.Capabilities.AppRegistration appRegistration, IEnumerable<VerbCapability> verbCapabilities, bool machineWide, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (appRegistration == null) throw new ArgumentNullException("appRegistration");
            if (verbCapabilities == null) throw new ArgumentNullException("verbCapabilities");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (string.IsNullOrEmpty(appRegistration.ID)) throw new InvalidDataException("Missing ID");
            if (string.IsNullOrEmpty(appRegistration.CapabilityRegPath)) throw new InvalidDataException("Invalid CapabilityRegPath");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;

            // TODO: Handle appRegistration.X64
            using (var capabilitiesKey = hive.CreateSubKeyChecked( /*CapabilityPrefix +*/ appRegistration.CapabilityRegPath))
            {
                capabilitiesKey.SetValue(RegValueAppName, target.Feed.Name ?? "");
                capabilitiesKey.SetValue(RegValueAppDescription, target.Feed.Descriptions.GetBestLanguage(CultureInfo.CurrentUICulture) ?? "");

                // Set icon if available
                var icon = target.Feed.GetIcon(Icon.MimeTypeIco);
                if (icon != null) capabilitiesKey.SetValue(RegValueAppIcon, IconProvider.GetIconPath(icon, handler, machineWide) + ",0");

                using (var fileAssocsKey = capabilitiesKey.CreateSubKeyChecked(RegSubKeyFileAssocs))
                {
                    foreach (var fileType in verbCapabilities.OfType<Store.Model.Capabilities.FileType>().Except(x => string.IsNullOrEmpty(x.ID)))
                    {
                        foreach (var extension in fileType.Extensions.Except(x => string.IsNullOrEmpty(x.Value)))
                            fileAssocsKey.SetValue(extension.Value, FileType.RegKeyPrefix + fileType.ID);
                    }
                }

                using (var urlAssocsKey = capabilitiesKey.CreateSubKeyChecked(RegSubKeyUrlAssocs))
                {
                    foreach (var urlProtocol in verbCapabilities.OfType<Store.Model.Capabilities.UrlProtocol>())
                    {
                        foreach (var prefix in urlProtocol.KnownPrefixes)
                            urlAssocsKey.SetValue(prefix.Value, FileType.RegKeyPrefix + urlProtocol.ID);
                    }
                }

                using (var startMenuKey = capabilitiesKey.CreateSubKeyChecked(RegSubKeyStartMenu))
                {
                    foreach (var defaultProgram in verbCapabilities.OfType<Store.Model.Capabilities.DefaultProgram>().Except(x => string.IsNullOrEmpty(x.ID) || string.IsNullOrEmpty(x.Service)))
                        startMenuKey.SetValue(defaultProgram.Service, defaultProgram.ID);
                }
            }

            using (var regAppsKey = hive.CreateSubKeyChecked(RegKeyMachineRegisteredApplications))
                regAppsKey.SetValue(appRegistration.ID, /*CapabilityPrefix +*/ appRegistration.CapabilityRegPath);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes application registration from the current system.
        /// </summary>
        /// <param name="appRegistration">The registration information to be removed.</param>
        /// <param name="machineWide">Apply the registration machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">The data in <paramref name="appRegistration"/>.</exception>
        public static void Unregister([NotNull] Store.Model.Capabilities.AppRegistration appRegistration, bool machineWide)
        {
            #region Sanity checks
            if (appRegistration == null) throw new ArgumentNullException("appRegistration");
            #endregion

            if (string.IsNullOrEmpty(appRegistration.ID)) throw new InvalidDataException("Missing ID");
            if (string.IsNullOrEmpty(appRegistration.CapabilityRegPath)) throw new InvalidDataException("Invalid CapabilityRegPath");

            var hive = machineWide ? Registry.LocalMachine : Registry.CurrentUser;

            using (var regAppsKey = hive.OpenSubKey(RegKeyMachineRegisteredApplications, writable: true))
            {
                if (regAppsKey != null)
                    regAppsKey.DeleteValue(appRegistration.ID, throwOnMissingValue: false);
            }

            // TODO: Handle appRegistration.X64
            try
            {
                hive.DeleteSubKeyTree( /*CapabilityPrefix +*/ appRegistration.CapabilityRegPath);
            }
                #region Error handling
            catch (ArgumentException)
            {
                // Ignore missing registry keys
            }
            #endregion
        }
        #endregion
    }
}
