﻿using System;
using System.IO;
using Common.Storage;
using Common.Utils;
using NUnit.Framework;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains test methods for <see cref="ModelUtils"/>.
    /// </summary>
    [TestFixture]
    public class ModelUtilsTest
    {
        /// <summary>
        /// Ensures <see cref="ModelUtils.IsValidUri(System.Uri)"/> correctly identify invalid feed URIs.
        /// </summary>
        [Test]
        public void TestIsValidUri()
        {
            Uri temp;

            // Test invalid URLs
            var invalidUris = new[]
            {
                @"foo://",
                @"ftp://",
                @"www://",
                @"http://.de/",
                @"http://abc§.de/",
                @"ggo;\\"
            };
            foreach (var uri in invalidUris)
                Assert.IsFalse(ModelUtils.TryParseUri(uri, out temp), "Should reject " + uri);

            // Test valid URLs
            var validUrls = new[]
            {
                @"http://0install.de/",
                @"https://0install.de/"
            };
            foreach (var uri in validUrls)
                Assert.IsTrue(ModelUtils.TryParseUri(uri, out temp), "Should accept " + uri);
        }

        /// <summary>
        /// Ensures <see cref="ModelUtils.ValidateInterfaceID"/> correctly identify invalid interface IDs.
        /// </summary>
        [Test]
        public void TestValidateInterfaceID()
        {
            // Test invalid URLs
            var invalidIDs = new[]
            {
                @"foo://",
                @"ftp://",
                @"www://",
                @"http://.de/",
                @"http://abc§.de/",
                @"ggo;\\",
                @"http://0install.de",
                @"relative"
            };
            foreach (var id in invalidIDs)
                Assert.Throws<InvalidInterfaceIDException>(() => ModelUtils.ValidateInterfaceID(id), "Should reject " + id);

            // Test valid URLs
            var validIDs = new[]
            {
                @"http://0install.de/",
                @"https://0install.de/",
                Path.GetFullPath(@"absolute")
            };
            foreach (var id in validIDs)
                Assert.DoesNotThrow(() => ModelUtils.ValidateInterfaceID(id), "Should accept " + id);
        }

        [Test]
        public void TestEscape()
        {
            Assert.AreEqual("http%3a%2f%2f0install.de%2ffeeds%2ftest%2ftest1.xml", ModelUtils.Escape("http://0install.de/feeds/test/test1.xml"));
        }

        [Test]
        public void TestUnescape()
        {
            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", ModelUtils.Unescape("http%3A%2F%2F0install.de%2Ffeeds%2Ftest%2Ftest1.xml"));
        }

        [Test]
        public void TestPrettyEscape()
        {
            Assert.AreEqual(
                // Colon is preserved on POSIX systems but not on other OSes
                MonoUtils.IsUnix ? "http:##0install.de#feeds#test#test1.xml" : "http%3a##0install.de#feeds#test#test1.xml",
                ModelUtils.PrettyEscape("http://0install.de/feeds/test/test1.xml"));
        }

        [Test]
        public void TestPrettyUnescape()
        {
            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", ModelUtils.PrettyUnescape("http:##0install.de#feeds#test#test1.xml"));
        }

        [Test(Description = "Ensures remote URIs are not changed by CanonicalID")]
        public void TestCanonicalIDRemoteUri()
        {
            const string uri = "http://0install.de/feeds/test/test1.xml";
            Assert.AreEqual(uri, ModelUtils.CanonicalID(uri));
        }

        [Test(Description = "Ensures existing local files are converted to absolute path by CanonicalID")]
        public void TestCanonicalIDLocalFile()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                // Change the working directory
                string workingDir = Environment.CurrentDirectory;
                Environment.CurrentDirectory = tempDir.Path;
                
                File.WriteAllText("test.xml", "");
                Assert.AreEqual(Path.Combine(tempDir.Path, "test.xml"), ModelUtils.CanonicalID("test.xml"));

                // Restore the original working directory
                Environment.CurrentDirectory = workingDir;
            }
        }
    }
}
