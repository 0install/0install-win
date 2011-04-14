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

using System.Text;

namespace Common.Controls
{
    /// <seealso cref="RtfBuilder.AppendPar"/>
    public enum RtfColor { Black = 1, Blue = 2, Green = 3, Yellow = 4, Orange = 5, Red = 6 }

    /// <summary>
    /// Helps build an RTF-formated string.
    /// </summary>
    public sealed class RtfBuilder
    {
        private static readonly StringBuilder _builder = new StringBuilder();

        private const string rtfHeader = "{\\rtf1\r\n{\\colortbl ;\\red0\\green0\\blue0;\\red0\\green0\\blue255;\\red0\\green255\\blue0;\\red255\\gree255\\blue0;\\red255\\gree106\\blue0;\\red255\\gree0\\blue0;}\r\n";
        private const string rtfFooter = "}";

        /// <summary>
        /// Appends a new paragraph.
        /// </summary>
        /// <param name="text">The text in the paragraph.</param>
        /// <param name="color">The color of the text.</param>
        public void AppendPar(string text, RtfColor color)
        {
            _builder.AppendLine("\\cf" + ((int)color) + " " + text + "\\par\\par");
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return rtfHeader + _builder + rtfFooter;
        }
    }
}
