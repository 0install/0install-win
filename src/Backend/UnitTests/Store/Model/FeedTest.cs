/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FluentAssertions;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Feed"/>.
    /// </summary>
    [TestFixture]
    public class FeedTest
    {
        #region Helpers
        public static readonly FeedUri Test1Uri = new FeedUri("http://0install.de/feeds/test/test1.xml");
        public static readonly FeedUri Test2Uri = new FeedUri("http://0install.de/feeds/test/test2.xml");
        public static readonly FeedUri Test3Uri = new FeedUri("http://0install.de/feeds/test/test3.xml");
        public static readonly FeedUri Sub1Uri = new FeedUri("http://0install.de/feeds/test/sub1.xml");
        public static readonly FeedUri Sub2Uri = new FeedUri("http://0install.de/feeds/test/sub2.xml");
        public static readonly FeedUri Sub3Uri = new FeedUri("http://0install.de/feeds/test/sub3.xml");

        /// <summary>
        /// Creates a fictive test <see cref="Feed"/>.
        /// </summary>
        public static Feed CreateTestFeed()
        {
            return new Feed
            {
                Uri = Test1Uri,
                Name = "MyApp",
                Categories = {"Category1", "Category2"},
                Homepage = new Uri("http://0install.de/"),
                Feeds = {new FeedReference {Source = Sub1Uri}},
                FeedFor = {new InterfaceReference {Target = new FeedUri("http://0install.de/feeds/test/super1.xml")}},
                Summaries = {"Default summary", {"de-DE", "German summary"}},
                Descriptions = {"Default description", {"de-DE", "German description"}},
                Icons = {new Icon {Href = new Uri("http://0install.de/feeds/images/test.png"), MimeType = Icon.MimeTypePng}},
                Elements = {CreateTestImplementation(), CreateTestPackageImplementation(), CreateTestGroup()},
                CapabilityLists = {CapabilityListTest.CreateTestCapabilityList()},
                EntryPoints =
                {
                    new EntryPoint
                    {
                        Command = Command.NameRun,
                        BinaryName = "myapp",
                        Names = {"Entry name", {"de-DE", "German entry name"}},
                        Summaries = {"Entry summary", {"de-DE", "German entry summary"}},
                        Icons = {new Icon {Href = new Uri("http://0install.de/feeds/images/test_command.png"), MimeType = Icon.MimeTypePng}}
                    }
                }
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="Implementation"/>.
        /// </summary>
        public static Implementation CreateTestImplementation()
        {
            return new Implementation
            {
                ID = "id1",
                ManifestDigest = new ManifestDigest(sha256: "123"),
                Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {"en-US"},
                Commands = {CommandTest.CreateTestCommand1()},
                DocDir = "doc",
                Stability = Stability.Developer,
                Dependencies =
                {
                    new Dependency
                    {
                        InterfaceUri = Test1Uri,
                        Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")}},
                        Bindings = {EnvironmentBindingTest.CreateTestBinding(), OverlayBindingTest.CreateTestBinding(), ExecutableInVarTest.CreateTestBinding(), ExecutableInPathTest.CreateTestBinding()}
                    }
                },
                Restrictions =
                {
                    new Restriction
                    {
                        InterfaceUri = Test2Uri,
                        Constraints = {new Constraint {Before = new ImplementationVersion("2.0")}}
                    }
                },
                RetrievalMethods =
                {
                    new Recipe
                    {
                        Steps =
                        {
                            new Archive {Href = new Uri("http://0install.de/files/test/test.zip"), Size = 1024},
                            new SingleFile {Href = new Uri("http://0install.de/files/test/test.dat"), Size = 1024, Destination = "test.dat"},
                            new RenameStep {Source = "a", Destination = "b"},
                            new RemoveStep {Path = "c"}
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="PackageImplementation"/>.
        /// </summary>
        public static PackageImplementation CreateTestPackageImplementation()
        {
            return new PackageImplementation
            {
                Package = "firefox",
                Distributions = {"RPM"},
                Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {"en-US"},
                Commands = {CommandTest.CreateTestCommand1()},
                DocDir = "doc",
                Stability = Stability.Developer,
                Dependencies =
                {
                    new Dependency
                    {
                        InterfaceUri = Test2Uri, Importance = Importance.Recommended,
                        Bindings = {EnvironmentBindingTest.CreateTestBinding(), OverlayBindingTest.CreateTestBinding(), ExecutableInVarTest.CreateTestBinding(), ExecutableInPathTest.CreateTestBinding()}
                    }
                }
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
                Architecture = new Architecture(OS.FreeBsd, Cpu.I586),
                License = "GPL",
                Stability = Stability.Developer,
                Elements =
                {
                    new Implementation {Commands = {new Command {Name = "run", Path = "main1"}}},
                    new Group {Elements = {new Implementation {Commands = {new Command {Name = "run", Path = "main2"}}}}}
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
            Feed feed1 = CreateTestFeed(), feed2;
            Assert.That(feed1, Is.XmlSerializable);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                feed1.SaveXml(tempFile);
                feed2 = XmlStorage.LoadXml<Feed>(tempFile);
            }

            // Ensure data stayed the same
            feed2.Should().Be(feed1, because: "Serialized objects should be equal.");
            feed2.GetHashCode().Should().Be(feed1.GetHashCode(), because: "Serialized objects' hashes should be equal.");
            feed2.Should().NotBeSameAs(feed1, because: "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned and compared.
        /// </summary>
        [Test]
        public void TestCloneEquals()
        {
            var feed1 = CreateTestFeed();
            feed1.Should().Be(feed1, because: "Equals() should be reflexive.");
            feed1.GetHashCode().Should().Be(feed1.GetHashCode(), because: "GetHashCode() should be reflexive.");

            var feed2 = feed1.Clone();
            feed2.Should().Be(feed1, because: "Cloned objects should be equal.");
            feed2.GetHashCode().Should().Be(feed1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            feed2.Should().NotBeSameAs(feed1, because: "Cloning should not return the same reference.");

            feed2.Elements.Add(new Implementation {ID = "dummy"});
            feed2.Should().NotBe(feed1, because: "Modified objects should no longer be equal");
        }

        /// <summary>
        /// Ensures that <see cref="Feed.Normalize"/> correctly collapses <see cref="Group"/> structures.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void TestNormalize()
        {
            var feed = new Feed {Elements = {CreateTestGroup()}};
            feed.Normalize(Test1Uri);

            var implementation = feed.Elements[0];
            implementation.Architecture.Should().Be(new Architecture(OS.FreeBsd, Cpu.I586));
            implementation.Languages.ToString().Should().Be("de");
            implementation.License.Should().Be("GPL");
            implementation.Stability.Should().Be(Stability.Developer);
            implementation[Command.NameRun].Path.Should().Be("main1");

            implementation = feed.Elements[1];
            implementation.Architecture.Should().Be(new Architecture(OS.FreeBsd, Cpu.I586));
            implementation.Languages.ToString().Should().Be("de");
            implementation.License.Should().Be("GPL");
            implementation.Stability.Should().Be(Stability.Developer);
            implementation[Command.NameRun].Path.Should().Be("main2");
        }

        /// <summary>
        /// Ensures that <see cref="Feed.Strip"/> correctly removes non-essential metadata.
        /// </summary>
        [Test]
        public void TestStrip()
        {
            var feed = CreateTestFeed();
            feed.Strip();

            feed.Elements.Should().BeEmpty();
            feed.CapabilityLists.Should().BeEmpty();
            feed.UnknownAttributes.Should().BeEmpty();
            feed.UnknownElements.Should().BeEmpty();
        }

        /// <summary>
        /// Ensures that <see cref="Feed.Normalize"/> correctly updates collection hash codes.
        /// </summary>
        [Test]
        public void TestNormalizeHash()
        {
            var feed = CreateTestFeed();

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                feed.SaveXml(tempFile);
                var feedReload = XmlStorage.LoadXml<Feed>(tempFile);

                feed.Normalize(new FeedUri(tempFile));
                feedReload.Normalize(new FeedUri(tempFile));
                feedReload.GetHashCode().Should().Be(feed.GetHashCode());
            }
        }

        /// <summary>
        /// Ensures that contained <see cref="Implementation"/>s are correctly returned by ID.
        /// </summary>
        [Test]
        public void TestGetImplementation()
        {
            var feed = CreateTestFeed();

            feed["id1"].Should().Be(CreateTestImplementation());
            // ReSharper disable once UnusedVariable
            feed.Invoking(x => { var _ = x["invalid"]; }).ShouldThrow<KeyNotFoundException>();
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetEntryPoint"/> correctly identifies contained <see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetEntryPoint()
        {
            var feed = CreateTestFeed();

            feed.GetEntryPoint().Should().Be(CreateTestFeed().EntryPoints[0]);
            feed.GetEntryPoint("unknown").Should().BeNull();
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetBestName"/> correctly finds best matching names for <see cref="Command"/>s/<see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetName()
        {
            var feed = CreateTestFeed();

            feed.GetBestName(CultureInfo.InvariantCulture).Should().Be("Entry name");
            feed.GetBestName(CultureInfo.InvariantCulture, "unknown").Should().Be(feed.Name + " unknown");
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetBestSummary"/> correctly finds best matching summaries for <see cref="Command"/>s/<see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetSummary()
        {
            var feed = CreateTestFeed();

            feed.GetBestSummary(CultureInfo.InvariantCulture).Should().Be("Entry summary");
            feed.GetBestSummary(CultureInfo.InvariantCulture, "unknown").Should().Be("Default summary");
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetIcon"/> correctly finds best matching <see cref="Icon"/>s for <see cref="Command"/>s/<see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetIcon()
        {
            var feed = CreateTestFeed();

            var feedIcon = new Icon {Href = new Uri("http://0install.de/feeds/images/test.png"), MimeType = Icon.MimeTypePng};
            var commandIcon = new Icon {Href = new Uri("http://0install.de/feeds/images/test_command.png"), MimeType = Icon.MimeTypePng};

            feed.GetIcon(Icon.MimeTypePng).Should().Be(commandIcon);
            feed.GetIcon(Icon.MimeTypePng, "unknown").Should().Be(feedIcon);
            feed.GetIcon("wrong", "unknown").Should().Be(null);
        }
    }
}
