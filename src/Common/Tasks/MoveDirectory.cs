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
using System.IO;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Moves the content of a directory to a new location preserving the original file modification times.
    /// </summary>
    public class MoveDirectory : CopyDirectory
    {
        /// <summary>
        /// Creates a new directory move task.
        /// </summary>
        /// <param name="sourcePath">The path of source directory. Must exist!</param>
        /// <param name="destinationPath">The path of the target directory. May exist. Must be empty if <paramref name="overwrite"/> is <see langword="false"/>.</param>
        /// <param name="preserveDirectoryTimestamps"><see langword="true"/> to preserve the modification times for directories as well; <see langword="false"/> to preserve only the file modification times.</param>
        /// <param name="overwrite">Overwrite exisiting files and directories at the <paramref name="destinationPath"/>. This will even replace read-only files!</param>
        public MoveDirectory(string sourcePath, string destinationPath, bool preserveDirectoryTimestamps = true, bool overwrite = false)
            : base(sourcePath, destinationPath, preserveDirectoryTimestamps, overwrite)
        {}

        protected override void Execute()
        {
            base.Execute();

            Directory.Delete(SourcePath, recursive: true);
        }

        protected override void CopyFile(FileInfo sourceFile, FileInfo destinationFile)
        {
            #region Sanity checks
            if (sourceFile == null) throw new ArgumentNullException("sourceFile");
            if (destinationFile == null) throw new ArgumentNullException("destinationFile");
            #endregion

            if (Overwrite && destinationFile.Exists) destinationFile.Delete();
            sourceFile.MoveTo(destinationFile.FullName);
        }
    }
}
