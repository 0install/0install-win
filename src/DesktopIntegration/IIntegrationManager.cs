using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Common;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

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
        /// Apply operations system-wide instead of just for the current user.
        /// </summary>
        bool SystemWide { get; }

        /// <summary>
        /// Creates a new <see cref="AppEntry"/> and adds it to the <see cref="AppList"/>.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application to remove.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        /// <returns>The newly created application entry (already added to <see cref="AppList"/>).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        AppEntry AddApp(string interfaceID, Feed feed);

        /// <summary>
        /// Removes an <see cref="AppEntry"/> from the <see cref="AppList"/> while unapplying any remaining <see cref="AccessPoint"/>s.
        /// </summary>
        /// <param name="appEntry">The application to remove.</param>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        void RemoveApp(AppEntry appEntry);

        /// <summary>
        /// Updates an <see cref="AppEntry"/> with new metadata and capabilities from a <see cref="Feed"/>. This may unapply and remove some existing <see cref="AccessPoint"/>s.
        /// </summary>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <param name="appEntry">The application entry to update.</param>
        /// <param name="feed">The feed providing additional metadata, capabilities, etc. for the application.</param>
        void UpdateApp(AppEntry appEntry, Feed feed);

        /// <summary>
        /// Applies <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        /// <param name="accessPoints">The access points to apply.</param>
        /// <exception cref="KeyNotFoundException">Thrown if a reference to <see cref="Capability"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="accessPoints"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="IntegrationManager.AppList"/>.</exception>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        void AddAccessPoints(AppEntry appEntry, Feed feed, IEnumerable<AccessPoint> accessPoints);

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="accessPoints">The access points to unapply.</param>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void RemoveAccessPoints(AppEntry appEntry, IEnumerable<AccessPoint> accessPoints);
    }
}
