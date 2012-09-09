/*
 * Copyright 2010-2012 Bastian Eicher, Roland Leopold Walkling
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

using System.Reflection;
using System.IO;
using Common.Streams;

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// Utility class that provides access to the test data contained in the assembly as resources.
    /// </summary>
    public static class TestData
    {
        private static readonly Assembly _testDataAssembly = Assembly.GetAssembly(typeof(TestData));

        public static Stream GetTestZipArchiveStream()
        {
            return GetTestDataResourceStreamByName("testArchive.zip");
        }

        public static Stream GetTestTarArchiveStream()
        {
            return GetTestDataResourceStreamByName("testArchive.tar");
        }

        public static Stream GetTestTarArchiveHardlinkStream()
        {
            return GetTestDataResourceStreamByName("testArchiveHardlink.tar");
        }

        public static Stream GetTestTarGzArchiveStream()
        {
            return GetTestDataResourceStreamByName("testArchive.tar.gz");
        }

        public static Stream GetTestTarBz2ArchiveStream()
        {
            return GetTestDataResourceStreamByName("testArchive.tar.bz2");
        }

        public static Stream GetTestTarLzmaArchiveStream()
        {
            return GetTestDataResourceStreamByName("testArchive.tar.lzma");
        }

        public static Stream GetTestGemArchiveStream()
        {
            return GetTestDataResourceStreamByName("testArchive.gem");
        }

        private static Stream GetTestDataResourceStreamByName(string name)
        {
            return _testDataAssembly.GetManifestResourceStream(typeof(TestData), name);
        }
    }
}
