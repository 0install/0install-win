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

using NUnit.Framework;

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// Contains test methods for <see cref="CommandCollector"/>.
    /// </summary>
    [TestFixture]
    public class CommandCollectorTest
    {
        private class MockCommand : IUndoCommand
        {
            public bool Executed { get; private set; }

            public void Execute()
            {
                Executed = true;
            }

            public void Undo()
            {
                Executed = false;
            }
        }

        /// <summary>
        /// Makes sure <see cref="CommandCollector"/> correctly collects and composes commands.
        /// </summary>
        [Test]
        public void Test()
        {
            var collector = new CommandCollector();

            var command1 = new MockCommand();
            collector.Execute(command1);
            Assert.IsTrue(command1.Executed, "Should execute while collecting");
            var command2 = new MockCommand();
            collector.Execute(command2);
            Assert.IsTrue(command2.Executed, "Should execute while collecting");

            var composite = collector.BuildComposite();
            composite.Execute();
            composite.Undo();
            Assert.IsFalse(command1.Executed, "Should undo as part of composite");
            // ReSharper disable once HeuristicUnreachableCode
            Assert.IsFalse(command2.Executed, "Should undo as part of composite");
        }
    }
}
