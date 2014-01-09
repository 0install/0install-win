﻿/*
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
using System.IO;
using System.Net;
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Provides extension methods for <see cref="IFeedManager"/>.
    /// </summary>
    public static class FeedManagerExtensions
    {
        /// <summary>
        /// Returns a specific <see cref="Feed"/>. Automatically updates cached feeds that have become stale.
        /// </summary>
        /// <param name="feedManager">The <see cref="IFeedManager"/> implementation.</param>
        /// <param name="feedID">The canonical ID used to identify the feed.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <remarks><see cref="Feed"/>s are always served from the <see cref="IFeedCache"/> if possible, unless <see cref="IFeedManager.Refresh"/> is set to <see langword="true"/>.</remarks>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedID"/> is an invalid interface ID.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data of a remote feed file could not be verified.</exception>
        public static Feed GetFeedFresh(this IFeedManager feedManager, string feedID)
        {
            #region Sanity checks
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            feedManager.Stale = false;
            var feed = feedManager.GetFeed(feedID);

            if (feedManager.Stale && !feedManager.Refresh)
            {
                feedManager.Refresh = true;
                try
                {
                    feed = feedManager.GetFeed(feedID);
                }
                    #region Sanity checks
                catch (IOException ex)
                {
                    Log.Warn(ex);
                }
                catch (WebException ex)
                {
                    Log.Warn(ex);
                }
                #endregion

                feedManager.Refresh = false;
                feedManager.Stale = false;
            }

            return feed;
        }
    }
}
