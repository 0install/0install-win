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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Common.Controls;
using Common.Storage;
using Common.Undo;

namespace Common.StructureEditor
{
    public partial class ContainerDescription<TContainer>
    {
        private partial class ListDescription<TList>
        {
            /// <inheritdoc/>
            public IListDescription<TList> AddElement<TElement, TEditor>()
                where TElement : class, TList, IEquatable<TElement>, new()
                where TEditor : Control, IEditorControl<TElement>, new()
            {
                _descriptions.Add(new ElementDescription<TElement, TEditor>());
                return this;
            }

            /// <inheritdoc/>
            public IListDescription<TList> AddElement<TElement>()
                where TElement : class, TList, IEquatable<TElement>, new()
            {
                return AddElement<TElement, EditorControl<TElement>>();
            }

            private interface IElementDescription
            {
                IEnumerable<EntryInfo> GetEntrysIn(IList<TList> list);
                ChildInfo GetPossibleChildFor(IList<TList> list);
            }

            private class ElementDescription<TElement, TEditor> : IElementDescription
                where TElement : class, TList, IEquatable<TElement>, new()
                where TEditor : Control, IEditorControl<TElement>, new()
            {
                public IEnumerable<EntryInfo> GetEntrysIn(IList<TList> list)
                {
                    return list.OfType<TElement>().Select(element =>
                        new EntryInfo(
                            target: element,
                            getEditorControl: commandExecutor => new TEditor {Target = element, CommandExecutor = commandExecutor},
                            toXmlString: element.ToXmlString,
                            fromXmlString: xmlString =>
                            {
                                var newValue = XmlStorage.FromXmlString<TElement>(xmlString);
                                return newValue.Equals(element) ? null : new ReplaceInList<TList>(list, element, newValue);
                            },
                            delete: commandExecutor => commandExecutor.ExecuteCommand(new RemoveFromCollection<TList>(list, element))));
                }

                public ChildInfo GetPossibleChildFor(IList<TList> list)
                {
                    return new ChildInfo(
                        name: typeof(TElement).Name,
                        create: () => new AddToCollection<TList>(list, new TElement()));
                }
            }
        }
    }
}
