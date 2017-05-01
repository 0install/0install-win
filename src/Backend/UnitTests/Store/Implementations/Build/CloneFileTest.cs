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
    /// Contains test methods for <see cref="CloneFile"/>.
    /// </summary>
    [TestFixture]
    public class CloneFileTest : CloneTestBase
    {
        [Test]
        public void CopyFile()
        {
            new TestRoot
            {
                new TestFile("fileA"),
                new TestFile("decoy") {Contents = "wrong"}
            }.Build(SourceDirectory);

            new CloneFile(Path.Combine(SourceDirectory, "fileA"), TargetDirectory) {TargetFileName = "fileB"}.Run();

            new TestRoot {new TestFile("fileB")}.Verify(TargetDirectory);
        }

        [Test]
        public void CopyFileSuffix()
        {
            new TestRoot {new TestFile("fileA")}.Build(SourceDirectory);

            new CloneFile(Path.Combine(SourceDirectory, "fileA"), TargetDirectory) {TargetSuffix = "suffix", TargetFileName = "fileB"}.Run();

            new TestRoot {new TestDirectory("suffix") {new TestFile("fileB")}}.Verify(TargetDirectory);
        }

        [Test]
        public void HardlinkFile()
        {
            new TestRoot {new TestFile("fileA")}.Build(SourceDirectory);

            FileUtils.EnableWriteProtection(SourceDirectory); // Hardlinking logic should work around write-protection by temporarily removing it
            try
            {
                new CloneFile(Path.Combine(SourceDirectory, "fileA"), TargetDirectory) {TargetFileName = "fileB", UseHardlinks = true}.Run();
            }
            finally
            {
                FileUtils.DisableWriteProtection(SourceDirectory);
            }

            new TestRoot {new TestFile("fileB")}.Verify(TargetDirectory);
            FileUtils.AreHardlinked(Path.Combine(SourceDirectory, "fileA"), Path.Combine(TargetDirectory, "fileB"));
        }

        [Test]
        public void CopySymlink()
        {
            new TestRoot {new TestSymlink("fileA", "target")}.Build(SourceDirectory);

            new CloneFile(Path.Combine(SourceDirectory, "fileA"), TargetDirectory) {TargetFileName = "fileB"}.Run();

            new TestRoot {new TestSymlink("fileB", "target")}.Verify(TargetDirectory);
        }

        [Test]
        public void OverwriteFile()
        {
            new TestRoot {new TestFile("fileA")}.Build(SourceDirectory);
            new TestRoot {new TestFile("fileB") {LastWrite = new DateTime(2000, 2, 2), Contents = "wrong", IsExecutable = true}}.Build(TargetDirectory);

            new CloneFile(Path.Combine(SourceDirectory, "fileA"), TargetDirectory) {TargetFileName = "fileB"}.Run();

            new TestRoot {new TestFile("fileB")}.Verify(TargetDirectory);
        }

        [Test]
        public void OverwriteSymlink()
        {
            new TestRoot {new TestFile("fileA")}.Build(SourceDirectory);
            new TestRoot {new TestSymlink("fileB", "target")}.Build(TargetDirectory);

            new CloneFile(Path.Combine(SourceDirectory, "fileA"), TargetDirectory) {TargetFileName = "fileB"}.Run();

            new TestRoot {new TestFile("fileB")}.Verify(TargetDirectory);
        }

        [Test]
        public void OverwriteWithSymlink()
        {
            new TestRoot {new TestSymlink("fileA", "target")}.Build(SourceDirectory);
            new TestRoot {new TestFile("fileB")}.Build(TargetDirectory);

            new CloneFile(Path.Combine(SourceDirectory, "fileA"), TargetDirectory) {TargetFileName = "fileB"}.Run();

            new TestRoot {new TestSymlink("fileB", "target")}.Verify(TargetDirectory);
        }
    }
}
