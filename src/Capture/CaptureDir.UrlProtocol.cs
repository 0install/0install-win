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
using System.Collections.Generic;
using System.IO;
using System.Security;
using Common.Collections;
using Microsoft.Win32;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Collects data about well-known URL protocol handlers indicated by a snapshot diff.
        /// </summary>
        /// <param name="protocolAssocs">A list of protocol associations for well-known protocols (e.g. HTTP, FTP, ...).</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectProtocolAssocs(IEnumerable<ComparableTuple<string>> protocolAssocs, CommandMapper commandMapper, CapabilityList capabilities)
        {
            #region Sanity checks
            if (protocolAssocs == null) throw new ArgumentNullException("protocolAssocs");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commandMapper == null) throw new ArgumentNullException("commandMapper");
            #endregion

            foreach (var protocolAssoc in protocolAssocs)
            {
                string protocol = protocolAssoc.Key;
                using (var protocolKey = Registry.ClassesRoot.OpenSubKey(protocol))
                {
                    capabilities.Entries.Add(new UrlProtocol
                    {
                        ID = protocol,
                        Descriptions = {Registry.GetValue(@"HKEY_CLASSES_ROOT\" + protocol, "", "") as string},
                        Verbs = {GetVerb(protocolKey, commandMapper, "open")}
                    });
                }
            }
        }
    }
}
