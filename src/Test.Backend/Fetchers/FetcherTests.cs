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
using Common.Collections;
using Common.Net;
using Common.Tasks;
using ZeroInstall.Injector;
using Common.Storage;
using Common.Utils;
using NUnit.Framework;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;

namespace ZeroInstall.Fetchers
{
    public delegate void Action<in T1, in T2>(T1 argument1, T2 argument2);

    internal class MockFetcher : Fetcher
    {
        private class MockImplementationFetch : ImplementationFetch
        {
            private readonly MockFetcher _outer;

            internal MockImplementationFetch(MockFetcher outer, Implementation implementation)
                : base(outer, implementation)
            {
                _outer = outer;
            }

            protected override void DownloadArchive(Archive archive, string destination, ITaskHandler handler)
            {
                _outer.DownloadAction(archive, destination);
            }
        }

        internal Action<Archive, string> DownloadAction;

        internal MockFetcher(IStore store)
            : base(store)
        {}

        protected override ImplementationFetch CreateFetch(Implementation implementation)
        {
            return new MockImplementationFetch(this, implementation);
        }
    }

    internal static class FetcherTesting
    {
        internal static PackageBuilder PreparePackageBuilder()
        {
            var builder = new PackageBuilder();

            builder.AddFile("file1", @"AAAA").
                AddFolder("folder1").AddFile("file2", @"dskf\nsdf\n").
                AddFolder("folder2").AddFile("file3", new byte[] {55, 55, 55});
            return builder;
        }

        internal static Archive PrepareArchiveForPackage(PackageBuilder package)
        {
            string archiveFile = FileUtils.GetTempFile("0install-unit-tests");
            package.GeneratePackageArchive(archiveFile);
            return new Archive
            {
                MimeType = "application/zip",
                Size = new FileInfo(archiveFile).Length,
                Location = new Uri(archiveFile, UriKind.Relative)
            };
        }

        internal static Recipe PrepareRecipeForPackages(IEnumerable<PackageBuilder> packages)
        {
            var result = new Recipe();

            foreach (var package in packages)
            {
                string archiveFile = FileUtils.GetTempFile("0install-unit-tests");
                package.GeneratePackageArchive(archiveFile);
                var correspondingArchive = new Archive
                {
                    MimeType = "application/zip",
                    Size = new FileInfo(archiveFile).Length,
                    Location = new Uri(archiveFile, UriKind.Relative)
                };
                result.Steps.Add(correspondingArchive);
            }

            return result;
        }

        internal static Archive HostArchiveOnMicroServer(Archive archive, out MicroServer server)
        {
            server = new MicroServer(File.OpenRead(archive.LocationString));
            var hostedArchive = (Archive)archive.Clone();
            hostedArchive.Location = server.FileUri;
            return hostedArchive;
        }

        internal static Implementation PrepareImplementation(PackageBuilder package, params RetrievalMethod[] sources)
        {
            var result = new Implementation
            {
                ManifestDigest = PackageBuilderManifestExtension.ComputePackageDigest(package)
            };
            result.RetrievalMethods.AddAll(sources);
            return result;
        }
    }

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
            _fetcher = new Fetcher(_store);
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

