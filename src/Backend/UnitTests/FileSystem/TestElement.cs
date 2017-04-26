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

using JetBrains.Annotations;

namespace ZeroInstall.FileSystem
{
    /// <summary>
    /// Represents a file system element used for testing file system operations.
    /// It can either be realized on-disk or compared against an existing on-disk element.
    /// </summary>
    /// <seealso cref="TestRoot"/>
    public abstract class TestElement
    {
        /// <summary>
        /// The name of the file system element.
        /// </summary>
        [NotNull]
        public string Name { get; }

        protected TestElement([NotNull] string name)
        {
            Name = name;
        }

        /// <summary>
        /// Realizes the element as an on-disk element.
        /// </summary>
        /// <param name="parentPath">The full path of the existing directory to realize the element in.</param>
        public abstract void Build([NotNull] string parentPath);

        /// <summary>
        /// Compares the element against an existing on-disk element using assertions.
        /// </summary>
        /// <param name="parentPath">The full path of the directory containing the element.</param>
        public abstract void Verify([NotNull] string parentPath);
    }
}
