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
using Common.Helpers;
using Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Contains test methods for <see cref="Manifest"/>.
    /// </summary>
    [TestFixture]
    public class ManifestTest
    {
        /// <summary>
        /// Ensures that <see cref="Manifest"/> is correctly generated using the old format and SHA1 algorithm, serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoadOld()
        {
            Manifest manifest1, manifest2;
            string tempFile = null, tempDir = null;
            try
            {
                // Create a test directory to create a manifest for
                tempDir = FileHelper.GetTempDirectory();
                File.WriteAllText(Path.Combine(tempDir, "file1"), @"content1");
                File.WriteAllText(Path.Combine(tempDir, "file2"), @"content2");
                string subDir = Path.Combine(tempDir, @"subdir");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(Path.Combine(subDir, "file"), @"content");

                // Generate manifest, write it to a file and read the file again
                tempFile = Path.GetTempFileName();
                manifest1 = Manifest.Generate(tempDir, ManifestFormat.Sha1Old);
                manifest1.Save(tempFile);
                manifest2 = Manifest.Load(tempFile, ManifestFormat.Sha1Old);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
                if (tempDir != null) Directory.Delete(tempDir, true);
            }

            // Ensure data stayed the same
            Assert.AreEqual(manifest1, manifest2);
        }

        /// <summary>
        /// Ensures that <see cref="Manifest"/> is correctly generated using the new format and SHA1 algorithm, serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoadNew()
        {
            Manifest manifest1, manifest2;
            string tempFile = null, tempDir = null;
            try
            {
                // Create a test directory to create a manifest for
                tempDir = FileHelper.GetTempDirectory();
                File.WriteAllText(Path.Combine(tempDir, "file1"), @"content1");
                File.WriteAllText(Path.Combine(tempDir, "file2"), @"content2");
                string subDir = Path.Combine(tempDir, @"subdir");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(Path.Combine(subDir, "file"), @"content");

                // Generate manifest, write it to a file and read the file again
                tempFile = Path.GetTempFileName();
                manifest1 = Manifest.Generate(tempDir, ManifestFormat.Sha1New);
                manifest1.Save(tempFile);
                manifest2 = Manifest.Load(tempFile, ManifestFormat.Sha1New);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
                if (tempDir != null) Directory.Delete(tempDir, true);
            }

            // Ensure data stayed the same
            Assert.AreEqual(manifest1, manifest2);
        }

        /// <summary>
        /// Ensures that <see cref="Manifest.CalculateHash"/> returns the same value as <see cref="Manifest.CreateDotFile"/>.
        /// </summary>
        [Test]
        public void TestCalculateHash()
        {
            string packageDir = StoreFunctionality.CreateArtificialPackage();
            string inMemoryHash = Manifest.Generate(packageDir, ManifestFormat.Sha256).CalculateHash();
            string diskHash = Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256);

            Assert.AreEqual(diskHash, inMemoryHash);
        }

        [Test]
        public void ShouldListExecutableWithFlag_F()
        {
            using (var package = new TemporaryDirectory())
            {
                string exePath = Path.Combine(package.Path, "test.exe");
                string manifestPath = Path.Combine(package.Path, ".manifest");
                using (File.Create(exePath))
                { }
                Assert.True(File.Exists(exePath), "Test implementation: Dummy file wasn't created");

                Manifest.Generate(package.Path, ManifestFormat.Sha256);
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256);
                using (var manifest = File.OpenText(manifestPath))
                {
                    string firstLine = manifest.ReadLine();
                    Assert.True(Regex.IsMatch(firstLine, @"^F \w+ \w+ \d test.exe$"), "Manifest didn't match expected format");
                }
            }
        }

        [Test]
        public void ShouldListFilesInXbitWith_X()
        {
            using (var package = new TemporaryDirectory())
            {
                string exePath = Path.Combine(package.Path, "test.exe");
                string xbitPath = Path.Combine(package.Path, ".xbit");
                string manifestPath = Path.Combine(package.Path, ".manifest");
                using (File.Create(exePath))
                { }
                Assert.True(File.Exists(exePath), "Test implementation: Dummy file wasn't created");

                using (var xbit = File.CreateText(xbitPath))
                {
                    xbit.WriteLine(@"\test.exe");
                }

                Manifest.Generate(package.Path, ManifestFormat.Sha256);
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256);
                using (var manifest = File.OpenText(manifestPath))
                {
                    string firstLine = manifest.ReadLine();
                    Assert.True(Regex.IsMatch(firstLine, @"^X \w+ \w+ \d test.exe$"), firstLine);
                }
            }
        }
    }
}
