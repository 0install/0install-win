/*
 * Copyright 2010-2014 Bastian Eicher
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
        [Test]
        public void TestBuild()
        {
            using (var implementationDir = new TemporaryDirectory("0install-unit-tests"))
            using (var builder = new FeedBuilder
            {
                Uri = new Uri("http://0install.de/feeds/test/test1.xml"),
                Icons =
                {
                    new Icon {MimeType = Icon.MimeTypePng, Href = new Uri("http://0install.de/test.png")},
                    new Icon {MimeType = Icon.MimeTypeIco, Href = new Uri("http://0install.de/test.ico")}
                },
                RetrievalMethod = new Archive(),
                SecretKey = new OpenPgpSecretKey("fingerprint", "key", "user", new DateTime(2000, 1, 1), OpenPgpAlgorithm.Rsa, 1024)
            })
            {
                builder.SetImplementationDirectory(implementationDir, new SilentTaskHandler());
                builder.MainCandidate = new WindowsExe
                {
                    RelativePath = "test",
                    Name = "TestApp",
                    Summary = "a test app",
                    Version = new ImplementationVersion("1.0"),
                    Architecture = new Architecture(OS.Windows, Cpu.All)
                };
                var signedFeed = builder.Build();

                Assert.AreEqual(expected: builder.MainCandidate.Name, actual: signedFeed.Feed.Name);
                Assert.AreEqual(expected: builder.Uri, actual: signedFeed.Feed.Uri);
                CollectionAssert.AreEqual(
                    expected: new LocalizableStringCollection {builder.MainCandidate.Summary},
                    actual: signedFeed.Feed.Summaries);
                Assert.AreEqual(expected: builder.MainCandidate.NeedsTerminal, actual: signedFeed.Feed.NeedsTerminal);
                CollectionAssert.AreEqual(
                    expected: new[]
                    {
                        new Implementation
                        {
                            ID = "sha1new=" + ManifestDigest.Empty.Sha1New,
                            ManifestDigest = ManifestDigest.Empty,
                            Version = builder.MainCandidate.Version,
                            Architecture = builder.MainCandidate.Architecture,
                            Commands = {new Command {Name = Command.NameRun, Path = "test"}},
                            RetrievalMethods = {builder.RetrievalMethod}
                        }
                    },
                    actual: signedFeed.Feed.Elements);
                CollectionAssert.AreEqual(expected: builder.Icons, actual: signedFeed.Feed.Icons);
                CollectionAssert.AreEqual(
                    expected: new[]
                    {
                        new EntryPoint
                        {
                            Command = Command.NameRun,
                            Names = {builder.MainCandidate.Name},
                            BinaryName = "test"
                        }
                    },
                    actual: signedFeed.Feed.EntryPoints);
                Assert.AreEqual(expected: builder.SecretKey, actual: signedFeed.SecretKey);
            }
        }

        [Test]
        public void TestTemporaryDirectory()
        {
            var tempDir1 = new TemporaryDirectory("0install-unit-tests");
            var tempDir2 = new TemporaryDirectory("0install-unit-tests");
            var builder = new FeedBuilder {TemporaryDirectory = tempDir1};

            Assert.IsTrue(Directory.Exists(tempDir1), "Directory should exist");
            builder.TemporaryDirectory = tempDir2;
            Assert.IsFalse(Directory.Exists(tempDir1), "Directory should be auto-disposed when replaced with a new one");

            Assert.IsTrue(Directory.Exists(tempDir2), "Directory should exist");
            builder.Dispose();
            Assert.IsFalse(Directory.Exists(tempDir2), "Directory should be disposed as a part of builder");
        }
    }
}
