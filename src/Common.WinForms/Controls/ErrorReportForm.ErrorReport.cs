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

using System.ComponentModel;
using System.Xml.Serialization;
using NanoByte.Common.Info;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Wraps information about an crash in a serializer-friendly format.
    /// </summary>
    // Note: Must be public, not internal, so XML Serialization will work
    [XmlRoot("error-report"), XmlType("error-report")]
    public class ErrorReport
    {
        /// <summary>
        /// Information about the current application.
        /// </summary>
        [XmlElement("application")]
        public AppInfo Application { get; set; }

        /// <summary>
        /// Information about the current operating system.
        /// </summary>
        [XmlElement("os")]
        public OSInfo OS { get; set; }

        /// <summary>
        /// Information about the exception that occured.
        /// </summary>
        [XmlElement("exception")]
        public ExceptionInfo Exception { get; set; }

        /// <summary>
        /// The contents of the <see cref="Log"/> file.
        /// </summary>
        [XmlElement("log"), DefaultValue("")]
        public string Log { get; set; }

        /// <summary>
        /// Comments about the crash entered by the user.
        /// </summary>
        [XmlElement("comments"), DefaultValue("")]
        public string Comments { get; set; }
    }
}
