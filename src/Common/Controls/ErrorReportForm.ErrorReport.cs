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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// Wraps information about an crash in a serializer-friendly format.
    /// </summary>
    // Note: Must be public, not internal, so XML Serialization will work
    [XmlType("error-report")]
    [XmlRoot("error-report")]
    public class ErrorReport
    {
        #region Properties
        /// <summary>
        /// Information about the current application.
        /// </summary>
        [XmlElement("application")]
        public ApplicationInformation Application { get; set; }

        /// <summary>
        /// Information about the exception that occured.
        /// </summary>
        [XmlElement("exception")]
        public ExceptionInformation Exception { get; set; }

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
        #endregion
    }

    /// <summary>
    /// Wraps information about an exception in a serializer-friendly format.
    /// </summary>
    // Note: Must be public, not internal, so XML Serialization will work
    [XmlType("exception")]
    public class ExceptionInformation
    {
        #region Properties
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

    /// <summary>
    /// Wraps information about the current application in a serializer-friendly format.
    /// </summary>
    // Note: Must be public, not internal, so XML Serialization will work
    [XmlType("application")]
    public class ApplicationInformation
    {
        #region Properties
        /// <summary>
        /// The name of the application.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The version number of the application.
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; }

        /// <summary>
        /// The command-line arguments the application was started with.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Used for XML serialization.")]
        [XmlElement("arg")]
        public string[] Arguments { get; set; }
        #endregion

        #region Collect
        /// <summary>
        /// Collects information about the current application
        /// </summary>
        public static ApplicationInformation Collect()
        {
            return new ApplicationInformation
            {
                Name = AppInfo.Name,
                Version = AppInfo.Version.ToString(),
                Arguments = Environment.GetCommandLineArgs()
            };
        }
        #endregion
    }
}