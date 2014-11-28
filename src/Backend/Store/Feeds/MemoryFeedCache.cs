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
using System.IO;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides an additional in-memory cache layer above another <see cref="IFeedCache"/>.
    /// </summary>
    /// <remarks>
    ///   <para>Local feed files are also cached when read.</para>
    ///   <para>Once a feed has been added to this cache it is considered trusted (signatures are not checked again).</para>
    /// </remarks>
    public sealed class MemoryFeedCache : IFeedCache
    {
        #region Variables
        /// <summary>The underlying cache used for authorative storage of <see cref="Feed"/>s.</summary>
        private readonly IFeedCache _backingCache;

        /// <summary>The in-memory cache for storing parsed <see cref="Feed"/>s.</summary>
        private readonly Dictionary<FeedUri, Feed> _feedDictionary = new Dictionary<FeedUri, Feed>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cache based on a backing <see cref="IFeedCache"/>.
        /// </summary>
        /// <param name="backingCache">The underlying cache used for authorative storage of <see cref="Feed"/>s. This must not be modified directly once the <see cref="MemoryFeedCache"/> is in use!</param>
        public MemoryFeedCache(IFeedCache backingCache)
        {
            #region Sanity checks
            if (backingCache == null) throw new ArgumentNullException("backingCache");
            #endregion

            _backingCache = backingCache;
        }
        #endregion

        //--------------------//

        #region Contains
        /// <inheritdoc/>
        public bool Contains(FeedUri feedUri)
        {
            return _backingCache.Contains(feedUri);
        }
        #endregion

        #region List all
        /// <inheritdoc/>
        public IEnumerable<FeedUri> ListAll()
        {
            return _backingCache.ListAll();
        }
        #endregion

        #region Get
        /// <inheritdoc/>
        public Feed GetFeed(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            lock (_feedDictionary)
            {
                if (!_feedDictionary.ContainsKey(feedUri))
                { // Add to memory cache if missing
                    Feed feed = _backingCache.GetFeed(feedUri);
                    _feedDictionary.Add(feedUri, feed);
                    return feed;
                }

                // Get from memory cache
                return _feedDictionary[feedUri];
            }
        }

        /// <inheritdoc/>
        public IEnumerable<OpenPgpSignature> GetSignatures(FeedUri feedUri)
        {
            return _backingCache.GetSignatures(feedUri);
        }
        #endregion

        #region Add
        /// <inheritdoc/>
        public void Add(FeedUri feedUri, byte[] data)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            // Add to underlying cache
            _backingCache.Add(feedUri, data);

            // Add to memory cache (replacing existing old versions)
            var feed = XmlStorage.LoadXml<Feed>(new MemoryStream(data));
            feed.Normalize(feedUri);
            lock (_feedDictionary)
            {
                _feedDictionary.Remove(feedUri);
                _feedDictionary.Add(feedUri, feed);
            }
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public void Remove(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            lock (_feedDictionary)
            {
                // Remove from memory cache
                _feedDictionary.Remove(feedUri);
            }

            // Remove from underlying cache
            _backingCache.Remove(feedUri);
        }
        #endregion

        #region Flush
        /// <inheritdoc/>
        public void Flush()
        {
            lock (_feedDictionary) _feedDictionary.Clear();
            _backingCache.Flush();
        }
        #endregion
    }
}
