/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Globalization;
using System.IO;
using Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Feed"/>.
    /// </summary>
    [TestFixture]
    public class FeedTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Feed"/>.
        /// </summary>
        public static Feed CreateTestFeed()
        {
            return new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/test1.xml"),
                Name = "MyApp",
                Categories = {"Category"},
                Homepage = new Uri("http://0install.de/"),
                Feeds = {new FeedReference {Source = "hhttp://0install.de/feeds/test/sub1.xml"}},
                FeedFor = {new InterfaceReference {Target = new Uri("http://0install.de/feeds/test/super1.xml")}},
                Summaries = {"Default summary", {"German summary", new CultureInfo("de-DE")}},
                Descriptions = {"Default descriptions", {"German descriptions", new CultureInfo("de-DE")}},
                Icons = {new Icon(new Uri("http://0install.de/images/test/icon.png"), "image/png")},
                Elements = {CreateTestImplementation(), CreateTestPackageImplementation(), CreateTestGroup()}
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="Implementation"/>.
        /// </summary>
        private static Implementation CreateTestImplementation()
        {
            return new Implementation
            {
                ID = "id1",
                ManifestDigest = new ManifestDigest("sha256=123"),
                Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {new CultureInfo("en-US")},
                Commands = {CommandTest.CreateTestCommand1()},
                DocDir = "doc",
                Stability = Stability.Developer,
                Dependencies = { new Dependency
                {
                    Interface = "http://0install.de/feeds/test/test1.xml",
                    Constraints = {new Constraint(new ImplementationVersion("1.0"), null), new Constraint(null, new ImplementationVersion("2.0"))},
                    Bindings = {EnvironmentBindingTest.CreateTestBinding(), OverlayBindingTest.CreateTestBinding()}
                } },
                RetrievalMethods = {new Recipe {Steps = {new Archive {Location = new Uri("http://0install.de/files/test/test.zip"), Size = 1024}}}}
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="PackageImplementation"/>.
        /// </summary>
        private static PackageImplementation CreateTestPackageImplementation()
        {
            return new PackageImplementation
            {
                Package = "firefox",
                Distributions = {"RPM"},
                Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {new CultureInfo("en-US")},
                Commands = {CommandTest.CreateTestCommand1()},
                DocDir = "doc",
                Stability = Stability.Developer,
                Dependencies = {new Dependency {Interface = "http://0install.de/feeds/test/test2.xml", Bindings = {EnvironmentBindingTest.CreateTestBinding(), OverlayBindingTest.CreateTestBinding()}}}
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="Group"/>.
        /// </summary>
        private static Group CreateTestGroup()
        {
            return new Group
            {
                Languages = {"de"},
                Architecture = new Architecture(OS.Solaris, Cpu.I586),
                License = "GPL",
                Stability = Stability.Developer,
                Elements =
                {
                    new Implementation {Commands = {new Command{Name = "run", Path = "main1"}}},
                    new Group {Elements = {new Implementation {Commands = {new Command{Name = "run", Path = "main2"}}}}},
                }
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Feed feed1, feed2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                feed1 = CreateTestFeed();
                feed1.Save(tempFile.Path);
                feed2 = Feed.Load(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(feed1, feed2, "Serialized objects should be equal.");
            Assert.AreEqual(feed1.GetHashCode(), feed2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(feed1, feed2), "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var feed1 = CreateTestFeed();
            var feed2 = feed1.CloneFeed();

            // Ensure data stayed the same
            Assert.AreEqual(feed1, feed2, "Cloned objects should be equal.");
            Assert.AreEqual(feed1.GetHashCode(), feed2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(feed1, feed2), "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that <see cref="Feed.Simplify"/> correctly collapsed <see cref="Group"/> structures.
        /// </summary>
        [Test]
        public void TestSimplify()
        {
            var feed = new Feed { Elements = { CreateTestGroup() } };
            feed.Simplify();

            var implementation = feed.Elements[0];
            Assert.AreEqual(new Architecture(OS.Solaris, Cpu.I586), implementation.Architecture);
            Assert.AreEqual("de", implementation.Languages.ToString());
            Assert.AreEqual("GPL", implementation.License);
            Assert.AreEqual(Stability.Developer, implementation.Stability);
            Assert.AreEqual("main1", implementation.GetCommand(Command.NameRun).Path);

            implementation = feed.Elements[1];
            Assert.AreEqual(new Architecture(OS.Solaris, Cpu.I586), implementation.Architecture);
            Assert.AreEqual("de", implementation.Languages.ToString());
            Assert.AreEqual("GPL", implementation.License);
            Assert.AreEqual(Stability.Developer, implementation.Stability);
            Assert.AreEqual("main2", implementation.GetCommand(Command.NameRun).Path);
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetImplementation(string)"/> correctly finds contained <see cref="Implementation"/>s.
        /// </summary>
        [Test]
        public void TestGetImplementationString()
        {
            var feed = CreateTestFeed();

            Assert.AreEqual(CreateTestImplementation(), feed.GetImplementation("id1"));
            Assert.IsNull(feed.GetImplementation("invalid"));
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetImplementation(ManifestDigest)"/> correctly finds contained <see cref="Implementation"/>s.
        /// </summary>
        [Test]
        public void TestGetImplementationDigest()
        {
            var feed = CreateTestFeed();

            Assert.AreEqual(CreateTestImplementation(), feed.GetImplementation(new ManifestDigest("sha256=123")));
            Assert.IsNull(feed.GetImplementation(new ManifestDigest("sha256=456")));
        }

        /// <summary>
        /// Ensures <see cref="Feed.IsValidUri(System.Uri)"/> correctly identify invalid feed URIs.
        /// </summary>
        [Test]
        public void TestIsValidUri()
        {
            Uri temp;

            // Test invalid URLs
            var invalidUris = new[]
            {
                @"foo://",
                @"ftp://",
                @"www://",
                @"http://.de/",
                @"http://abc§.de/",
                @"ggo;\\"
            };
            foreach (var uri in invalidUris)
                Assert.IsFalse(Feed.TryParseUri(uri, out temp), "Should reject " + uri);

            // Test valid URLs
            var validUrls = new[]
            {
                @"http://0install.de/",
                @"https://0install.de/"
            };
            foreach (var uri in validUrls)
                Assert.IsTrue(Feed.TryParseUri(uri, out temp), "Should accept " + uri);
        }

        /// <summary>
        /// Ensures <see cref="Feed.ValidateInterfaceID"/> correctly identify invalid interface IDs.
        /// </summary>
        [Test]
        public void TestValidateInterfaceID()
        {
            // Test invalid URLs
            var invalidIDs = new[]
            {
                @"foo://",
                @"ftp://",
                @"www://",
                @"http://.de/",
                @"http://abc§.de/",
                @"ggo;\\",
                @"http://0install.de",
                @"relative"
            };
            foreach (var id in invalidIDs)
                Assert.Throws<InvalidInterfaceIDException>(() => Feed.ValidateInterfaceID(id), "Should reject " + id);

            // Test valid URLs
            var validIDs = new[]
            {
                @"http://0install.de/",
                @"https://0install.de/",
                Path.GetFullPath(@"absolute")
            };
            foreach (var id in validIDs)
                Assert.DoesNotThrow(() => Feed.ValidateInterfaceID(id), "Should accept " + id);
        }
    }
}
