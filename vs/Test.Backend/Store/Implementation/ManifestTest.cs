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
using Common;
using System.Threading;
using Common.Helpers;
using System.Diagnostics;

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
            string packageDir = StoreFunctionality.CreateArtificialPackage();
            try
            {
                ManifestDigest digest = Manifest.CreateDigest(packageDir, null);
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
            if (Environment.OSVersion.Platform != PlatformID.Unix) Assert.Inconclusive("Can only run on Unix systems");

            using (var package = new TemporaryDirectory())
            {
                File.WriteAllText(Path.Combine(package.Path, "test\nfile"), @"AAA");
                Assert.Throws<ArgumentException>(() => Manifest.Generate(package.Path, ManifestFormat.Sha256, null));
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
            using (var package = new TemporaryDirectory())
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
            using (var package = new TemporaryDirectory())
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
                Manifest.CreateDotFile(package.Path, ManifestFormat.Sha256, null);
                using (var manifestFile = File.OpenText(path["manifest"]))
                {
                    string currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^D /inner$"), "Manifest didn't match expected format:\n" + currentLine);
                    currentLine = manifestFile.ReadLine();
                    Assert.True(Regex.IsMatch(currentLine, @"^X \w+ \w+ \d+ inner.exe$"), "Manifest didn't match expected format:\n" + currentLine);
                }
            }
        }

        [Test]
        public void ShouldCallProgressCallback()
        {
            string packageDir = StoreFunctionality.CreateArtificialPackage();
            try
            {
                bool callbackCalled = false;
                Manifest.Generate(packageDir, ManifestFormat.Sha256, delegate { callbackCalled = true; });
                Assert.IsTrue(callbackCalled);
            }
            finally
            {
                Directory.Delete(packageDir, true);
            }
        }
    }

    [TestFixture]
    public class ManifestGeneration
    {
        private ManifestGenerator someGenerator;
        private TemporaryDirectoryReplacement sandbox;
        private string packageFolder
        {
            get { return sandbox.Path; }
        }

        [SetUp]
        public void SetUp()
        {
            var packageBuilder = new PackageBuilder()
                .AddFile("file1", Guid.NewGuid().ToByteArray())
                .AddFolder("someFolder")
                .AddFolder("someOtherFolder")
                .AddFile("nestedFile", "abc");

            sandbox = new TemporaryDirectoryReplacement(Path.Combine(Path.GetTempPath(), "ManifestGeneration-Sandbox"));
            packageBuilder.WritePackageInto(packageFolder);

            someGenerator = new ManifestGenerator(packageFolder, ManifestFormat.Sha256);
        }

        [TearDown]
        public void TearDown()
        {
            sandbox.Dispose();
        }

        [Test]
        public void ShouldGenerateManifestWithAllFilesListed()
        {
            someGenerator.RunSync();
            Assert.IsNotNull(someGenerator.Result);
            ValidatePackage();
        }

        private void ValidatePackage()
        {
            var theManifest = someGenerator.Result;

            string currentSubdir = "";
            foreach (var node in theManifest.Nodes)
            {
                if (node is ManifestDirectory)
                {
                    DirectoryMustExistInDirectory((ManifestDirectory)node, packageFolder);
                    currentSubdir = ((ManifestDirectory)node).FullPath.Replace('/', Path.DirectorySeparatorChar).Substring(1);
                }
                else if (node is ManifestFileBase)
                {
                    string directory = Path.Combine(packageFolder, currentSubdir);
                    FileMustExistInDirectory((ManifestFileBase)node, directory);
                }
                else Debug.Fail("Unknown manifest node found: " + node.ToString());
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
        public void ShouldReportReadyStateAtBeginning()
        {
            Assert.AreEqual(ProgressState.Ready, someGenerator.State);
        }

        [Test]
        public void ShouldReportTransitionFromReadyToStarted()
        {
            bool changedToStarted = false;
            someGenerator.StateChanged += (IProgress sender) => { if (sender.State == ProgressState.Started) changedToStarted = true; };
            someGenerator.RunSync();
            Assert.IsTrue(changedToStarted);
        }

        [Test]
        public void ShouldReportTransitionToComplete()
        {
            bool changedToComplete = false;
            someGenerator.StateChanged += (sender) => { if (sender.State == ProgressState.Complete) changedToComplete = true; };
            someGenerator.RunSync();
            Assert.AreEqual(ProgressState.Complete, someGenerator.State);
            Assert.IsTrue(changedToComplete);
        }

        [Test]
        public void ShouldRunAsynchronously()
        {
            var testerLock = new ManualResetEvent(false);
            var injectionLock = new ManualResetEvent(false);
            var completionLock = new ManualResetEvent(false);
            bool noTimeout = true;

            // The assumption here is, that the StateChanged event is called
            // from within the working thread.
            someGenerator.StateChanged += (sender) =>
            {
                if (sender.State == ProgressState.Header)
                {
                    testerLock.Set();
                    noTimeout &= injectionLock.WaitOne(100);
                }
                if (sender.State == ProgressState.Complete) completionLock.Set();
            };
            someGenerator.Start();
            noTimeout &= testerLock.WaitOne(100);
            Assert.IsTrue(noTimeout, "A timeout occurred");
            Assert.AreEqual(ProgressState.Header, someGenerator.State, "ManifestGenerator was not in Started state.");
            injectionLock.Set();
            completionLock.WaitOne();
        }

        [Test]
        public void ShouldOfferJoin()
        {
            Thread testerThread = Thread.CurrentThread;
            System.Threading.ThreadState? testerThreadState = null;
            var completedLock = new ManualResetEvent(false);
            someGenerator.StateChanged += (sender) =>
            {
                if (sender.State == ProgressState.Started)
                {
                    // Yield rest of time slice so that main thread can continue with Join()
                    Thread.Sleep(0);
                    testerThreadState = testerThread.ThreadState;
                }
                if (sender.State == ProgressState.Complete)
                {
                    completedLock.Set();
                }
            };
            someGenerator.Start();
            someGenerator.Join();
            bool didTerminate;
            try
            {
                Assert.AreEqual(ProgressState.Complete, someGenerator.State, "After Join() the ManifestGenerator must be in Complete state.");
                Assert.AreNotEqual(System.Threading.ThreadState.Running, testerThreadState, "The thread that called join must be in blocking state for some time.");
            }
            finally
            {
                didTerminate = completedLock.WaitOne(2000);
            }
            Assert.IsTrue(didTerminate, "ManifestGenerator did not terminate");
        }

        [Test]
        public void ShouldReportChangedProgress()
        {
            bool progressChanged = false;
            someGenerator.ProgressChanged += delegate { progressChanged = true; };
            someGenerator.RunSync();
            Assert.IsTrue(progressChanged);
        }
    }
}
