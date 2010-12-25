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
using System.ComponentModel;
using System.IO;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> for display in a GUI.
    /// </summary>
    public abstract class ImplementationNode : StoreNode
    {
        #region Variables
        private readonly IStore _store;
        private readonly ManifestDigest _digest;
        #endregion

        #region Properities
        /// <summary>
        /// The digest identifying the implementation in the store.
        /// </summary>
        [Description("The digest identifying the implementation in the store.")]
        public string Digest { get { return _digest.BestDigest; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation node.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <param name="digest">The digest identifying the implementation.</param>
        protected ImplementationNode(IStore store, ManifestDigest digest)
        {
            _store = store;
            _digest = digest;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes this implemenation from the <see cref="IStore"/> it is located in.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if no matching implementation could be found in the <see cref="IStore"/>.</exception>
        /// <exception cref="IOException">Thrown if the implementation could not be deleted because it was in use.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the store is not permitted.</exception>
        public override void Delete()
        {
            try { _store.Remove(_digest); }
            #region Error handling
            catch (ImplementationNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }
            #endregion
        }
        #endregion
    }
}
