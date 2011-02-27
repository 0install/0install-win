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
using System.ComponentModel;
using System.IO;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about a <see cref="Model.Feed"/> / interface in the <see cref="IFeedCache"/> for display in a GUI.
    /// </summary>
    public sealed class InterfaceNode : StoreNode
    {
        #region Variables
        private readonly IFeedCache _cache;
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
        /// <param name="cache">The <see cref="IFeedCache"/> the <see cref="Model.Feed"/> / interface is located in.</param>
        /// <param name="feed">The <see cref="Model.Feed"/> / interface to be represented by this node.</param>
        public InterfaceNode(IFeedCache cache, Model.Feed feed)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            _cache = cache;
            _feed = feed;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes this <see cref="Model.Feed"/> / interface from the <see cref="IFeedCache"/> it is located in.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="KeyNotFoundException">Thrown if no matching feed could be found in the <see cref="IFeedCache"/>.</exception>
        /// <exception cref="IOException">Thrown if the feed could not be deleted because it was in use.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        public override void Delete(IIOHandler handler)
        {
            _cache.Remove(_feed.Uri.ToString());
        }
        #endregion

        #region Verify
        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Verify(IIOHandler handler)
        {}
        #endregion
    }
}
