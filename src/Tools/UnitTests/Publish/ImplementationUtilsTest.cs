/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Security.Cryptography;
using FluentAssertions;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NanoByte.Common.Undo;
using NUnit.Framework;
using ZeroInstall.FileSystem;
using ZeroInstall.Services;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Implementations.Manifests;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Contains test methods for <see cref="ImplementationUtils"/>.
    /// </summary>
    [TestFixture]
    public class ImplementationUtilsTest
    {
        private const string ArchiveSha256Digest = "AQCBCMCZVBAQO4SKOSVHF4AWO3QOSYORL5YJC2MTP5PXJMSF5C2A";

        private const string SingleFileData = "data";
        private const string SingleFileName = "file.dat";

        private static readonly string _singleFileSha256Digest = new Manifest(ManifestFormat.Sha256New,
            new ManifestNormalFile(SingleFileData.Hash(SHA256.Create()), FileUtils.FromUnixTime(0), SingleFileData.Length, SingleFileName)).CalculateDigest();

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void BuildArchive()
        {
            using (var stream = typeof(ArchiveExtractorTest).GetEmbeddedStream("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", stream))
            {
                var implementation = ImplementationUtils.Build(new Archive {Href = microServer.FileUri}, new SilentTaskHandler());
                implementation.ManifestDigest.Sha256New.Should().Be(ArchiveSha256Digest);

                var archive = (Archive)implementation.RetrievalMethods[0];
                archive.MimeType.Should().Be(Archive.MimeTypeZip);
                archive.Size.Should().Be(stream.Length);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="SingleFile"/>s.
        /// </summary>
        [Test]
        public void BuildSingleFile()
        {
            using (var originalStream = SingleFileData.ToStream())
            using (var microServer = new MicroServer(SingleFileName, originalStream))
            {
                var implementation = ImplementationUtils.Build(new SingleFile {Href = microServer.FileUri, Destination = SingleFileName}, new SilentTaskHandler());
                ("sha256new_" + implementation.ManifestDigest.Sha256New).Should().Be(_singleFileSha256Digest);

                var file = (SingleFile)implementation.RetrievalMethods[0];
                file.Size.Should().Be(originalStream.Length);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void BuildRecipe()
        {
            using (var stream = typeof(ArchiveExtractorTest).GetEmbeddedStream("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", stream))
            {
                var implementation = ImplementationUtils.Build(new Recipe {Steps = {new Archive {Href = microServer.FileUri}}}, new SilentTaskHandler());
                implementation.ManifestDigest.Sha256New.Should().Be(ArchiveSha256Digest);

                var archive = (Archive)((Recipe)implementation.RetrievalMethods[0]).Steps[0];
                archive.MimeType.Should().Be(Archive.MimeTypeZip);
                archive.Size.Should().Be(stream.Length);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void AddMissingArchive()
        {
            using (var stream = typeof(ArchiveExtractorTest).GetEmbeddedStream("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", stream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Archive {Href = microServer.FileUri}}};
                implementation.AddMissing(new SilentTaskHandler());
                implementation.ManifestDigest.Sha256New.Should().Be(ArchiveSha256Digest);

                var archive = (Archive)implementation.RetrievalMethods[0];
                archive.MimeType.Should().Be(Archive.MimeTypeZip);
                archive.Size.Should().Be(stream.Length);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="SingleFile"/>s.
        /// </summary>
        [Test]
        public void AddMissingSingleFile()
        {
            using (var originalStream = SingleFileData.ToStream())
            using (var microServer = new MicroServer(SingleFileName, originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new SingleFile {Href = microServer.FileUri}}};
                implementation.AddMissing(new SilentTaskHandler());
                ("sha256new_" + implementation.ManifestDigest.Sha256New).Should().Be(_singleFileSha256Digest);

                var file = (SingleFile)implementation.RetrievalMethods[0];
                file.Size.Should().Be(originalStream.Length);
                file.Destination.Should().Be(SingleFileName);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void AddMissingRecipe()
        {
            using (var stream = typeof(ArchiveExtractorTest).GetEmbeddedStream("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", stream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Recipe {Steps = {new Archive {Href = microServer.FileUri}}}}};
                implementation.AddMissing(new SilentTaskHandler());
                implementation.ManifestDigest.Sha256New.Should().Be(ArchiveSha256Digest);

                var archive = (Archive)((Recipe)implementation.RetrievalMethods[0]).Steps[0];
                archive.MimeType.Should().Be(Archive.MimeTypeZip);
                archive.Size.Should().Be(stream.Length);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> generates missing <see cref="Archive"/>s from <see cref="ImplementationBase.LocalPath"/>s.
        /// </summary>
        [Test]
        public void GenerateMissingArchive()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                string feedPath = Path.Combine(tempDir, "feed.xml");
                Directory.CreateDirectory(Path.Combine(tempDir, "impl"));
                FileUtils.Touch(Path.Combine(tempDir, "impl", "file"));

                var archive = new Archive {Href = new Uri("archive.zip", UriKind.Relative)};
                var implementation = new Implementation {LocalPath = "impl", RetrievalMethods = {archive}};

                implementation.AddMissing(new SilentTaskHandler(), new SimpleCommandExecutor {Path = feedPath});

                implementation.LocalPath.Should().BeNull();
                implementation.ManifestDigest.Should().NotBe(default(ManifestDigest));
                archive.Size.Should().NotBe(0);

                File.Exists(Path.Combine(tempDir, "archive.zip")).Should().BeTrue();
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> throws <see cref="DigestMismatchException"/>s when appropriate.
        /// </summary>
        [Test]
        public void AddMissingExceptions()
        {
            using (var stream = typeof(ArchiveExtractorTest).GetEmbeddedStream("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", stream))
            {
                var implementation = new Implementation {ManifestDigest = new ManifestDigest(sha1New: "invalid"), RetrievalMethods = {new Archive {Href = microServer.FileUri}}};
                implementation.Invoking(x => x.AddMissing(new SilentTaskHandler())).ShouldThrow<DigestMismatchException>();
            }
        }

        [Test]
        public void GenerateDigest()
        {
            using (var testDir = new TemporaryDirectory("0install-unit-tests"))
            {
                new TestRoot
                {
                    new TestDirectory("subdir") {new TestFile("file")}
                }.Build(testDir);

                ManifestDigest digest1 = ImplementationUtils.GenerateDigest(testDir, new MockTaskHandler());
                digest1.Sha1.Should().BeNullOrEmpty(because: "sha1 is deprecated");
                digest1.Sha1New.Should().NotBeNullOrEmpty();
                digest1.Sha256.Should().NotBeNullOrEmpty();
                digest1.Sha256New.Should().NotBeNullOrEmpty();

                ManifestDigest digest2 = ImplementationUtils.GenerateDigest(testDir, new MockTaskHandler());
                digest2.Should().Be(digest1);
            }
        }
    }
}
