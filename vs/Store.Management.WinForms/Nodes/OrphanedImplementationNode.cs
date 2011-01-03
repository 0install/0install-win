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
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> without a known owning interface for display in a GUI.
    /// </summary>
    public sealed class OrphanedImplementationNode : ImplementationNode
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name
        {
            get { return "Unknown#" + Digest + (SuffixCounter == 0 ? "" : " " + SuffixCounter); }
            set { throw new NotSupportedException(); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new orphaned implementation node.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <param name="digest">The digest identifying the implementation.</param>
        /// <param name="parent">The window containing this node. Used for callbacks.</param>
        public OrphanedImplementationNode(IStore store, ManifestDigest digest, MainForm parent)
            : base(store, digest, parent)
        {}        
        #endregion
    }
}
