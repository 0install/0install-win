/*
 * Copyright 2010-2014 Bastian Eicher, Roland Leopold Walkling
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
using System.Diagnostics;
using System.IO;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Contains test methods for <see cref="ManifestGenerator"/>.
    /// </summary>
    [TestFixture]
    public class ManifestGeneratorTest
    {
        private ManifestGenerator _someGenerator;
        private TemporaryDirectory _sandbox;

        [SetUp]
        public void SetUp()
        {
            var packageBuilder = new PackageBuilder()
                .AddFolder("someFolder")
                .AddFolder("someOtherFolder")
                .AddFile("nestedFile1", "abc")
                .AddFile("nestedFile2", "123");

            _sandbox = new TemporaryDirectory("0install-unit-tests");
            packageBuilder.WritePackageInto(_sandbox);

            _someGenerator = new ManifestGenerator(_sandbox, ManifestFormat.Sha256);
        }

        [TearDown]
        public void TearDown()
        {
            _sandbox.Dispose();
        }

        [Test]
        public void ShouldGenerateManifestWithAllFilesListed()
        {
            _someGenerator.Run();
            Assert.IsNotNull(_someGenerator.Result);
            ValidatePackage();
        }

        [Test]
        public void ShouldHandleRelativePaths()
        {
            // Change the working directory
            string oldWorkingDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _sandbox;

            try
            {
                // Replace default generator with one using a relative path
                _someGenerator = new ManifestGenerator(".", ManifestFormat.Sha256);

                ShouldGenerateManifestWithAllFilesListed();
            }
            finally
            {
                // Restore the original working directory
                Environment.CurrentDirectory = oldWorkingDir;
            }
        }

        private void ValidatePackage()
        {
            var theManifest = _someGenerator.Result;

            string currentSubdir = "";
            foreach (var node in theManifest)
            {
                var manifestDirectory = node as ManifestDirectory;
                if (manifestDirectory != null)
                {
                    DirectoryMustExistInDirectory(manifestDirectory, _sandbox);
                    currentSubdir = manifestDirectory.FullPath.Replace('/', Path.DirectorySeparatorChar).Substring(1);
                }
                else
                {
                    var manifestFile = node as ManifestFileBase;
                    if (manifestFile != null)
                    {
                        string directory = Path.Combine(_sandbox, currentSubdir);
                        FileMustExistInDirectory(manifestFile, directory);
                    }
                    else Debug.Fail("Unknown manifest node found: " + node);
                }
            }
        }

        private static void DirectoryMustExistInDirectory(ManifestDirectory node, string packageRoot)
        {
            string fullPath = Path.Combine(packageRoot, node.FullPath.Replace('/', Path.DirectorySeparatorChar).Substring(1));
            Assert.IsTrue(Directory.Exists(fullPath), "Directory " + fullPath + " does not exist.");
        }

        private static void FileMustExistInDirectory(ManifestFileBase node, string directory)
        {
            string fullPath = Path.Combine(directory, node.FileName);
            Assert.IsTrue(File.Exists(fullPath), "File " + fullPath + " does not exist.");
        }

        [Test]
        public void ShouldCalculateCorrectTotalSize()
        {
            _someGenerator.Run();
            Assert.AreEqual(6, _someGenerator.Result.TotalSize);
        }
    }
}
