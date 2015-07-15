/*
 * Copyright 2010-2015 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.Publish.EntryPoints;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Contains test methods for <see cref="FeedBuilder"/>.
    /// </summary>
    [TestFixture]
    public class FeedBuilderTest
    {
        private FeedBuilder _builder;
        private TemporaryDirectory _implementationDir;

        [SetUp]
        public void SetUp()
        {
            _builder = new FeedBuilder();
            _implementationDir = new TemporaryDirectory("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Dispose();
            _implementationDir.Dispose();
        }

        [Test]
        public void TestCalculateDigest()
        {
            _builder.ImplementationDirectory = _implementationDir;
            _builder.CalculateDigest(new SilentTaskHandler());
            Assert.AreEqual(ManifestDigest.Empty, _builder.ManifestDigest);
        }

        [Test]
        public void TestDetectCandidates()
        {
            _builder.ImplementationDirectory = _implementationDir;
            _builder.DetectCandidates(new SilentTaskHandler());
        }

        [Test]
        public void TestGenerateCommands()
        {
            _builder.MainCandidate = new WindowsExe
            {
                RelativePath = "test",
                Name = "TestApp",
                Summary = "a test app",
                Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.All)
            };
            _builder.GenerateCommands();
        }

        [Test]
        public void TestBuild()
        {
            TestCalculateDigest();
            TestDetectCandidates();
            TestGenerateCommands();

            _builder.RetrievalMethod = new Archive();
            _builder.Uri = new FeedUri("http://0install.de/feeds/test/test1.xml");
            _builder.Icons.Add(new Icon {MimeType = Icon.MimeTypePng, Href = new Uri("http://0install.de/test.png")});
            _builder.Icons.Add(new Icon {MimeType = Icon.MimeTypeIco, Href = new Uri("http://0install.de/test.ico")});
            _builder.SecretKey = new OpenPgpSecretKey(keyID: 123, fingerprint: new byte[] {1, 2, 3}, userID: "user");
            var signedFeed = _builder.Build();

            Assert.AreEqual(expected: _builder.MainCandidate.Name, actual: signedFeed.Feed.Name);
            Assert.AreEqual(expected: _builder.Uri, actual: signedFeed.Feed.Uri);
            CollectionAssert.AreEqual(
                expected: new LocalizableStringCollection {_builder.MainCandidate.Summary},
                actual: signedFeed.Feed.Summaries);
            Assert.AreEqual(expected: _builder.MainCandidate.NeedsTerminal, actual: signedFeed.Feed.NeedsTerminal);
            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new Implementation
                    {
                        ID = "sha1new=" + ManifestDigest.Empty.Sha1New,
                        ManifestDigest = ManifestDigest.Empty,
                        Version = _builder.MainCandidate.Version,
                        Architecture = _builder.MainCandidate.Architecture,
                        Commands = {new Command {Name = Command.NameRun, Path = "test"}},
                        RetrievalMethods = {_builder.RetrievalMethod}
                    }
                },
                actual: signedFeed.Feed.Elements);
            CollectionAssert.AreEqual(expected: _builder.Icons, actual: signedFeed.Feed.Icons);
            Assert.AreEqual(expected: _builder.SecretKey, actual: signedFeed.SecretKey);
        }

        [Test]
        public void TestTemporaryDirectory()
        {
            var tempDir1 = new TemporaryDirectory("0install-unit-tests");
            var tempDir2 = new TemporaryDirectory("0install-unit-tests");

            _builder.TemporaryDirectory = tempDir1;
            Assert.IsTrue(Directory.Exists(tempDir1), "Directory should exist");

            _builder.TemporaryDirectory = tempDir2;
            Assert.IsFalse(Directory.Exists(tempDir1), "Directory should be auto-disposed when replaced with a new one");
            Assert.IsTrue(Directory.Exists(tempDir2), "Directory should exist");

            _builder.Dispose();
            Assert.IsFalse(Directory.Exists(tempDir2), "Directory should be disposed together with FeedBuilder");
        }
    }
}
