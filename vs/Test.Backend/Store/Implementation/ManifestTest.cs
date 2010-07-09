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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;

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
            string tempDir = StoreFunctionality.CreateArtificialPackage();

            try
            {
                // Generate manifest, write it to a file and read the file again
                return Manifest.Generate(tempDir, ManifestFormat.Sha1Old);
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
            string tempFile = Path.GetTempFileName();
            try
            {
                // Generate manifest, write it to a file and read the file again
                manifest1 = CreateTestManifest();
                manifest1.Save(tempFile);
                manifest2 = Manifest.Load(tempFile, ManifestFormat.Sha1Old);
            }
            finally
            { // Clean up
                File.Delete(tempFile);
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
            string packageDir = StoreFunctionality.CreateArtificialPackage();
            try
            {
                string inMemoryHash = Manifest.Generate(packageDir, ManifestFormat.Sha256).CalculateDigest();
                string diskHash = Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256);
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
            string packageDir = StoreFunctionality.CreateArtificialPackage();
            try
            {
                ManifestDigest digest = Manifest.CreateDigest(packageDir);
                Assert.IsNotNullOrEmpty(digest.Sha1Old);
                Assert.IsNotNullOrEmpty(digest.Sha1New);
                Assert.IsNotNullOrEmpty(digest.Sha256);
            }
            finally
            {
                Directory.Delete(packageDir, true);
            }
        }

        [Test]
        public void ShouldRejectFileNamesWithNewline()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix) throw new InconclusiveException("Can only run on Unix systems");

            using (var package = new TemporaryDirectory())
            {
                File.WriteAllText(Path.Combine(package.Path, "test\nfile"), @"AAA");
                Assert.Throws<ArgumentException>(() => Manifest.Generate(package.Path, ManifestFormat.Sha256));
            }
        }

        [Test]
        public void ShouldListExecutableWithFlagF()
        {
            using (var package = new TemporaryDirectory())
            {
                string exePath = Path.Combine(package.Path, "test.exe");
                string manifestPath = Path.Combine(package.Path, ".manifest");

                File.WriteAllText(exePath, "");
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256);

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
            using (var package = new TemporaryDirectory())
            {
                string exePath = Path.Combine(package.Path, "test.exe");
                string xbitPath = Path.Combine(package.Path, ".xbit");
                string manifestPath = Path.Combine(package.Path, ".manifest");
                
                File.WriteAllText(exePath, "");
                File.WriteAllText(xbitPath, @"/test.exe");
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256);

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
            using (var package = new TemporaryDirectory())
            {
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256);
                using (var manifestFile = File.OpenRead(Path.Combine(package.Path, ".manifest")))
                {
                    Assert.AreEqual(0, manifestFile.Length, "Empty package directory should make an empty manifest");
                }
            }
        }

        [Test]
        public void ShouldHandleSubdirectoriesWithExecutables()
        {
            using (var package = new TemporaryDirectory())
            using (var inner = new TemporaryDirectory(Path.Combine(package.Path, "inner")))
            {
                var path = new Dictionary<string, string>();
                path["inner folder"] = inner.Path;
                path["inner exe"] = Path.Combine(path["inner folder"], "inner.exe");
                path["xbit"] = Path.Combine(package.Path, ".xbit");
                path["manifest"] = Path.Combine(package.Path, ".manifest");
                File.WriteAllText(path["inner exe"], @"xxxxxxx");
                File.WriteAllText(path["xbit"], @"/inner/inner.exe");
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256);
                using (var manifestFile = File.OpenText(path["manifest"]))
                {
                    string currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^D /inner$"), "Manifest didn't match expected format:\n" + currentLine);
                    currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^X \w+ \w+ \d+ inner.exe$"), "Manifest didn't match expected format:\n" + currentLine);
                }
            }
        }
    }
}
