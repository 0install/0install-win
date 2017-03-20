/*
 * Copyright 2010-2017 Bastian Eicher
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
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.FileSystem;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Contains test methods for <see cref="DirectoryBuilder"/>.
    /// </summary>
    [TestFixture]
    public class DirectoryBuilderTest
    {
        private TemporaryDirectory _tempDir;
        private DirectoryBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _builder = new DirectoryBuilder(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public void Basic()
        {
            _builder.Initialize();
            _builder.CreateDirectory("dir", TestFile.DefaultLastWrite);
            File.WriteAllText(_builder.NewFilePath("dir/file", TestFile.DefaultLastWrite), TestFile.DefaultContents);
            _builder.QueueHardlink("dir/hardlink", "dir/file");
            File.WriteAllText(_builder.NewFilePath("dir/executable", TestFile.DefaultLastWrite, executable: true), TestFile.DefaultContents);
            _builder.CreateSymlink("dir/symlink", "target");
            _builder.CompletePending();

            new TestRoot
            {
                new TestDirectory("dir")
                {
                    new TestFile("file"),
                    new TestFile("hardlink"),
                    new TestFile("executable") {IsExecutable = true},
                    new TestSymlink("symlink", "target")
                }
            }.Verify(_tempDir);
        }

        [Test]
        public void Suffix()
        {
            _builder.TargetSuffix = "suffix";
            _builder.Initialize();
            _builder.CreateDirectory("dir", TestFile.DefaultLastWrite);
            File.WriteAllText(_builder.NewFilePath("dir/file", TestFile.DefaultLastWrite), TestFile.DefaultContents);
            _builder.QueueHardlink("dir/hardlink", "dir/file");
            File.WriteAllText(_builder.NewFilePath("dir/executable", TestFile.DefaultLastWrite, executable: true), TestFile.DefaultContents);
            _builder.CreateSymlink("dir/symlink", "target");
            _builder.CompletePending();

            new TestRoot
            {
                new TestDirectory("suffix")
                {
                    new TestDirectory("dir")
                    {
                        new TestFile("file"),
                        new TestFile("hardlink"),
                        new TestFile("executable") {IsExecutable = true},
                        new TestSymlink("symlink", "target")
                    }
                }
            }.Verify(_tempDir);
        }

        [Test]
        public void OverwriteFile()
        {
            new TestRoot {new TestFile("file") {LastWrite = new DateTime(2000, 2, 2), Contents = "wrong", IsExecutable = true}}.Build(_tempDir);

            _builder.Initialize();
            File.WriteAllText(_builder.NewFilePath("file", TestFile.DefaultLastWrite), TestFile.DefaultContents);
            _builder.CompletePending();

            new TestRoot
            {
                new TestFile("file")
            }.Verify(_tempDir);
        }

        [Test]
        public void OverwriteSymlink()
        {
            new TestRoot {new TestSymlink("file", "target")}.Build(_tempDir);

            _builder.Initialize();
            File.WriteAllText(_builder.NewFilePath("file", TestFile.DefaultLastWrite), TestFile.DefaultContents);
            _builder.CompletePending();

            new TestRoot
            {
                new TestFile("file")
            }.Verify(_tempDir);
        }

        [Test]
        public void OverwriteWithSymlink()
        {
            new TestRoot {new TestFile("file")}.Build(_tempDir);

            _builder.Initialize();
            _builder.CreateSymlink("file", "target");
            _builder.CompletePending();

            new TestRoot
            {
                new TestSymlink("file", "target")
            }.Verify(_tempDir);
        }
    }
}