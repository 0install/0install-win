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
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// Contains test methods for <see cref="IFetcher"/> implementations.
    /// </summary>
    public abstract class FetcherTest<TFetcher> : TestWithContainer<TFetcher>
        where TFetcher : class, IFetcher
    {
        protected Mock<IStore> StoreMock => GetMock<IStore>();

        [Test]
        public void DownloadSingleArchive()
        {
            StoreMock.Setup(x => x.Flush());
            using (var server = new MicroServer("archive.zip", TestData.ZipArchiveStream))
            {
                TestDownloadArchives(
                    new Archive {Href = server.FileUri, MimeType = Archive.MimeTypeZip, Size = TestData.ZipArchiveStream.Length, Extract = "extract", Destination = "destination"});
            }
        }

        [Test]
        public void DownloadLocalArchive()
        {
            StoreMock.Setup(x => x.Flush());
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                TestData.ZipArchiveStream.CopyToFile(tempFile);
                TestDownloadArchives(
                    new Archive {Href = new Uri(tempFile), MimeType = Archive.MimeTypeZip, Size = TestData.ZipArchiveStream.Length, Extract = "extract", Destination = "destination"});
            }
        }

        [Test]
        public void DownloadMultipleArchives()
        {
            StoreMock.Setup(x => x.Flush());
            using (var server1 = new MicroServer("archive.zip", TestData.ZipArchiveStream))
            using (var server2 = new MicroServer("archive.zip", TestData.ZipArchiveStream))
            {
                TestDownloadArchives(
                    new Archive {Href = server1.FileUri, MimeType = Archive.MimeTypeZip, Size = TestData.ZipArchiveStream.Length, Extract = "extract1", Destination = "destination1"},
                    new Archive {Href = server2.FileUri, MimeType = Archive.MimeTypeZip, Size = TestData.ZipArchiveStream.Length, Extract = "extract2", Destination = "destination2"});
            }
        }

        [Test]
        public void DownloadSingleFile()
        {
            StoreMock.Setup(x => x.Flush());
            using (var server = new MicroServer("regular", TestData.RegularString.ToStream()))
            {
                TestDownload(
                    dirPath => File.Exists(Path.Combine(dirPath, "regular")),
                    new SingleFile {Href = server.FileUri, Size = TestData.RegularString.Length, Destination = "regular"});
            }
        }

        [Test]
        public void DownloadRecipe()
        {
            StoreMock.Setup(x => x.Flush());
            using (var serverArchive = new MicroServer("archive.zip", TestData.ZipArchiveStream))
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
                            new Archive {Href = serverArchive.FileUri, MimeType = Archive.MimeTypeZip, Size = TestData.ZipArchiveStream.Length},
                            new RenameStep {Source = "executable", Destination = "executable2"},
                            new SingleFile {Href = serverSingleFile.FileUri, Size = TestData.RegularString.Length, Destination = "regular2"}
                        }
                    });
            }
        }

        [Test]
        public void SkipBroken()
        {
            StoreMock.Setup(x => x.Flush());
            using (var serverArchive = new MicroServer("archive.zip", TestData.ZipArchiveStream))
            using (var serverSingleFile = new MicroServer("regular", TestData.RegularString.ToStream()))
            {
                TestDownload(
                    dirPath => File.Exists(Path.Combine(dirPath, "regular")),
                    // broken: wrong size
                    new Archive {Href = serverArchive.FileUri, MimeType = Archive.MimeTypeZip, Size = 0},
                    // broken: unknown archive format
                    new Archive {Href = serverArchive.FileUri, MimeType = "test/format", Size = TestData.ZipArchiveStream.Length},
                    // works
                    new Recipe {Steps = {new SingleFile {Href = serverSingleFile.FileUri, Size = TestData.RegularString.Length, Destination = "regular"}}});
            }
        }

        #region Helpers
        protected void TestDownloadArchives(params Archive[] archives)
        {
            var digest = new ManifestDigest(sha256New: "test123");
            var archiveInfos = archives.Select(archive => new ArchiveFileInfo {SubDir = archive.Extract, Destination = archive.Destination, MimeType = archive.MimeType, StartOffset = archive.StartOffset, OriginalSource = archive.Href});
            var testImplementation = new Implementation {ID = "test", ManifestDigest = digest, RetrievalMethods = {GetRetrievalMethod(archives)}};

            StoreMock.Setup(x => x.Contains(digest)).Returns(false);
            StoreMock.Setup(x => x.AddArchives(archiveInfos.IsEqual(), digest, Handler)).Returns("");

            Target.Fetch(new[] {testImplementation});
        }

        private static RetrievalMethod GetRetrievalMethod(Archive[] archives)
        {
            if (archives.Length == 1) return archives[0];
            else
            {
                var recipe = new Recipe();
                recipe.Steps.AddRange(archives);
                return recipe;
            }
        }

        private void TestDownload(Predicate<string> directoryCheck, params RetrievalMethod[] retrievalMethod)
        {
            var digest = new ManifestDigest(sha256New: "test123");
            var testImplementation = new Implementation {ID = "test", ManifestDigest = digest};
            testImplementation.RetrievalMethods.AddRange(retrievalMethod);

            StoreMock.Setup(x => x.Contains(digest)).Returns(false);
            StoreMock.Setup(x => x.AddDirectory(It.Is<string>(path => directoryCheck(path)), digest, Handler)).Returns("");

            Target.Fetch(new[] {testImplementation});
        }
        #endregion

        [Test]
        public void RunExternalConfirm()
        {
            bool installInvoked = false;
            Handler.AnswerQuestionWith = true;
            Target.Fetch(new[]
            {
                new Implementation
                {
                    ID = ExternalImplementation.PackagePrefix + "123",
                    RetrievalMethods =
                    {
                        new ExternalRetrievalMethod
                        {
                            ConfirmationQuestion = "Install?",
                            Install = () => { installInvoked = true; }
                        }
                    }
                }
            });
            installInvoked.Should().BeTrue();
        }

        [Test]
        public void RunExternalDeny()
        {
            bool installInvoked = false;
            Handler.AnswerQuestionWith = false;
            Target.Invoking(x => x.Fetch(new[]
            {
                new Implementation
                {
                    ID = ExternalImplementation.PackagePrefix + "123",
                    RetrievalMethods =
                    {
                        new ExternalRetrievalMethod
                        {
                            ConfirmationQuestion = "Install?",
                            Install = () => { installInvoked = true; }
                        }
                    }
                }
            })).ShouldThrow<OperationCanceledException>();
            installInvoked.Should().BeFalse();
        }

        [Test]
        public void SkipExisting()
        {
            var digest = new ManifestDigest(sha256New: "test123");
            var testImplementation = new Implementation {ID = "test", ManifestDigest = digest, RetrievalMethods = {new Recipe()}};
            StoreMock.Setup(x => x.Flush());
            StoreMock.Setup(x => x.Contains(digest)).Returns(true);

            Target.Fetch(new[] {testImplementation});
        }

        [Test]
        public void NoSuitableMethod()
        {
            var implementation = new Implementation {ID = "test", ManifestDigest = new ManifestDigest(sha256New: "test123")};
            StoreMock.Setup(x => x.Flush());
            StoreMock.Setup(x => x.Contains(implementation.ManifestDigest)).Returns(false);

            Target.Invoking(x => x.Fetch(new[] {implementation})).ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void UnsupportedArchiveFormat()
        {
            var implementation = new Implementation
            {
                ID = "test",
                ManifestDigest = new ManifestDigest(sha256New: "test123"),
                RetrievalMethods = {new Archive {MimeType = "test/format"}}
            };
            StoreMock.Setup(x => x.Flush());
            StoreMock.Setup(x => x.Contains(implementation.ManifestDigest)).Returns(false);

            Target.Invoking(x => x.Fetch(new[] {implementation})).ShouldThrow<NotSupportedException>();
        }

        [Test]
        public void UnsupportedArchiveFormatInRecipe()
        {
            var implementation = new Implementation
            {
                ID = "test",
                ManifestDigest = new ManifestDigest(sha256New: "test123"),
                RetrievalMethods = {new Recipe {Steps = {new Archive {MimeType = Archive.MimeTypeZip}, new Archive {MimeType = "test/format"}}}}
            };
            StoreMock.Setup(x => x.Flush());
            StoreMock.Setup(x => x.Contains(implementation.ManifestDigest)).Returns(false);

            Target.Invoking(x => x.Fetch(new[] {implementation})).ShouldThrow<NotSupportedException>();
        }
    }
}
