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

using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Common.Storage
{
    /// <summary>
    /// Contains test methods for <see cref="XmlStorage"/>.
    /// </summary>
    [TestFixture]
    public class XmlStorageTest
    {
        /// <summary>
        /// A data-structure used to test serialization.
        /// </summary>
        public class TestData
        {
            public string Data { get; set; }
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.Save{T}(System.IO.Stream,T)"/> and <see cref="XmlStorage.Load{T}(System.IO.Stream)"/> work correctly.
        /// </summary>
        [Test]
        public void TestFile()
        {
            TestData testData1, testData2;
            using (var tempFile = new TemporaryFile("unit-tests"))
            {
                // Write and read file
                testData1 = new TestData {Data = "Hello"};
                XmlStorage.Save(tempFile.Path, testData1);
                testData2 = XmlStorage.Load<TestData>(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.ToZip{T}(System.IO.Stream,T,string,System.Collections.Generic.IEnumerable{Common.Storage.EmbeddedFile})"/> and <see cref="XmlStorage.FromZip{T}(System.IO.Stream,string,System.Collections.Generic.IEnumerable{Common.Storage.EmbeddedFile})"/> work correctly with no password.
        /// </summary>
        [Test]
        public void TestZipNoPassword()
        {
            // Write and read file
            var testData1 = new TestData {Data = "Hello"};
            var tempStream = new MemoryStream();
            XmlStorage.ToZip(tempStream, testData1, null, new EmbeddedFile[0]);
            tempStream.Seek(0, SeekOrigin.Begin);
            var testData2 = XmlStorage.FromZip<TestData>(tempStream, null, new EmbeddedFile[0]);

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.ToZip{T}(System.IO.Stream,T,string,System.Collections.Generic.IEnumerable{Common.Storage.EmbeddedFile})"/> and <see cref="XmlStorage.FromZip{T}(System.IO.Stream,string,System.Collections.Generic.IEnumerable{Common.Storage.EmbeddedFile})"/> work correctly with a password.
        /// </summary>
        [Test]
        public void TestZipPassword()
        {
            // Write and read file
            var testData1 = new TestData {Data = "Hello"};
            var tempStream = new MemoryStream();
            XmlStorage.ToZip(tempStream, testData1, "Test password", new EmbeddedFile[0]);
            tempStream.Seek(0, SeekOrigin.Begin);
            var testData2 = XmlStorage.FromZip<TestData>(tempStream, "Test password", new EmbeddedFile[0]);

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.FromZip{T}(System.IO.Stream,string,System.Collections.Generic.IEnumerable{Common.Storage.EmbeddedFile})"/> correctly detects incorrect passwords.
        /// </summary>
        [Test]
        public void TestIncorrectPassword()
        {
            var tempStream = new MemoryStream();
            var testData = new TestData {Data = "Hello"};
            XmlStorage.ToZip(tempStream, testData, "Correct password", new EmbeddedFile[0]);
            tempStream.Seek(0, SeekOrigin.Begin);
            Assert.Throws<ZipException>(() => XmlStorage.FromZip<TestData>(tempStream, "Wront password", new EmbeddedFile[0]));
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.ToString{T}"/> and <see cref="XmlStorage.FromString{T}"/> work correctly.
        /// </summary>
        [Test]
        public void TestString()
        {
            var testData1 = new TestData {Data = "Hello"};

            // Serialize and deserialize
            string xml = XmlStorage.ToString(testData1);
            var testData2 = XmlStorage.FromString<TestData>(xml);

            Assert.AreEqual(testData1.Data, testData2.Data);
        }
    }
}