        [Test]
        public void ShouldDownloadIntoStore()
        {
            var package = FetcherTesting.PreparePackageBuilder();
            var localArchive = FetcherTesting.PrepareArchiveForPackage(package);
            MicroServer server;
            var hostedArchive = FetcherTesting.HostArchiveOnMicroServer(localArchive, out server);
            var implementation = FetcherTesting.PrepareImplementation(package, hostedArchive);

            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation}, new SilentHandler());
                _fetcher.Start(request);
                _fetcher.Join(request);
            }
            finally
            {
                // Clean up temp file
                server.Dispose();
                File.Delete(localArchive.Location.ToString());
            }

            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldCorrectlyExtractSelfExtractingArchives()
        {
            const int archiveOffset = 0x1000;
            var package = FetcherTesting.PreparePackageBuilder();
            WritePackageToArchiveWithOffset(package, "archive.zip", archiveOffset);

            MicroServer server;
            Implementation implementation = SynthesizeImplementation("archive.zip", archiveOffset, PackageBuilderManifestExtension.ComputePackageDigest(package), out server);
            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation}, new SilentHandler());
                _fetcher.Start(request);
                _fetcher.Join(request);
            }
            finally
            {
                server.Dispose();
            }

            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
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
                .AddFile("SKIPPED", new byte[] {});
            interferingZipData.GeneratePackageArchive(archiveStream);
        }

        [Test]
        public void ShouldHandleExecutables()
        {
            using (var testArchive = TestData.GetTestZipArchiveStream())
            {
                var reader = new BinaryReader(testArchive);
                File.WriteAllBytes("archive.zip", reader.ReadBytes((int)testArchive.Length));

                testArchive.Seek(0, SeekOrigin.Begin);
            }

            var builder = new PackageBuilder();
            using (var regular = TestData.GetTestRegularStream())
            {
                var reader = new BinaryReader(regular);
                builder.AddFile("regular", reader.ReadBytes((int)regular.Length), new DateTime(2000, 1, 1, 11, 0, 0));
            }
            using (var executable = TestData.GetTestExecutableStream())
            {
                var reader = new BinaryReader(executable);
                builder.AddExecutable("executable", reader.ReadBytes((int)executable.Length), new DateTime(2000, 1, 1, 11, 0, 0));
            }

            MicroServer server;
            var implementation = SynthesizeImplementation("archive.zip", 0, PackageBuilderManifestExtension.ComputePackageDigest(builder), out server);
            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation}, new SilentHandler());
                _fetcher.Start(request);
                _fetcher.Join(request);
            }
            finally
            {
                server.Dispose();
            }

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
                    Steps = {archive1, archive2}
                };

                var implementation = new Implementation
                {
                    ManifestDigest = PackageBuilderManifestExtension.ComputePackageDigest(merged),
                    RetrievalMethods = {recipe}
                };
                var request = new FetchRequest(new List<Implementation> {implementation}, new SilentHandler());
                _fetcher.Start(request);
                Assert.DoesNotThrow(() => _fetcher.Join(request));
                Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
            }
        }

        [Test]
        public void ShouldTryNextRetrievalMethodOnFailure()
        {
            var package = FetcherTesting.PreparePackageBuilder();
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
                    RetrievalMethods = {fakeArchive, archive}
                };

                var request = new FetchRequest(new List<Implementation> {implementation}, new SilentHandler());
                _fetcher.Start(request);
                _fetcher.Join(request);
                Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
            }
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
                RetrievalMethods = {archive}
            };
            return result;
        }
    }

    [TestFixture]
    public class ImplementationSelection
    {
        private TemporaryDirectory _testFolder;
        private TemporaryDirectory _storeDir;
        private DirectoryStore _store;
        private string _oldWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            _testFolder = new TemporaryDirectory("0install-unit-tests");
            _storeDir = new TemporaryDirectory("0install-unit-tests");
            _store = new DirectoryStore(_storeDir.Path);
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

        [Test]
        public void ShouldPreferArchiveOverRecipe()
        {
            var part1 = new PackageBuilder()
                .AddFile("FILE1", "This file was in part1");
            var part2 = new PackageBuilder()
                .AddFile("FILE2", "This file was in part2");
            var merged = new PackageBuilder()
                .AddFile("FILE1", "This file was in part1")
                .AddFile("FILE2", "This file was in part2");

            var theRecipe = FetcherTesting.PrepareRecipeForPackages(new List<PackageBuilder> {part1, part2});
            var theCompleteArchive = FetcherTesting.PrepareArchiveForPackage(merged);

            var implementation = FetcherTesting.PrepareImplementation(merged, theRecipe, theCompleteArchive);

            Uri downloaded = null;

            var fetcher = new MockFetcher(_store)
            {
                DownloadAction = (archive, target) =>
                {
                    downloaded = archive.Location;
                    File.Delete(target); // Clean up temp file
                    throw new MockException();
                }
            };
            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation}, new SilentHandler());
                fetcher.Start(request);
                fetcher.Join(request);
            }
            catch (MockException)
            {}
            finally
            {
                // Clean up temp files
                File.Delete(theCompleteArchive.Location.ToString());
                foreach (var archive in EnumerableUtils.OfType<Archive>(theRecipe.Steps))
                    File.Delete(archive.Location.ToString());
            }
            Assert.AreEqual(downloaded, theCompleteArchive.Location);
        }

        [Test]
        public void ShouldPreferZip()
        {
            // ToDo: One left-over temp file

            var implementation = new Implementation
            {
                RetrievalMethods =
                    {
                        new Archive {MimeType = "application/zip"},
                        new Archive {MimeType = "application/x-compressed"}
                    }
            };

            string selectedType = null;

            var fetcher = new MockFetcher(_store)
            {
                DownloadAction = (archive, target) =>
                {
                    selectedType = archive.MimeType;
                    File.Delete(target); // Clean up temp file
                    throw new MockException();
                }
            };
            try
            {
                var request = new FetchRequest(new List<Implementation> {implementation}, new SilentHandler());
                fetcher.Start(request);
                fetcher.Join(request);
            }
            catch (MockException)
            {}

            Assert.AreEqual("application/zip", selectedType);
        }

        private class MockException : Exception
        {}
    }
}
