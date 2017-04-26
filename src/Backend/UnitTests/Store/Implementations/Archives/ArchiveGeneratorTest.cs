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

using System.IO;
using NanoByte.Common.Storage;
using ZeroInstall.FileSystem;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Common test cases for <see cref="ArchiveGenerator"/> sub-classes.
    /// </summary>
    /// <typeparam name="TGenerator">The specific type of <see cref="ArchiveGenerator"/> to test.</typeparam>
    public abstract class ArchiveGeneratorTest<TGenerator>
        where TGenerator : ArchiveGenerator
    {
        protected abstract TGenerator CreateGenerator(string sourceDirectory, Stream stream);

        protected Stream BuildArchive(TestRoot root)
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                root.Build(tempDir);
                return BuildArchive(tempDir);
            }
        }

        protected virtual Stream BuildArchive(string sourcePath)
        {
            using (var archiveWriteStream = new MemoryStream())
            {
                using (var generator = CreateGenerator(sourcePath, archiveWriteStream))
                    generator.Run();
                return new MemoryStream(archiveWriteStream.ToArray(), writable: false);
            }
        }
    }
}