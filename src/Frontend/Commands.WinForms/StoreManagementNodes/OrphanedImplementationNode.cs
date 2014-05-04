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
using System.IO;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.WinForms.StoreManagementNodes
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> without a known owning interface for display in a GUI.
    /// </summary>
    public sealed class OrphanedImplementationNode : ImplementationNode
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return Resources.UnknownInterface + "#" + Digest + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new orphaned implementation node.
        /// </summary>
        /// <param name="digest">The digest identifying the implementation.</param>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about IO tasks.</param>
        /// <exception cref="FormatException">Thrown if the manifest file is not valid.</exception>
        /// <exception cref="IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public OrphanedImplementationNode(ManifestDigest digest, IStore store, ITaskHandler handler)
            : base(digest, store, handler)
        {}
        #endregion
    }
}
