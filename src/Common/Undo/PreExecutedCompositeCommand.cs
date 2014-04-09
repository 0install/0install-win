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
    /// Combines multiple already executed <see cref="IUndoCommand"/>s into a single atomic transaction.
    /// </summary>
    public class PreExecutedCompositeCommand : PreExecutedCommand
    {
        #region Variables
        private readonly List<IUndoCommand> _commands;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new composite command.
        /// </summary>
        /// <param name="commands">The commands to be contained inside the transaction.</param>
        public PreExecutedCompositeCommand(IEnumerable<IUndoCommand> commands)
        {
            #region Sanity checks
            if (commands == null) throw new ArgumentNullException("commands");
            #endregion

            // Defensive copy
            _commands = new List<IUndoCommand>(commands);
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <summary>
        /// Executes all the contained <see cref="IUndoCommand"/>s in order.
        /// </summary>
        protected override void OnRedo()
        {
            int countExecute = 0;
            try
            { // Try to execute all commands
                for (countExecute = 0; countExecute < _commands.Count; countExecute++)
                    _commands[countExecute].Execute();
            }
            catch
            { // Rollback before reporting exception
                for (int countUndo = countExecute - 1; countUndo >= 0; countUndo--)
                    _commands[countUndo].Undo();
                throw;
            }
        }

        /// <summary>
        /// Undoes all the contained <see cref="IUndoCommand"/>s in reverse order.
        /// </summary>
        protected override void OnUndo()
        {
            int countUndo = 0;
            try
            { // Try to undo all commands
                for (countUndo = _commands.Count - 1; countUndo >= 0; countUndo--)
                    _commands[countUndo].Undo();
            }
            catch
            { // Rollback before reporting exception
                for (int countExecute = countUndo + 1; countExecute < _commands.Count; countExecute++)
                    _commands[countExecute].Execute();
                throw;
            }
        }
        #endregion
    }
}
