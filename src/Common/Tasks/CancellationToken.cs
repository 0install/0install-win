﻿/*
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

namespace Common.Tasks
{
    /// <summary>
    /// Propagates notification that operations should be canceled.
    /// </summary>
    [Serializable]
    public struct CancellationToken
    {
        private readonly CancellationTokenSource _source;

        /// <summary>
        /// Creates a new token controlled by a specific <see cref="CancellationTokenSource"/>.
        /// </summary>
        internal CancellationToken(CancellationTokenSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Registers a delegate that will be called when <see cref="IsCancellationRequested"/> is set.
        /// </summary>
        /// <param name="callback">The delegate to be executed when <see cref="IsCancellationRequested"/> is set.</param>
        /// <returns>A handle that can be used to deregister the callback.</returns>
        /// <remarks>
        /// The callback is called from a background thread. Wrap via synchronization context to update UI elements.
        /// Handling this blocks the task, therefore observers should handle the event quickly.
        /// </remarks>
        public CancellationTokenRegistration Register(Action callback)
        {
            return new CancellationTokenRegistration(_source, callback);
        }

        /// <summary>
        /// Indicates whether <see cref="IsCancellationRequested"/> has been set.
        /// </summary>
        public bool IsCancellationRequested { get { return (_source != null) && _source.IsCancellationRequested; } }

        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if <see cref="IsCancellationRequested"/> has been set.
        /// </summary>
        /// <exception cref="OperationCanceledException">Thrown if <see cref="IsCancellationRequested"/> has been set.</exception>
        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested) throw new OperationCanceledException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "CancellationToken {IsCancellationRequested=" + IsCancellationRequested + "}";
        }
    }
}
