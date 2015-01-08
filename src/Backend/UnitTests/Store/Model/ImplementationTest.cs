/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;
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
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Implementation"/>.
        /// </summary>
        public static Implementation CreateTestImplementation()
        {
            return new Implementation
            {
                ID = "id", ManifestDigest = new ManifestDigest(sha256: "123"), Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586), Languages = {"en-US"},
                Main = "executable", DocDir = "doc", Stability = Stability.Developer,
                Bindings = {EnvironmentBindingTest.CreateTestBinding()},
                RetrievalMethods = {ArchiveTest.CreateTestArchive(), new Recipe {Steps = {ArchiveTest.CreateTestArchive()}}},
                Commands = {CommandTest.CreateTestCommand1()}
            };
        }
        #endregion

        /// <summary>
        /// Ensures that <see cref="Element.ContainsCommand"/> correctly checks for commands.
        /// </summary>
        [Test]
        public void TestContainsCommand()
        {
            var implementation = CreateTestImplementation();
            Assert.IsTrue(implementation.ContainsCommand(Command.NameRun));
            Assert.IsFalse(implementation.ContainsCommand("other-command"));
        }

        /// <summary>
        /// Ensures that <see cref="Element.GetCommand"/> and <see cref="Element.this"/> correctly retrieve commands.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void TestGetCommand()
        {
            var implementation = CreateTestImplementation();

            Assert.AreEqual(implementation.Commands[0], implementation.GetCommand(Command.NameRun));
            Assert.AreEqual(implementation.Commands[0], implementation[Command.NameRun]);

            Assert.Throws<ArgumentNullException>(() => { var dummy = implementation.GetCommand(""); });
            Assert.IsNull(implementation[""]);

            Assert.IsNull(implementation.GetCommand("invalid"));
            Assert.Throws<KeyNotFoundException>(() => { var dummy = implementation["invalid"]; });
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Normalize"/> correctly identifies manifest digests in the ID tag.
        /// </summary>
        [Test]
        public void TestNormalizeID()
        {
            var implementation = new Implementation {ID = "sha256=123"};
            implementation.Normalize(FeedTest.Test1Uri);
            Assert.AreEqual("123", implementation.ManifestDigest.Sha256);

            implementation = new Implementation {ID = "sha256=wrong", ManifestDigest = new ManifestDigest(sha256: "correct")};
            implementation.Normalize(FeedTest.Test1Uri);
            Assert.AreEqual("correct", implementation.ManifestDigest.Sha256);

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
            Assert.AreEqual("main", implementation[Command.NameRun].Path);
            Assert.AreEqual("test", implementation[Command.NameTest].Path);
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
            Assert.AreEqual(
                expected: WindowsUtils.IsWindows ? @"C:\local\.\subdir" : "/local/./subdir",
                actual: implementation1.ID);
            Assert.AreEqual(
                expected: WindowsUtils.IsWindows ? @"C:\local\.\subdir" : "/local/./subdir",
                actual: implementation1.LocalPath);

            var implementation2 = new Implementation {ID = "./wrong", LocalPath = "subdir"};
            implementation2.Normalize(localUri);
            Assert.AreEqual(
                expected: "./wrong",
                actual: implementation2.ID);
            Assert.AreEqual(
                expected: WindowsUtils.IsWindows ? @"C:\local\subdir" : "/local/subdir",
                actual: implementation2.LocalPath);
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Normalize"/> throws <see cref="IOException"/> for relative paths in non-local feeds.
        /// </summary>
        [Test]
        public void TestNormalizeRemotePath()
        {
            Assert.Throws<IOException>(() => new Implementation {ID = "./subdir"}.Normalize(FeedTest.Test1Uri));
            Assert.Throws<IOException>(() => new Implementation {LocalPath = "subdir"}.Normalize(FeedTest.Test1Uri));
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
            Assert.AreEqual(implementation1, implementation2, "Cloned objects should be equal.");
            Assert.AreEqual(implementation1.GetHashCode(), implementation2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(implementation1, implementation2), "Cloning should not return the same reference.");
        }
    }
}
