/*
 * Copyright 2010-2017 Bastian Eicher
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;

namespace ZeroInstall.FileSystem
{
    /// <summary>
    /// Represents a directory used for testing file system operations.
    /// It can either be realized on-disk or compared against an existing on-disk directory.
    /// </summary>
    /// <seealso cref="TestRoot"/>
    public class TestDirectory : TestElement, IEnumerable<TestElement>
    {
        /// <summary>
        /// The last write time of the directory.
        /// </summary>
        public DateTime LastWrite { get; set; }

        /// <summary>
        /// The <seealso cref="TestElement"/>s contained within the directory.
        /// Walked recursively by <seealso cref="Build"/> and <seealso cref="Verify"/>.
        /// </summary>
        public ICollection<TestElement> Children { get; } = new List<TestElement>();

        public IEnumerator<TestElement> GetEnumerator() => Children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Adds an element to <seealso cref="Children"/>.
        /// </summary>
        public void Add(TestElement element) => Children.Add(element);

        /// <summary>
        /// Creates a new test directory.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        public TestDirectory([NotNull] string name) : base(name)
        {}

        public override void Build(string parentPath)
        {
            string path = Path.Combine(parentPath, Name);
            Directory.CreateDirectory(path);

            foreach (var element in Children)
                element.Build(path);

            if (LastWrite != default(DateTime))
                Directory.SetLastWriteTimeUtc(path, LastWrite);
        }

        public override void Verify(string parentPath)
        {
            string path = Path.Combine(parentPath, Name);
            Directory.Exists(path).Should().BeTrue(because: $"Directory '{path}' should exist.");
            if (LastWrite != default(DateTime))
                Directory.GetLastWriteTimeUtc(path).Should().Be(LastWrite, because: $"Directory '{path}' should have correct last-write time.");

            foreach (var element in Children)
                element.Verify(path);
        }
    }
}
