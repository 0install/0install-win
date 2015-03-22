﻿/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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
using System.Security;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Publish.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Retrieves data about registered applications aindicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <param name="appName">Is set to the name of the application as displayed to the user; unchanged if the name was not found.</param>
        /// <param name="appDescription">Is set to a user-friendly description of the application; unchanged if the name was not found.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static AppRegistration GetAppRegistration([NotNull] Snapshot snapshotDiff, [NotNull] CommandMapper commandMapper, [NotNull] CapabilityList capabilities, ref string appName, ref string appDescription)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commandMapper == null) throw new ArgumentNullException("commandMapper");
            #endregion

            // Ambiguity warnings
            if (snapshotDiff.RegisteredApplications.Length == 0)
                return null;
            if (snapshotDiff.RegisteredApplications.Length > 1)
                Log.Warn(Resources.MultipleRegisteredAppsDetected);

            // Get registry path pointer
            string appRegName = snapshotDiff.RegisteredApplications[0];
            var capabilitiesRegPath = RegistryUtils.GetString(@"HKEY_LOCAL_MACHINE\" + DesktopIntegration.Windows.AppRegistration.RegKeyMachineRegisteredApplications, appRegName);
            if (string.IsNullOrEmpty(capabilitiesRegPath))
                return null;

            bool x64;
            using (var capsKey = RegistryUtils.OpenHklmKey(capabilitiesRegPath, out x64))
            {
                if (string.IsNullOrEmpty(appName)) appName = capsKey.GetValue(DesktopIntegration.Windows.AppRegistration.RegValueAppName, "").ToString();
                if (string.IsNullOrEmpty(appDescription)) appDescription = capsKey.GetValue(DesktopIntegration.Windows.AppRegistration.RegValueAppDescription, "").ToString();

                CollectProtocolAssocsEx(capsKey, commandMapper, capabilities);
                CollectFileAssocsEx(capsKey, capabilities);
                // Note: Contenders for StartMenu entries are detected elsewhere

                return new AppRegistration
                {
                    ID = appRegName,
                    CapabilityRegPath = capabilitiesRegPath,
                    X64 = x64
                };
            }
        }

        #region Protocols
        /// <summary>
        /// Collects data about URL protocol handlers indicated by registered application capabilities.
        /// </summary>
        /// <param name="capsKey">A registry key containing capability information for a registered application.</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static void CollectProtocolAssocsEx([NotNull] RegistryKey capsKey, [NotNull] CommandMapper commandMapper, [NotNull] CapabilityList capabilities)
        {
            #region Sanity checks
            if (capsKey == null) throw new ArgumentNullException("capsKey");
            if (commandMapper == null) throw new ArgumentNullException("commandMapper");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            #endregion

            using (var urlAssocKey = capsKey.OpenSubKey(DesktopIntegration.Windows.AppRegistration.RegSubKeyUrlAssocs))
            {
                if (urlAssocKey == null) return;

                // TODO: Fold multiple prefixes pointing to one protocol together
                foreach (string protocol in urlAssocKey.GetValueNames())
                {
                    string progID = urlAssocKey.GetValue(protocol, "").ToString();
                    using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
                    {
                        if (progIDKey == null) continue;

                        var capability = new UrlProtocol
                        {
                            ID = progID,
                            Descriptions = {progIDKey.GetValue("", "").ToString()},
                            KnownPrefixes = {new KnownProtocolPrefix {Value = protocol}}
                        };

                        capability.Verbs.AddRange(GetVerbs(progIDKey, commandMapper));
                        capabilities.Entries.Add(capability);
                    }
                }
            }
        }
        #endregion

        #region File associations
        /// <summary>
        /// Collects data about file assocations indicated by registered application capabilities.
        /// </summary>
        /// <param name="capsKey">A registry key containing capability information for a registered application.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static void CollectFileAssocsEx([NotNull] RegistryKey capsKey, [NotNull] CapabilityList capabilities)
        {
            #region Sanity checks
            if (capsKey == null) throw new ArgumentNullException("capsKey");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            #endregion

            using (var fileAssocKey = capsKey.OpenSubKey(DesktopIntegration.Windows.AppRegistration.RegSubKeyFileAssocs))
            {
                if (fileAssocKey == null) return;

                foreach (string extension in fileAssocKey.GetValueNames())
                {
                    var progID = fileAssocKey.GetValue(extension, "") as string;
                    if (!string.IsNullOrEmpty(progID)) AddExtensionToFileType(extension, progID, capabilities);
                }
            }
        }

        /// <summary>
        /// Adds an extension to an existing <see cref="FileType"/>.
        /// </summary>
        /// <param name="extension">The file extension including the leading dot (e.g. ".png").</param>
        /// <param name="progID">The ID of the <see cref="FileType"/> to add the extension to.</param>
        /// <param name="capabilities">The list of capabilities to find existing <see cref="FileType"/>s in.</param>
        private static void AddExtensionToFileType([NotNull] string extension, [NotNull] string progID, [NotNull] CapabilityList capabilities)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(progID)) throw new ArgumentNullException("progID");
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException("extension");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            #endregion

            // Find the matching existing file type
            var fileType = capabilities.Entries.OfType<FileType>().FirstOrDefault(type => type.ID == progID);

            if (fileType != null)
            {
                // Check if the file type already has the extension and add it if not
                if (!fileType.Extensions.Any(element => StringUtils.EqualsIgnoreCase(element.Value, extension)))
                    fileType.Extensions.Add(new FileTypeExtension {Value = extension.ToLower()});
            }
        }
        #endregion
    }
}
