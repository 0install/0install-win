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

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Represents a callback delegate that has been registered with a <see cref="CancellationToken"/>.
    /// </summary>
    [Serializable]
    public struct CancellationTokenRegistration : IDisposable
    {
        [SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields", Justification = "Access to this field is remoted.")]
        private readonly CancellationTokenSource _source;

        private readonly Action _callback;

        internal CancellationTokenRegistration(CancellationTokenSource source, Action callback)
        {
            _source = source;
            _callback = callback;

            if (_source != null) _source.CancellationRequested += _callback;
        }

        /// <summary>
        /// Unregisters the callback.
        /// </summary>
        public void Dispose()
        {
            if (_source != null) _source.CancellationRequested -= _callback;
        }
    }
}
