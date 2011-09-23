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
using System.Runtime.Serialization;
using Common.Properties;

namespace Common
{
    /// <summary>
    /// To be thrown when an action was canceled because the user wished it to be.
    /// </summary>
    [Serializable]
    public sealed class UserCancelException : Exception
    {
        #region Constructor
        /// <summary>
        /// Creates a new user cancel exception.
        /// </summary>
        public UserCancelException() : base(Resources.UserCancel)
        {}

        /// <inheritdoc />
        public UserCancelException(string message) : base(message)
        {}

        /// <inheritdoc />
        public UserCancelException(string message, Exception innerException) : base(message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private UserCancelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
        #endregion
    }
}
