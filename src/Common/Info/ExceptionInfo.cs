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
using System.Xml.Serialization;

namespace NanoByte.Common.Info
{
    /// <summary>
    /// Wraps information about an exception in a serializer-friendly format.
    /// </summary>
    [XmlType("exception")]
    public class ExceptionInfo
    {
        /// <summary>
        /// The type of exception.
        /// </summary>
        [XmlAttribute("type")]
        public string ExceptionType { get; set; }

        /// <summary>
        /// The message describing the exception.
        /// </summary>
        [XmlElement("message")]
        public string Message { get; set; }

        /// <summary>
        /// The name of the application or the object that causes the error.
        /// </summary>
        [XmlElement("source")]
        public string Source { get; set; }

        /// <summary>
        /// A string representation of the frames on the call stack at the time the exception was thrown.
        /// </summary>
        [XmlElement("stack-trace")]
        public string StackTrace { get; set; }

        /// <summary>
        /// Information about the exception that originally caused the exception being described here.
        /// </summary>
        [XmlElement("inner-exception")]
        public ExceptionInfo InnerException { get; set; }

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public ExceptionInfo()
        {}

        /// <summary>
        /// Creates an exception information based on an exception.
        /// </summary>
        /// <param name="ex">The exception whose information to extract.</param>
        public ExceptionInfo(Exception ex)
            : this()
        {
            #region Sanity checks
            if (ex == null) throw new ArgumentNullException("ex");
            #endregion

            ExceptionType = ex.GetType().ToString();
            Message = ex.Message;
            Source = ex.Source;
            StackTrace = ex.StackTrace;

            if (ex.InnerException != null)
                InnerException = new ExceptionInfo(ex.InnerException);
        }
        #endregion
    }
}
