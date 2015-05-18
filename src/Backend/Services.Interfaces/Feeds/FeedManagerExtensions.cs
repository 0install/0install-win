/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Provides extension methods for <see cref="IFeedManager"/>.
    /// </summary>
    public static class FeedManagerExtensions
    {
        /// <summary>
        /// Returns a specific <see cref="Feed"/>. Automatically updates cached feeds when indicated by <see cref="IFeedManager.ShouldRefresh"/>.
        /// </summary>
        /// <param name="feedManager">The <see cref="IFeedManager"/> implementation.</param>
        /// <param name="feedUri">The canonical ID used to identify the feed.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <remarks><see cref="Feed"/>s are always served from the <see cref="IFeedCache"/> if possible, unless <see cref="IFeedManager.Refresh"/> is set to <see langword="true"/>.</remarks>
        /// <exception cref="OperationCanceledException">The user canceled the process.</exception>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">A problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">The signature data of a remote feed file could not be verified.</exception>
        /// <exception cref="UriFormatException"><see cref="Feed.Uri"/> is missing or does not match <paramref name="feedUri"/>.</exception>
        public static Feed GetFeedFresh([NotNull] this IFeedManager feedManager, [NotNull] FeedUri feedUri)
        {
            #region Sanity checks
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            var feed = feedManager.GetFeed(feedUri);

            if (!feedManager.Refresh && feedManager.ShouldRefresh)
            {
                feedManager.Stale = false;
                feedManager.Refresh = true;
                try
                {
                    feed = feedManager.GetFeed(feedUri);
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

                finally
                {
                    feedManager.Refresh = false;
                }
            }

            return feed;
        }
    }
}
