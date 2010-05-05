using System;
using System.IO;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Describes an object that allows the storage of <see cref="Interface"/>s.
    /// </summary>
    public interface IImplementationProvider
    {
        /// <summary>
        /// Determines whether this store contains a local copy of an <see cref="ZeroInstall.Store.Implementation"/> identified by a specific <see cref="Model.ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest of the <see cref="ZeroInstall.Store.Implementation"/> to check for.</param>
        bool Contains(ManifestDigest manifestDigest);

        /// <summary>
        /// Determines the local path of an <see cref="ZeroInstall.Store.Implementation"/> with a given <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest the <see cref="ZeroInstall.Store.Implementation"/> to look for.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the requested <see cref="ZeroInstall.Store.Implementation"/> could not be found in this store.</exception>
        /// <returns></returns>
        string GetPath(ManifestDigest manifestDigest);

        /// <summary>
        /// Moves a directory containing an <see cref="ZeroInstall.Store.Implementation"/> into this store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="source">The directory containing the <see cref="ZeroInstall.Store.Implementation"/>.</param>
        /// <param name="manifestDigest">The digest the <see cref="ZeroInstall.Store.Implementation"/> is supposed to match.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the <paramref name="source"/> directory doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="source"/> directory cannot be moved or the digest cannot be calculated.</exception>
        void Add(string source, ManifestDigest manifestDigest);
    }
}
