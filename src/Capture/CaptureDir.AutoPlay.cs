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
using System.Security;
using Common.Collections;
using Microsoft.Win32;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;
using Windows = ZeroInstall.DesktopIntegration.Windows;

namespace ZeroInstall.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Collects data about AutoPlay handlers indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectAutoPlays(Snapshot snapshotDiff, CommandProvider commandProvider, CapabilityList capabilities)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            #endregion

            foreach (string handler in snapshotDiff.AutoPlayHandlersUser)
            {
                var autoPlay = GetAutoPlay(handler, Registry.CurrentUser, snapshotDiff.AutoPlayAssocsUser, commandProvider);
                if (autoPlay != null) capabilities.Entries.Add(autoPlay);
            }

            foreach (string handler in snapshotDiff.AutoPlayHandlersMachine)
            {
                var autoPlay = GetAutoPlay(handler, Registry.LocalMachine, snapshotDiff.AutoPlayAssocsMachine, commandProvider);
                if (autoPlay != null) capabilities.Entries.Add(autoPlay);
            }
        }

        /// <summary>
        /// Retreives data about a AutoPlay handler type from a snapshot diff.
        /// </summary>
        /// <param name="handler">The internal name of the AutoPlay handler.</param>
        /// <param name="hive">The registry hive to search in (usually HKCU or HKLM).</param>
        /// <param name="autoPlayAssocs">A list of associations of an AutoPlay events with an AutoPlay handlers</param>
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static Capability GetAutoPlay(string handler, RegistryKey hive, IEnumerable<ComparableTuple<string>> autoPlayAssocs, CommandProvider commandProvider)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            if (hive == null) throw new ArgumentNullException("hive");
            if (autoPlayAssocs == null) throw new ArgumentNullException("autoPlayAssocs");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            #endregion

            using (var handlerKey = hive.OpenSubKey(Windows.AutoPlay.RegKeyHandlers + @"\" + handler))
            {
                if (handlerKey == null) return null;

                string progID = handlerKey.GetValue(Windows.AutoPlay.RegValueProgID, "").ToString();
                string verbName = handlerKey.GetValue(Windows.AutoPlay.RegValueVerb, "").ToString();

                using (var progIDKey = Registry.ClassesRoot.OpenSubKey(progID))
                {
                    var autoPlay = new AutoPlay
                    {
                        ID = handler,
                        Provider = handlerKey.GetValue(Windows.AutoPlay.RegValueProvider, "").ToString(),
                        Description = handlerKey.GetValue(Windows.AutoPlay.RegValueDescription, "").ToString(),
                        ProgID = progID,
                        Verb = GetVerb(progIDKey, commandProvider, verbName)
                    };

                    foreach (var autoPlayAssoc in autoPlayAssocs)
                        if (autoPlayAssoc.Value == handler) autoPlay.Events.Add(new AutoPlayEvent { Name = autoPlayAssoc.Key });

                    return autoPlay;
                }
            }
        }
    }
}
