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

using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using ZeroInstall.Store.Implementations.Build;

namespace ZeroInstall.FileSystem
{
    /// <summary>
    /// Represents a directory structure used for testing file system operations.
    /// It can either be realized on-disk or compared against an existing on-disk directory.
    /// </summary>
    /// <seealso cref="TestDirectory"/>
    /// <seealso cref="TestFile"/>
    /// <seealso cref="TestSymlink"/>
    public class TestRoot : List<TestElement>
    {
        /// <summary>
        /// Realizes the directory structure as an on-disk directory.
        /// </summary>
        /// <param name="path">The full path of the directory to realize the structure in.</param>
        public void Build([NotNull] string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!UnixUtils.IsUnix)
                File.WriteAllText(Path.Combine(path, FlagUtils.XbitFile), "");

            foreach (var element in this)
                element.Build(path);
        }

        /// <summary>
        /// Compares the structure against an existing on-disk directory using assertions.
        /// </summary>
        /// <param name="path">The full path of the directory to compare the structure against.</param>
        /// <returns>Always <c>true</c>.</returns>
        public bool Verify([NotNull] string path)
        {
            foreach (var element in this)
                element.Verify(path);
            return true;
        }
    }
}
