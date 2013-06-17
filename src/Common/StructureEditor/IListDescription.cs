/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Common.Controls;

namespace Common.StructureEditor
{
    public interface IListDescription<TList> where TList : class
    {
        /// <summary>
        /// Adds a list element type to the description.
        /// </summary>
        /// <typeparam name="TElement">The type of a specific element type in the list.</typeparam>
        /// <typeparam name="TEditor">An editor for modifying this type of element.</typeparam>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generics used as type-safe reflection replacement.")]
        IListDescription<TList> AddElement<TElement, TEditor>()
            where TElement : class, TList, IEquatable<TElement>, new()
            where TEditor : Control, IEditorControl<TElement>, new();

        /// <summary>
        /// Adds a list element type to the description.
        /// </summary>
        /// <typeparam name="TElement">The type of a specific element type in the list.</typeparam>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generics used as type-safe reflection replacement.")]
        IListDescription<TList> AddElement<TElement>()
            where TElement : class, TList, IEquatable<TElement>, new();
    }
}
