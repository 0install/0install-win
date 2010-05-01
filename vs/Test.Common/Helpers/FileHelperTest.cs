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

using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Common.Helpers
{
    /// <summary>
    /// Contains test methods for <see cref="FileHelper"/>.
    /// </summary>
    [TestFixture]
    public class FileHelperTest
    {
        /// <summary>
        /// Ensures <see cref="FileHelper.ComputeHash"/> correctly hashes files.
        /// </summary>
        [Test]
        public void TestString()
        {
            const string sha1ForEmptyString = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

            string tempFile = null;
            try
            {
                // Create and hash an empty file
                tempFile = Path.GetTempFileName();
                Assert.AreEqual(sha1ForEmptyString, FileHelper.ComputeHash(tempFile, SHA1.Create()));
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }
        }
    }
}
