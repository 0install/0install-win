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
using System.IO;
using System.Net;
using NUnit.Framework;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Contains test methods for <see cref="ForEachTask{T}"/>.
    /// </summary>
    [TestFixture]
    public class ForEachTaskTest
    {
        [Test(Description = "Ensures the work delegate gets called synchronously.")]
        public void TestCallback()
        {
            var target = new[] {"element1", "element2", "element2"};
            var calledFor = new List<string>();

            var task = new ForEachTask<string>("Test task", target, calledFor.Add);
            task.Run();

            CollectionAssert.AreEqual(target, calledFor);
        }

        [Test(Description = "Ensures exceptions from the work delegate get correctly passed through.")]
        public void TestExceptionPassing()
        {
            Assert.Throws<IOException>(() => new ForEachTask<string>("Test task", new[] {""}, delegate { throw new IOException("Test exception"); }).Run());
            Assert.Throws<WebException>(() => new ForEachTask<string>("Test task", new[] {""}, delegate { throw new WebException("Test exception"); }).Run());
        }
    }
}
