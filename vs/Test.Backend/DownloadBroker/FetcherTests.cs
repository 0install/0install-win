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
using Common;
using ZeroInstall.Store.Implementation.Archive;
using Common.Storage;
using Common.Utils;
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
        private string _oldWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            _testFolder = new TemporaryDirectory("0install-unit-tests");
            _storeDir = new TemporaryDirectory("0install-unit-tests");
            _store = new DirectoryStore(_storeDir.Path);
            _fetcher = new Fetcher(new SilentHandler(), _store);
            _oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _testFolder.Path;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = _oldWorkingDirectory;
            _storeDir.Dispose();
            _testFolder.Dispose();
        }

        private static Implementation SynthesizeImplementation(string archiveFile, int offset, ManifestDigest digest, out MicroServer server)
        {
            server = new MicroServer(File.OpenRead(archiveFile));

            var archive = new Archive
            {
                StartOffset = offset,
                MimeType = "application/zip",
                Size = new FileInfo(archiveFile).Length - offset,
                Location = server.FileUri
            };
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
            package.GeneratePackageArchive("archive.zip");

            MicroServer server;
            Implementation implementation = SynthesizeImplementation("archive.zip", 0, PackageBuilderManifestExtension.ComputePackageDigest(package), out server);
            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation});
                _fetcher.RunSync(request);
            }
            finally { server.Dispose(); }

            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldRejectRemoteArchiveOfDifferentSize()
        {
            var package = PreparePackageBuilder();
            package.GeneratePackageArchive("archive.zip");

            MicroServer server;
            Implementation implementation = SynthesizeImplementation("archive.zip", 0, PackageBuilderManifestExtension.ComputePackageDigest(package), out server);
            try
            {
                ((Archive)implementation.RetrievalMethods[0]).Size = 0;
                var request = new FetchRequest(new List<Implementation> {implementation});
                Assert.Throws<FetcherException>(() => _fetcher.RunSync(request));
            }
            finally { server.Dispose(); }
        }

        [Test]
        public void ShouldCorrectlyExtractSelfExtractingArchives()
        {
            const int archiveOffset = 0x1000;
            var package = PreparePackageBuilder();
            WritePackageToArchiveWithOffset(package, "archive.zip", archiveOffset);

            MicroServer server;
            Implementation implementation = SynthesizeImplementation("archive.zip", archiveOffset, PackageBuilderManifestExtension.ComputePackageDigest(package), out server);
            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation});
                _fetcher.RunSync(request);
            }
            finally { server.Dispose(); }

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
                File.WriteAllBytes("archive.zip", reader.ReadBytes((int)sdlArchive.Length));

                sdlArchive.Seek(0, SeekOrigin.Begin);
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

            MicroServer server;
            var implementation = SynthesizeImplementation("archive.zip", 0, PackageBuilderManifestExtension.ComputePackageDigest(builder), out server);
            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation});
                _fetcher.RunSync(request);
            }
            finally { server.Dispose(); }

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

            part1.GeneratePackageArchive("part1.zip");
            part2.GeneratePackageArchive("part2.zip");

            using (var server1 = new MicroServer(File.OpenRead("part1.zip")))
            using (var server2 = new MicroServer(File.OpenRead("part2.zip")))
            {
                var archive1 = new Archive
                {
                    MimeType = "application/zip",
                    Size = new FileInfo("part1.zip").Length,
                    Location = server1.FileUri
                };

                var archive2 = new Archive
                {
                    MimeType = "application/zip",
                    Size = new FileInfo("part2.zip").Length,
                    Location = server2.FileUri
                };

                var recipe = new Recipe
                {
                    Steps = { archive1, archive2 }
                };

                var implementation = new Implementation
                {
                    ManifestDigest = PackageBuilderManifestExtension.ComputePackageDigest(merged),
                    RetrievalMethods = { recipe }
                };
                var request = new FetchRequest(new List<Implementation> { implementation });
                Assert.DoesNotThrow(() => _fetcher.RunSync(request));
                Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
            }
        }

        //[Test]
        //public void ShouldSkipStartOffsetIfPossible()
        //{
        //    const int ArchiveOffset = 0x1000;
        //    var package = PreparePackageBuilder();
        //    WritePackageToArchiveWithOffset(package, "archive.zip", ArchiveOffset);
        //    Implementation implementation = SynthesizeImplementation("archive.zip", ArchiveOffset, PackageBuilderManifestExtension.ComputePackageDigest(package));

        //    bool suppliedRangeToDownload = false;

        //    _server.Accept += delegate(object sender, AcceptEventArgs eventArgs)
        //    {
        //        if (eventArgs.Context.Request != null)
        //        {
        //            string httpRange = eventArgs.Context.Request.Headers["Range"];
        //            suppliedRangeToDownload = httpRange == "bytes=" + ArchiveOffset.ToString() + "-";
        //        }
        //    };

        //    var request = new FetchRequest(new List<Implementation> { implementation });
        //    _fetcher.RunSync(request);
        //    Assert.IsTrue(suppliedRangeToDownload, "The Fetcher must use a range to download only part of the file");
        //}

        [Test]
        public void ShouldTryNextRetrievalMethodOnFailure()
        {
            var package = PreparePackageBuilder();
            package.GeneratePackageArchive("archive.zip");

            using (var server = new MicroServer(File.OpenRead("archive.zip")))
            {
                var archive = new Archive
                {
                    MimeType = "application/zip",
                    Size = new FileInfo("archive.zip").Length,
                    Location = server.FileUri
                };
                var fakeArchive = new Archive
                {
                    MimeType = "application/zip",
                    Size = new FileInfo("archive.zip").Length,
                    Location = new Uri(server.FileUri + "/invalid")
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
}