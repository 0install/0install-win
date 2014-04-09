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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting;
using System.Threading;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Signals to <see cref="CancellationToken"/>s that they should be canceled.
    /// </summary>
    public sealed class CancellationTokenSource : MarshalNoTimeout, IDisposable
    {
        /// <summary>
        /// Gets a <see cref="CancellationToken"/> associated with this <see cref="CancellationTokenSource"/>.
        /// </summary>
        public CancellationToken Token { get { return new CancellationToken(this); } }

        private volatile bool _isCancellationRequested; // Volatile justification: Write access is locked, many reads

        /// <summary>
        /// Raised the first time <see cref="Cancel"/> is called. Subsequent calls will not raise this event again.
        /// </summary>
        /// <remarks>
        /// The event is raised from a background thread. Wrap via synchronization context to update UI elements.
        /// Handling this blocks the task, therefore observers should handle the event quickly.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action CancellationRequested;

        /// <summary>
        /// Indicates whether <see cref="Cancel"/> has been called.
        /// </summary>
        public bool IsCancellationRequested { get { return _isCancellationRequested; } }

        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);

        /// <summary>
        /// Gets a wait handle that is signaled when see cref="Cancel"/> has been called.
        /// </summary>
        internal WaitHandle WaitHandle { get { return _waitHandle; } }

        private readonly object _lock = new object();

        /// <summary>
        /// Notifies all listening <see cref="CancellationToken"/>s that operations should be canceled.
        /// </summary>
        public void Cancel()
        {
            lock (_lock)
            {
                // Don't trigger more than once
                if (_isCancellationRequested) return;

                _waitHandle.Set();

                _isCancellationRequested = true;
                if (CancellationRequested != null)
                {
                    try
                    {
                        CancellationRequested();
                    }
                    catch (RemotingException)
                    {}
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "CancellationTokenSource {IsCancellationRequested=" + IsCancellationRequested + "}";
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _waitHandle.Close();
        }
    }
}
