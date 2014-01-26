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

using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// An interface ID combined with <see cref="Feed"/> data aquired from that ID.
    /// </summary>
    public struct InterfaceFeed
    {
        /// <summary>
        /// The URI or local path (must be absolute) to the interface.
        /// </summary>
        public readonly string InterfaceID;

        /// <summary>
        /// The data aquired from <see cref="InterfaceID"/>. <see cref="Store.Model.Feed.Normalize"/> has already been called.
        /// </summary>
        public readonly Feed Feed;

        /// <summary>
        /// Creates a new interface-feed reference.
        /// </summary>
        /// <param name="interfaceID">The URI or local path (must be absolute) to the interface.</param>
        /// <param name="feed">The data aquired from <paramref name="interfaceID"/>.</param>
        public InterfaceFeed(string interfaceID, Feed feed)
        {
            InterfaceID = interfaceID;
            Feed = feed;
        }
    }
}
