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
using System.Xml;

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// Allows you to specify a <see cref="XmlQualifiedName"/> (namespace short-name) for <see cref="XmlStorage"/> to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Values set in constructor are available via QualifiedName")]
    public sealed class XmlNamespaceAttribute : Attribute
    {
        /// <summary>
        /// The <see cref="XmlQualifiedName"/>.
        /// </summary>
        public XmlQualifiedName QualifiedName { get; set; }

        /// <summary>
        /// Specified a <see cref="XmlQualifiedName"/> (namespace short-name) for <see cref="XmlStorage"/> to use.
        /// </summary>
        /// <param name="name">The short-name.</param>
        /// <param name="ns">The full namespace URI.</param>
        public XmlNamespaceAttribute(string name, string ns)
        {
            QualifiedName = new XmlQualifiedName(name, ns);
        }
    }
}
