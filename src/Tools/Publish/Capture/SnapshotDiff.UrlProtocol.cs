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
using NanoByte.Common;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Publish.Capture
{
    partial class SnapshotDiff
    {
        /// <summary>
        /// Collects data about well-known URL protocol handlers.
        /// </summary>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        public void CollectProtocolAssocs([NotNull] CommandMapper commandMapper, [NotNull] CapabilityList capabilities)
        {
            #region Sanity checks
            if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
            if (commandMapper == null) throw new ArgumentNullException(nameof(commandMapper));
            #endregion

            foreach (string protocol in ProtocolAssocs.Select(protocolAssoc => protocolAssoc.Key))
            {
                using (var protocolKey = Registry.ClassesRoot.OpenSubKey(protocol))
                {
                    if (protocolKey == null) throw new IOException(protocol + " not found");
                    capabilities.Entries.Add(new UrlProtocol
                    {
                        ID = protocol,
                        Descriptions = {RegistryUtils.GetString(@"HKEY_CLASSES_ROOT\" + protocol, valueName: null, defaultValue: protocol)},
                        Verbs = {GetVerb(protocolKey, commandMapper, "open")}
                    });
                }
            }
        }
    }
}
