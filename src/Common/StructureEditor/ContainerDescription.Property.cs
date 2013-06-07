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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Common.Controls;
using Common.Storage;
using Common.Undo;

namespace Common.StructureEditor
{
    public partial class ContainerDescription<TContainer>
    {
        /// <summary>
        /// Adds a property to the description.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <typeparam name="TEditor">An editor for modifying the content of the property.</typeparam>
        /// <param name="getPointer">A function to retrieve a pointer to property in the container.</param>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generics used as type-safe reflection replacement.")]
        public ContainerDescription<TContainer> AddProperty<TProperty, TEditor>(Func<TContainer, PropertyPointer<TProperty>> getPointer)
            where TProperty : class, new()
            where TEditor : Control, IEditorControl<TProperty>, new()
        {
            _descriptions.Add(new PropertyDescription<TProperty, TEditor>(getPointer));
            return this;
        }

        public ContainerDescription<TContainer> AddProperty<TProperty>(Func<TContainer, PropertyPointer<TProperty>> getPointer)
            where TProperty : class, new()
        {
            return AddProperty<TProperty, EditorControl<TProperty>>(getPointer);
        }

        private class PropertyDescription<TProperty, TEditor> : DescriptionBase
            where TProperty : class, new()
            where TEditor : Control, IEditorControl<TProperty>, new()
        {
            private readonly Func<TContainer, PropertyPointer<TProperty>> _getPointer;

            public PropertyDescription(Func<TContainer, PropertyPointer<TProperty>> getPointer)
            {
                _getPointer = getPointer;
            }

            public override IEnumerable<EntryInfo> GetEntrysIn(TContainer container)
            {
                var pointer = _getPointer(container);
                if (pointer.Value != null)
                {
                    yield return new EntryInfo(
                        pointer.Value,
                        commandExecutor => new TEditor {Target = pointer.Value, CommandExecutor = commandExecutor},
                        pointer.Value.ToXmlString,
                        commandExecutor => commandExecutor.ExecuteCommand(new SetValueCommand<TProperty>(pointer, null)));
                }
            }

            public override IEnumerable<ChildInfo> GetPossibleChildrenFor(TContainer container)
            {
                return new[]
                {
                    new ChildInfo(typeof(TProperty).Name, executor => executor.ExecuteCommand(
                        new SetValueCommand<TProperty>(_getPointer(container), new TProperty())))
                };
            }
        }
    }
}
