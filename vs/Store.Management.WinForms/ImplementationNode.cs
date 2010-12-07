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

using System.Windows.Forms;

namespace ZeroInstall.Store.Management.WinForms
{
    public class ImplementationNode : InterfaceNode
    {
        #region Variables
        private readonly Model.Implementation _implementation;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return base.Name + "#" + _implementation; } }
        #endregion

        #region Constructor
        public ImplementationNode(Model.Feed feed, Model.Implementation implementation) : base(feed)
        {
            _implementation = implementation;
        }
        #endregion

        /// <inheritdoc/>
        public override ContextMenu GetContextMenu()
        {
            return null;
        }
    }
}
