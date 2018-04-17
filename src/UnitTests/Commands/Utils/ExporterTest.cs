// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.IO;
using FluentAssertions;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using Xunit;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.Utils
{
    public class ExporterTest : TestWithMocks
    {
        private readonly TemporaryDirectory _destination;
        private readonly Exporter _target;

        public ExporterTest()
        {
            _destination = new TemporaryDirectory("0install-unit-test");
            var selections = Fake.Selections;
            _target = new Exporter(selections, new Architecture(), _destination);
        }

        public override void Dispose()
        {
            base.Dispose();
            _destination.Dispose();
        }

        [Fact]
        public void TestExportFeeds()
        {
            using (var feedFile1 = new TemporaryFile("0install-unit-tests"))
            using (var feedFile2 = new TemporaryFile("0install-unit-tests"))
            {
                var feedCacheMock = CreateMock<IFeedCache>();

                feedCacheMock.Setup(x => x.GetPath(Fake.SubFeed1Uri)).Returns(feedFile1);
                feedCacheMock.Setup(x => x.GetPath(Fake.SubFeed2Uri)).Returns(feedFile2);

                var signature = new ValidSignature(123, new byte[0], new DateTime(2000, 1, 1));
                feedCacheMock.Setup(x => x.GetSignatures(Fake.SubFeed1Uri)).Returns(new OpenPgpSignature[] {signature});
                feedCacheMock.Setup(x => x.GetSignatures(Fake.SubFeed2Uri)).Returns(new OpenPgpSignature[] {signature});

                var openPgpMock = CreateMock<IOpenPgp>();
                openPgpMock.Setup(x => x.ExportKey(signature)).Returns("abc");

                _target.ExportFeeds(feedCacheMock.Object, openPgpMock.Object);

                string contentDir = Path.Combine(_destination, "content");
                File.Exists(Path.Combine(contentDir, Fake.SubFeed1Uri.PrettyEscape())).Should().BeTrue();
                File.Exists(Path.Combine(contentDir, Fake.SubFeed2Uri.PrettyEscape())).Should().BeTrue();

                File.ReadAllText(Path.Combine(contentDir, "000000000000007B.gpg")).Should()
                    .Be("abc", because: "GPG keys should be exported.");
            }
        }

        [Fact]
        public void TestExportImplementations()
        {
            using (var implDir1 = new TemporaryDirectory("0install-unit-tests"))
            using (var implDir2 = new TemporaryDirectory("0install-unit-tests"))
            {
                var storeMock = CreateMock<IStore>();
                storeMock.Setup(x => x.GetPath(new ManifestDigest(null, null, "123", null))).Returns(implDir1);
                storeMock.Setup(x => x.GetPath(new ManifestDigest(null, null, "abc", null))).Returns(implDir2);

                _target.ExportImplementations(storeMock.Object, new SilentTaskHandler());
            }

            string contentDir = Path.Combine(_destination, "content");
            File.Exists(Path.Combine(contentDir, "sha256=123.tbz2")).Should()
                .BeTrue(because: "Implementation should be exported.");
            File.Exists(Path.Combine(contentDir, "sha256=abc.tbz2")).Should()
                .BeTrue(because: "Implementation should be exported.");
        }
    }
}
