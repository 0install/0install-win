﻿/*
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

using NUnit.Framework;

namespace NanoByte.Common.Dispatch
{
    /// <summary>
    /// Contains test methods for <see cref="AggregateDispatcher{TBase,TResultElement}"/>.
    /// </summary>
    [TestFixture]
    public class AggregateDispatcherTest
    {
        private abstract class Base
        {}

        private class Sub1 : Base
        {}

        private class Sub2 : Sub1
        {}

        [Test]
        public void Aggregate()
        {
            var dispatcher = new AggregateDispatcher<Base, string>
            {
                (Sub1 sub1) => new[] {"sub1"},
                (Sub2 sub2) => new[] {"sub2"}
            };

            CollectionAssert.AreEqual(new[] {"sub1"}, dispatcher.Dispatch(new Sub1()));
            CollectionAssert.AreEqual(new[] {"sub1", "sub2"}, dispatcher.Dispatch(new Sub2()));
        }

        [Test]
        public void UnknownType()
        {
            var dispatcher = new AggregateDispatcher<Base, string>();
            Assert.IsEmpty(dispatcher.Dispatch(new Sub1()));
        }
    }
}
