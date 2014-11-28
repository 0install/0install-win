using System;
using System.Collections.Generic;
using System.IO;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to a cache of <see cref="Feed"/>s that were downloaded via HTTP(S).
    /// </summary>
    /// <remarks>
    ///   <para>All methods are thread-safe.</para>
    ///   <para>Local feed files may be simply passed through the cache.</para>
    ///   <para>Once a feed has been added to this cache it is considered trusted (signatures are not checked again).</para>
    /// </remarks>
    public interface IFeedCache
    {
        /// <summary>
        /// Determines whether this cache contains a local copy of a <see cref="Feed"/> identified by a specific URL.
        /// </summary>
        /// <param name="feedUri">The canonical ID used to identify the feed.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified feed is available in this cache;
        ///   <see langword="false"/> if the specified feed is not available in this cache.
        /// </returns>
        bool Contains(FeedUri feedUri);

        /// <summary>
        /// Returns a list of all <see cref="Feed"/>s stored in this cache.
        /// </summary>
        /// <returns>
        /// A list of feed URIs (e.g. "http://somedomain.net/interface.xml").
        /// Usually these can also be considered interface URIs.
        /// </returns>
        /// <exception cref="IOException">A problem occured while reading from the cache.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the cache is not permitted.</exception>
        IEnumerable<FeedUri> ListAll();

        /// <summary>
        /// Gets a specific <see cref="Feed"/> from this cache. <see cref="Feed.Normalize"/> is automatically called.
        /// </summary>
        /// <param name="feedUri">The canonical ID used to identify the feed.</param>
        /// <returns>The parsed <see cref="Feed"/> object. Do not modify this object! It may be a reference to an in-memory cache entry.</returns>
        /// <exception cref="KeyNotFoundException">The requested <paramref name="feedUri"/> was not found in the cache.</exception>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidDataException">The feed file could not be parsed.</exception>
        Feed GetFeed(FeedUri feedUri);

        /// <summary>
        /// Determines which signatures a <see cref="Feed"/> from this cache is signed with.
        /// </summary>
        /// <param name="feedUri">The canonical ID used to identify the feed.</param>
        /// <returns>A list of signatures found, both valid and invalid.</returns>
        /// <exception cref="KeyNotFoundException">The requested <paramref name="feedUri"/> was not found in the cache.</exception>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        /// <exception cref="SignatureException">The signature data could not be handled.</exception>
        IEnumerable<OpenPgpSignature> GetSignatures(FeedUri feedUri);

        /// <summary>
        /// Adds a new <see cref="Feed"/> to the cache. Only do this after the feed source has been verified and trusted and replay attacks filtered!
        /// </summary>
        /// <param name="feedUri">The canonical ID used to identify the feed.</param>
        /// <param name="data">The content of the feed file as a byte array.</param>
        /// <exception cref="IOException">A problem occured while writing the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the cache is not permitted.</exception>
        /// <exception cref="InvalidDataException">The feed file could not be parsed.</exception>
        void Add(FeedUri feedUri, byte[] data);

        /// <summary>
        /// Removes a specific <see cref="Feed"/> from this cache. No exception is thrown if the specified <see cref="Feed"/> is not in the cache.
        /// </summary>
        /// <param name="feedUri">The canonical ID used to identify the feed.</param>
        /// <exception cref="KeyNotFoundException">The requested <paramref name="feedUri"/> was not found in the cache.</exception>
        /// <exception cref="IOException">The feed could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the cache is not permitted.</exception>
        void Remove(FeedUri feedUri);

        /// <summary>
        /// Clears any in-memory caches.
        /// </summary>
        void Flush();
    }
}
