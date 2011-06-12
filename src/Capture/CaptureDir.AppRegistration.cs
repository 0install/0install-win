/*
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
using System.Security;
using Common;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Capture.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Retreives data about registered applications aindicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <param name="appDescription">Returns user-friendly description of the application; <see langword="null"/> if the description was not found.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static AppRegistration GetAppRegistration(Snapshot snapshotDiff, CommandProvider commandProvider, CapabilityList capabilities, out string appDescription)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            #endregion

            // Ambiguity warnings
            if (snapshotDiff.RegisteredApplications.Length == 0)
            {
                appDescription = null;
                return null;
            }
            if (snapshotDiff.RegisteredApplications.Length > 1)
                Log.Warn(Resources.MultipleRegisteredAppsDetected);

            // Get registry path pointer
            string appRegName = snapshotDiff.RegisteredApplications[0];
            string capabilitiesRegPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\" + DesktopIntegration.Windows.AppRegistration.RegKeyMachineRegisteredApplications, appRegName, "") as string;
            if (string.IsNullOrEmpty(capabilitiesRegPath))
            {
                appDescription = null;
                return null;
            }

            bool x64;
            using (var capsKey = WindowsUtils.OpenHklmKey(capabilitiesRegPath, out x64))
            {
                if (capsKey == null)
                {
                    Log.Warn(string.Format(Resources.InvalidCapabilitiesRegistryPath, capabilitiesRegPath));
                    appDescription = null;
                    return null;
                }

                appDescription = capsKey.GetValue(DesktopIntegration.Windows.AppRegistration.RegValueAppDescription, "").ToString();

                CollectProtocolAssocsEx(capsKey, commandProvider, capabilities);
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
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectProtocolAssocsEx(RegistryKey capsKey, CommandProvider commandProvider, CapabilityList capabilities)
        {
            #region Sanity checks
            if (capsKey == null) throw new ArgumentNullException("capsKey");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            #endregion

            using (var urlAssocKey = capsKey.OpenSubKey(DesktopIntegration.Windows.AppRegistration.RegSubKeyUrlAssocs))
            {
                if (urlAssocKey == null) return;

                foreach (string protocol in urlAssocKey.GetValueNames())
                {
                    string progID = urlAssocKey.GetValue(protocol, "").ToString();
                    using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
                    {
                        if (progIDKey == null) continue;

                        var capability = new UrlProtocol
                        {
                            ID = progID,
                            Description = progIDKey.GetValue("", "").ToString(),
                            Prefix = protocol
                        };

                        capability.Verbs.AddAll(GetVerbs(progIDKey, commandProvider));
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
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectFileAssocsEx(RegistryKey capsKey, CapabilityList capabilities)
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
                    string progID = fileAssocKey.GetValue(extension, "").ToString();
                    AddExtensionToFileType(extension, progID, capabilities);
                }
            }
        }

        /// <summary>
        /// Adds an extension to an existing <see cref="FileType"/>.
        /// </summary>
        /// <param name="extension">The file extension including the leading dot (e.g. ".png").</param>
        /// <param name="progID">The ID of the <see cref="FileType"/> to add the extension to.</param>
        /// <param name="capabilities">The list of capabilities to find existing <see cref="FileType"/>s in.</param>
        private static void AddExtensionToFileType(string extension, string progID, CapabilityList capabilities)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(progID)) throw new ArgumentNullException("progID");
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException("extension");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            #endregion

            foreach (var capability in capabilities.Entries)
            {
                // Find the matching existing file type
                var fileType = capability as FileType;
                if (fileType != null && fileType.ID == progID)
                {
                    // Check if the file type already has the extension and add it if not
                    FileTypeExtension temp;
                    if (!fileType.Extensions.Find(element => element.Value == extension, out temp))
                        fileType.Extensions.Add(new FileTypeExtension {Value = extension});
                    break;
                }
            }
        }
        #endregion
    }
}
