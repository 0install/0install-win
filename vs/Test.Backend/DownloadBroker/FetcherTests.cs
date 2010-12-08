/*
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
using System.Collections.Generic;
using System.IO;
using ZeroInstall.Store.Implementation.Archive;
using Common.Storage;
using Common.Utils;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using ZeroInstall.Injector;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;

namespace ZeroInstall.DownloadBroker
{
    [TestFixture]
    public class DownloadFunctionality
    {
        private TemporaryDirectory _testFolder;
        private TemporaryDirectory _storeDir;
        private DirectoryStore _store;
        private Fetcher _fetcher;
        private string _archiveFile;
        private ArchiveProvider _server;
        private const string ServerPrefix = "http://localhost:50222/archives/";
        string _oldWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            _testFolder = new TemporaryDirectory("0install-unit-tests");
            _storeDir = new TemporaryDirectory("0install-unit-tests");
            _store = new DirectoryStore(_storeDir.Path);
            _fetcher = new Fetcher(new SilentHandler(), _store);
            _archiveFile = Path.Combine(_testFolder.Path, "archive.zip");
            _server = new ArchiveProvider(_archiveFile);
            _server.Start();
            _oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _testFolder.Path;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = _oldWorkingDirectory;
            _server.Dispose();
            _storeDir.Dispose();
            _testFolder.Dispose();
        }

        private static Archive SynthesizeArchive(string zippedPackage, int offset)
        {
            var result = new Archive
            {
                StartOffset = offset,
                MimeType = "application/zip",
                Size = new FileInfo(zippedPackage).Length - offset,
                Location = new Uri(ServerPrefix + "test.zip")
            };
            return result;
        }

        private static Implementation SynthesizeImplementation(string archiveFile, int offset, ManifestDigest digest)
        {
            var archive = SynthesizeArchive(archiveFile, offset);
            var result = new Implementation
            {
                ManifestDigest = digest,
                RetrievalMethods = { archive }
            };
            return result;
        }

        [Test]
        public void ShouldDownloadIntoStore()
        {
            var package = PreparePackageBuilder();
            package.GeneratePackageArchive(_archiveFile);
            Implementation implementation = SynthesizeImplementation(_archiveFile, 0, PackageBuilderManifestExtension.ComputePackageDigest(package));
            var request = new FetchRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldRejectRemoteArchiveOfDifferentSize()
        {
            var package = PreparePackageBuilder();
            package.GeneratePackageArchive(_archiveFile);
            Implementation implementation = SynthesizeImplementation(_archiveFile, 0, PackageBuilderManifestExtension.ComputePackageDigest(package));
            using (var archiveStream = File.Create(_archiveFile))
            {
                package.AddFile("excess", new byte[] { });
                package.GeneratePackageArchive(archiveStream);
            }
            var request = new FetchRequest(new List<Implementation> { implementation });
            Assert.Throws<FetcherException>(() => _fetcher.RunSync(request));
        }

        [Test]
        public void ShouldCorrectlyExtractSelfExtractingArchives()
        {
            const int ArchiveOffset = 0x1000;
            var package = PreparePackageBuilder();
            WritePackageToArchiveWithOffset(package, _archiveFile, ArchiveOffset);
            Implementation implementation = SynthesizeImplementation(_archiveFile, ArchiveOffset, PackageBuilderManifestExtension.ComputePackageDigest(package));
            var request = new FetchRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        private static PackageBuilder PreparePackageBuilder()
        {
            var builder = new PackageBuilder();

            builder.AddFile("file1", @"AAAA").
                AddFolder("folder1").AddFile("file2", @"dskf\nsdf\n").
                AddFolder("folder2").AddFile("file3", new byte[] { 55, 55, 55 });
            return builder;
        }

        private static void WritePackageToArchiveWithOffset(PackageBuilder package, string path, int offset)
        {
            using (var archiveStream = File.Create(path))
            {
                WriteInterferingData(archiveStream);
                archiveStream.Seek(offset, SeekOrigin.Begin);
                package.GeneratePackageArchive(archiveStream);
            }
        }

        private static void WriteInterferingData(FileStream archiveStream)
        {
            PackageBuilder interferingZipData = new PackageBuilder()
                .AddFile("SKIPPED", new byte[] { });
            interferingZipData.GeneratePackageArchive(archiveStream);
        }

        [Test]
        public void ShouldHandleExecutables()
        {
            using (var sdlArchive = TestData.GetSdlZipArchiveStream())
            {
                var reader = new BinaryReader(sdlArchive);
                File.WriteAllBytes(_archiveFile, reader.ReadBytes((int)sdlArchive.Length));

                sdlArchive.Seek(0, SeekOrigin.Begin);
                var zip = new ZipFile(sdlArchive);
            }

            var builder = new PackageBuilder();
            using (var readme = TestData.GetSdlReadmeStream())
            {
                var reader = new BinaryReader(readme);
                builder.AddFile("README-SDL.txt", reader.ReadBytes((int)readme.Length), new DateTime(2007, 7, 20, 7, 25, 30));
            }
            using (var sdlDll = TestData.GetSdlDllStream())
            {
                var reader = new BinaryReader(sdlDll);
                builder.AddExecutable("SDL.dll", reader.ReadBytes((int)sdlDll.Length), new DateTime(2009, 10, 17, 19, 17, 0));
            }

            var implementation = SynthesizeImplementation(_archiveFile, 0, PackageBuilderManifestExtension.ComputePackageDigest(builder));
            var request = new FetchRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldAddRecipe()
        {
            var part1 = new PackageBuilder()
                .AddFile("FILE1", "This file was in part1");
            var part2 = new PackageBuilder()
                .AddFile("FILE2", "This file was in part2");
            var merged = new PackageBuilder()
                .AddFile("FILE1", "This file was in part1")
                .AddFile("FILE2", "This file was in part2");

            string archiveFile1 = Path.GetFullPath("part1.zip");
            part1.GeneratePackageArchive(archiveFile1);
            string archiveFile2 = Path.GetFullPath("part2.zip");
            part2.GeneratePackageArchive(archiveFile2);

            _server.Add("part1.zip", archiveFile1);
            _server.Add("part2.zip", archiveFile2);

            var archive1 = new Archive()
            {
                MimeType = "application/zip",
                Size = new FileInfo(archiveFile1).Length,
                Location = new Uri(ServerPrefix + "part1.zip")
            };

            var archive2 = new Archive()
            {
                MimeType = "application/zip",
                Size = new FileInfo(archiveFile1).Length,
                Location = new Uri(ServerPrefix + "part2.zip")
            };

            var recipe = new Recipe()
            {
                Steps = { archive1, archive2 }
            };

            var implementation = new Implementation()
            {
                ManifestDigest = PackageBuilderManifestExtension.ComputePackageDigest(merged),
                RetrievalMethods = { recipe }
            };
            var request = new FetchRequest(new List<Implementation> { implementation });
            Assert.DoesNotThrow(() => _fetcher.RunSync(request));
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldSkipStartOffsetIfPossible()
        {
            const int ArchiveOffset = 0x1000;
            var package = PreparePackageBuilder();
            WritePackageToArchiveWithOffset(package, _archiveFile, ArchiveOffset);
            Implementation implementation = SynthesizeImplementation(_archiveFile, ArchiveOffset, PackageBuilderManifestExtension.ComputePackageDigest(package));

            bool suppliedRangeToDownload = false;

            _server.Accept += delegate(object sender, AcceptEventArgs eventArgs)
            {
                if (eventArgs.Context.Request != null)
                {
                    string httpRange = eventArgs.Context.Request.Headers["Range"];
                    suppliedRangeToDownload = httpRange == "bytes=" + ArchiveOffset.ToString() + "-";
                }
            };

            var request = new FetchRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.IsTrue(suppliedRangeToDownload, "The Fetcher must use a range to download only part of the file");
        }

        [Test]
        public void ShouldTryNextRetrievalMethodOnFailure()
        {
            var package = PreparePackageBuilder();
            package.GeneratePackageArchive(_archiveFile);

            var archive = new Archive
            {
                MimeType = "application/zip",
                Size = new FileInfo(_archiveFile).Length,
                Location = new Uri(ServerPrefix + "test.zip")
            };
            var fakeArchive = new Archive
            {
                MimeType = "application/zip",
                Size = new FileInfo(_archiveFile).Length,
                Location = new Uri(ServerPrefix + "nothingHere.zip")
            };
            var implementation = new Implementation
            {
                ID = "testPackage",
                ManifestDigest = PackageBuilderManifestExtension.ComputePackageDigest(package),
                RetrievalMethods = { fakeArchive, archive }
            };

            var request = new FetchRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }
    }
}