/*
 * Copyright 2006-2011 Bastian Eicher
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

namespace Common.Undo
{
    /// <summary>
    /// Combines multiple <see cref="IUndoCommand"/>s into a single atomic transaction.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    public class CompositeCommand : SimpleCommand
    {
        #region Variables
        private readonly C5.ISequenced<IUndoCommand> _commands = new C5.LinkedList<IUndoCommand>();
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new composite command.
        /// </summary>
        /// <param name="commands">The commands to be contained inside the transaction.</param>
        public CompositeCommand(IEnumerable<IUndoCommand> commands)
        {
            #region Sanity checks
            if (commands == null) throw new ArgumentNullException("commands");
            #endregion

            // Defensive copy to prevent outside modification
            _commands.AddAll(commands);
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <summary>
        /// Executes all the contained <see cref="IUndoCommand"/>s in order.
        /// </summary>
        protected override void OnExecute()
        {
            foreach (var command in _commands)
                command.Execute();
        }

        /// <summary>
        /// Undoes all the contained <see cref="IUndoCommand"/>s in reverse order.
        /// </summary>
        protected override void OnUndo()
        {
            foreach (var command in _commands.Backwards())
                command.Undo();
        }
        #endregion
    }
}
