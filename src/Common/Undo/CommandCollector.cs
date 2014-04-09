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

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// Executes <see cref="IUndoCommand"/>s and collects them into a <see cref="CompositeCommand"/> allowing a combined undo later on.
    /// </summary>
    public class CommandCollector : ICommandExecutor
    {
        /// <inheritdoc/>
        public string Path { get; set; }

        private readonly List<IUndoCommand> _commands = new List<IUndoCommand>();

        /// <summary>
        /// Store an <see cref="IUndoCommand"/> for later execution.
        /// </summary>
        /// <param name="command">The command to be stored.</param>
        public void Execute(IUndoCommand command)
        {
            #region Sanity checks
            if (command == null) throw new ArgumentNullException("command");
            #endregion

            command.Execute();
            _commands.Add(command);
        }

        /// <summary>
        /// Creates a new <see cref="CompositeCommand"/> containing all <see cref="IUndoCommand"/>s collected so far.
        /// </summary>
        public IUndoCommand BuildComposite()
        {
            return new PreExecutedCompositeCommand(_commands);
        }
    }
}
