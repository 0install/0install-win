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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Collects data about AutoPlay handlers indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static void CollectAutoPlays([NotNull] Snapshot snapshotDiff, [NotNull] CommandMapper commandMapper, [NotNull] CapabilityList capabilities)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commandMapper == null) throw new ArgumentNullException("commandMapper");
            #endregion

            capabilities.Entries.AddRange(snapshotDiff.AutoPlayHandlersUser
                .Select(handler => GetAutoPlay(handler, Registry.CurrentUser, snapshotDiff.AutoPlayAssocsUser, commandMapper))
                .WhereNotNull());

            capabilities.Entries.AddRange(snapshotDiff.AutoPlayHandlersMachine
                .Select(handler => GetAutoPlay(handler, Registry.LocalMachine, snapshotDiff.AutoPlayAssocsMachine, commandMapper))
                .WhereNotNull());
        }

        /// <summary>
        /// Retrieves data about a AutoPlay handler type from a snapshot diff.
        /// </summary>
        /// <param name="handler">The internal name of the AutoPlay handler.</param>
        /// <param name="hive">The registry hive to search in (usually HKCU or HKLM).</param>
        /// <param name="autoPlayAssocs">A list of associations of an AutoPlay events with an AutoPlay handlers</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Read access to the registry was not permitted.</exception>
        private static Capability GetAutoPlay([NotNull] string handler, [NotNull] RegistryKey hive, [NotNull, ItemNotNull] IEnumerable<ComparableTuple<string>> autoPlayAssocs, [NotNull] CommandMapper commandMapper)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            if (hive == null) throw new ArgumentNullException("hive");
            if (autoPlayAssocs == null) throw new ArgumentNullException("autoPlayAssocs");
            if (commandMapper == null) throw new ArgumentNullException("commandMapper");
            #endregion

            using (var handlerKey = hive.OpenSubKey(DesktopIntegration.Windows.AutoPlay.RegKeyHandlers + @"\" + handler))
            {
                if (handlerKey == null) return null;

                string progID = handlerKey.GetValue(DesktopIntegration.Windows.AutoPlay.RegValueProgID, "").ToString();
                string verbName = handlerKey.GetValue(DesktopIntegration.Windows.AutoPlay.RegValueVerb, "").ToString();

                using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
                {
                    if (progIDKey == null) throw new IOException(progID + " key not found");
                    var autoPlay = new AutoPlay
                    {
                        ID = handler,
                        Provider = handlerKey.GetValue(DesktopIntegration.Windows.AutoPlay.RegValueProvider, "").ToString(),
                        Descriptions = {handlerKey.GetValue(DesktopIntegration.Windows.AutoPlay.RegValueDescription, "").ToString()},
                        Verb = GetVerb(progIDKey, commandMapper, verbName)
                    };

                    autoPlay.Events.AddRange(
                        from autoPlayAssoc in autoPlayAssocs
                        where autoPlayAssoc.Value == handler
                        select new AutoPlayEvent {Name = autoPlayAssoc.Key});

                    return autoPlay;
                }
            }
        }
    }
}
