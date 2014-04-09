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
using NanoByte.Common.Properties;

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// An undo command that automatically tracks when <see cref="Execute"/> and <see cref="Undo"/> can be called.
    /// </summary>
    public abstract class SimpleCommand : IUndoCommand
    {
        #region Variables
        private bool _undoAvailable;
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Performs the desired action.
        /// </summary>
        public void Execute()
        {
            // We cannot perform the action repeatedly in a row
            if (_undoAvailable) throw new InvalidOperationException(Resources.RedoNotAvailable);

            OnExecute();

            // Ready for undo, don't redo
            _undoAvailable = true;
        }

        /// <summary>
        /// Hook to perform the desired action.
        /// </summary>
        protected abstract void OnExecute();
        #endregion

        #region Undo
        /// <summary>
        /// Undoes the changes made by <see cref="Execute"/>.
        /// </summary>
        public virtual void Undo()
        {
            // If the action has not been performed yet, we cannnot undo it
            if (!_undoAvailable) throw new InvalidOperationException(Resources.UndoNotAvailable);

            OnUndo();

            // As if the action had never happened
            _undoAvailable = false;
        }

        /// <summary>
        /// Hook to undo the changes made by <see cref="OnExecute"/>.
        /// </summary>
        protected abstract void OnUndo();
        #endregion
    }
}
