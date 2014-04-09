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

using System.ComponentModel;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Provides an interface to a control that edits a single object and has a reference to the containing object.
    /// </summary>
    /// <typeparam name="T">The type of object to edit.</typeparam>
    /// <typeparam name="TContainer">The type of the container of <typeparamref name="T"/>.</typeparam>
    public interface IEditorControlContainerRef<T, TContainer> : IEditorControl<T>
    {
        /// <summary>
        /// The <see cref="IEditorControl{T}.Target"/>'s container.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        TContainer ContainerRef { get; set; }
    }
}
