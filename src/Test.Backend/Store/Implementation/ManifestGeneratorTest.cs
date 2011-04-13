/*
 * Copyright 2010 Bastian Eicher, Roland Leopold Walkling
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
using System.IO;
using Common.Storage;
using Common.Tasks;
using NUnit.Framework;
using System.Threading;
using Common.Utils;
using System.Diagnostics;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Contains test methods for <see cref="ManifestGenerator"/>.
    /// </summary>
    [TestFixture]
    public class ManifestGeneratorTest
    {
        private ManifestGenerator _someGenerator;
        private TemporaryDirectory _sandbox;
        private string PackageFolder
        {
            get { return _sandbox.Path; }
        }

        [SetUp]
        public void SetUp()
        {
            var packageBuilder = new PackageBuilder()
                .AddFolder("someFolder")
                .AddFolder("someOtherFolder")
                .AddFile("nestedFile1", "abc")
                .AddFile("nestedFile2", "123");

            _sandbox = new TemporaryDirectory("0install-unit-tests");
            packageBuilder.WritePackageInto(PackageFolder);

            _someGenerator = new ManifestGenerator(PackageFolder, ManifestFormat.Sha256);
        }

        [TearDown]
        public void TearDown()
        {
            _sandbox.Dispose();
        }

        [Test]
        public void ShouldGenerateManifestWithAllFilesListed()
        {
            _someGenerator.RunSync();
            Assert.IsNotNull(_someGenerator.Result);
            ValidatePackage();
        }

        [Test]
        public void ShouldHandleRelativePaths()
        {
            // Change the working directory
            string workingDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = PackageFolder;

            // Replace default generator with one using a relative path
            _someGenerator = new ManifestGenerator(".", ManifestFormat.Sha256);

            ShouldGenerateManifestWithAllFilesListed();

            // Restore the original working directory
            Environment.CurrentDirectory = workingDir;
        }

        private void ValidatePackage()
        {
            var theManifest = _someGenerator.Result;

            string currentSubdir = "";
            foreach (var node in theManifest.Nodes)
            {
                if (node is ManifestDirectory)
                {
                    DirectoryMustExistInDirectory((ManifestDirectory)node, PackageFolder);
                    currentSubdir = ((ManifestDirectory)node).FullPath.Replace('/', Path.DirectorySeparatorChar).Substring(1);
                }
                else if (node is ManifestFileBase)
                {
                    string directory = Path.Combine(PackageFolder, currentSubdir);
                    FileMustExistInDirectory((ManifestFileBase)node, directory);
                }
                else Debug.Fail("Unknown manifest node found: " + node);
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
            Assert.AreEqual(TaskState.Ready, _someGenerator.State);
        }

        [Test]
        public void ShouldReportTransitionFromReadyToStarted()
        {
            bool changedToStarted = false;
            _someGenerator.StateChanged += sender => { if (sender.State == TaskState.Started) changedToStarted = true; };
            _someGenerator.RunSync();
            Assert.IsTrue(changedToStarted);
        }

        [Test]
        public void ShouldReportTransitionToComplete()
        {
            bool changedToComplete = false;
            _someGenerator.StateChanged += sender => { if (sender.State == TaskState.Complete) changedToComplete = true; };
            _someGenerator.RunSync();
            Assert.AreEqual(TaskState.Complete, _someGenerator.State);
            Assert.IsTrue(changedToComplete);
        }

        [Test]
        public void ShouldOfferJoin()
        {
            var completedLock = new ManualResetEvent(false);
            _someGenerator.StateChanged += delegate(ITask sender)
            {
                if (sender.State == TaskState.Complete)
                {
                    completedLock.Set();
                }
            };
            _someGenerator.Start();
            _someGenerator.Join();
            bool didTerminate;
            try
            {
                Assert.AreEqual(TaskState.Complete, _someGenerator.State, "After Join() the ManifestGenerator must be in Complete state.");
            }
            finally
            {
                didTerminate = completedLock.WaitOne(2000, false);
            }
            Assert.IsTrue(didTerminate, "ManifestGenerator did not terminate");
        }

        [Test]
        public void ShouldReportChangedProgress()
        {
            bool progressChanged = false;
            _someGenerator.ProgressChanged += delegate { progressChanged = true; };
            _someGenerator.RunSync();
            Assert.IsTrue(progressChanged);
        }

        [Test]
        public void ShouldCalculateCorrectTotalSize()
        {
            _someGenerator.RunSync();
            Assert.AreEqual(6, _someGenerator.Result.TotalSize);
        }
    }
}
