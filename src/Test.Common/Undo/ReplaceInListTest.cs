﻿/*
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

using System.Collections.Generic;
using NUnit.Framework;

namespace Common.Undo
{
    /// <summary>
    /// Contains test methods for <see cref="ReplaceInList{T}"/>.
    /// </summary>
    [TestFixture]
    public class ReplaceInListTest
    {
        /// <summary>
        /// Makes sure <see cref="ReplaceInList{T}"/> correctly performs executions and undos.
        /// </summary>
        [Test]
        public void TestExecute()
        {
            var list = new List<string> {"a", "b", "c"};
            var command = new ReplaceInList<string>(list, "b", "x");

            command.Execute();
            CollectionAssert.AreEqual(new[] { "a", "x", "c" }, list);

            command.Undo();
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, list);
        }
    }
}
