/*
 * Copyright 2010 Bastian Eicher
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
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Describes an object that allows the storage of <see cref="Interface"/>s.
    /// </summary>
    /// <remarks>These objects do not download new files by themselves.</remarks>
    public interface IStore
    {
        /// <summary>
        /// Determines whether this store contains a local copy of an <see cref="ZeroInstall.Store.Implementation"/> identified by a specific <see cref="Model.ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest of the <see cref="ZeroInstall.Store.Implementation"/> to check for.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        bool Contains(ManifestDigest manifestDigest);

        /// <summary>
        /// Determines the local path of an <see cref="ZeroInstall.Store.Implementation"/> with a given <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest the <see cref="ZeroInstall.Store.Implementation"/> to look for.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the requested <see cref="ZeroInstall.Store.Implementation"/> could not be found in this store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        /// <returns>A fully qualified path to the directory containing the <see cref="ZeroInstall.Store.Implementation"/>.</returns>
        string GetPath(ManifestDigest manifestDigest);

        /// <summary>
        /// Moves a directory containing an <see cref="Implementation"/> into this store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="path">The directory containing the <see cref="Implementation"/>.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <remarks>The directory is moved away from <paramref name="path"/> before validation starts. It will be automatically restored if validation fails.</remarks>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if <paramref name="path"/> doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="path"/> cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to <paramref name="path"/> or write access to the store is not permitted.</exception>
        void Add(string path, ManifestDigest manifestDigest);

        /// <summary>
        /// Extracts an archive containing an <see cref="ZeroInstall.Store.Implementation"/> into this store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="path">The archive containing the <see cref="Implementation"/>.</param>
        /// <param name="mimeTyp">The type of the archive as a MIME type.</param>
        /// <param name="manifestDigest">The digest the <see cref="Implementation"/> is supposed to match.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the archive content doesn't match the <paramref name="manifestDigest"/>.</exception>
        /// <exception cref="IOException">Thrown if the archive cannot be extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the archive or write access to the store is not permitted.</exception>
        void AddArchive(string path, string mimeTyp, ManifestDigest manifestDigest);
    }
}
