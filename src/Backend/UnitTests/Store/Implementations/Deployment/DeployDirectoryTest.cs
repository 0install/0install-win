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
using FluentAssertions;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.Store.Implementations.Manifests;

namespace ZeroInstall.Store.Implementations.Deployment
{
    /// <summary>
    /// Contains test methods for <see cref="DeployDirectory"/>.
    /// </summary>
    [TestFixture]
    public class DeployDirectoryTest : DirectoryOperationTestBase
    {
        private TemporaryDirectory _destinationDirectory;
        private string _destinationManifestPath;
        private string _destinationFile1Path;
        private string _destinationSubdirPath;
        private string _destinationFile2Path;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _destinationDirectory = new TemporaryDirectory("0install-unit-tests");
            _destinationManifestPath = Path.Combine(_destinationDirectory, Manifest.ManifestFile);
            _destinationFile1Path = Path.Combine(_destinationDirectory, "file1");
            _destinationSubdirPath = Path.Combine(_destinationDirectory, "subdir");
            _destinationFile2Path = Path.Combine(_destinationSubdirPath, "file2");
        }

        [TearDown]
        public override void TearDown()
        {
            _destinationDirectory.Dispose();
            base.TearDown();
        }

        [Test]
        public void StageAndCommit()
        {
            Directory.Delete(_destinationDirectory);

            using (var operation = new DeployDirectory(TempDir, Manifest, _destinationDirectory, new SilentTaskHandler()))
            {
                operation.Stage();
                File.Exists(_destinationManifestPath).Should().BeFalse(because: "Final destination manifest file should not exist yet after staging.");
                File.Exists(_destinationFile1Path).Should().BeFalse(because: "Final destination file should not exist yet after staging.");
                Directory.Exists(_destinationSubdirPath).Should().BeTrue(because: "Directories should be created after staging.");
                File.Exists(_destinationFile2Path).Should().BeFalse(because: "Final destination file should not exist yet after staging.");
                Directory.GetFileSystemEntries(_destinationDirectory).Length.Should().Be(3, because: "Temp files should be preset after staging.");
                Directory.GetFileSystemEntries(_destinationSubdirPath).Length.Should().Be(1, because: "Temp files should be preset after staging.");

                operation.Commit();
            }

            Manifest.Load(_destinationManifestPath, Manifest.Format).Should().Equal(Manifest, because: "Destination manifest file should equal in-memory manifest used as copy instruction.");
            File.Exists(_destinationManifestPath).Should().BeTrue(because: "Final destination manifest file should exist after commit.");
            File.Exists(_destinationFile1Path).Should().BeTrue(because: "Final destination file should exist after commit.");
            File.Exists(_destinationFile2Path).Should().BeTrue(because: "Final destination file should exist after commit.");
        }

        [Test]
        public void StageAndRollBack()
        {
            Directory.Delete(_destinationDirectory);

            using (var operation = new DeployDirectory(TempDir, Manifest, _destinationDirectory, new SilentTaskHandler()))
            {
                operation.Stage();
                // Missing .Commit() automatically triggers rollback
            }

            Directory.Exists(_destinationDirectory).Should().BeFalse(because: "Directory should be gone after rollback.");
        }

        [Test]
        public void PreExistingFiles()
        {
            FileUtils.Touch(Path.Combine(_destinationDirectory, "preexisting"));

            using (var operation = new DeployDirectory(TempDir, Manifest, _destinationDirectory, new SilentTaskHandler()))
            {
                operation.Stage();
                // Missing .Commit() automatically triggers rollback
            }

            Directory.GetFileSystemEntries(_destinationDirectory).Length.Should().Be(1, because: "All new content should be gone after rollback.");
        }

        [Test]
        public void ReadOnlyAttribute()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("Read-only file attribute is only available on Windows");

            FileUtils.Touch(_destinationFile1Path);
            new FileInfo(_destinationFile1Path).IsReadOnly = true;

            using (var operation = new DeployDirectory(TempDir, Manifest, _destinationDirectory, new SilentTaskHandler()))
            {
                operation.Stage();
                operation.Commit();
            }

            File.Exists(_destinationFile1Path).Should().BeTrue(because: "Final destination file should exist after commit.");
            File.Exists(_destinationFile2Path).Should().BeTrue(because: "Final destination file should exist after commit.");
        }
    }
}