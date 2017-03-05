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

using System;
using System.IO;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Common test cases for <see cref="DirectoryWalkTask"/> sub-classes.
    /// </summary>
    public abstract class DirectoryWalkTest<T>
        where T : DirectoryWalkTask
    {
        private TemporaryDirectory _sourceDirectory;
        protected T Target;

        [SetUp]
        public void SetUp()
        {
            _sourceDirectory = new TemporaryDirectory("0install-unit-tests");
            Target = CreateTarget(_sourceDirectory);
        }

        protected abstract T CreateTarget(string sourceDirectory);

        [TearDown]
        public void TearDown()
        {
            _sourceDirectory.Dispose();
        }

        [Test]
        public void TestFileOrder()
        {
            WriteFile("x");
            WriteFile("y");
            WriteFile("Z");

            Execute();

            VerifyFileOrder();
        }

        protected abstract void VerifyFileOrder();

        [Test]
        public void TestFileTypes()
        {
            WriteFile("executable", executable: true);
            WriteFile("normal");
            CreateSymlink("symlink");
            CreateDir("dir");
            WriteFile(Path.Combine("dir", "sub"));

            Execute();

            VerifyFileTypes();
        }

        protected virtual void Execute()
        {
            using (var handler = new SilentTaskHandler())
                handler.RunTask(Target);
        }

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly DateTime Timestamp = new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc);

        protected const string Contents = "abc";

        protected void WriteFile(string name, bool executable = false)
        {
            string path = Path.Combine(_sourceDirectory, name);
            File.WriteAllText(path, Contents);
            File.SetLastWriteTimeUtc(path, Timestamp);

            if (executable)
            {
                if (UnixUtils.IsUnix) FileUtils.SetExecutable(path, true);
                else FlagUtils.Set(Path.Combine(_sourceDirectory, FlagUtils.XbitFile), name);
            }
        }

        protected void CreateDir(string name)
        {
            string path = Path.Combine(_sourceDirectory, name);
            Directory.CreateDirectory(path);
        }

        protected void CreateSymlink(string name)
        {
            if (WindowsUtils.IsWindows)
            {
                CygwinUtils.CreateSymlink(
                    sourcePath: Path.Combine(_sourceDirectory, name),
                    targetPath: Contents);
            }
            else
            {
                FileUtils.CreateSymlink(
                    sourcePath: Path.Combine(_sourceDirectory, name),
                    targetPath: Contents);
            }
        }

        protected void CreateHardlink(string name, string target)
        {
            FileUtils.CreateHardlink(
                sourcePath: Path.Combine(_sourceDirectory, name),
                targetPath: Path.Combine(_sourceDirectory, target));
        }

        protected abstract void VerifyFileTypes();
    }
}
