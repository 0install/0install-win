﻿/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Describes an object that allows the storage and retrieval of <see cref="Model.Implementation"/> directories.
    /// </summary>
    /// <remarks>A store caches <see cref="Model.Implementation"/>s identified by their <see cref="ManifestDigest"/>s.</remarks>
    public interface IStore
    {
        /// <summary>
        /// Returns a list of all implementations currently in the store.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        /// <returns>A list of implementations formated as "algorithm=digest" (e.g. "sha256=123abc") in C-sorted order (ordinal comparison, increasing).</returns>
        IEnumerable<ManifestDigest> ListAll();

        /// <summary>
        /// Determines whether the store contains a local copy of an implementation identified by a specific <see cref="Model.ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest of the implementation to check for.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified implementation is available in the store;
        ///   <see langword="false"/> if the specified implementation is not available in the store or if read access to the store is not permitted.
        /// </returns>
        /// <remarks>If read access to the store is not permitted, no exception is thrown.</remarks>
        bool Contains(ManifestDigest manifestDigest);

        /// <summary>
        /// Determines the local path of an implementation with a given <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="manifestDigest">The digest the implementation to look for.</param>
        /// <exception cref="ImplementationNotFoundException">Thrown if the requested implementation could not be found in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        /// <returns>A fully qualified path to the directory containing the implementation.</returns>
        string GetPath(ManifestDigest manifestDigest);

        /// <summary>
        /// Copies a directory containing an implementation into the store if it matches the provided <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="path">The directory containing the implementation.</param>
        /// <param name="manifestDigest">The digest the implementation is supposed to match.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="path"/> cannot be moved or the digest cannot be calculated.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Model.Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to <paramref name="path"/> or write access to the store is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if <paramref name="path"/> doesn't match the <paramref name="manifestDigest"/>.</exception>
        void AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler);

        /// <summary>
        /// Extracts multiple archives, that together contain the files of an implementation, into the same folder, compares that folder's manifest to <paramref name="manifestDigest"/> and adds it to the store.
        /// </summary>
        /// <param name="archiveInfos">Multiple parameter objects providing the information to extract each archive.</param>
        /// <param name="manifestDigest">The digest the implementation is supposed to match.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="NotSupportedException">Thrown if an archive type is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if one of the archives cannot be extracted.</exception>
        /// <exception cref="ImplementationAlreadyInStoreException">Thrown if there is already an <see cref="Model.Implementation"/> with the specified <paramref name="manifestDigest"/> in the store.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to one of the archives or write access to the store is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the archives content doesn't match the <paramref name="manifestDigest"/>.</exception>
        void AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler);

        /// <summary>
        /// Removes a specific implementation from the cache.
        /// </summary>
        /// <param name="manifestDigest">The digest of the implementation to be removed.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> provides no hash methods.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if no implementation matching <paramref name="manifestDigest"/> could be found in the store.</exception>
        /// <exception cref="IOException">Thrown if the implementation could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the store is not permitted.</exception>
        void Remove(ManifestDigest manifestDigest, ITaskHandler handler);

        /// <summary>
        /// Reads in all the manifest files in the store and looks for duplicates (files with the same permissions, modification time and digest). When it finds a pair, it deletes one and replaces it with a hard-link to the other.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if two files could not be hard-linked together.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the store is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if a damaged implementation is encountered while optimizing.</exception>
        /// <remarks>If the store does not support optimising this method call may be silently ignored.</remarks>
        void Optimise(ITaskHandler handler);

        /// <summary>
        /// Recalculates the digests for an entry in the store and ensures it is correct. Will not delete any defective entries!
        /// </summary>
        /// <param name="manifestDigest">The digest of the implementation to be verified.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="manifestDigest"/> indicates no known hash methods.</exception>
        /// <exception cref="IOException">Thrown if the entry's directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the entry's directory is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown if the entry's directory doesn't match <paramref name="manifestDigest"/>.</exception>
        /// <remarks>If the store does not support verification this method call may be silently ignored.</remarks>
        void Verify(ManifestDigest manifestDigest, ITaskHandler handler);

        /// <summary>
        /// Recalculates the digests for all entries in the store and ensures they are correct. Will not delete any defective entries!
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <returns>
        /// A list of digest mismatch problems that were encountered. The comparisons are only performed on demand as the returned enumerator is probed.<br/>
        /// -or-<br/>
        /// <see langword="null"/> if the store does not support auditing.
        /// </returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a directory in the store could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the store is not permitted.</exception>
        IEnumerable<DigestMismatchException> Audit(ITaskHandler handler);
    }
}
