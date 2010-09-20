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
using System.Collections.Generic;
using System.IO;
using Common;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Describes an object that allows the storage and retrieval of <see cref="Implementation"/> directories.
    /// </summary>
    /// <remarks>These objects do not download new files by themselves.</remarks>
    public interface IStore
    {
        /// <summary>
        /// Determines whether this store contains a local copy of an implementation identified by a specific <see cref="Model.ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest of the implementation to check for.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        bool Contains(ManifestDigest manifestDigest);

        /// <summary>
        /// Determines the local path of an implementation with a given <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest the implementation to look for.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the requested implementation could not be found in this store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        /// <returns>A fully qualified path to the directory containing the implementation.</returns>
        string GetPath(ManifestDigest manifestDigest);

        /// <summary>
        /// Copies a directory containing an implementation into this store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="path">The directory containing the implementation.</param>
        /// <param name="manifestDigest">The digest the implementation is supposed to match.</param>
        /// <param name="startingManifest">Callback to be called when a new manifest generation task (hashing files) is about to be started; may be <see langword="null"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="path"/> cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to <paramref name="path"/> or write access to the store is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if <paramref name="path"/> doesn't match the <paramref name="manifestDigest"/>.</exception>
        void AddDirectory(string path, ManifestDigest manifestDigest, Action<IProgress> startingManifest);

        /// <summary>
        /// Extracts an archive containing the files of an implementation into this store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="archiveInfo">Parameter object providing the information to extract the archive.</param>
        /// <param name="manifestDigest">The digest the implementation is supposed to match.</param>
        /// <param name="startingExtraction">Callback to be called when a new extraction task is about to be started; may be <see langword="null"/>.</param>
        /// <param name="startingManifest">Callback to be called when a new manifest generation task (hashing files) is about to be started; may be <see langword="null"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="IOException">Thrown if the archive cannot be extracted.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the archive or write access to the store is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the archive content doesn't match the <paramref name="manifestDigest"/>.</exception>
        void AddArchive(ArchiveFileInfo archiveInfo, ManifestDigest manifestDigest, Action<IProgress> startingExtraction, Action<IProgress> startingManifest);
        
        /// <summary>
        /// Extracts multiple archives, that together contain the files of an implementation, into the same folder, compares that folder's manifest to <paramref name="manifestDigest"/> and adds it to the store.
        /// </summary>
        /// <param name="archiveInfos">Multiple parameter objects providing the information to extract each archive.</param>
        /// <param name="manifestDigest">The digest the implementation is supposed to match.</param>
        /// <param name="startingExtraction">Callback to be called when a new extraction task is about to be started; may be <see langword="null"/>.</param>
        /// <param name="startingManifest">Callback to be called when a new manifest generation task (hashing files) is about to be started; may be <see langword="null"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="IOException">Thrown if one of the archives cannot be extracted.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to one of the archives or write access to the store is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the archives content doesn't match the <paramref name="manifestDigest"/>.</exception>
        void AddMultipleArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, Action<IProgress> startingExtraction, Action<IProgress> startingManifest);
    }
}
