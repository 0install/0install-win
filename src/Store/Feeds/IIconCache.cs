using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Common;
using Common.Tasks;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to a cache of icons that were downloaded via HTTP(S).
    /// </summary>
    public interface IIconCache
    {
        /// <summary>
        /// Determines whether this cache contains a local copy of an icon located at a specific URL.
        /// </summary>
        /// <param name="iconUrl">The location of the icon. Must be an HTTP(S) URL.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified icon is available in this cache;
        ///   <see langword="false"/> if the specified icon is not available in this cache.
        /// </returns>
        bool Contains(Uri iconUrl);

        /// <summary>
        /// Returns a list of all icons stored in this cache.
        /// </summary>
        /// <returns>A list of icon URIs in C-sorted order (ordinal comparison, increasing).</returns>
        /// <exception cref="IOException">Thrown if a problem occured while reading from the cache.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        IEnumerable<string> ListAll();

        /// <summary>
        /// Gets a specific icon from this cache. If the icon is missing it will be downloaded automatically.
        /// </summary>
        /// <param name="iconUrl">The location of the icon. Must be an HTTP(S) URL.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about icon downloading.</param>
        /// <returns>The parsed icon object. Do not modify this object! It may be a reference to an in-memory cache entry.</returns>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while adding the icon to the cache.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading the icon.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or write access to the cache is not permitted.</exception>
        string GetIcon(Uri iconUrl, ITaskHandler handler);

        /// <summary>
        /// Removes a specific icon from this cache.
        /// </summary>
        /// <param name="iconUrl">The location of the icon. Must be an HTTP(S) URL.</param>
        /// <exception cref="IOException">Thrown if the icon could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        void Remove(Uri iconUrl);
    }
}
