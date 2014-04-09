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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using NanoByte.Common.Undo;
using NanoByte.Common.Utils;

namespace NanoByte.Common.StructureEditor
{
    public partial class ContainerDescription<TContainer>
    {
        private partial class ListDescription<TList>
        {
            /// <inheritdoc/>
            public IListDescription<TContainer, TList> AddElement<TElement, TEditor>(string name)
                where TElement : class, TList, IEquatable<TElement>, new()
                where TEditor : Control, IEditorControl<TElement>, new()
            {
                _descriptions.Add(new ElementDescription<TElement, TEditor>(name));
                return this;
            }

            /// <inheritdoc/>
            public IListDescription<TContainer, TList> AddElementContainerRef<TElement, TEditor>(string name)
                where TElement : class, TList, IEquatable<TElement>, new()
                where TEditor : Control, IEditorControlContainerRef<TElement, TContainer>, new()
            {
                _descriptions.Add(new ElementDescriptionContainerRef<TElement, TEditor>(name));
                return this;
            }

            /// <inheritdoc/>
            public IListDescription<TContainer, TList> AddElement<TElement>(string name)
                where TElement : class, TList, IEquatable<TElement>, new()
            {
                return AddElement<TElement, GenericEditorControl<TElement>>(name);
            }

            private interface IElementDescription
            {
                IEnumerable<EntryInfo> GetEntrysIn(TContainer container, IList<TList> list);
                ChildInfo GetPossibleChildFor(IList<TList> list);
            }

            private class ElementDescription<TElement, TEditor> : IElementDescription
                where TElement : class, TList, IEquatable<TElement>, new()
                where TEditor : Control, IEditorControl<TElement>, new()
            {
                private readonly string _name;

                public ElementDescription(string name)
                {
                    _name = name;
                }

                public IEnumerable<EntryInfo> GetEntrysIn(TContainer container, IList<TList> list)
                {
                    var description = AttributeUtils.GetAttributes<DescriptionAttribute, TElement>().FirstOrDefault();
                    return list.OfType<TElement>().Select(element =>
                        new EntryInfo(
                            name: _name,
                            description: (description == null) ? null : description.Description,
                            target: element,
                            getEditorControl: executor => CreateEditor(container, element, executor),
                            toXmlString: element.ToXmlString,
                            fromXmlString: xmlString =>
                            {
                                var newValue = XmlStorage.FromXmlString<TElement>(xmlString);
                                return newValue.Equals(element) ? null : new ReplaceInList<TList>(list, element, newValue);
                            },
                            removeCommand: new RemoveFromCollection<TList>(list, element)));
                }

                protected virtual TEditor CreateEditor(TContainer container, TElement value, Undo.ICommandExecutor executor)
                {
                    return new TEditor {Target = value, CommandExecutor = executor};
                }

                public ChildInfo GetPossibleChildFor(IList<TList> list)
                {
                    var description = AttributeUtils.GetAttributes<DescriptionAttribute, TElement>().FirstOrDefault();
                    return new ChildInfo(
                        name: _name,
                        description: (description == null) ? null : description.Description,
                        create: () => new AddToCollection<TList>(list, new TElement()));
                }
            }

            private class ElementDescriptionContainerRef<TElement, TEditor> : ElementDescription<TElement, TEditor>
                where TElement : class, TList, IEquatable<TElement>, new()
                where TEditor : Control, IEditorControlContainerRef<TElement, TContainer>, new()
            {
                public ElementDescriptionContainerRef(string name) : base(name)
                {}

                protected override TEditor CreateEditor(TContainer container, TElement value, Undo.ICommandExecutor executor)
                {
                    return new TEditor {Target = value, ContainerRef = container, CommandExecutor = executor};
                }
            }
        }
    }
}
