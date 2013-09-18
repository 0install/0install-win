/*
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

using System.ComponentModel;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Icons;

namespace ZeroInstall.Central
{
    /// <summary>
    /// A graphical widget that displays a list of <see cref="IAppTile"/>s.
    /// </summary>
    public interface IAppTileList
    {
        /// <summary>
        /// The icon cache used by newly created <see cref="IAppTile"/>s to retrieve application icons.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        IIconCache IconCache { get; set; }

        /// <summary>
        /// Prepares a new application tile to be added to the list. Will be added in bulk when <see cref="IAppTileList.AddQueuedTiles"/> is called.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application this tile represents.</param>
        /// <param name="appName">The name of the application this tile represents.</param>
        /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="C5.DuplicateNotAllowedException">Thrown if the list already contains an <see cref="IAppTile"/> with the specified <paramref name="interfaceID"/>.</exception>
        IAppTile QueueNewTile(string interfaceID, string appName, AppStatus status, bool machineWide);

        /// <summary>
        /// Adds all new tiles queued by <see cref="IAppTileList.QueueNewTile"/> calls.
        /// </summary>
        void AddQueuedTiles();

        /// <summary>
        /// Retrieves a specific application tile from the list.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application the tile to retrieve represents.</param>
        /// <returns>The requested <see cref="IAppTile"/>; <see langword="null"/> if no matching entry was found.</returns>
        IAppTile GetTile(string interfaceID);

        /// <summary>
        /// Removes an application tile from the list. Does nothing if no matching tile can be found.
        /// </summary>
        /// <param name="interfaceID">The interface ID of the application the tile to remove represents.</param>
        void RemoveTile(string interfaceID);

        /// <summary>
        /// Removes all application tiles from the list.
        /// </summary>
        void Clear();

        /// <summary>
        /// Show a list of categories of the current tiles.
        /// </summary>
        void ShowCategories();
    }
}