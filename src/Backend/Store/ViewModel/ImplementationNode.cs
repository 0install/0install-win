/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> for display in a UI.
    /// </summary>
    public abstract class ImplementationNode : StoreNode
    {
        /// <summary>
        /// Creates a new implementation node.
        /// </summary>
        /// <param name="digest">The digest identifying the implementation.</param>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <exception cref="FormatException">The manifest file is not valid.</exception>
        /// <exception cref="IOException">The manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        protected ImplementationNode(ManifestDigest digest, [NotNull] IStore store)
            : base(store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _digest = digest;

            // Determine the total size of an implementation via its manifest file
            string path = store.GetPath(digest);
            if (path == null) return;
            string manifestPath = System.IO.Path.Combine(path, Manifest.ManifestFile);
            Size = Manifest.Load(manifestPath, ManifestFormat.FromPrefix(digest.AvailableDigests.FirstOrDefault())).TotalSize;
        }

        private readonly ManifestDigest _digest;

        /// <summary>
        /// The digest identifying the implementation in the store.
        /// </summary>
        [Description("The digest identifying the implementation in the store.")]
        [NotNull]
        public string Digest { get { return _digest.AvailableDigests.First(); } }

        /// <summary>
        /// The total size of the implementation in bytes.
        /// </summary>
        [Description("The total size of the implementation in bytes.")]
        public long Size { get; private set; }

        /// <inheritdoc/>
        public override string Path { get { return Store.GetPath(_digest); } }

        /// <summary>
        /// Deletes this implementation from the <see cref="IStore"/> it is located in.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about IO tasks.</param>
        /// <exception cref="KeyNotFoundException">No matching implementation could be found in the <see cref="IStore"/>.</exception>
        /// <exception cref="IOException">The implementation could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the store is not permitted.</exception>
        public override void Delete(ITaskHandler handler)
        {
            try
            {
                Store.Remove(_digest, handler);
            }
                #region Error handling
            catch (ImplementationNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }
            #endregion
        }

        /// <summary>
        /// Verify this implementation is undamaged.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about IO tasks.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">The entry's directory could not be processed.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the entry's directory is not permitted.</exception>
        public void Verify([NotNull] ITaskHandler handler)
        {
            Store.Verify(_digest, handler);
        }

        /// <summary>
        /// Returns the Node in the form "Digest". Safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Digest;
        }
    }
}
