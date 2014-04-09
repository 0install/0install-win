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
using NUnit.Framework;

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// Contains test methods for <see cref="PreExecutedCompositeCommand"/>.
    /// </summary>
    [TestFixture]
    public class PreExecutedCompositeCommandTest
    {
        private class MockCommand : IUndoCommand
        {
            private readonly Action _executeCallback, _undoCallback;

            public MockCommand(Action executeCallback, Action undoCallback)
            {
                _executeCallback = executeCallback;
                _undoCallback = undoCallback;
            }

            public void Execute()
            {
                _executeCallback();
            }

            public void Undo()
            {
                _undoCallback();
            }
        }

        [Test(Description = "Makes sure executing and undoing a PreExecutedCompositeCommandTest correctly skips the first execution and undos the contained child commands.")]
        public void TestExecuteUndo()
        {
            var executeCalls = new List<int>(3);
            var undoCalls = new List<int>(3);
            var command = new PreExecutedCompositeCommand(new IUndoCommand[]
            {
                new MockCommand(() => executeCalls.Add(0), () => undoCalls.Add(0)),
                new MockCommand(() => executeCalls.Add(1), () => undoCalls.Add(1)),
                new MockCommand(() => executeCalls.Add(2), () => undoCalls.Add(2))
            });

            command.Execute();
            CollectionAssert.AreEqual(new int[] {}, executeCalls, "First execution should do nothing");

            command.Undo();
            CollectionAssert.AreEqual(new[] {2, 1, 0}, undoCalls, "Child commands should be undone in descending order");

            command.Execute();
            CollectionAssert.AreEqual(new[] {0, 1, 2}, executeCalls, "Child commands should be executed in ascending order");
        }
    }
}
