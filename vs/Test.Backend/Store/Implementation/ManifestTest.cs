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
using System.Text.RegularExpressions;
using Common.Storage;
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Model;
using Common.Utils;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Contains test methods for <see cref="Manifest"/>.
    /// </summary>
    [TestFixture]
    public class ManifestTest
    {
        #region Helpers
        /// <summary>
        /// Creates a <see cref="Manifest"/> from a temporary directory.
        /// </summary>
        private static Manifest CreateTestManifest()
        {
            // Create a test directory to create a manifest for
            string tempDir = DirectoryStoreTest.CreateArtificialPackage();

            try
            {
                // Generate manifest, write it to a file and read the file again
                return Manifest.Generate(tempDir, ManifestFormat.Sha1Old, null);
            }
            finally
            { // Clean up
                Directory.Delete(tempDir, true);
            }
        }
        #endregion

        /// <summary>
        /// Ensures that <see cref="Manifest"/> is correctly generated, serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Manifest manifest1, manifest2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Generate manifest, write it to a file and read the file again
                manifest1 = CreateTestManifest();
                manifest1.Save(tempFile.Path);
                manifest2 = Manifest.Load(tempFile.Path, ManifestFormat.Sha1Old);
            }

            // Ensure data stayed the same
            Assert.AreEqual(manifest1, manifest2);
        }

        /// <summary>
        /// Ensures that <see cref="Manifest.CalculateDigest"/> returns the same value as <see cref="Manifest.CreateDotFile"/>.
        /// </summary>
        [Test]
        public void TestCalculateHash()
        {
            string packageDir = DirectoryStoreTest.CreateArtificialPackage();
            try
            {
                string inMemoryHash = Manifest.Generate(packageDir, ManifestFormat.Sha256, null).CalculateDigest();
                string diskHash = Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256, null);
                Assert.AreEqual(diskHash, inMemoryHash);
            }
            finally 
            {
                Directory.Delete(packageDir, true);
            }
        }

        /// <summary>
        /// Ensures that <see cref="Manifest.CreateDigest"/> correctly generates a <see cref="ManifestDigest"/> with multiple <see cref="ManifestFormat"/>s.
        /// </summary>
        [Test]
        public void TestCreateDigest()
        {
            string packageDir = DirectoryStoreTest.CreateArtificialPackage();
            try
            {
                ManifestDigest digest1 = Manifest.CreateDigest(packageDir, null);
                Assert.IsNullOrEmpty(digest1.Sha1Old); // Sha1Old is deprecated
                Assert.IsNotNullOrEmpty(digest1.Sha1New);
                Assert.IsNotNullOrEmpty(digest1.Sha256);

                ManifestDigest digest2 = Manifest.CreateDigest(packageDir, null);
                Assert.AreEqual(digest1, digest2);
            }
            finally
            {
                Directory.Delete(packageDir, true);
            }
        }

        [Test]
        public void ShouldListExecutableWithFlagF()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string exePath = Path.Combine(package.Path, "test.exe");
                string manifestPath = Path.Combine(package.Path, ".manifest");

                File.WriteAllText(exePath, "");
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256, null);

                using (var manifest = File.OpenText(manifestPath))
                {
                    string firstLine = manifest.ReadLine();
                    Assert.True(Regex.IsMatch(firstLine, @"^F \w+ \w+ \d+ test.exe$"), "Manifest didn't match expected format");
                }
            }
        }

        [Test]
        public void ShouldListFilesInXbitWithFlagX()
        {
            if (!MonoUtils.IsUnix) throw new InconclusiveException(".xbit files are not used on Unix platforms");

            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string exePath = Path.Combine(package.Path, "test.exe");
                string xbitPath = Path.Combine(package.Path, ".xbit");
                string manifestPath = Path.Combine(package.Path, ".manifest");
                
                File.WriteAllText(exePath, "");
                File.WriteAllText(xbitPath, @"/test.exe");
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256, null);

                using (var manifest = File.OpenText(manifestPath))
                {
                    string firstLine = manifest.ReadLine();
                    Assert.True(Regex.IsMatch(firstLine, @"^X \w+ \w+ \d+ test.exe$"), "Manifest didn't match expected format");
                }
            }
        }

        [Test]
        public void ShouldListNothingForEmptyPackage()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256, null);
                using (var manifestFile = File.OpenRead(Path.Combine(package.Path, ".manifest")))
                {
                    Assert.AreEqual(0, manifestFile.Length, "Empty package directory should make an empty manifest");
                }
            }
        }

        [Test]
        public void ShouldHandleSubdirectoriesWithExecutables()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string innerPath = Path.Combine(package.Path, "inner");
                Directory.CreateDirectory(innerPath);

                string innerExePath = Path.Combine(innerPath, "inner.exe");
                string xbitPath = Path.Combine(package.Path, ".xbit");
                string manifestPath = Path.Combine(package.Path, ".manifest");
                File.WriteAllText(innerExePath, @"xxxxxxx");
                File.WriteAllText(xbitPath, @"/inner/inner.exe");
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256, null);
                using (var manifestFile = File.OpenText(manifestPath))
                {
                    string currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^D /inner$"), "Manifest didn't match expected format:\n" + currentLine);
                    currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^X \w+ \w+ \d+ inner.exe$"), "Manifest didn't match expected format:\n" + currentLine);
                }
            }
        }

        [Test]
        public void ShouldHandleSha1Old()
        {
            using (var package = new TemporaryDirectory("0install-unit-tests"))
            {
                string innerPath = Path.Combine(package.Path, "inner");
                Directory.CreateDirectory(innerPath);

                string innerExePath = Path.Combine(innerPath, "inner.exe");
                string xbitPath = Path.Combine(package.Path, ".xbit");
                string manifestPath = Path.Combine(package.Path, ".manifest");
                File.WriteAllText(innerExePath, @"xxxxxxx");
                File.WriteAllText(xbitPath, @"/inner/inner.exe");
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha1Old, null);
                using (var manifestFile = File.OpenText(manifestPath))
                {
                    string currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^D \w+ /inner$"), "Manifest didn't match expected format:\n" + currentLine);
                    currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^X \w+ \w+ \d+ inner.exe$"), "Manifest didn't match expected format:\n" + currentLine);
                }
            }
        }

        [Test]
        public void ShouldCallProgressCallback()
        {
            string packageDir = DirectoryStoreTest.CreateArtificialPackage();
            try
            {
                var handlerMock = new DynamicMock("MockHandler", typeof(IImplementationHandler));
                handlerMock.Expect("RunIOTask");
                Manifest.Generate(packageDir, ManifestFormat.Sha256, (IImplementationHandler)handlerMock.MockInstance);
                handlerMock.Verify();
            }
            finally
            {
                Directory.Delete(packageDir, true);
            }
        }
    }
}
