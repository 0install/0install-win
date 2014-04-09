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
using System.Diagnostics.CodeAnalysis;
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
        /// <summary>
        /// Adds a property to the description.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <typeparam name="TEditor">An editor for modifying the content of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getPointer">A function to retrieve a pointer to property in the container.</param>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generics used as type-safe reflection replacement.")]
        public ContainerDescription<TContainer> AddProperty<TProperty, TEditor>(string name, Func<TContainer, PropertyPointer<TProperty>> getPointer)
            where TProperty : class, IEquatable<TProperty>, new()
            where TEditor : Control, IEditorControl<TProperty>, new()
        {
            _descriptions.Add(new PropertyDescription<TProperty, TEditor>(name, getPointer));
            return this;
        }

        /// <summary>
        /// Adds a property to the description. Gives the <typeparamref name="TEditor"/> access to the <typeparamref name="TContainer"/>.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <typeparam name="TEditor">An editor for modifying the content of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getPointer">A function to retrieve a pointer to property in the container.</param>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Generics used as type-safe reflection replacement.")]
        public ContainerDescription<TContainer> AddPropertyContainerRef<TProperty, TEditor>(string name, Func<TContainer, PropertyPointer<TProperty>> getPointer)
            where TProperty : class, IEquatable<TProperty>, new()
            where TEditor : Control, IEditorControlContainerRef<TProperty, TContainer>, new()
        {
            _descriptions.Add(new PropertyDescriptionContainerRef<TProperty, TEditor>(name, getPointer));
            return this;
        }

        /// <summary>
        /// Adds a property to the description.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="getPointer">A function to retrieve a pointer to property in the container.</param>
        /// <returns>The "this" pointer for use in a "Fluent API" style.</returns>
        public ContainerDescription<TContainer> AddProperty<TProperty>(string name, Func<TContainer, PropertyPointer<TProperty>> getPointer)
            where TProperty : class, IEquatable<TProperty>, new()
        {
            return AddProperty<TProperty, GenericEditorControl<TProperty>>(name, getPointer);
        }

        private class PropertyDescription<TProperty, TEditor> : DescriptionBase
            where TProperty : class, IEquatable<TProperty>, new()
            where TEditor : Control, IEditorControl<TProperty>, new()
        {
            private readonly string _name;
            private readonly Func<TContainer, PropertyPointer<TProperty>> _getPointer;

            public PropertyDescription(string name, Func<TContainer, PropertyPointer<TProperty>> getPointer)
            {
                _name = name;
                _getPointer = getPointer;
            }

            public override IEnumerable<EntryInfo> GetEntrysIn(TContainer container)
            {
                var pointer = _getPointer(container);
                if (pointer.Value != null)
                {
                    var description = AttributeUtils.GetAttributes<DescriptionAttribute, TProperty>().FirstOrDefault();
                    yield return new EntryInfo(
                        name: _name,
                        description: (description == null) ? null : description.Description,
                        target: pointer.Value,
                        getEditorControl: executor => CreateEditor(container, pointer.Value, executor),
                        toXmlString: pointer.Value.ToXmlString,
                        fromXmlString: xmlString =>
                        {
                            var newValue = XmlStorage.FromXmlString<TProperty>(xmlString);
                            return newValue.Equals(pointer.Value) ? null : new SetValueCommand<TProperty>(pointer, newValue);
                        },
                        removeCommand: new SetValueCommand<TProperty>(pointer, null));
                }
            }

            protected virtual TEditor CreateEditor(TContainer container, TProperty value, Undo.ICommandExecutor executor)
            {
                return new TEditor {Target = value, CommandExecutor = executor};
            }

            public override IEnumerable<ChildInfo> GetPossibleChildrenFor(TContainer container)
            {
                var description = AttributeUtils.GetAttributes<DescriptionAttribute, TProperty>().FirstOrDefault();
                return new[]
                {
                    new ChildInfo(
                        name: _name,
                        description: (description == null) ? null : description.Description,
                        create: () => new SetValueCommand<TProperty>(_getPointer(container), new TProperty()))
                };
            }
        }

        private class PropertyDescriptionContainerRef<TProperty, TEditor> : PropertyDescription<TProperty, TEditor>
            where TProperty : class, IEquatable<TProperty>, new()
            where TEditor : Control, IEditorControlContainerRef<TProperty, TContainer>, new()
        {
            public PropertyDescriptionContainerRef(string name, Func<TContainer, PropertyPointer<TProperty>> getPointer) : base(name, getPointer)
            {}

            protected override TEditor CreateEditor(TContainer container, TProperty value, Undo.ICommandExecutor executor)
            {
                return new TEditor {Target = value, ContainerRef = container, CommandExecutor = executor};
            }
        }
    }
}
