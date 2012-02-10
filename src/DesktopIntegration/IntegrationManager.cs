/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Threading;
using Common.Tasks;
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
    public partial class IntegrationManager : IIntegrationManager, IDisposable
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
        #endregion

        #region Properties
        /// <inheritdoc/>
        public AppList AppList { get; private set; }

        /// <inheritdoc/>
        public bool SystemWide { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new integration manager using a custom <see cref="DesktopIntegration.AppList"/>. Do not use directly unless for testing purposes!
        /// </summary>
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        /// <param name="appListPath">The storage location of the <see cref="AppList"/> file.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool systemWide, string appListPath, ITaskHandler handler)
        {
            SystemWide = systemWide;
            AppListPath = appListPath;
            Handler = handler;

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
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the <see cref="AppList"/> file is not permitted or if another desktop integration class is currently active.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool systemWide, ITaskHandler handler)
            : this(systemWide, AppList.GetDefaultPath(systemWide), handler)
        {
            // Prevent multiple concurrent desktop integration operations
            _mutex = new Mutex(true, systemWide ? @"Global\" + MutexName : MutexName);
            if (!_mutex.WaitOne(1000, false))
            {
                _mutex = null; // Don't try to release mutex if it wasn't acquired
                throw new UnauthorizedAccessException(Resources.IntegrationMutex);
            }
        }
        #endregion

        //--------------------//

        #region Apps
        /// <inheritdoc/>
        public AppEntry AddApp(string interfaceID, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var appEntry = AddAppHelper(interfaceID, feed);
            Complete();
            return appEntry;
        }

        /// <inheritdoc/>
        public void RemoveApp(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            try
            {
                RemoveAppHelper(appEntry);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Complete();
            }
        }

        /// <inheritdoc/>
        public void UpdateApp(AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            try
            {
                UpdateAppHelper(appEntry, feed);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Complete();
            }
        }
        #endregion

        #region AccessPoints
        /// <inheritdoc/>
        public void AddAccessPoints(AppEntry appEntry, Feed feed, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            try
            {
                AddAccessPointsHelper(appEntry, feed, accessPoints);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Complete();
            }
        }

        /// <inheritdoc/>
        public void RemoveAccessPoints(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (accessPoints == null) throw new ArgumentNullException("accessPoints");
            #endregion

            try
            {
                RemoveAccessPointsHelper(appEntry, accessPoints);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Complete();
            }
        }

        /// <inheritdoc/>
        public void Repair(Converter<string, Feed> feedRetriever)
        {
            #region Sanity checks
            if (feedRetriever == null) throw new ArgumentNullException("feedRetriever");
            #endregion

            try
            {
                foreach (var appEntry in AppList.Entries)
                {
                    var toReAdd = (appEntry.AccessPoints == null)
                        ? new AccessPoint[0]
                        : appEntry.AccessPoints.Entries.ToArray();
                    AddAccessPointsHelper(appEntry, feedRetriever(appEntry.InterfaceID), toReAdd);
                }
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Complete();
            }
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
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
            }
        }
        #endregion
    }
}
