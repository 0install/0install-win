/*
 * Copyright 2010 Bastian Eicher
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
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// Contains test methods for <see cref="Extractor"/>.
    /// </summary>
    [TestFixture]
    public class ExtractorTest
    {
        [Test(Description = "Ensures Extractor.CreateExtractor() correctly creates a ZipExtractor.")]
        public void TestCreateExtractor()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                string path = Path.Combine(tempDir.Path, "a.zip");

                using (var file = File.Create(path))
                using (var zipStream = new ZipOutputStream(file) {IsStreamOwner = false})
                {
                    var entry = new ZipEntry("file");
                    zipStream.PutNextEntry(entry);
                    zipStream.CloseEntry();
                }

                using (var extractor = Extractor.CreateExtractor(null, path, 0, tempDir.Path))
                    Assert.IsInstanceOf(typeof(ZipExtractor), extractor);
            }
        }
    }
}
