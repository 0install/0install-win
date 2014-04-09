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
using System.Threading;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Reports progress updates using events. Automatically handles thread- and IPC-marshaling.
    /// </summary>
    public class Progress<T> : MarshalByRefObject, IProgress<T>
    {
        /// <summary>
        /// Raised for each reported progress value.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action<T> ProgressChanged;

        private readonly SynchronizationContext _synchronizationContext;

        /// <summary>
        /// Captures the current synchronization context for callbacks.
        /// </summary>
        public Progress(Action<T> callback = null)
        {
            _synchronizationContext = SynchronizationContext.Current;

            if (callback != null) ProgressChanged += callback;
        }

        void IProgress<T>.Report(T value)
        {
            OnReport(value);
        }

        protected virtual void OnReport(T value)
        {
            var callback = ProgressChanged;
            if (callback != null)
            {
                if (_synchronizationContext != null) _synchronizationContext.Post(_ => callback(value), null);
                else ThreadPool.QueueUserWorkItem(_ => callback(value));
            }
        }
    }
}
