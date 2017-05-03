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
using FluentAssertions;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.FileSystem;
using ZeroInstall.Services;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations.Build
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
                typeof(ArchiveExtractorTest).CopyEmbeddedToFile("testArchive.zip", archiveFile);

                var downloadedFiles = new[] {archiveFile};
                var recipe = new Recipe {Steps = {new Archive {MimeType = Archive.MimeTypeZip, Destination = "dest"}}};

                using (var recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    new TestRoot
                    {
                        new TestDirectory("dest")
                        {
                            new TestSymlink("symlink", "subdir1/regular"),
                            new TestDirectory("subdir1")
                            {
                                new TestFile("regular") {LastWrite = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)}
                            },
                            new TestDirectory("subdir2")
                            {
                                new TestFile("executable") {IsExecutable = true, LastWrite = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)}
                            }
                        }
                    }.Verify(recipeDir);
                }
            }
        }

        [Test]
        public void TestApplyRecipeSingleFile()
        {
            using (var singleFile = new TemporaryFile("0install-unit-tests"))
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(singleFile, TestFile.DefaultContents);
                typeof(ArchiveExtractorTest).CopyEmbeddedToFile("testArchive.zip", archiveFile);

                var downloadedFiles = new[] {archiveFile, singleFile};
                var recipe = new Recipe {Steps = {new Archive {MimeType = Archive.MimeTypeZip}, new SingleFile {Destination = "subdir2/executable"}}};

                using (var recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    new TestRoot
                    {
                        new TestDirectory("subdir2")
                        {
                            new TestFile("executable")
                            {
                                IsExecutable = false, // Executable file was overwritten by a non-executable one
                                LastWrite = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }.Verify(recipeDir);
                }
            }
        }

        [Test]
        public void TestApplyRecipeRemove()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(ArchiveExtractorTest).CopyEmbeddedToFile("testArchive.zip", archiveFile);

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

                using (var recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    new TestRoot
                    {
                        new TestDeletedFile("symlink"),
                        new TestDirectory("subdir1")
                        {
                            new TestFile("regular") {LastWrite = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)}
                        },
                        new TestDeletedDirectory("subdir2")
                    }.Verify(recipeDir);
                }
            }
        }

        [Test]
        public void TestApplyRecipeRename()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(ArchiveExtractorTest).CopyEmbeddedToFile("testArchive.zip", archiveFile);

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

                using (var recipeDir = recipe.Apply(downloadedFiles, new SilentTaskHandler()))
                {
                    new TestRoot
                    {
                        new TestDeletedFile("symlink"),
                        new TestDirectory("subdir1")
                        {
                            new TestFile("regular") {LastWrite = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)},
                        },
                        new TestDirectory("subdir2")
                        {
                            new TestDeletedFile("executable"),
                            new TestFile("executable2") {IsExecutable = true, LastWrite = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)}
                        },
                        new TestDirectory("subdir3")
                        {
                            new TestSymlink("symlink2", "subdir1/regular")
                        }
                    }.Verify(recipeDir);
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

            new Recipe {Steps = {new CopyFromStep {ID = "id123", Destination = "../destination"}}}
                .Invoking(x => x.Apply(new TemporaryFile[0], new SilentTaskHandler()))
                .ShouldThrow<IOException>(because: "Should reject breakout path in CopyFromStep.Destination");
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

                new SingleFile {Destination = "file"}.Apply(tempFile, workingDir);

                File.Exists(tempFile).Should().BeFalse(because: "Files passed in as temp objects should be moved");
                File.Exists(Path.Combine(workingDir, "file")).Should().BeTrue();
            }
        }
    }
}
