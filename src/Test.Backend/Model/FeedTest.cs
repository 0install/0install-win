﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Globalization;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model.Capabilities;

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
                Categories = {"Category1", "Category2"},
                Homepage = new Uri("http://0install.de/"),
                Feeds = {new FeedReference {Source = "hhttp://0install.de/feeds/test/sub1.xml"}},
                FeedFor = {new InterfaceReference {Target = new Uri("http://0install.de/feeds/test/super1.xml")}},
                Summaries = {"Default summary", {"German summary", new CultureInfo("de-DE")}},
                Descriptions = {"Default description", {"German description", new CultureInfo("de-DE")}},
                Icons = {new Icon(new Uri("http://0install.de/feeds/images/test.png"), Icon.MimeTypePng), new Icon(new Uri("http://0install.de/feeds/images/test.ico"), Icon.MimeTypeIco)},
                Elements = {CreateTestImplementation(), CreateTestPackageImplementation(), CreateTestGroup()},
                CapabilityLists = {CapabilityListTest.CreateTestCapabilityList()},
                EntryPoints =
                {
                    new EntryPoint
                    {
                        Command = Command.NameRun,
                        BinaryName = "myapp",
                        Names = {"Entry name", {"German entry name", new CultureInfo("de-DE")}},
                        Summaries = {"Entry summary", {"German entry summary", new CultureInfo("de-DE")}},
                        Icons = {new Icon(new Uri("http://0install.de/feeds/images/test_command.png"), Icon.MimeTypePng), new Icon(new Uri("http://0install.de/feeds/images/test_command.ico"), Icon.MimeTypeIco)}
                    }
                }
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
                ManifestDigest = new ManifestDigest(sha256: "123"),
                Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {new CultureInfo("en-US")},
                Commands = {CommandTest.CreateTestCommand1()},
                DocDir = "doc",
                Stability = Stability.Developer,
                Dependencies =
                {
                    new Dependency
                    {
                        Interface = "http://0install.de/feeds/test/test1.xml",
                        Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")}},
                        Bindings = {EnvironmentBindingTest.CreateTestBinding(), OverlayBindingTest.CreateTestBinding(), ExecutableInVarTest.CreateTestBinding(), ExecutableInPathTest.CreateTestBinding()}
                    }
                },
                Restrictions =
                {
                    new Restriction
                    {
                        Interface = "http://0install.de/feeds/test/test2.xml",
                        Constraints = {new Constraint {Before = new ImplementationVersion("2.0")}}
                    }
                },
                RetrievalMethods =
                {
                    new Recipe
                    {
                        Steps =
                        {
                            new Archive {Location = new Uri("http://0install.de/files/test/test.zip"), Size = 1024},
                            new RenameStep {Source = "a", Destination = "b"},
                            new RemoveStep {Path = "c"},
                            new AddDirectoryStep {Path = "x/y"}
                        }
                    }
                }
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
                Dependencies =
                {
                    new Dependency
                    {
                        Interface = "http://0install.de/feeds/test/test2.xml", Importance = Importance.Recommended,
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
                Architecture = new Architecture(OS.FreeBSD, Cpu.I586),
                License = "GPL",
                Stability = Stability.Developer,
                Elements =
                {
                    new Implementation {Commands = {new Command {Name = "run", Path = "main1"}}},
                    new Group {Elements = {new Implementation {Commands = {new Command {Name = "run", Path = "main2"}}}}},
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
            Assert.AreEqual(feed1, feed2, "Serialized objects should be equal.");
            Assert.AreEqual(feed1.GetHashCode(), feed2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(feed1, feed2), "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned and compared.
        /// </summary>
        [Test]
        public void TestCloneEquals()
        {
            var feed1 = CreateTestFeed();
            Assert.AreEqual(feed1, feed1, "Equals() should be reflexive.");
            Assert.AreEqual(feed1.GetHashCode(), feed1.GetHashCode(), "GetHashCode() should be reflexive.");

            var feed2 = feed1.Clone();
            Assert.AreEqual(feed1, feed2, "Cloned objects should be equal.");
            Assert.AreEqual(feed1.GetHashCode(), feed2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(feed1, feed2), "Cloning should not return the same reference.");

            feed2.Elements.Add(new Implementation {ID = "dummy"});
            Assert.AreNotEqual(feed1, feed2, "Modified objects should no longer be equal");
            //Assert.AreNotEqual(feed1.GetHashCode(), feed2.GetHashCode(), "Modified objects' hashes should no longer be equal");
        }

        /// <summary>
        /// Ensures that <see cref="Feed.Normalize"/> correctly collapses <see cref="Group"/> structures.
        /// </summary>
        [Test]
        public void TestNormalize()
        {
            var feed = new Feed {Elements = {CreateTestGroup()}};
            feed.Normalize("http://0install.de/feeds/test/test1.xml");

            var implementation = feed.Elements[0];
            Assert.AreEqual(new Architecture(OS.FreeBSD, Cpu.I586), implementation.Architecture);
            Assert.AreEqual("de", implementation.Languages.ToString());
            Assert.AreEqual("GPL", implementation.License);
            Assert.AreEqual(Stability.Developer, implementation.Stability);
            Assert.AreEqual("main1", implementation[Command.NameRun].Path);

            implementation = feed.Elements[1];
            Assert.AreEqual(new Architecture(OS.FreeBSD, Cpu.I586), implementation.Architecture);
            Assert.AreEqual("de", implementation.Languages.ToString());
            Assert.AreEqual("GPL", implementation.License);
            Assert.AreEqual(Stability.Developer, implementation.Stability);
            Assert.AreEqual("main2", implementation[Command.NameRun].Path);
        }

        /// <summary>
        /// Ensures that <see cref="Feed.Strip"/> correctly removes non-essential metadata.
        /// </summary>
        [Test]
        public void TestStrip()
        {
            var feed = CreateTestFeed();
            feed.Strip();

            CollectionAssert.IsEmpty(feed.Elements);
            CollectionAssert.IsEmpty(feed.CapabilityLists);
            CollectionAssert.IsEmpty(feed.UnknownAttributes);
            CollectionAssert.IsEmpty(feed.UnknownElements);
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

                feed.Normalize(tempFile);
                feedReload.Normalize(tempFile);
                Assert.AreEqual(feed.GetHashCode(), feedReload.GetHashCode());
            }
        }

        /// <summary>
        /// Ensures that contained <see cref="Implementation"/>s are correctly returned by ID.
        /// </summary>
        [Test]
        public void TestGetImplementationString()
        {
            var feed = CreateTestFeed();

            Assert.AreEqual(CreateTestImplementation(), feed["id1"]);
            // ReSharper disable UnusedVariable
            Assert.Throws<KeyNotFoundException>(() => { var dummy = feed["invalid"]; });
            // ReSharper restore UnusedVariable
        }

        /// <summary>
        /// Ensures that contained <see cref="Implementation"/>s are correctly returned by <see cref="ManifestDigest"/>.
        /// </summary>
        [Test]
        public void TestGetImplementationDigest()
        {
            var feed = CreateTestFeed();

            Assert.AreEqual(CreateTestImplementation(), feed[new ManifestDigest(sha256: "123")]);
            Assert.Throws<KeyNotFoundException>(() => { var dummy = feed[new ManifestDigest(sha256: "456")]; });
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetEntryPoint"/> correctly identifies contained <see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetEntryPoint()
        {
            var feed = CreateTestFeed();

            Assert.AreEqual(CreateTestFeed().EntryPoints[0], feed.GetEntryPoint(Command.NameRun));
            Assert.IsNull(feed.GetEntryPoint("unknown"));
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetBestName"/> correctly finds best matching names for <see cref="Command"/>s/<see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetName()
        {
            var feed = CreateTestFeed();

            Assert.AreEqual("Entry name", feed.GetBestName(CultureInfo.InvariantCulture, Command.NameRun));
            Assert.AreEqual(feed.Name, feed.GetBestName(CultureInfo.InvariantCulture, "unknown"));
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetBestSummary"/> correctly finds best matching summaries for <see cref="Command"/>s/<see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetSummary()
        {
            var feed = CreateTestFeed();

            Assert.AreEqual("Entry summary", feed.GetBestSummary(CultureInfo.InvariantCulture, Command.NameRun));
            Assert.AreEqual("Default summary", feed.GetBestSummary(CultureInfo.InvariantCulture, "unknown"));
        }

        /// <summary>
        /// Ensures that <see cref="Feed.GetIcon"/> correctly finds best matching <see cref="Icon"/>s for <see cref="Command"/>s/<see cref="EntryPoint"/>s.
        /// </summary>
        [Test]
        public void TestGetIcon()
        {
            var feed = CreateTestFeed();

            var feedIcon = new Icon(new Uri("http://0install.de/feeds/images/test.ico"), Icon.MimeTypeIco);
            var commandIcon = new Icon(new Uri("http://0install.de/feeds/images/test_command.ico"), Icon.MimeTypeIco);

            Assert.AreEqual(commandIcon, feed.GetIcon(Icon.MimeTypeIco, Command.NameRun));
            Assert.AreEqual(feedIcon, feed.GetIcon(Icon.MimeTypeIco, "unknown"));
            Assert.AreEqual(null, feed.GetIcon("wrong", "unknown"));
        }
    }
}
