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
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace NanoByte.Common.Storage
{
    /// <summary>
    /// Contains test methods for <see cref="XmlStorage"/>.
    /// </summary>
    [TestFixture]
    public class XmlStorageTest
    {
        // ReSharper disable MemberCanBePrivate.Global
        /// <summary>
        /// A data-structure used to test serialization.
        /// </summary>
        public class TestData
        {
            public string Data { get; set; }
        }

        // ReSharper restore MemberCanBePrivate.Global

        /// <summary>
        /// Ensures <see cref="XmlStorage.SaveXml{T}(T,string)"/> and <see cref="XmlStorage.LoadXml{T}(string)"/> work correctly.
        /// </summary>
        [Test]
        public void TestFile()
        {
            TestData testData1 = new TestData {Data = "Hello"}, testData2;
            using (var tempFile = new TemporaryFile("unit-tests"))
            {
                // Write and read file
                testData1.SaveXml(tempFile);
                testData2 = XmlStorage.LoadXml<TestData>(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.SaveXml{T}(T,string)"/> and <see cref="XmlStorage.LoadXml{T}(string)"/> work correctly with relative paths.
        /// </summary>
        [Test]
        public void TestFileRelative()
        {
            TestData testData1 = new TestData {Data = "Hello"}, testData2;
            using (new TemporaryWorkingDirectory("unit-tests"))
            {
                // Write and read file
                testData1.SaveXml("file.xml");
                testData2 = XmlStorage.LoadXml<TestData>("file.xml");
            }

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.SaveXmlZip{T}(T,string,string,EmbeddedFile[])"/> and <see cref="XmlStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> work correctly with no password.
        /// </summary>
        [Test]
        public void TestZipNoPassword()
        {
            // Write and read file
            var testData1 = new TestData {Data = "Hello"};
            var tempStream = new MemoryStream();
            testData1.SaveXmlZip(tempStream);
            tempStream.Seek(0, SeekOrigin.Begin);
            var testData2 = XmlStorage.LoadXmlZip<TestData>(tempStream);

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.SaveXmlZip{T}(T,string,string,EmbeddedFile[])"/> and <see cref="XmlStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> work correctly with a password.
        /// </summary>
        [Test]
        public void TestZipPassword()
        {
            // Write and read file
            var testData1 = new TestData {Data = "Hello"};
            var tempStream = new MemoryStream();
            testData1.SaveXmlZip(tempStream, "Test password");
            tempStream.Seek(0, SeekOrigin.Begin);
            var testData2 = XmlStorage.LoadXmlZip<TestData>(tempStream, password: "Test password");

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.LoadXmlZip{T}(string,string,EmbeddedFile[])"/> correctly detects incorrect passwords.
        /// </summary>
        [Test]
        public void TestIncorrectPassword()
        {
            var tempStream = new MemoryStream();
            var testData = new TestData {Data = "Hello"};
            testData.SaveXmlZip(tempStream, "Correct password");
            tempStream.Seek(0, SeekOrigin.Begin);
            Assert.Throws<ZipException>(() => XmlStorage.LoadXmlZip<TestData>(tempStream, password: "Wrong password"));
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.ToXmlString{T}"/> and <see cref="XmlStorage.FromXmlString{T}"/> work correctly.
        /// </summary>
        [Test]
        public void TestString()
        {
            var testData1 = new TestData {Data = "Hello"};

            // Serialize and deserialize
            string xml = testData1.ToXmlString();
            var testData2 = XmlStorage.FromXmlString<TestData>(xml);

            Assert.AreEqual(testData1.Data, testData2.Data);
        }
    }
}
