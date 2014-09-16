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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Models information about a <see cref="Feed"/> in the <see cref="IFeedCache"/> for display in a UI.
    /// </summary>
    public sealed class FeedNode : CacheNode
    {
        #region Dependencies
        private readonly IFeedCache _cache;
        private readonly Feed _feed;

        /// <summary>
        /// Creates a new feed node.
        /// </summary>
        /// <param name="feed">The <see cref="Feed"/> to be represented by this node.</param>
        /// <param name="cache">The <see cref="IFeedCache"/> the <see cref="Feed"/> is located in.</param>
        public FeedNode(Feed feed, IFeedCache cache)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            _cache = cache;
            _feed = feed;
        }
        #endregion

        /// <inheritdoc/>
        public override string Name { get { return _feed.Name + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }

        /// <summary>
        /// The URI indentifying this feed.
        /// </summary>
        [Description("The URI indentifying this feed.")]
        public Uri Uri { get { return _feed.Uri; } }

        /// <summary>
        /// The main website of the application.
        /// </summary>
        [Description("The main website of the application.")]
        public Uri Homepage { get { return _feed.Homepage; } }

        /// <summary>
        /// A short one-line description of the application.
        /// </summary>
        [Description("A short one-line description of the application.")]
        public string Summary { get { return _feed.GetBestSummary(CultureInfo.CurrentUICulture); } }

        /// <summary>
        /// A comma-separated list of categories the applications fits into.
        /// </summary>
        [Description("A comma-separated list of categories the applications fits into.")]
        public string Categories { get { return StringUtils.Join(",", _feed.Categories.Select(x => x.Name)); } }

        /// <summary>
        /// Deletes this <see cref="Feed"/> from the <see cref="IFeedCache"/> it is located in.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No matching feed could be found in the <see cref="IFeedCache"/>.</exception>
        /// <exception cref="IOException">The feed could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the cache is not permitted.</exception>
        public override void Delete()
        {
            _cache.Remove(_feed.UriString);
        }
    }
}
