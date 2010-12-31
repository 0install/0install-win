using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Provides access to a cache of <see cref="Model.Feed"/>s that were downloaded via HTTP(S).
    /// </summary>
    /// <remarks>
    ///   <para>Local feed files are not handled by this cache.</para>
    ///   <para>Once a feed has been added to this cache it is considered trusted (signature is not checked again).</para>
    /// </remarks>
    public interface IFeedCache
    {
        /// <summary>
        /// Determines whether this cache contains a local copy of a feed identified by a specific URL.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed. Must be an HTTP(s) URL and not a local file path.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified feed is available in this cache;
        ///   <see langword="false"/> if the specified feed is not available in this cache.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the requested <paramref name="feedUrl"/> is not a valid URL.</exception>
        bool Contains(Uri feedUrl);

        /// <summary>
        /// Returns a list of all <see cref="Model.Feed"/>s stored in this cache.
        /// </summary>
        /// <returns>
        /// A list of feed URLs (e.g. "http://somedomain.net/interface.xml") in C-sorted order (ordinal comparison, increasing).
        /// Usually these can also be considered interface URIs.
        /// This list will always reflect the current state in the filesystem and can not be modified!
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        IEnumerable<Uri> ListAll();

        /// <summary>
        /// Gets a specific <see cref="Model.Feed"/> from this cache.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed. Must be an HTTP(s) URL and not a local file path.</param>
        /// <returns>The parsed <see cref="Model.Feed"/> object.</returns>
        /// <exception cref="ArgumentException">Thrown if the requested <paramref name="feedUrl"/> is not a valid URL.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedUrl"/> was not found in the cache.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        Model.Feed GetFeed(Uri feedUrl);

        /// <summary>
        /// Loads all <see cref="Model.Feed"/>s currently in this cache.
        /// </summary>
        /// <returns>A list of <see cref="Model.Feed"/>s  in C-sorted order (ordinal comparison, increasing).</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs disk IO")]
        IEnumerable<Model.Feed> GetAll();

        /// <summary>
        /// Adds a new <see cref="Model.Feed"/> file to the cache. Only do this after the feed source has been verified and trusted!
        /// </summary>
        /// <param name="path">The path of the file to be added.</param>
        /// <exception cref="ReplayAttackException">Thrown if the file to be added is older than a version already located in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        void Add(string path);

        /// <summary>
        /// Removes a specific <see cref="Model.Feed"/> from this cache.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed. Must be an HTTP(s) URL and not a local file path.</param>
        /// <exception cref="ArgumentException">Thrown if the requested <paramref name="feedUrl"/> is not a valid URL.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedUrl"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if the feed could not be deleted because it was in use.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        void Remove(Uri feedUrl);
    }
}
