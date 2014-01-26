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
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.WinForms.StoreManagementNodes
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> with a known owning interface for display in a GUI.
    /// </summary>
    public sealed class OwnedImplementationNode : ImplementationNode
    {
        #region Variables
        private readonly FeedNode _iface;
        private readonly Implementation _implementation;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return _iface.Name + "#" + Version + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }

        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Description("The version number of the implementation.")]
        public ImplementationVersion Version { get { return _implementation.Version; } }

        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Description("The version number of the implementation.")]
        public Architecture Architecture { get { return _implementation.Architecture; } }

        /// <summary>
        /// A unique identifier for the implementation. Used when storing implementation-specific user preferences.
        /// </summary>
        [Description("A unique identifier for the implementation. Used when storing implementation-specific user preferences.")]
        public string ID { get { return _implementation.ID; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new owned implementation node.
        /// </summary>
        /// <param name="parent">The window containing this node. Used for callbacks.</param>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <param name="digest">The digest identifying the implementation.</param>
        /// <param name="iface">The node of the interface owning the implementation.</param>
        /// <param name="implementation">Information about the implementation from a <see cref="Feed"/> file.</param>
        /// <exception cref="FormatException">Thrown if the manifest file is not valid.</exception>
        /// <exception cref="IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public OwnedImplementationNode(StoreManageForm parent, IStore store, ManifestDigest digest, FeedNode iface, Implementation implementation)
            : base(parent, store, digest)
        {
            _iface = iface;
            _implementation = implementation;
        }
        #endregion
    }
}
