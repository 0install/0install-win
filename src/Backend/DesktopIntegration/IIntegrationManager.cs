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
    /// Manages an <see cref="AppList"/> and desktop integration via <see cref="AccessPoint"/>s.
    /// </summary>
    public interface IIntegrationManager
    {
        /// <summary>
        /// Stores a list of applications and their desktop integrations. Only use for read-access externally! Use this class' methods for any modifications.
        /// </summary>
        AppList AppList { get; }

        /// <summary>
        /// Apply operations machine-wide instead of just for the current user.
        /// </summary>
        bool MachineWide { get; }

        /// <summary>
        /// Creates a new unnamed <see cref="AppEntry"/> and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application to add.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <returns>The newly created application entry (already added to <see cref="AppList"/>).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        AppEntry AddApp(string interfaceID, Feed feed);

        /// <summary>
        /// Creates a new  named <see cref="AppEntry"/> and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="petName">The user-defined pet-name of the application.</param>
        /// <param name="requirements">The requirements describing the application to add.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <returns>The newly created application entry (already added to <see cref="AppList"/>).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        AppEntry AddApp(string petName, Requirements requirements, Feed feed);

        /// <summary>
        /// Removes an <see cref="AppEntry"/> from the <see cref="AppList"/> while unapplying any remaining <see cref="AccessPoint"/>s.
        /// </summary>
        /// <param name="appEntry">The application to remove.</param>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void RemoveApp(AppEntry appEntry);

        /// <summary>
        /// Updates an <see cref="AppEntry"/> with new metadata and capabilities from a <see cref="Feed"/>. This may unapply and remove some existing <see cref="AccessPoint"/>s.
        /// </summary>
        /// <param name="appEntry">The application entry to update.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        void UpdateApp(AppEntry appEntry, Feed feed);

        /// <summary>
        /// Updates a named <see cref="AppEntry"/> with new <see cref="Requirements"/>.
        /// </summary>
        /// <param name="appEntry">The application entry to update.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <param name="requirements">The new requirements to apply to the app.</param>
        void UpdateApp(AppEntry appEntry, Feed feed, Requirements requirements);

        /// <summary>
        /// Applies <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        /// <param name="accessPoints">The access points to apply.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="ConflictException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="IIntegrationManager.AppList"/>.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void AddAccessPoints(AppEntry appEntry, Feed feed, IEnumerable<AccessPoint> accessPoints);

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="accessPoints">The access points to unapply.</param>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void RemoveAccessPoints(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints);

        /// <summary>
        /// Reapplies all <see cref="AccessPoint"/>s for all <see cref="AppEntry"/>s.
        /// </summary>
        /// <param name="feedRetriever">Callback method used to retrieve additional <see cref="Feed"/>s on demand.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="ConflictException">Thrown if the <see cref="IIntegrationManager.AppList"/> has inner conflicts.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Store.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void Repair(Converter<string, Feed> feedRetriever);
    }
}
