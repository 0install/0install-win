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
using Microsoft.Win32;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Collects data about file types and also URL protocol handlers indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectFileTypes(Snapshot snapshotDiff, CommandProvider commandProvider, CapabilityList capabilities)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            #endregion

            foreach (string progID in snapshotDiff.ProgIDs)
            {
                if (string.IsNullOrEmpty(progID)) continue;
                var fileType = GetFileType(progID, snapshotDiff, commandProvider);
                if (fileType != null) capabilities.Entries.Add(fileType);
            }
        }

        /// <summary>
        /// Retreives data about a specific file type or URL protocol from a snapshot diff.
        /// </summary>
        /// <param name="progID">The programatic identifier of the file type.</param>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <returns>Data about the file type or <see paramref="null"/> if no file type for this <paramref name="progID"/> was detected.</returns>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static VerbCapability GetFileType(string progID, Snapshot snapshotDiff, CommandProvider commandProvider)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(progID)) throw new ArgumentNullException("progID");
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            #endregion

            using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
            {
                if (progIDKey == null) return null;

                VerbCapability capability;
                if (progIDKey.GetValue(DesktopIntegration.Windows.UrlProtocol.ProtocolIndicator) == null)
                { // Normal file type
                    var fileType = new FileType
                    {
                        ID = progID,
                        Description = progIDKey.GetValue("", "").ToString()
                    };

                    foreach (var fileAssoc in snapshotDiff.FileAssocs)
                    {
                        if (fileAssoc.Value != progID || string.IsNullOrEmpty(fileAssoc.Key)) continue;

                        using (var assocKey = Registry.ClassesRoot.OpenSubKey(fileAssoc.Key))
                        {
                            if (assocKey == null) continue;

                            fileType.Extensions.Add(new FileTypeExtension
                            {
                                Value = fileAssoc.Key,
                                MimeType = assocKey.GetValue(DesktopIntegration.Windows.FileType.RegValueContentType, "").ToString(),
                                PerceivedType = assocKey.GetValue(DesktopIntegration.Windows.FileType.RegValuePerceivedType, "").ToString()
                            });
                        }
                    }

                    capability = fileType;
                }
                else
                { // URL protocol handler
                    capability = new UrlProtocol
                    {
                        ID = progID,
                        Description = progIDKey.GetValue("", "").ToString()
                    };
                }

                capability.Verbs.AddAll(GetVerbs(progIDKey, commandProvider));

                // Only return capabilities that have verbs associated with them
                if (capability.Verbs.IsEmpty) return null;

                return capability;
            }
        }
    }
}
