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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NanoByte.Common.Native;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Implementation"/>.
    /// </summary>
    [TestFixture]
    public class ImplementationTest
    {
        /// <summary>
        /// Creates a fictive test <see cref="Implementation"/>.
        /// </summary>
        public static Implementation CreateTestImplementation() => new Implementation
        {
            ID = "id", ManifestDigest = new ManifestDigest(sha256: "123"), Version = new ImplementationVersion("1.0"),
            Architecture = new Architecture(OS.Windows, Cpu.I586), Languages = {"en-US"},
            Main = "executable", DocDir = "doc", Stability = Stability.Developer,
            Bindings = {EnvironmentBindingTest.CreateTestBinding()},
            RetrievalMethods = {ArchiveTest.CreateTestArchive(), new Recipe {Steps = {ArchiveTest.CreateTestArchive()}}},
            Commands = {CommandTest.CreateTestCommand1()}
        };

        /// <summary>
        /// Ensures that <see cref="Element.ContainsCommand"/> correctly checks for commands.
        /// </summary>
        [Test]
        public void TestContainsCommand()
        {
            var implementation = CreateTestImplementation();
            implementation.ContainsCommand(Command.NameRun).Should().BeTrue();
            implementation.ContainsCommand("other-command").Should().BeFalse();
        }

        /// <summary>
        /// Ensures that <see cref="Element.GetCommand"/> and <see cref="Element.this"/> correctly retrieve commands.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void TestGetCommand()
        {
            var implementation = CreateTestImplementation();

            implementation.GetCommand(Command.NameRun).Should().Be(implementation.Commands[0]);
            implementation[Command.NameRun].Should().Be(implementation.Commands[0]);

            implementation.Invoking(x => x.GetCommand("")).ShouldThrow<ArgumentNullException>();
            implementation[""].Should().BeNull();

            implementation.GetCommand("invalid").Should().BeNull();
            implementation.Invoking(x => { var _ = x["invalid"]; }).ShouldThrow<KeyNotFoundException>();
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Normalize"/> correctly identifies manifest digests in the ID tag.
        /// </summary>
        [Test]
        public void TestNormalizeID()
        {
            var implementation = new Implementation {ID = "sha256=123"};
            implementation.Normalize(FeedTest.Test1Uri);
            implementation.ManifestDigest.Sha256.Should().Be("123");

            implementation = new Implementation {ID = "sha256=wrong", ManifestDigest = new ManifestDigest(sha256: "correct")};
            implementation.Normalize(FeedTest.Test1Uri);
            implementation.ManifestDigest.Sha256.Should().Be("correct");

            implementation = new Implementation {ID = "abc"};
            implementation.Normalize(FeedTest.Test1Uri);
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Normalize"/> correctly converts <see cref="Element.Main"/> and <see cref="Element.SelfTest"/> to <see cref="Command"/>s.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void TestNormalizeCommand()
        {
            var implementation = new Implementation {Main = "main", SelfTest = "test"};
            implementation.Normalize(FeedTest.Test1Uri);
            implementation[Command.NameRun].Path.Should().Be("main");
            implementation[Command.NameTest].Path.Should().Be("test");
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Normalize"/> correctly makes <see cref="ImplementationBase.LocalPath"/> absolute.
        /// </summary>
        [Test]
        public void TestNormalizeLocalPath()
        {
            var localUri = new FeedUri(WindowsUtils.IsWindows ? @"C:\local\feed.xml" : "/local/feed.xml");

            var implementation1 = new Implementation {ID = "./subdir"};
            implementation1.Normalize(localUri);
            implementation1.ID
                .Should().Be(WindowsUtils.IsWindows ? @"C:\local\.\subdir" : "/local/./subdir");
            implementation1.LocalPath
                .Should().Be(WindowsUtils.IsWindows ? @"C:\local\.\subdir" : "/local/./subdir");

            var implementation2 = new Implementation {ID = "./wrong", LocalPath = "subdir"};
            implementation2.Normalize(localUri);
            implementation2.ID.Should().Be("./wrong");
            implementation2.LocalPath
                .Should().Be(WindowsUtils.IsWindows ? @"C:\local\subdir" : "/local/subdir");
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Normalize"/> rejects local paths in non-local feeds.
        /// </summary>
        [Test]
        public void TestNormalizeRejectLocalPath()
        {
            var implmementation = new Implementation {LocalPath = "subdir"};
            implmementation.Normalize(FeedTest.Test1Uri);
            implmementation.LocalPath.Should().BeNull();
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Normalize"/> rejects relative <see cref="DownloadRetrievalMethod.Href"/>s in non-local feeds.
        /// </summary>
        [Test]
        public void TestNormalizeRejectRelativeHref()
        {
            var relative = new Archive {Href = new Uri("relative", UriKind.Relative)};
            var absolute = new Archive {Href = new Uri("http://server/absolute.zip", UriKind.Absolute)};
            var implmementation = new Implementation {RetrievalMethods = {relative, absolute}};

            implmementation.Normalize(FeedTest.Test1Uri);
            implmementation.RetrievalMethods.Should().Equal(absolute);
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var implementation1 = CreateTestImplementation();
            var implementation2 = implementation1.CloneImplementation();

            // Ensure data stayed the same
            implementation2.Should().Be(implementation1, because: "Cloned objects should be equal.");
            implementation2.GetHashCode().Should().Be(implementation1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            implementation2.Should().NotBeSameAs(implementation1, because: "Cloning should not return the same reference.");
        }
    }
}
