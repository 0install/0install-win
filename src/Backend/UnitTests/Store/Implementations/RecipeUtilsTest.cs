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
                typeof(ExtractorTest).GetEmbedded("testArchive.zip").CopyToFile(archiveFile);

                var downloadedFiles = new[] {archiveFile};
                var recipe = new Recipe {Steps = {new Archive {MimeType = Archive.MimeTypeZip, Destination = "subDir"}}};

                using (TemporaryDirectory recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    // /dest/symlink [S]
                    string path = Path.Combine(recipeDir, "subDir", "symlink");
                    File.Exists(path).Should().BeTrue(because: "File should exist: " + path);
                    if (UnixUtils.IsUnix) FileUtils.IsSymlink(path).Should().BeTrue();
                    else CygwinUtils.IsSymlink(path).Should().BeTrue();

                    // /dest/subdir2/executable [deleted]
                    path = Path.Combine(recipeDir, "subDir", "subdir2", "executable");
                    File.Exists(path).Should().BeTrue(because: "File should exist: " + path);
                    if (!UnixUtils.IsUnix) FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir).Should().BeEquivalentTo(path);
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
                typeof(ExtractorTest).GetEmbedded("testArchive.zip").CopyToFile(archiveFile);

                var downloadedFiles = new[] {archiveFile, singleFile};
                var recipe = new Recipe {Steps = {new Archive {MimeType = Archive.MimeTypeZip}, new SingleFile {Destination = "subdir2/executable"}}};

                using (TemporaryDirectory recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    // /subdir2/executable [!X]
                    string path = Path.Combine(recipeDir, "subdir2", "executable");
                    File.Exists(path).Should().BeTrue(because: "File should exist: " + path);
                    File.ReadAllText(path).Should().Be("data");
                    File.GetLastWriteTimeUtc(path).ToUnixTime()
                        .Should().Be(0, because: "Single files should be set to Unix epoch");
                    if (!UnixUtils.IsUnix) FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir).Should().BeEmpty();
                }
            }
        }

        [Test]
        public void TestApplyRecipeRemove()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(ExtractorTest).GetEmbedded("testArchive.zip").CopyToFile(archiveFile);

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
                        FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir).Should().BeEmpty();
                        FlagUtils.GetFiles(FlagUtils.SymlinkFile, recipeDir).Should().BeEmpty();
                    }

                    // /symlink [deleted]
                    string path = Path.Combine(recipeDir, "symlink");
                    File.Exists(path).Should().BeFalse(because: "File should not exist: " + path);

                    // /subdir2 [deleted]
                    path = Path.Combine(recipeDir, "subdir2");
                    Directory.Exists(path).Should().BeFalse(because: "Directory should not exist: " + path);
                }
            }
        }

        [Test]
        public void TestApplyRecipeRename()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(ExtractorTest).GetEmbedded("testArchive.zip").CopyToFile(archiveFile);

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
                        FlagUtils.GetFiles(FlagUtils.XbitFile, recipeDir)
                            .Should().BeEquivalentTo(Path.Combine(recipeDir, "subdir2", "executable2"));
                    }

                    // /symlink [deleted]
                    string path = Path.Combine(recipeDir, "symlink");
                    File.Exists(path).Should().BeFalse(because: "File should not exist: " + path);

                    // /subdir3/symlink2 [S]
                    path = Path.Combine(recipeDir, "subdir3", "symlink2");
                    File.Exists(path).Should().BeTrue(because: "Missing file: " + path);
                    if (UnixUtils.IsUnix) FileUtils.IsSymlink(path).Should().BeTrue();
                    else CygwinUtils.IsSymlink(path).Should().BeTrue();

                    // /subdir2/executable [deleted]
                    path = Path.Combine(recipeDir, "subdir2", "executable");
                    File.Exists(path).Should().BeFalse(because: "File should not exist: " + path);

                    // /subdir2/executable2 [X]
                    path = Path.Combine(recipeDir, "subdir2", "executable2");
                    File.Exists(path).Should().BeTrue(because: "Missing file: " + path);
                    if (UnixUtils.IsUnix) FileUtils.IsExecutable(path).Should().BeTrue(because: "Not executable: " + path);
                }
            }
        }

        [Test]
        public void TestApplyRecipeExceptions()
        {
            using (var tempArchive = new TemporaryFile("0install-unit-tests"))
            {
                new Recipe {Steps = {new Archive {Destination = "../destination"}}}
                    .Invoking(x => x.Apply(new[] {tempArchive}, new SilentTaskHandler()))
                    .ShouldThrow<IOException>(because: "Should reject breakout path in Archive.Destination");
            }

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                new Recipe {Steps = {new SingleFile {Destination = "../file"}}}
                    .Invoking(x => x.Apply(new[] {tempFile}, new SilentTaskHandler()))
                    .ShouldThrow<IOException>(because: "Should reject breakout path in SingleFile.Destination");
            }

            new Recipe {Steps = {new RemoveStep {Path = "../file"}}}
                .Invoking(x => x.Apply(new TemporaryFile[0], new SilentTaskHandler()))
                .ShouldThrow<IOException>(because: "Should reject breakout path in RemoveStep.Path");

            new Recipe {Steps = {new RenameStep {Source = "../source", Destination = "destination"}}}
                .Invoking(x => x.Apply(new TemporaryFile[0], new SilentTaskHandler()))
                .ShouldThrow<IOException>(because: "Should reject breakout path in RenameStep.Source");
            new Recipe {Steps = {new RenameStep {Source = "source", Destination = "../destination"}}}
                .Invoking(x => x.Apply(new TemporaryFile[0], new SilentTaskHandler()))
                .ShouldThrow<IOException>(because: "Should reject breakout path in RenameStep.Destination");
        }

        [Test]
        public void TestApplySingleFilePath()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            using (var workingDir = new TemporaryDirectory("0install-unit-tests"))
            {
                File.WriteAllText(tempFile, "data");

                new SingleFile {Destination = "file"}.Apply(tempFile.Path, workingDir, new MockTaskHandler());

                File.Exists(tempFile).Should().BeTrue(because: "Files passed in as string paths should be copied");
                File.Exists(Path.Combine(workingDir, "file")).Should().BeTrue();
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

                File.Exists(tempFile).Should().BeFalse(because: "Files passed in as temp objects should be moved");
                File.Exists(Path.Combine(workingDir, "file")).Should().BeTrue();
            }
        }
    }
}
