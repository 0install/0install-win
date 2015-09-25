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
using FluentAssertions;
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
            _builder.ManifestDigest.Should().Be(ManifestDigest.Empty);
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

            signedFeed.Feed.Name.Should().Be(_builder.MainCandidate.Name);
            signedFeed.Feed.Uri.Should().Be(_builder.Uri);
            signedFeed.Feed.Summaries.Should().Equal(new LocalizableStringCollection {_builder.MainCandidate.Summary});
            signedFeed.Feed.NeedsTerminal.Should().Be(_builder.MainCandidate.NeedsTerminal);
            signedFeed.Feed.Elements.Should().Equal(
                new Implementation
                {
                    ID = "sha1new=" + ManifestDigest.Empty.Sha1New,
                    ManifestDigest = ManifestDigest.Empty,
                    Version = _builder.MainCandidate.Version,
                    Architecture = _builder.MainCandidate.Architecture,
                    Commands = {new Command {Name = Command.NameRun, Path = "test"}},
                    RetrievalMethods = {_builder.RetrievalMethod}
                });
            signedFeed.Feed.Icons.Should().Equal(_builder.Icons);
            signedFeed.SecretKey.Should().Be(_builder.SecretKey);
        }

        [Test]
        public void TestTemporaryDirectory()
        {
            var tempDir1 = new TemporaryDirectory("0install-unit-tests");
            var tempDir2 = new TemporaryDirectory("0install-unit-tests");

            _builder.TemporaryDirectory = tempDir1;
            Directory.Exists(tempDir1).Should().BeTrue(because: "Directory should exist");

            _builder.TemporaryDirectory = tempDir2;
            Directory.Exists(tempDir1).Should().BeFalse(because: "Directory should be auto-disposed when replaced with a new one");
            Directory.Exists(tempDir2).Should().BeTrue(because: "Directory should exist");

            _builder.Dispose();
            Directory.Exists(tempDir2).Should().BeFalse(because: "Directory should be disposed together with FeedBuilder");
        }
    }
}
