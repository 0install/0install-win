/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Net;
using System.Threading;
using Common;
using Common.Storage;
using Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages desktop integration via <see cref="AccessPoint"/>s.
    /// </summary>
    /// <remarks>
    /// To prevent raceconditions there may only be one desktop integration class active at any given time.
    /// This class becomes active upon calling its constructor and becomes inactive upon calling <see cref="Dispose()"/>.
    /// </remarks>
    public partial class IntegrationManager : IIntegrationManager, IDisposable
    {
        #region Constants
        /// <summary>
        /// The name of the cross-process mutex used to signal that a desktop integration process class is currently active.
        /// </summary>
        public const string MutexName = "ZeroInstall.DesktopIntegration";
        #endregion

        #region Variables
        /// <summary>Apply operations system-wide instead of just for the current user.</summary>
        protected readonly bool SystemWide;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        protected readonly string AppListPath;

        /// <summary>Prevents multiple processes from performing desktop integration operations simultaneously.</summary>
        private readonly Mutex _mutex;
        #endregion

        #region Properties
        /// <summary>
        /// Stores a list of applications and their desktop integrations. Do not modify this externally! Use this class' methods instead.
        /// </summary>
        public AppList AppList { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new integration manager using a custom <see cref="DesktopIntegration.AppList"/>. Do not use directly unless for testing purposes!
        /// </summary>
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        /// <param name="appListPath">The storage location of the <see cref="AppList"/> file.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool systemWide, string appListPath)
        {
            SystemWide = systemWide;
            AppListPath = appListPath;

            if (File.Exists(AppListPath)) AppList = AppList.Load(AppListPath);
            else
            {
                AppList = new AppList();
                AppList.Save(AppListPath);
            }
        }

        /// <summary>
        /// Creates a new integration manager using the default <see cref="DesktopIntegration.AppList"/>.
        /// </summary>
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool systemWide)
            : this(systemWide, Path.Combine(Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration"), "app-list.xml"))
        {
            // Prevent multiple concurrent desktop integration operations
            _mutex = new Mutex(true, systemWide ? @"Global\" + MutexName : MutexName);
            if (!_mutex.WaitOne(1000))
            {
                _mutex = null; // Don't try to release mutex if it wasn't acquired
                throw new UnauthorizedAccessException(Resources.IntegrationMutex);
            }
        }
        #endregion

        //--------------------//

        #region Apps
        /// <summary>
        /// Adds an application to the application list.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public void AddApp(InterfaceFeed target)
        {
            // Prevent double entries
            AppEntry existingEntry = FindAppEntry(target.InterfaceID);
            if (existingEntry != null) throw new InvalidOperationException(string.Format(Resources.AppAlreadyInList, existingEntry.Name));

            AppList.Entries.Add(BuildAppEntry(target));
            Complete();
        }

        /// <summary>
        /// Removes an application from the application list.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        public void RemoveApp(string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));

            RemoveAppEntry(appEntry);
            Complete();
        }
        #endregion

        #region AccessPoints
        /// <summary>
        /// Applies <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="toAdd">The <see cref="AccessPoint"/>s to apply.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="toAdd"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        public void AddAccessPoints(InterfaceFeed target, IEnumerable<AccessPoint> toAdd, ITaskHandler handler)
        {
            #region Sanity checks
            if (toAdd == null) throw new ArgumentNullException("toAdd");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Implicitly add application to list if missing
            AppEntry appEntry = FindAppEntry(target.InterfaceID);
            if (appEntry == null)
            {
                appEntry = BuildAppEntry(target);
                AppList.Entries.Add(appEntry);
            }

            AddAccessPoints(toAdd, appEntry, target, handler);
            Complete();
        }

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="toRemove">The <see cref="AccessPoint"/>s to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capabilities.Capability"/>s is invalid.</exception>
        public void RemoveAccessPoints(string interfaceID, IEnumerable<AccessPoint> toRemove)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (toRemove == null) throw new ArgumentNullException("toRemove");
            #endregion

            // Handle missing entries
            AppEntry appEntry = FindAppEntry(interfaceID);
            if (appEntry == null) throw new InvalidOperationException(string.Format(Resources.AppNotInList, interfaceID));
            if (appEntry.AccessPoints == null) return;

            RemoveAccessPoints(toRemove, appEntry);
            Complete();
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

        /// <summary>
        /// Releases the mutex and any unmanaged resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called manually and not by the garbage collector.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_mutex != null) _mutex.ReleaseMutex();
        }
        #endregion
    }
}
