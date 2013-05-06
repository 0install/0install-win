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

using System.Collections.Generic;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// An internal helper class for <see cref="ISolver"/>s that caches results from <see cref="IFeedManager"/> .
    /// </summary>
    internal class SolverFeedCache
    {
        private readonly IFeedManager _feedManager;
        private readonly Dictionary<string, Feed> _feedCache = new Dictionary<string, Feed>();

        public bool StaleFeeds;

        public SolverFeedCache(IFeedManager feedManager)
        {
            _feedManager = feedManager;
        }

        public Feed GetFeed(string feedID)
        {
            lock (_feedCache)
            {
                Feed feed;
                if (!_feedCache.TryGetValue(feedID, out feed))
                {
                    feed = _feedManager.GetFeed(feedID, ref StaleFeeds);
                    _feedCache.Add(feedID, feed);
                }
                return feed;
            }
        }
    }
}
