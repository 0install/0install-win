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

namespace Common.Undo
{
    /// <summary>
    /// An undo command that uses a delegates for getting and setting values from a backing model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SetValueCommand<T> : SimpleCommand
    {
        #region Variables
        private readonly T _newValue;
        private T _oldValue;
        private readonly SimpleResult<T> _getValue;
        private readonly Action<T> _setValue;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new value-setting command.
        /// </summary>
        /// <param name="newValue">The new value to be set</param>
        /// <param name="getValue">A delegate that returns the current value from the model.</param>
        /// <param name="setValue">A delegate that changes the value in the model.</param>
        public SetValueCommand(T newValue, SimpleResult<T> getValue, Action<T> setValue)
        {
            #region Sanity checks
            if (getValue == null) throw new ArgumentNullException("getValue");
            if (setValue == null) throw new ArgumentNullException("setValue");
            #endregion

            _newValue = newValue;
            _getValue = getValue;
            _setValue = setValue;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <summary>
        /// Sets the new value in the model.
        /// </summary>
        protected override void OnExecute()
        {
            _oldValue = _getValue();
            _setValue(_newValue);
        }

        /// <summary>
        /// Restores the old value in the model.
        /// </summary>
        protected override void OnUndo()
        {
            _setValue(_oldValue);
        }
        #endregion
    }
}
