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
using System.Net;
using Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages desktop integration via <see cref="AccessPoint"/>s, grouping them into categories.
    /// </summary>
    public interface ICategoryIntegrationManager : IIntegrationManager
    {
        /// <summary>
        /// Applies a category of <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="feed">The feed providing additional metadata, icons, etc. for the application.</param>
        /// <param name="categories">A list of all <see cref="AccessPoint"/> categories to be added to the already applied ones.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capability"/>s is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if one or more of the <paramref name="categories"/> would cause a conflict with the existing <see cref="AccessPoint"/>s in <see cref="AppList"/>.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void AddAccessPointCategories(AppEntry appEntry, Feed feed, ICollection<string> categories, ITaskHandler handler);

        /// <summary>
        /// Removes a category of already applied <see cref="AccessPoint"/>s for an application.
        /// </summary>
        /// <param name="appEntry">The application being integrated.</param>
        /// <param name="categories">A list of all <see cref="AccessPoint"/> categories to be removed from the already applied ones.</param>
        /// <exception cref="InvalidDataException">Thrown if one of the <see cref="AccessPoint"/>s or <see cref="Capability"/>s is invalid.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        void RemoveAccessPointCategories(AppEntry appEntry, ICollection<string> categories);
    }
}
