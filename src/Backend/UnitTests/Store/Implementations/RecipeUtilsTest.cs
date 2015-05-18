/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Linq;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.Services;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Contains test methods for <see cref="RecipeUtils"/>.
    /// </summary>
    [TestFixture]
    public class RecipeUtilsTest
    {
        [Test]
        public void TestApplyRecipeArchiv()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(ExtractorTest).WriteEmbeddedFile("testArchive.zip", archiveFile);

                var downloadedFiles = new[] {archiveFile};
                var recipe = new Recipe {Steps = {new Archive {MimeType = Archive.MimeTypeZip, Destination = "subDir"}}};

                using (TemporaryDirectory recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    // /dest/symlink [S]
                    string path = new[] {recipeDir, "subDir", "symlink"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "File should exist: " + path);
                    if (!UnixUtils.IsUnix) CollectionAssert.AreEquivalent(new[] {path}, FlagUtils.GetFiles(FlagUtils.SymlinkFile, recipeDir));

                    // /dest/subdir2/executable [deleted]
                    path = new[] {recipeDir, "subDir", "subdir2", "executable"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "File should exist: " + path);
                    if (!UnixUtils.IsUnix) CollectionAssert.AreEquivalent(new[] {path}, FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir));
                }
            }
        }

        [Test]
        public void TestApplyRecipeSingleFile()
        {
            using (var singleFile = new TemporaryFile("0install-unit-tests"))
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(singleFile, "data");
                typeof(ExtractorTest).WriteEmbeddedFile("testArchive.zip", archiveFile);

                var downloadedFiles = new[] {archiveFile, singleFile};
                var recipe = new Recipe {Steps = {new Archive {MimeType = Archive.MimeTypeZip}, new SingleFile {Destination = "subdir2/executable"}}};

                using (TemporaryDirectory recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    // /subdir2/executable [!X]
                    string path = new[] {recipeDir, "subdir2", "executable"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "File should exist: " + path);
                    Assert.AreEqual("data", File.ReadAllText(path));
                    Assert.AreEqual(0, File.GetLastWriteTimeUtc(path).ToUnixTime(), "Single files should be set to Unix epoch");
                    if (!UnixUtils.IsUnix) Assert.IsEmpty(FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir));
                }
            }
        }

        [Test]
        public void TestApplyRecipeRemove()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(ExtractorTest).WriteEmbeddedFile("testArchive.zip", archiveFile);

                var downloadedFiles = new[] {archiveFile};
                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Archive {MimeType = Archive.MimeTypeZip},
                        new RemoveStep {Path = "symlink"},
                        new RemoveStep {Path = "subdir2"}
                    }
                };

                using (TemporaryDirectory recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    if (!UnixUtils.IsUnix)
                    {
                        Assert.IsEmpty(FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir));
                        Assert.IsEmpty(FlagUtils.GetFiles(FlagUtils.SymlinkFile, recipeDir));
                    }

                    // /symlink [deleted]
                    string path = Path.Combine(recipeDir, "symlink");
                    Assert.IsFalse(File.Exists(path), "File should not exist: " + path);

                    // /subdir2 [deleted]
                    path = Path.Combine(recipeDir, "subdir2");
                    Assert.IsFalse(Directory.Exists(path), "Directory should not exist: " + path);
                }
            }
        }

        [Test]
        public void TestApplyRecipeRename()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(ExtractorTest).WriteEmbeddedFile("testArchive.zip", archiveFile);

                var downloadedFiles = new[] {archiveFile};
                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Archive {MimeType = Archive.MimeTypeZip},
                        new RenameStep {Source = "symlink", Destination = "subdir3/symlink2"},
                        new RenameStep {Source = "subdir2/executable", Destination = "subdir2/executable2"}
                    }
                };

                using (TemporaryDirectory recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    if (!UnixUtils.IsUnix)
                    {
                        CollectionAssert.AreEquivalent(
                            new[] {new[] {recipeDir, "subdir2", "executable2"}.Aggregate(Path.Combine)},
                            FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir));
                        CollectionAssert.AreEquivalent(
                            new[] {new[] {recipeDir, "subdir3", "symlink2"}.Aggregate(Path.Combine)},
                            FlagUtils.GetFiles(FlagUtils.SymlinkFile, recipeDir));
                    }

                    // /symlink [deleted]
                    string path = Path.Combine(recipeDir, "symlink");
                    Assert.IsFalse(File.Exists(path), "File should not exist: " + path);

                    // /subdir3/symlink2 [S]
                    path = new[] {recipeDir, "subdir3", "symlink2"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);
                    if (UnixUtils.IsUnix) Assert.IsTrue(FileUtils.IsSymlink(path), "Not symlink: " + path);

                    // /subdir2/executable [deleted]
                    path = new[] {recipeDir, "subdir2", "executable"}.Aggregate(Path.Combine);
                    Assert.IsFalse(File.Exists(path), "File should not exist: " + path);

                    // /subdir2/executable2 [X]
                    path = new[] {recipeDir, "subdir2", "executable2"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);
                    if (UnixUtils.IsUnix) Assert.IsTrue(FileUtils.IsExecutable(path), "Not executable: " + path);
                }
            }
        }

        [Test]
        public void TestApplyRecipeExceptions()
        {
            using (var tempArchive = new TemporaryFile("0install-unit-tests"))
            {
                Assert.Throws<IOException>(() => new Recipe {Steps = {new Archive {Destination = "../destination"}}}.Apply(new[] {tempArchive}, new SilentTaskHandler()),
                    "Should reject breakout path in Archive.Destination");
            }

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                Assert.Throws<IOException>(() => new Recipe {Steps = {new SingleFile {Destination = "../file"}}}.Apply(new[] {tempFile}, new SilentTaskHandler()),
                    "Should reject breakout path in SingleFile.Destination");
            }

            Assert.Throws<IOException>(() => new Recipe {Steps = {new RemoveStep {Path = "../file"}}}.Apply(new TemporaryFile[0], new SilentTaskHandler()),
                "Should reject breakout path in RemoveStep.Path");

            Assert.Throws<IOException>(() => new Recipe {Steps = {new RenameStep {Source = "../source", Destination = "destination"}}}.Apply(new TemporaryFile[0], new SilentTaskHandler()),
                "Should reject breakout path in RenameStep.Source");
            Assert.Throws<IOException>(() => new Recipe {Steps = {new RenameStep {Source = "source", Destination = "../destination"}}}.Apply(new TemporaryFile[0], new SilentTaskHandler()),
                "Should reject breakout path in RenameStep.Destination");
        }

        [Test]
        public void TestApplySingleFilePath()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            using (var workingDir = new TemporaryDirectory("0install-unit-tests"))
            {
                File.WriteAllText(tempFile, "data");

                new SingleFile {Destination = "file"}.Apply(tempFile.Path, workingDir, new MockTaskHandler());

                Assert.IsTrue(File.Exists(tempFile), "Files passed in as string paths should be copied");
                Assert.IsTrue(File.Exists(Path.Combine(workingDir, "file")));
            }
        }

        [Test]
        public void TestApplySingleFileTemp()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            using (var workingDir = new TemporaryDirectory("0install-unit-tests"))
            {
                File.WriteAllText(tempFile, "data");

                new SingleFile {Destination = "file"}.Apply(tempFile, workingDir, new MockTaskHandler());

                Assert.IsFalse(File.Exists(tempFile), "Files passed in as temp objects should be moved");
                Assert.IsTrue(File.Exists(Path.Combine(workingDir, "file")));
            }
        }
    }
}
