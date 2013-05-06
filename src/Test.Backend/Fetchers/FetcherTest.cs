﻿/*
 * Copyright 2010 Roland Leopold Walkling
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
using System.Linq;
using Common;
using Common.Tasks;
using Common.Utils;
using Moq;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Contains test methods for <see cref="IFetcher"/> implementations.
    /// </summary>
    public abstract class FetcherTest
    {
        #region Shared
        private readonly ITaskHandler _handler = new SilentTaskHandler();
        private Mock<IStore> _storeMock;
        private IFetcher _fetcher;

        protected abstract IFetcher CreateFetcher(IStore store, ITaskHandler handler);

        [SetUp]
        public void SetUp()
        {
            _storeMock = new Mock<IStore>();
            _fetcher = CreateFetcher(_storeMock.Object, _handler);
        }

        [TearDown]
        public void TearDown()
        {
            _storeMock.Verify();
        }
        #endregion

        [Test]
        public void DownloadSingleArchive()
        {
            using (var server = new MicroServer("archive.zip", TestData.GetZipArchiveStream()))
            {
                TestDownloadArchives(
                    new Archive {Location = server.FileUri, MimeType = "application/zip", Size = TestData.ZipArchiveSize, Extract = "extract", Destination = "destination"});
            }
        }

        [Test]
        public void DownloadMultipleArchives()
        {
            using (var server = new MicroServer("archive.zip", TestData.GetZipArchiveStream()))
            {
                TestDownloadArchives(
                    new Archive {Location = server.FileUri, MimeType = "application/zip", Size = TestData.ZipArchiveSize, Extract = "extract1", Destination = "destination1"},
                    new Archive {Location = server.FileUri, MimeType = "application/zip", Size = TestData.ZipArchiveSize, Extract = "extract2", Destination = "destination2"});
            }
        }

        [Test]
        public void DownloadSingleFile()
        {
            using (var server = new MicroServer("regular", TestData.RegularString.ToStream()))
            {
                TestDownload(
                    dirPath => File.Exists(Path.Combine(dirPath, "regular")),
                    new SingleFile {Location = server.FileUri, Size = TestData.RegularString.Length, Destination = "regular"});
            }
        }

        [Test]
        public void DownloadRecipe()
        {
            using (var serverArchive = new MicroServer("archive.zip", TestData.GetZipArchiveStream()))
            using (var serverSingleFile = new MicroServer("regular", TestData.RegularString.ToStream()))
            {
                TestDownload(
                    dirPath => File.Exists(Path.Combine(dirPath, "regular")) &&
                        !File.Exists(Path.Combine(dirPath, "executable")) && File.Exists(Path.Combine(dirPath, "executable2")) &&
                        File.Exists(Path.Combine(dirPath, "regular2")),
                    new Recipe
                    {
                        Steps =
                        {
                            new Archive {Location = serverArchive.FileUri, MimeType = "application/zip", Size = TestData.ZipArchiveSize},
                            new RenameStep {Source = "executable", Destination = "executable2"},
                            new SingleFile {Location = serverSingleFile.FileUri, Size = TestData.RegularString.Length, Destination = "regular2"}
                        }
                    });
            }
        }

        [Test]
        public void SkipBroken()
        {
            using (var serverArchive = new MicroServer("archive.zip", TestData.GetZipArchiveStream()))
            using (var serverSingleFile = new MicroServer("regular", TestData.RegularString.ToStream()))
            {
                TestDownload(
                    dirPath => File.Exists(Path.Combine(dirPath, "regular")),
                    // broken: wrong size
                    new Archive {Location = serverArchive.FileUri, MimeType = "application/zip", Size = 0},
                    // broken: unknown archive format
                    new Archive {Location = serverArchive.FileUri, MimeType = "test/format", Size = TestData.ZipArchiveSize},
                    // works
                    new Recipe {Steps = {new SingleFile {Location = serverSingleFile.FileUri, Size = TestData.RegularString.Length, Destination = "regular"}}});
            }
        }

        #region Helpers
        private void TestDownloadArchives(params Archive[] archives)
        {
            var digest = new ManifestDigest(sha256New: "test123");

            var archiveInfos = archives.Select(archive => new ArchiveFileInfo {SubDir = archive.Extract, Destination = archive.Destination, MimeType = archive.MimeType, StartOffset = archive.StartOffset});
            RetrievalMethod retrievalMethod;
            if (archives.Length == 1) retrievalMethod = archives[0];
            else
            {
                var recipe = new Recipe();
                recipe.Steps.AddAll(archives);
                retrievalMethod = recipe;
            }
            var testImplementation = new Implementation {ManifestDigest = digest, RetrievalMethods = {retrievalMethod}};

            _storeMock.Setup(x => x.Contains(digest)).Returns(false).Verifiable();
            _storeMock.Setup(x => x.AddArchives(archiveInfos.IsEqual(), digest, _handler)).Verifiable();

            _fetcher.Fetch(new[] {testImplementation});
        }

        private void TestDownload(Predicate<string> directoryCheck, params RetrievalMethod[] retrievalMethod)
        {
            var digest = new ManifestDigest(sha256New: "test123");
            var testImplementation = new Implementation {ManifestDigest = digest};
            testImplementation.RetrievalMethods.AddAll(retrievalMethod);

            _storeMock.Setup(x => x.Contains(digest)).Returns(false).Verifiable();
            _storeMock.Setup(x => x.AddDirectory(It.Is<string>(path => directoryCheck(path)), digest, _handler)).Verifiable();

            _fetcher.Fetch(new[] {testImplementation});
        }
        #endregion

        [Test]
        public void SkipExisting()
        {
            var digest = new ManifestDigest(sha256New: "test123");
            var testImplementation = new Implementation {ManifestDigest = digest, RetrievalMethods = {new Recipe()}};

            _storeMock.Setup(x => x.Contains(digest)).Returns(true).Verifiable();
            _fetcher.Fetch(new[] {testImplementation});
        }

        [Test]
        public void NoSuitableMethod()
        {
            var implementation = new Implementation {ManifestDigest = new ManifestDigest(sha256New: "test123")};

            Assert.Throws<NotSupportedException>(() => _fetcher.Fetch(new[] {implementation}));
        }

        [Test]
        public void UnsupportedArchiveFormat()
        {
            var implementation = new Implementation
            {
                ManifestDigest = new ManifestDigest(sha256New: "test123"),
                RetrievalMethods = {new Archive {MimeType = "test/format"}}
            };

            Assert.Throws<NotSupportedException>(() => _fetcher.Fetch(new[] {implementation}));
        }

        [Test]
        public void UnsupportedArchiveFormatInRecipe()
        {
            var implementation = new Implementation
            {
                ManifestDigest = new ManifestDigest(sha256New: "test123"),
                RetrievalMethods = {new Recipe {Steps = {new Archive {MimeType = "application/zip"}, new Archive {MimeType = "test/format"}}}}
            };

            Assert.Throws<NotSupportedException>(() => _fetcher.Fetch(new[] {implementation}));
        }
    }
}
