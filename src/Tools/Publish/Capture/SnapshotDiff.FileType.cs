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
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Publish.Capture
{
    partial class SnapshotDiff
    {
        /// <summary>
        /// Collects data about file types and also URL protocol handlers.
        /// </summary>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        public void CollectFileTypes([NotNull] CommandMapper commandMapper, [NotNull] CapabilityList capabilities)
        {
            #region Sanity checks
            if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
            if (commandMapper == null) throw new ArgumentNullException(nameof(commandMapper));
            #endregion

            capabilities.Entries.AddRange((
                from progID in ProgIDs
                where !string.IsNullOrEmpty(progID)
                select GetFileType(progID, commandMapper)).WhereNotNull());
        }

        /// <summary>
        /// Retrieves data about a specific file type or URL protocol from a snapshot diff.
        /// </summary>
        /// <param name="progID">The programatic identifier of the file type.</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <returns>Data about the file type or <see paramref="null"/> if no file type for this <paramref name="progID"/> was detected.</returns>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        [CanBeNull]
        private VerbCapability GetFileType([NotNull] string progID, [NotNull] CommandMapper commandMapper)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(progID)) throw new ArgumentNullException(nameof(progID));
            if (commandMapper == null) throw new ArgumentNullException(nameof(commandMapper));
            #endregion

            using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
            {
                if (progIDKey == null) return null;

                VerbCapability capability;
                if (progIDKey.GetValue(DesktopIntegration.Windows.UrlProtocol.ProtocolIndicator) == null)
                { // Normal file type
                    var fileType = new FileType {ID = progID};

                    foreach (var fileAssoc in FileAssocs.Where(fileAssoc => fileAssoc.Value == progID && !string.IsNullOrEmpty(fileAssoc.Key)))
                    {
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
                    capability = new UrlProtocol {ID = progID};
                }

                string description = progIDKey.GetValue(DesktopIntegration.Windows.FileType.RegValueFriendlyName, "").ToString();
                if (string.IsNullOrEmpty(description)) description = progIDKey.GetValue("", "").ToString();
                capability.Descriptions.Add(description);

                capability.Verbs.AddRange(GetVerbs(progIDKey, commandMapper));

                // Only return capabilities that have verbs associated with them
                return capability.Verbs.Count == 0 ? null : capability;
            }
        }
    }
}
