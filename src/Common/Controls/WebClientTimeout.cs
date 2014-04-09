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
using System.Net;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Adds a customizable timout to <see cref="WebClient"/>.
    /// </summary>
    public class WebClientTimeout : WebClient
    {
        /// <summary>
        /// The default timeout value, in milliseconds, used when no explicit value is specified.
        /// </summary>
        public const int DefaultTimeout = 20000; // 20 seconds

        private readonly int _timeout;

        /// <summary>
        /// Creates a new <see cref="WebClient"/> using <see cref="DefaultTimeout"/>.
        /// </summary>
        public WebClientTimeout() : this(DefaultTimeout)
        {}

        /// <summary>
        /// Creates a new <see cref="WebClient"/>.
        /// </summary>
        /// <param name="timeout">The length of time, in milliseconds, before requests made by this <see cref="WebClient"/> time out.</param>
        public WebClientTimeout(int timeout)
        {
            _timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            if (result != null) result.Timeout = _timeout;
            return result;
        }
    }
}
