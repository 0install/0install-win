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

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Common test cases for <see cref="ArchiveGenerator"/> sub-classes.
    /// </summary>
    public abstract class ArchiveGeneratorTest<T> : DirectoryWalkTest<T>
        where T : ArchiveGenerator
    {
        private MemoryStream _archiveWriteStream;

        protected override T CreateTarget(string sourceDirectory)
        {
            _archiveWriteStream = new MemoryStream();
            return CreateGenerator(sourceDirectory, _archiveWriteStream);
        }

        protected abstract T CreateGenerator(string sourceDirectory, Stream stream);

        protected virtual Stream OpenArchive()
        {
            // NOTE: Must open new stream for reading because write stream gets closed
            return new MemoryStream(_archiveWriteStream.ToArray(), writable: false);
        }

        protected override void Execute()
        {
            base.Execute();
            Target.Dispose();
        }
    }
}
