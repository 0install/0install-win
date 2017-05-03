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

using System.IO;
using FluentAssertions;
using JetBrains.Annotations;

namespace ZeroInstall.FileSystem
{
    /// <summary>
    /// Represents a non-existing/deleted directory used for testing file system operations.
    /// It can either be deleted from disk or compared against the existance of an on-disk directory.
    /// </summary>
    /// <seealso cref="TestRoot"/>
    public class TestDeletedDirectory : TestElement
    {
        /// <summary>
        /// Creates a new test deleted directory.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        public TestDeletedDirectory([NotNull] string name) : base(name)
        {}

        public override void Build(string parentPath)
        {
            string path = Path.Combine(parentPath, Name);
            Directory.Delete(path, recursive: true);
        }

        public override void Verify(string parentPath)
        {
            string path = Path.Combine(parentPath, Name);
            Directory.Exists(path).Should().BeFalse(because: $"Directory '{path}' should not exist.");
        }
    }
}
