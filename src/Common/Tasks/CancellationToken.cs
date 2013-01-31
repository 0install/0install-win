/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Common.Tasks
{
    /// <summary>
    /// Propagates notification that operations should be canceled.<br/>
    /// Once a token has been signaled it remains in that state and cannot be reused.
    /// </summary>
    public sealed class CancellationToken : MarshalByRefObject
    {
        private volatile bool _isCancellationRequested; // Volatile justification: Write access is locked, many reads

        /// <summary>
        /// Indicates whether <see cref="RequestCancellation"/> has been called.
        /// </summary>
        public bool IsCancellationRequested { get { return _isCancellationRequested; } }

        /// <summary>
        /// Raised the first time <see cref="RequestCancellation"/> is called. Subsequent calls will not raise this event again.
        /// </summary>
        /// <remarks>
        ///   <para>This event is raised from a background thread. Wrap via <see cref="Control.Invoke(System.Delegate)"/> to update UI elements.</para>
        ///   <para>Handling this blocks the task, therefore observers should handle the event quickly.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action CancellationRequested;

        private readonly object _lock = new object();

        /// <summary>
        /// Notifies all listening entities that their operations should be canceled.
        /// </summary>
        public void RequestCancellation()
        {
            lock (_lock)
            {
                if (_isCancellationRequested) return; // Don't trigger more than once
                _isCancellationRequested = true;
                if (CancellationRequested != null) CancellationRequested();
            }
        }

        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if <see cref="RequestCancellation"/> has been called.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if <see cref="RequestCancellation"/> has been called.</exception>
        public void ThrowIfCancellationRequested()
        {
            if (_isCancellationRequested) throw new OperationCanceledException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "CancellationToken {IsCancellationRequested=" + IsCancellationRequested + "}";
        }
    }
}
