using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Common;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages an <see cref="AppList"/> and desktop integration via <see cref="AccessPoint"/>s.
    /// </summary>
    public interface IIntegrationManager
    {
        /// <summary>
        /// Stores a list of applications and their desktop integrations. Do not modify this externally! Use this class' methods instead.
        /// </summary>
        AppList AppList { get; }

        /// <summary>
        /// Adds an application to the application list.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void AddApp(InterfaceFeed target);

        /// <summary>
        /// Removes an application from the application list.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        void RemoveApp(string interfaceID);

        /// <summary>
        /// Applies <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="target">The application being integrated.</param>
        /// <param name="toAdd">The <see cref="AccessPoint"/>s to apply.</param>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="toAdd"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="IntegrationManager.AppList"/>.</exception>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        void AddAccessPoints(InterfaceFeed target, IEnumerable<AccessPoint> toAdd);

        /// <summary>
        /// Removes already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="toRemove">The <see cref="AccessPoint"/>s to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is not in the list.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="ZeroInstall.Model.Capabilities.Capability"/>s is invalid.</exception>
        void RemoveAccessPoints(string interfaceID, IEnumerable<AccessPoint> toRemove);
    }
}
