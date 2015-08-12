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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages an <see cref="AppList"/> and desktop integration via <see cref="AccessPoint"/>s.
    /// </summary>
    /// <remarks>
    /// To prevent raceconditions there may only be one desktop integration class instance active at any given time.
    /// This class acquires a mutex upon calling its constructor and releases it upon calling <see cref="IDisposable.Dispose"/>.
    /// </remarks>
    public class IntegrationManager : IntegrationManagerBase
    {
        #region Constants
        /// <summary>
        /// The name of the cross-process mutex used to signal that a desktop integration process class is currently active.
        /// </summary>
        protected override string MutexName { get { return "ZeroInstall.DesktopIntegration"; } }

        /// <summary>
        /// The window message ID (for use with <see cref="WindowsUtils.BroadcastMessage"/>) that signals integration changes to interested observers.
        /// </summary>
        public static readonly int ChangedWindowMessageID = WindowsUtils.RegisterWindowMessage("ZeroInstall.DesktopIntegration");
        #endregion

        /// <summary>
        /// The storage location of the <see cref="AppList"/> file.
        /// </summary>
        public string AppListPath { get; private set; }

        #region Constructor
        /// <summary>
        /// Creates a new integration manager using the default <see cref="DesktopIntegration.AppList"/> (creating a new one if missing). Performs Mutex-based locking!
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">A problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to the <see cref="AppList"/> file is not permitted or another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        public IntegrationManager([NotNull] ITaskHandler handler, bool machineWide = false)
            : base(handler, machineWide)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            try
            {
                AquireMutex();
            }
                #region Error handling
            catch (UnauthorizedAccessException)
            {
                // Replace exception to add more context
                throw new UnauthorizedAccessException(Resources.IntegrationMutex);
            }
            #endregion

            try
            {
                AppListPath = AppList.GetDefaultPath(machineWide);
                if (File.Exists(AppListPath))
                {
                    Log.Debug("Loading AppList for IntegrationManager from: " + AppListPath);
                    AppList = XmlStorage.LoadXml<AppList>(AppListPath);
                }
                else
                {
                    Log.Debug("Creating new AppList for IntegrationManager: " + AppListPath);
                    AppList = new AppList();
                    AppList.SaveXml(AppListPath);
                }
            }
                #region Error handling
            catch
            {
                // Avoid abandoned mutexes
                Dispose();
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Creates a new integration manager using a custom <see cref="DesktopIntegration.AppList"/>. Uses no mutex!
        /// </summary>
        /// <param name="appListPath">The storage location of the <see cref="AppList"/> file.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="FileNotFoundException"><paramref name="appListPath"/> does not existing.</exception>
        /// <exception cref="IOException">A problem occurs while accessing <paramref name="appListPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to <paramref name="appListPath"/> file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        public IntegrationManager([NotNull] string appListPath, [NotNull] ITaskHandler handler, bool machineWide = false)
            : base(handler, machineWide)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appListPath)) throw new ArgumentNullException(nameof(appListPath));
            if (appListPath == null) throw new ArgumentNullException(nameof(appListPath));
            #endregion

            AppListPath = appListPath;
            AppList = XmlStorage.LoadXml<AppList>(AppListPath);
        }
        #endregion

        //--------------------//

        #region Apps
        /// <inheritdoc/>
        protected override AppEntry AddAppInternal(FeedTarget target)
        {
            // Prevent double entries
            if (AppList.ContainsEntry(target.Uri)) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, target.Feed.Name));

            // Get basic metadata and copy of capabilities from feed
            var appEntry = new AppEntry {InterfaceUri = target.Uri, Name = target.Feed.Name, Timestamp = DateTime.UtcNow};
            appEntry.CapabilityLists.AddRange(target.Feed.CapabilityLists.CloneElements());

            AppList.Entries.Add(appEntry);
            WriteAppDir(appEntry);
            return appEntry;
        }

        /// <inheritdoc/>
        protected override AppEntry AddAppInternal(string petName, Requirements requirements, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(petName)) throw new ArgumentNullException(nameof(petName));
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            throw new NotImplementedException();
            /*
            // Prevent double entries
            if (AppList.ContainsEntry(petName)) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, feed.Name));

            // Get basic metadata and copy of capabilities from feed
            var appEntry = new AppEntry {InterfaceUri = petName, Requirements = requirements, Name = feed.Name, Timestamp = DateTime.UtcNow};
            appEntry.CapabilityLists.AddRange(feed.CapabilityLists.CloneElements());

            AppList.Entries.Add(appEntry);
            WriteAppDir(appEntry);
            return appEntry;
            */
        }

        /// <inheritdoc/>
        protected override void AddAppInternal(AppEntry prototype, Converter<FeedUri, Feed> feedRetriever)
        {
            #region Sanity checks
            if (prototype == null) throw new ArgumentNullException(nameof(prototype));
            if (feedRetriever == null) throw new ArgumentNullException(nameof(feedRetriever));
            #endregion

            var appEntry = prototype.Clone();
            AppList.Entries.Add(appEntry);
            WriteAppDir(appEntry);

            if (appEntry.AccessPoints != null)
                AddAccessPointsInternal(appEntry, feedRetriever(appEntry.InterfaceUri), appEntry.AccessPoints.Clone().Entries);
        }

        /// <inheritdoc/>
        protected override void RemoveAppInternal(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
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
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            // Temporarily remove capability-based access points but remember them for later reapplication
            var toReapply = new List<AccessPoint>();
            if (appEntry.AccessPoints != null)
                toReapply.AddRange(appEntry.AccessPoints.Entries.Where(accessPoint => accessPoint is DefaultAccessPoint || accessPoint is CapabilityRegistration));
            RemoveAccessPointsInternal(appEntry, toReapply);

            // Update metadata and capabilities
            appEntry.Name = feed.Name;
            appEntry.CapabilityLists.Clear();
            appEntry.CapabilityLists.AddRange(feed.CapabilityLists.CloneElements());

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
                    Log.Warn(string.Format("Access point '{0}' no longer compatible with interface '{1}'.", accessPoint, appEntry.InterfaceUri));
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
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            if (accessPoints == null) throw new ArgumentNullException(nameof(accessPoints));
            if (appEntry.AccessPoints != null && ReferenceEquals(appEntry.AccessPoints.Entries, accessPoints)) throw new ArgumentException("Must not be equal to appEntry.AccessPoints.Entries", nameof(accessPoints));
            #endregion

            // Skip entries with mismatching hostname
            if (appEntry.Hostname != null && !Regex.IsMatch(Environment.MachineName, appEntry.Hostname)) return;

            if (appEntry.AccessPoints == null) appEntry.AccessPoints = new AccessPointList();

            AppList.CheckForConflicts(accessPoints, appEntry);

            accessPoints.ApplyWithRollback(
                accessPoint => accessPoint.Apply(appEntry, feed, Handler, MachineWide),
                accessPoint =>
                {
                    // Don't perform rollback if the access point was already applied previously and this was only a refresh
                    if (!appEntry.AccessPoints.Entries.Contains(accessPoint))
                        accessPoint.Unapply(appEntry, MachineWide);
                });

            appEntry.AccessPoints.Entries.RemoveRange(accessPoints); // Replace pre-existing entries
            appEntry.AccessPoints.Entries.AddRange(accessPoints);
            appEntry.Timestamp = DateTime.UtcNow;
        }

        /// <inheritdoc/>
        protected override void RemoveAccessPointsInternal(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            if (accessPoints == null) throw new ArgumentNullException(nameof(accessPoints));
            #endregion

            if (appEntry.AccessPoints == null) return;

            foreach (var accessPoint in accessPoints)
                accessPoint.Unapply(appEntry, MachineWide);

            // Remove the access points from the AppList
            appEntry.AccessPoints.Entries.RemoveRange(accessPoints);
            appEntry.Timestamp = DateTime.UtcNow;
        }

        /// <inheritdoc/>
        protected override void RepairAppInternal(AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            var toReAdd = (appEntry.AccessPoints == null)
                ? Enumerable.Empty<AccessPoint>()
                : appEntry.AccessPoints.Entries.ToList();
            AddAccessPointsInternal(appEntry, feed, toReAdd);

            WriteAppDir(appEntry);
        }
        #endregion

        #region Finish
        /// <inheritdoc/>
        protected override void Finish()
        {
            Log.Debug("Saving AppList to: " + AppListPath);
            // Retry to handle race conditions with read-only access to the file
            ExceptionUtils.Retry<IOException>(delegate { AppList.SaveXml(AppListPath); });

            WindowsUtils.NotifyAssocChanged(); // Notify Windows Explorer of changes
            WindowsUtils.BroadcastMessage(ChangedWindowMessageID); // Notify Zero Install GUIs of changes
        }
        #endregion

        #region AppDir
        private static void WriteAppDir(AppEntry appEntry)
        {
            // TODO: Implement
        }

        private static void DeleteAppDir(AppEntry appEntry)
        {
            // TODO: Implement
        }
        #endregion
    }
}
