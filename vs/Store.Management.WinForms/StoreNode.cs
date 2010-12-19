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
using System.ComponentModel;
using System.Windows.Forms;
using Common;
using Common.Controls;

namespace ZeroInstall.Store.Management.WinForms
{
    public abstract class StoreNode : INamed, IContextMenu
    {
        /// <inheritdoc/>
        [Browsable(false)]
        public abstract string Name { get; set; }

        /// <inheritdoc/>
        public abstract ContextMenu GetContextMenu();

        #region Comparison
        public int CompareTo(object other)
        {
            string otherName;
            var named = other as INamed;
            if (named != null) otherName = named.Name;
            else if (other != null) otherName = other.ToString();
            else otherName = null;
            return string.Compare(Name, otherName, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
