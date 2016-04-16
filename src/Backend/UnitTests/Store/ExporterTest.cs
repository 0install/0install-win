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
using FluentAssertions;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store
{
    [TestFixture]
    public class ExporterTest : TestWithContainer<Exporter>
    {
        [Test]
        public void TestExportFeeds()
        {
            var selections = SelectionsTest.CreateTestSelections();

            using (var outputDir = new TemporaryDirectory("0install-unit-test"))
            using (var feedFile1 = new TemporaryFile("0install-unit-tests"))
            using (var feedFile2 = new TemporaryFile("0install-unit-tests"))
            {
                var feedCacheMock = GetMock<IFeedCache>();

                feedCacheMock.Setup(x => x.GetPath(FeedTest.Sub1Uri)).Returns(feedFile1);
                feedCacheMock.Setup(x => x.GetPath(FeedTest.Sub2Uri)).Returns(feedFile2);

                var signature = new ValidSignature(123, new byte[0], new DateTime(2000, 1, 1));
                feedCacheMock.Setup(x => x.GetSignatures(FeedTest.Sub1Uri)).Returns(new OpenPgpSignature[] { signature });
                feedCacheMock.Setup(x => x.GetSignatures(FeedTest.Sub2Uri)).Returns(new OpenPgpSignature[] { signature });
                GetMock<IOpenPgp>().Setup(x => x.ExportKey(signature)).Returns("abc");

                Target.ExportFeeds(selections, outputDir);

                FileAssert.AreEqual(
                    expected: new FileInfo(feedFile1),
                    actual: new FileInfo(Path.Combine(outputDir, FeedTest.Sub1Uri.PrettyEscape() + ".xml")),
                    message: "Feed should be exported.");
                FileAssert.AreEqual(
                    expected: new FileInfo(feedFile2),
                    actual: new FileInfo(Path.Combine(outputDir, FeedTest.Sub2Uri.PrettyEscape() + ".xml")),
                    message: "Feed should be exported.");

                File.ReadAllText(Path.Combine(outputDir, "000000000000007B.gpg")).Should()
                    .Be("abc", because: "GPG keys should be exported.");
            }
        }

        [Test]
        public void TestExportImplementations()
        {
            var selections = SelectionsTest.CreateTestSelections();

            using (var outputDir = new TemporaryDirectory("0install-unit-test"))
            using (var implDir1 = new TemporaryDirectory("0install-unit-tests"))
            using (var implDir2 = new TemporaryDirectory("0install-unit-tests"))
            {
                var storeMock = GetMock<IStore>();
                storeMock.Setup(x => x.GetPath(new ManifestDigest(null, null, "123", null))).Returns(implDir1);
                storeMock.Setup(x => x.GetPath(new ManifestDigest(null, null, "abc", null))).Returns(implDir2);

                Target.ExportImplementations(selections, outputDir, new SilentTaskHandler());

                File.Exists(Path.Combine(outputDir, "sha256=123.tbz2")).Should()
                    .BeTrue(because: "Implementation should be exported.");
                File.Exists(Path.Combine(outputDir, "sha256=abc.tbz2")).Should()
                    .BeTrue(because: "Implementation should be exported.");
            }
        }
    }
}