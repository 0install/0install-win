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
    /// An undo command that does something different on the first call to <see cref="Execute"/> than on subsequent redo calls.
    /// </summary>
    public abstract class FirstExecuteCommand : IUndoCommand
    {
        #region Variables
        private bool _actionPerformed, _undoPerformed;
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Performs the desired action.
        /// </summary>
        public void Execute()
        {
            // We cannot perform the action repeatedly in a row
            if (_actionPerformed && !_undoPerformed) throw new InvalidOperationException(Resources.RedoNotAvailable);

            if (_actionPerformed) OnRedo();
            else OnFirstExecute();

            // Action performed at least once, ready for undo
            _actionPerformed = true;
            _undoPerformed = false;
        }

        /// <summary>
        /// Hook to perform the desired action the first time.
        /// </summary>
        protected abstract void OnFirstExecute();

        /// <summary>
        /// Hook to perform the desired action again.
        /// </summary>
        protected abstract void OnRedo();
        #endregion

        #region Undo
        /// <summary>
        /// Undoes the changes made by <see cref="Execute"/>.
        /// </summary>
        public virtual void Undo()
        {
            // If the action has not been performed yet, we cannnot undo it
            if (!_actionPerformed || _undoPerformed) throw new InvalidOperationException(Resources.UndoNotAvailable);

            OnUndo();

            // Ready for redo, don't undo again
            _undoPerformed = true;
        }

        /// <summary>
        /// Hook to undo the changes made by <see cref="OnFirstExecute"/> or <see cref="OnRedo"/>.
        /// </summary>
        protected abstract void OnUndo();
        #endregion
    }
}
