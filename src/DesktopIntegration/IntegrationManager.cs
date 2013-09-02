﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Common;
using Common.Collections;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages an <see cref="AppList"/> and desktop integration via <see cref="AccessPoint"/>s.
    /// </summary>
    /// <remarks>
    /// To prevent raceconditions there may only be one desktop integration class instance active at any given time.
    /// This class aquires a mutex upon calling its constructor and releases it upon calling <see cref="Dispose()"/>.
    /// </remarks>
    public class IntegrationManager : IntegrationManagerBase, IDisposable
    {
        #region Constants
        /// <summary>
        /// The name of the cross-process mutex used to signal that a desktop integration process class is currently active.
        /// </summary>
        public const string MutexName = "ZeroInstall.DesktopIntegration";
        #endregion

        #region Variables
        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        protected readonly string AppListPath;

        /// <summary>A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</summary>
        protected readonly ITaskHandler Handler;

        /// <summary>Prevents multiple processes from performing desktop integration operations simultaneously.</summary>
        private readonly Mutex _mutex;

        /// <summary>The window message ID (for use with <see cref="WindowsUtils.BroadcastMessage"/>) that signals integration changes to interested observers.</summary>
        public static readonly int ChangedWindowMessageID = WindowsUtils.RegisterWindowMessage(MutexName);
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new integration manager using a custom <see cref="DesktopIntegration.AppList"/>. Do not use directly except for testing purposes!
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="appListPath">The storage location of the <see cref="AppList"/> file.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool machineWide, string appListPath, ITaskHandler handler)
        {
            MachineWide = machineWide;
            AppListPath = appListPath;
            Handler = handler;

            if (File.Exists(AppListPath)) AppList = XmlStorage.LoadXml<AppList>(AppListPath);
            else
            {
                AppList = new AppList();
                AppList.SaveXml(AppListPath);
            }
        }

        /// <summary>
        /// Creates a new integration manager using the default <see cref="DesktopIntegration.AppList"/>.
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool machineWide, ITaskHandler handler)
        {
            // Prevent multiple concurrent desktop integration operations
            _mutex = new Mutex(false, machineWide ? @"Global\" + MutexName : MutexName);
            if (!_mutex.WaitOne(1000, false))
            {
                _mutex = null; // Don't try to release mutex if it wasn't acquired
                throw new UnauthorizedAccessException(Resources.IntegrationMutex);
            }

            MachineWide = machineWide;
            AppListPath = AppList.GetDefaultPath(machineWide);
            Handler = handler;

            if (File.Exists(AppListPath)) AppList = XmlStorage.LoadXml<AppList>(AppListPath);
            else
            {
                AppList = new AppList();
                AppList.SaveXml(AppListPath);
            }
        }
        #endregion

        //--------------------//

        #region Apps
        /// <inheritdoc/>
        protected override AppEntry AddAppInternal(string interfaceID, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            // Prevent double entries
            if (AppList.Contains(interfaceID)) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, feed.Name));

            // Get basic metadata and copy of capabilities from feed
            var appEntry = new AppEntry {InterfaceID = interfaceID, Name = feed.Name, Timestamp = DateTime.UtcNow};
            appEntry.CapabilityLists.AddAll(feed.CapabilityLists.Map(list => list.Clone()));

            AppList.Entries.Add(appEntry);
            WriteAppDir(appEntry);
            return appEntry;
        }

        /// <inheritdoc/>
        protected override AppEntry AddAppInternal(string petName, Requirements requirements, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(petName)) throw new ArgumentNullException("petName");
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            // Prevent double entries
            if (AppList.Contains(petName)) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, feed.Name));

            // Get basic metadata and copy of capabilities from feed
            var appEntry = new AppEntry {InterfaceID = petName, Requirements = requirements, Name = feed.Name, Timestamp = DateTime.UtcNow};
            appEntry.CapabilityLists.AddAll(feed.CapabilityLists.Map(list => list.Clone()));

            AppList.Entries.Add(appEntry);
            WriteAppDir(appEntry);
            return appEntry;
        }

        /// <inheritdoc/>
        protected override void AddAppInternal(AppEntry prototype, Converter<string, Feed> feedRetriever)
        {
            #region Sanity checks
            if (prototype == null) throw new ArgumentNullException("prototype");
            if (feedRetriever == null) throw new ArgumentNullException("feedRetriever");
            #endregion

            var appEntry = prototype.Clone();
            AppList.Entries.Add(appEntry);
            WriteAppDir(appEntry);

            if (appEntry.AccessPoints != null)
                AddAccessPointsInternal(appEntry, feedRetriever(appEntry.InterfaceID), appEntry.AccessPoints.Clone().Entries);
        }

        /// <inheritdoc/>
        protected override void RemoveAppInternal(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            DeleteAppDir(appEntry);

            if (appEntry.AccessPoints != null)
            {
                // Unapply any remaining access points
                foreach (var accessPoint in appEntry.AccessPoints.Entries)
                    accessPoint.Unapply(appEntry, MachineWide);
            }

            AppList.Entries.Remove(appEntry);
        }

        /// <inheritdoc/>
        protected override void UpdateAppInternal(AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            // Temporarily remove capability-based access points but remember them for later reapplication
            var toReapply = new List<AccessPoint>();
            if (appEntry.AccessPoints != null)
                toReapply.AddRange(appEntry.AccessPoints.Entries.Filter(accessPoint => accessPoint is DefaultAccessPoint || accessPoint is CapabilityRegistration));
            RemoveAccessPointsInternal(appEntry, toReapply);

            // Update metadata and capabilities
            appEntry.Name = feed.Name;
            appEntry.CapabilityLists.Clear();
            appEntry.CapabilityLists.AddAll(feed.CapabilityLists.Map(list => list.Clone()));

            // Reapply removed access points dumping any that have become incompatible
            foreach (var accessPoint in toReapply)
            {
                try
                {
                    AddAccessPointsInternal(appEntry, feed, new[] {accessPoint});
                }
                    #region Error handling
                catch (KeyNotFoundException)
                {
                    Log.Warn(string.Format("Access point '{0}' no longer compatible with interface '{1}'.", accessPoint, appEntry.InterfaceID));
                }
                #endregion
            }

            WriteAppDir(appEntry);
            appEntry.Timestamp = DateTime.UtcNow;
        }
        #endregion

        #region AccessPoint
        /// <inheritdoc/>
        protected override void AddAccessPointsInternal(AppEntry appEntry, Feed feed, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            if (appEntry.AccessPoints != null && appEntry.AccessPoints.Entries == accessPoints) throw new ArgumentException("Must not be equal to appEntry.AccessPoints.Entries", "accessPoints");
            #endregion

            // Skip entries with mismatching hostname
            if (appEntry.Hostname != null && !Regex.IsMatch(Environment.MachineName, appEntry.Hostname)) return;

            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            // ReSharper disable PossibleMultipleEnumeration
            CheckForConflicts(appEntry, accessPoints);

            accessPoints.ApplyWithRollback(
                accessPoint => accessPoint.Apply(appEntry, feed, MachineWide, Handler),
                accessPoint =>
                {
                    // Don't perform rollback if the access point was already applied previously and this was only a refresh
                    if (!appEntry.AccessPoints.Entries.Contains(accessPoint))
                        accessPoint.Unapply(appEntry, MachineWide);
                });

            // Add the access points to the AppList
            foreach (var accessPoint in accessPoints)
                appEntry.AccessPoints.Entries.UpdateOrAdd(accessPoint); // Replace pre-existing entries
            appEntry.Timestamp = DateTime.UtcNow;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <inheritdoc/>
        protected override void RemoveAccessPointsInternal(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            if (appEntry.AccessPoints == null) return;

            // ReSharper disable PossibleMultipleEnumeration
            foreach (var accessPoint in accessPoints)
                accessPoint.Unapply(appEntry, MachineWide);

            // Remove the access points from the AppList
            appEntry.AccessPoints.Entries.RemoveAll(accessPoints);
            appEntry.Timestamp = DateTime.UtcNow;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <inheritdoc/>
        protected override void RepairAppInternal(AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var toReAdd = (appEntry.AccessPoints == null)
                ? new AccessPoint[0]
                : appEntry.AccessPoints.Entries.ToArray();
            AddAccessPointsInternal(appEntry, feed, toReAdd);

            WriteAppDir(appEntry);
        }
        #endregion

        #region Finish
        /// <inheritdoc/>
        protected override void Finish()
        {
            try
            {
                AppList.SaveXml(AppListPath);
            }
            catch (IOException)
            {
                // Bypass race conditions where another thread is reading an old version of the list while we are trying to write a new one
                Thread.Sleep(1000);
                AppList.SaveXml(AppListPath);
            }

            WindowsUtils.NotifyAssocChanged(); // Notify Windows Explorer of changes
            WindowsUtils.BroadcastMessage(ChangedWindowMessageID); // Notify Zero Install GUIs of changes
        }
        #endregion

        #region AppDir
        private void WriteAppDir(AppEntry appEntry)
        {
            // TODO: Implement
        }

        private void DeleteAppDir(AppEntry appEntry)
        {
            // TODO: Implement
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~IntegrationManager()
        {
            Dispose(false);
        }

        // ReSharper disable UnusedParameter.Global
        /// <summary>
        /// Releases the mutex and any unmanaged resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called manually and not by the garbage collector.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_mutex != null)
            {
                if (disposing) _mutex.ReleaseMutex();
                _mutex.Close();
            }
        }

        // ReSharper restore UnusedParameter.Global
        #endregion
    }
}
