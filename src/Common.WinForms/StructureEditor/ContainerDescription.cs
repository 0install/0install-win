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

using System.Collections.Generic;
using System.Linq;
using NanoByte.Common.Collections;

namespace NanoByte.Common.StructureEditor
{
    /// <summary>
    /// Describes an object that contains properties and/or lists. Provides information about how to edit this content.
    /// </summary>
    /// <typeparam name="TContainer">The type of the container to be described.</typeparam>
    public partial class ContainerDescription<TContainer> where TContainer : class
    {
        private readonly List<DescriptionBase> _descriptions = new List<DescriptionBase>();

        /// <summary>
        /// Returns information about entries found in a specific instance of <typeparamref name="TContainer"/>.
        /// </summary>
        /// <param name="container">The container instance to look in to.</param>
        /// <returns>A list of entry information structures.</returns>
        internal IEnumerable<EntryInfo> GetEntrysIn(TContainer container)
        {
            return _descriptions.SelectMany(description => description.GetEntrysIn(container));
        }

        /// <summary>
        /// Returns information about possible new children for a specific instance of <typeparamref name="TContainer"/>.
        /// </summary>
        /// <param name="container">The container instance to look at.</param>
        /// <returns>A list of child information structures.</returns>
        internal IEnumerable<ChildInfo> GetPossibleChildrenFor(TContainer container)
        {
            return _descriptions.SelectMany(description => description.GetPossibleChildrenFor(container))
                .Append(null); // split marker
        }

        private abstract class DescriptionBase
        {
            public abstract IEnumerable<EntryInfo> GetEntrysIn(TContainer container);
            public abstract IEnumerable<ChildInfo> GetPossibleChildrenFor(TContainer container);
        }
    }
}
