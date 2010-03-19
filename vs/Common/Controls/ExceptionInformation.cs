/*
 * Copyright 2006-2010 Bastian Eicher
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

namespace Common.Controls
{
    /// <summary>
    /// Wraps information about an exception in a serializer-friendly format.
    /// </summary>
    // Note: Must be public, not internal, so XML Serialization will work
    public class ExceptionInformation
    {
        #region Properties
        /// <summary>
        /// The type of exception.
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// The message describing the exception.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The name of the application or the object that causes the error.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// A string representation of the frames on the call stack at the time the exception was thrown.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Information about the exception that originally caused the exception being described here.
        /// </summary>
        public ExceptionInformation InnerException { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public ExceptionInformation()
        {}

        /// <summary>
        /// Creates an exception information based on an exception.
        /// </summary>
        /// <param name="ex">The exception whose information to extract.</param>
        public ExceptionInformation(Exception ex) : this()
        {
            #region Sanity checks
            if (ex == null) throw new ArgumentNullException("ex");
            #endregion

            ExceptionType = ex.GetType().ToString();
            Message = ex.Message;
            Source = ex.Source;
            StackTrace = ex.StackTrace;

            if (ex.InnerException != null)
                InnerException = new ExceptionInformation(ex.InnerException);
        }
        #endregion
    }
}