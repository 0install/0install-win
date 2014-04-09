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
using System.Text;

namespace NanoByte.Common.Controls
{
    /// <seealso cref="RtfBuilder.AppendPar"/>
    public enum RtfColor
    {
        Black = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3,
        Orange = 4,
        Red = 5
    }

    /// <summary>
    /// Helps build an RTF-formated string.
    /// </summary>
    public sealed class RtfBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// Appends a new paragraph.
        /// </summary>
        /// <param name="text">The text in the paragraph.</param>
        /// <param name="color">The color of the text.</param>
        public void AppendPar(string text, RtfColor color)
        {
            #region Sanity checks
            if (text == null) throw new ArgumentNullException("text");
            #endregion

            text = text.Replace(@"\", @"\\").Replace(Environment.NewLine, "\\par\n");
            _builder.AppendLine("\\cf" + ((int)color + 1) + " " + text + "\\par\\par\n");
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            const string rtfHeader = "{\\rtf1\r\n{\\colortbl ;\\red0\\green0\\blue0;\\red0\\green0\\blue255;\\red0\\green255\\blue0;\\red255\\green255\\blue0;\\red255\\green106\\blue0;\\red255\\green0\\blue0;}\r\n";
            const string rtfFooter = "}";
            return rtfHeader + _builder + rtfFooter;
        }
    }
}
