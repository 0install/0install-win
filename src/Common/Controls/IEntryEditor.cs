﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// Provides an interface to a dialog that edits a single element in a feed.
    /// </summary>
    /// <typeparam name="T">The type of element to edit.</typeparam>
    public interface IEntryEditor<T> : IDisposable where T : class
    {
        /// <summary>
        /// Displays a modal dialog for editing an element.
        /// </summary>
        /// <param name="owner">The parent window used to make the dialog modal.</param>
        /// <param name="element">The element to be edited.</param>
        /// <returns>Save changes if <see cref="DialogResult.OK"/>; discard changes if  <see cref="DialogResult.Cancel"/>.</returns>
        DialogResult ShowDialog(IWin32Window owner, T element);
    }
}
