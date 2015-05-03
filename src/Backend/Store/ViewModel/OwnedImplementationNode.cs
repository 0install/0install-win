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
using System.ComponentModel;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Values.Design;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> with a known owning interface for display in a UI.
    /// </summary>
    public sealed class OwnedImplementationNode : ImplementationNode
    {
        [NotNull]
        private readonly Implementation _implementation;

        [NotNull]
        private readonly FeedNode _parent;

        /// <summary>
        /// Creates a new owned implementation node.
        /// </summary>
        /// <param name="digest">The digest identifying the implementation.</param>
        /// <param name="implementation">Information about the implementation from a <see cref="Feed"/> file.</param>
        /// <param name="parent">The node of the feed owning the implementation.</param>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <exception cref="FormatException">The manifest file is not valid.</exception>
        /// <exception cref="IOException">The manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        public OwnedImplementationNode(ManifestDigest digest, [NotNull] Implementation implementation, [NotNull] FeedNode parent, [NotNull] IStore store)
            : base(digest, store)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            if (parent == null) throw new ArgumentNullException("parent");
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _parent = parent;
            _implementation = implementation;
        }

        /// <inheritdoc/>
        public override string Name { get { return _parent.Name + "\\" + Version + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }

        /// <summary>
        /// The URI of the feed describing the implementation.
        /// </summary>
        [Description("The URI of the feed describing the implementation.")]
        public FeedUri FeedUri { get { return _parent.Uri; } }

        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Description("The version number of the implementation.")]
        [NotNull]
        public ImplementationVersion Version { get { return _implementation.Version; } }

        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Description("The version number of the implementation.")]
        [TypeConverter(typeof(StringConstructorConverter<Architecture>))]
        public Architecture Architecture { get { return _implementation.Architecture; } }

        /// <summary>
        /// A unique identifier for the implementation. Used when storing implementation-specific user preferences.
        /// </summary>
        [Description("A unique identifier for the implementation. Used when storing implementation-specific user preferences.")]
        [NotNull]
        public string ID { get { return _implementation.ID; } }

        /// <summary>
        /// Returns the Node in the form "Digest URI Version Architecture". Safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Digest + " " + _parent.Uri.ToStringRfc() + " " + Version + " " + Architecture;
        }
    }
}
