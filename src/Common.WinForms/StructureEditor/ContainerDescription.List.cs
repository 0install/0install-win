/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common.Controls;

namespace NanoByte.Common.StructureEditor
{
    public partial class ContainerDescription<TContainer>
    {
        /// <summary>
        /// Adds a list to the description.
        /// </summary>
        /// <typeparam name="TList">The type of elements in the list.</typeparam>
        /// <param name="getList">A function to retrieve the list from the container.</param>
        /// <returns>A list description, enabling you to specify explicit sub-types of <typeparamref name="TList"/> allowed in the list.</returns>
        public IListDescription<TContainer, TList> AddList<TList>(Func<TContainer, IList<TList>> getList)
            where TList : class
        {
            var listDescription = new ListDescription<TList>(getList);
            _descriptions.Add(listDescription);
            return listDescription;
        }

        /// <summary>
        /// Adds a list with only one type of element to the description.
        /// </summary>
        /// <typeparam name="TElement">The type of elements in the list.</typeparam>
        /// <typeparam name="TEditor">An editor for modifying this type of element.</typeparam>
        /// <param name="getList">A function to retrieve the list from the container.</param>
        /// <param name="name">The name of the element type.</param>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generics used as type-safe reflection replacement.")]
        public ContainerDescription<TContainer> AddPlainList<TElement, TEditor>(string name, Func<TContainer, IList<TElement>> getList)
            where TElement : class, IEquatable<TElement>, new()
            where TEditor : Control, IEditorControl<TElement>, new()
        {
            var listDescription = new ListDescription<TElement>(getList);
            listDescription.AddElement<TElement, TEditor>(name);
            _descriptions.Add(listDescription);
            return this;
        }

        /// <summary>
        /// Adds a list with only one type of element to the description. Gives the <typeparamref name="TEditor"/> access to the <typeparamref name="TContainer"/>.
        /// </summary>
        /// <typeparam name="TElement">The type of elements in the list.</typeparam>
        /// <typeparam name="TEditor">An editor for modifying this type of element.</typeparam>
        /// <param name="getList">A function to retrieve the list from the container.</param>
        /// <param name="name">The name of the element type.</param>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generics used as type-safe reflection replacement.")]
        public ContainerDescription<TContainer> AddPlainListContainerRef<TElement, TEditor>(string name, Func<TContainer, IList<TElement>> getList)
            where TElement : class, IEquatable<TElement>, new()
            where TEditor : Control, IEditorControlContainerRef<TElement, TContainer>, new()
        {
            var listDescription = new ListDescription<TElement>(getList);
            listDescription.AddElementContainerRef<TElement, TEditor>(name);
            _descriptions.Add(listDescription);
            return this;
        }

        /// <summary>
        /// Adds a list with only one type of element to the description.
        /// </summary>
        /// <typeparam name="TElement">The type of elements in the list.</typeparam>
        /// <param name="name">The name of the element type.</param>
        /// <param name="getList">A function to retrieve the list from the container.</param>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        public ContainerDescription<TContainer> AddPlainList<TElement>(string name, Func<TContainer, IList<TElement>> getList)
            where TElement : class, IEquatable<TElement>, new()
        {
            var listDescription = new ListDescription<TElement>(getList);
            listDescription.AddElement<TElement>(name);
            _descriptions.Add(listDescription);
            return this;
        }

        private partial class ListDescription<TList> : DescriptionBase, IListDescription<TContainer, TList>
            where TList : class
        {
            private readonly Func<TContainer, IList<TList>> _getList;
            private readonly List<IElementDescription> _descriptions = new List<IElementDescription>();

            public ListDescription(Func<TContainer, IList<TList>> getList)
            {
                _getList = getList;
            }

            public override IEnumerable<EntryInfo> GetEntrysIn(TContainer container)
            {
                return _descriptions.SelectMany(description => description.GetEntrysIn(container, _getList(container)));
            }

            public override IEnumerable<ChildInfo> GetPossibleChildrenFor(TContainer container)
            {
                return _descriptions.Select(description => description.GetPossibleChildFor(_getList(container)));
            }
        }
    }
}
