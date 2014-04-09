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
using System.Threading;
using NanoByte.Common.Utils;

namespace NanoByte.Common
{
    /// <summary>
    /// Implicitly represents the result of an asynchronous operation.
    /// </summary>
    /// <typeparam name="T">The type of the result of the operation.</typeparam>
    public class Future<T>
    {
        private readonly Thread _thread;
        private Func<T> _operation;
        private T _result;

        /// <summary>
        /// Starts an asynchronous operation.
        /// </summary>
        /// <param name="operation">The operation returning a result.</param>
        public Future(Func<T> operation)
        {
            _operation = operation;
            _thread = ProcessUtils.RunBackground(() =>
            {
                _result = _operation();
                _operation = null; // Release input data memory as soon as calculation is complete
            });
        }

        /// <summary>
        /// Waits for the asynchronous operation to complete and returns the result.
        /// </summary>
        public static implicit operator T(Future<T> future)
        {
            if (future == null) return default(T);

            future._thread.Join();
            return future._result;
        }
    }
}
