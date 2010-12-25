/*
 * Copyright 2010 Bastian Eicher
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
using System.ComponentModel;
using System.IO;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about a <see cref="Feed"/> / interface in the <see cref="FeedCache"/> for display in a GUI.
    /// </summary>
    public sealed class InterfaceNode : StoreNode
    {
        #region Variables
        private readonly FeedCache _cache;
        private readonly Model.Feed _feed;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name
        {
            get { return Title + (SuffixCounter == 0 ? "" : " " + SuffixCounter); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// The URI indentifying this interface.
        /// </summary>
        [Description("The URI indentifying this interface.")]
        public Uri Uri { get { return _feed.Uri; } }

        /// <summary>
        /// The name of the application represented by this interface.
        /// </summary>
        [Description("The name of the application represented by this interface.")]
        public string Title { get { return _feed.Name; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new interface node.
        /// </summary>
        /// <param name="cache">The <see cref="FeedCache"/> the <see cref="Feed"/> / interface is located in.</param>
        /// <param name="feed">The <see cref="Feed"/> / interface to be represented by this node.</param>
        public InterfaceNode(FeedCache cache, Model.Feed feed)
        {
            _cache = cache;
            _feed = feed;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes this <see cref="Feed"/> / interface from the <see cref="FeedCache"/> it is located in.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if no matching feed could be found in the <see cref="FeedCache"/>.</exception>
        /// <exception cref="IOException">Thrown if the feed could not be deleted because it was in use.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        public override void Delete()
        {
            _cache.Remove(_feed.Uri);
        }
        #endregion
    }
}
