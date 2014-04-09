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

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// A temporary directory with a file that may or may not exist to indicate whether a certain condition is true or false.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
    public class TemporaryFlagFile : TemporaryDirectory
    {
        /// <inheritdoc/>
        public TemporaryFlagFile(string prefix) : base(prefix)
        {}

        #region Properties
        /// <summary>
        /// The fully qualified path of the flag file.
        /// </summary>
        public new string Path { get { return System.IO.Path.Combine(base.Path, "flag"); } }

        public static implicit operator string(TemporaryFlagFile dir)
        {
            return (dir == null) ? null : dir.Path;
        }

        /// <summary>
        /// Indicates or controls whether the file exists.
        /// </summary>
        public bool Set
        {
            get { return File.Exists(Path); }
            set
            {
                if (value) File.WriteAllText(Path, "");
                else File.Delete(Path);
            }
        }
        #endregion
    }
}
