/*
 * Copyright 2010-2014 Bastian Eicher
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
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Template methods for implementing <see cref="IIntegrationManager"/>.
    /// </summary>
    public abstract class IntegrationManagerBase : IIntegrationManager
    {
        #region Dependencies
        /// <summary>A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</summary>
        protected readonly ITaskHandler Handler;

        /// <summary>
        /// Creates an integration manager base.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        protected IntegrationManagerBase(ITaskHandler handler)
        {
            Handler = handler;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Stores a list of applications and their desktop integrations. Only use for read-access externally! Use this class' methods for any modifications.
        /// </summary>
        public AppList AppList { get; protected set; }

        /// <summary>
        /// Apply operations machine-wide instead of just for the current user.
        /// </summary>
        public bool MachineWide { get; protected set; }
        #endregion

        //--------------------//

        #region Interface
        /// <inheritdoc/>
        public AppEntry AddApp(string interfaceID, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var appEntry = AddAppInternal(interfaceID, feed);
            Finish();
            return appEntry;
        }

        /// <inheritdoc/>
        public AppEntry AddApp(string petName, Requirements requirements, Feed feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(petName)) throw new ArgumentNullException("petName");
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var appEntry = AddAppInternal(petName, requirements, feed);
            Finish();
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
                RemoveAppInternal(appEntry);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
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
                UpdateAppInternal(appEntry, feed);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
            }
        }

        /// <inheritdoc/>
        public void UpdateApp(AppEntry appEntry, Feed feed, Requirements requirements)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            if (requirements == null) throw new ArgumentNullException("requirements");
            #endregion

            try
            {
                appEntry.Requirements = requirements;
                UpdateAppInternal(appEntry, feed);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
            }
        }

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
                AddAccessPointsInternal(appEntry, feed, accessPoints);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
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
                RemoveAccessPointsInternal(appEntry, accessPoints);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
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
                    RepairAppInternal(appEntry, feedRetriever(appEntry.InterfaceID));
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(ex.Message, ex);
            }
            finally
            {
                Finish();
            }
        }
        #endregion

        #region Template methods
        /// <summary>
        /// Creates a new unnamed <see cref="AppEntry"/> and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application to add.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <returns>The newly created application entry (already added to <see cref="AppList"/>).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        protected abstract AppEntry AddAppInternal(string interfaceID, Feed feed);

        /// <summary>
        /// Creates a new named <see cref="AppEntry"/> and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="petName">The user-defined pet-name of the application.</param>
        /// <param name="requirements">The requirements describing the application to add.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <returns>The newly created application entry (already added to <see cref="AppList"/>).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        protected abstract AppEntry AddAppInternal(string petName, Requirements requirements, Feed feed);

        /// <summary>
        /// Creates a new <see cref="AppEntry"/> based on an existing prototype (applying any <see cref="AccessPoint"/>s) and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="prototype">An existing <see cref="AppEntry"/> to use as a prototype.</param>
        /// <param name="feedRetriever">Callback method used to retrieve additional <see cref="Feed"/>s on demand.</param>
        protected abstract void AddAppInternal(AppEntry prototype, Converter<string, Feed> feedRetriever);

        /// <summary>
        /// Removes an <see cref="AppEntry"/> from the <see cref="AppList"/> while unapplying any remaining <see cref="AccessPoint"/>s.
        /// </summary>
        /// <param name="appEntry">The application to remove.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        protected abstract void RemoveAppInternal(AppEntry appEntry);

        /// <summary>
        /// Updates an <see cref="AppEntry"/> with new metadata and capabilities from a <see cref="Feed"/>. This may unapply and remove some existing <see cref="AccessPoint"/>s.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <param name="appEntry">The application entry to update.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        protected abstract void UpdateAppInternal(AppEntry appEntry, Feed feed);

        /// <summary>
        /// Applies <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        /// <param name="accessPoints">The access points to apply.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="ConflictException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="IIntegrationManager.AppList"/>.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        protected abstract void AddAccessPointsInternal(AppEntry appEntry, Feed feed, IEnumerable<AccessPoint> accessPoints);

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The <see cref="AppEntry"/> containing the <paramref name="accessPoints"/>.</param>
        /// <param name="accessPoints">The access points to unapply.</param>
        /// <exception cref="KeyNotFoundException">Thrown if an <see cref="AccessPoint"/> reference to a <see cref="Store.Model.Capabilities.Capability"/> is invalid.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        protected abstract void RemoveAccessPointsInternal(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints);

        /// <summary>
        /// Reapplies all <see cref="AccessPoint"/>s for a specific <see cref="AppEntry"/>.
        /// </summary>
        /// <param name="appEntry">The application entry to repair.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="ConflictException">Thrown if <paramref name="appEntry"/> conflicts with the rest of the <see cref="AppList"/>.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        protected abstract void RepairAppInternal(AppEntry appEntry, Feed feed);

        /// <summary>
        /// To be called after integration operations have been completed to inform the desktop environment and save the <see cref="DesktopIntegration.AppList"/>.
        /// </summary>
        protected abstract void Finish();
        #endregion
    }
}
