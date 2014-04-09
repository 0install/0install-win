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

using System.IO;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace NanoByte.Common.Cli
{
    /// <summary>
    /// Contains test methods for <see cref="ArgumentUtils"/>.
    /// </summary>
    [TestFixture]
    public class ArgumentUtilsTest
    {
        [Test]
        public void TestGetFilesAbsolute()
        {
            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                File.WriteAllText(Path.Combine(tempDir, "a.txt"), @"a");
                File.WriteAllText(Path.Combine(tempDir, "b.txt"), @"b");
                File.WriteAllText(Path.Combine(tempDir, "c.inf"), @"c");
                File.WriteAllText(Path.Combine(tempDir, "d.nfo"), @"d");

                string subdirPath = Path.Combine(tempDir, "dir");
                Directory.CreateDirectory(subdirPath);
                File.WriteAllText(Path.Combine(subdirPath, "1.txt"), @"1");
                File.WriteAllText(Path.Combine(subdirPath, "2.inf"), @"a");

                var result = ArgumentUtils.GetFiles(new[]
                {
                    Path.Combine(tempDir, "*.txt"), // Wildcard
                    Path.Combine(tempDir, "d.nfo"), // Specifc file
                    subdirPath // Directory with implict default wildcard
                }, "*.txt");
                Assert.AreEqual(result[0].FullName, Path.Combine(tempDir, "a.txt"));
                Assert.AreEqual(result[1].FullName, Path.Combine(tempDir, "b.txt"));
                Assert.AreEqual(result[2].FullName, Path.Combine(tempDir, "d.nfo"));
                Assert.AreEqual(result[3].FullName, Path.Combine(subdirPath, "1.txt"));
            }
        }

        [Test]
        public void TestGetFilesRelative()
        {
            using (var tempDir = new TemporaryWorkingDirectory("unit-tests"))
            {
                File.WriteAllText("a.txt", @"a");
                File.WriteAllText("b.txt", @"b");
                File.WriteAllText("c.inf", @"c");
                File.WriteAllText("d.nfo", @"d");

                const string subdirPath = "dir";
                Directory.CreateDirectory(subdirPath);
                File.WriteAllText(Path.Combine(subdirPath, "1.txt"), @"1");
                File.WriteAllText(Path.Combine(subdirPath, "2.inf"), @"a");

                var result = ArgumentUtils.GetFiles(new[]
                {
                    "*.txt", // Wildcard
                    "d.nfo", // Specifc file
                    subdirPath // Directory with implict default wildcard
                }, "*.txt");
                Assert.AreEqual(result[0].FullName, Path.Combine(tempDir, "a.txt"));
                Assert.AreEqual(result[1].FullName, Path.Combine(tempDir, "b.txt"));
                Assert.AreEqual(result[2].FullName, Path.Combine(tempDir, "d.nfo"));
                Assert.AreEqual(result[3].FullName, Path.Combine(Path.Combine(tempDir, subdirPath), "1.txt"));
            }
        }
    }
}
