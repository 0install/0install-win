/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Contains test methods for <see cref="Fetchers.RecipeUtils"/>.
    /// </summary>
    [TestFixture]
    public class RecipeUtilsTest
    {
        [Test]
        public void TestApplyRecipeArchiv()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                using (FileStream stream = File.Create(archiveFile))
                    Store.Implementation.Archive.TestData.GetTestZipArchiveStream().CopyTo(stream);
                var downloadedFiles = new[] {archiveFile};

                var recipe = new Recipe {Steps = {new Archive {MimeType = "application/zip", Destination = "subDir"}}};
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, downloadedFiles, new SilentTaskHandler(), null))
                {
                    // /dest/symlink [S]
                    string path = new[] {recipeDir, "subDir", "symlink"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "File should exist: " + path);
                    if (!MonoUtils.IsUnix) CollectionAssert.AreEquivalent(new[] {path}, FlagUtils.GetExternalFlags(".symlink", recipeDir));

                    // /dest/subdir2/executable [deleted]
                    path = new[] {recipeDir, "subDir", "subdir2", "executable"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "File should exist: " + path);
                    if (!MonoUtils.IsUnix) CollectionAssert.AreEquivalent(new[] {path}, FlagUtils.GetExternalFlags(".xbit", recipeDir));
                }
            }
        }

        [Test]
        public void TestApplyRecipeSingleFile()
        {
            using (var singleFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(singleFile, "data");
                var downloadedFiles = new[] {singleFile};

                var recipe = new Recipe {Steps = {new SingleFile {Destination = "subdir2/executable"}}};
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, downloadedFiles, new SilentTaskHandler(), null))
                {
                    // /subdir2/executable [!X]
                    string path = new[] {recipeDir, "subdir2", "executable"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "File should exist: " + path);
                    Assert.AreEqual("data", File.ReadAllText(path));
                    Assert.AreEqual(0, File.GetLastWriteTimeUtc(path).ToUnixTime(), "Single files should be set to Unix epoch");
                    if (!MonoUtils.IsUnix) CollectionAssert.IsEmpty(FlagUtils.GetExternalFlags(".xbit", recipeDir));
                }
            }
        }

        [Test]
        public void TestApplyRecipeRemove()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                using (FileStream stream = File.Create(archiveFile))
                    Store.Implementation.Archive.TestData.GetTestZipArchiveStream().CopyTo(stream);
                var downloadedFiles = new[] {archiveFile};

                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Archive {MimeType = "application/zip"},
                        new RemoveStep {Path = "symlink"},
                        new RemoveStep {Path = "subdir2"}
                    }
                };
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, downloadedFiles, new SilentTaskHandler(), null))
                {
                    if (!MonoUtils.IsUnix)
                    {
                        CollectionAssert.IsEmpty(FlagUtils.GetExternalFlags(".xbit", recipeDir));
                        CollectionAssert.IsEmpty(FlagUtils.GetExternalFlags(".symlink", recipeDir));
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
                using (FileStream stream = File.Create(archiveFile))
                    Store.Implementation.Archive.TestData.GetTestZipArchiveStream().CopyTo(stream);
                var downloadedFiles = new[] {archiveFile};

                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Archive {MimeType = "application/zip"},
                        new RenameStep {Source = "symlink", Destination = "subdir3/symlink2"},
                        new RenameStep {Source = "subdir2/executable", Destination = "subdir2/executable2"}
                    }
                };
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, downloadedFiles, new SilentTaskHandler(), null))
                {
                    if (!MonoUtils.IsUnix)
                    {
                        CollectionAssert.AreEquivalent(
                            new[] {new[] {recipeDir, "subdir2", "executable2"}.Aggregate(Path.Combine)},
                            FlagUtils.GetExternalFlags(".xbit", recipeDir));
                        CollectionAssert.AreEquivalent(
                            new[] {new[] {recipeDir, "subdir3", "symlink2"}.Aggregate(Path.Combine)},
                            FlagUtils.GetExternalFlags(".symlink", recipeDir));
                    }

                    // /symlink [deleted]
                    string path = Path.Combine(recipeDir, "symlink");
                    Assert.IsFalse(File.Exists(path), "File should not exist: " + path);

                    // /subdir3/symlink2 [S]
                    path = new[] {recipeDir, "subdir3", "symlink2"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);
                    if (MonoUtils.IsUnix) Assert.IsTrue(FileUtils.IsSymlink(path), "Not symlink: " + path);

                    // /subdir2/executable [deleted]
                    path = new[] {recipeDir, "subdir2", "executable"}.Aggregate(Path.Combine);
                    Assert.IsFalse(File.Exists(path), "File should not exist: " + path);

                    // /subdir2/executable2 [X]
                    path = new[] {recipeDir, "subdir2", "executable2"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);
                    if (MonoUtils.IsUnix) Assert.IsTrue(FileUtils.IsExecutable(path), "Not executable: " + path);
                }
            }
        }

        [Test]
        public void TestApplyRecipeExceptions()
        {
            using (var tempArchive = new TemporaryFile("0install-unit-tests"))
            {
                Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe {Steps = {new Archive {Destination = "../destination"}}}, new[] {tempArchive}, new SilentTaskHandler(), null),
                    "Should reject breakout path in Archive.Destination");
            }

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe {Steps = {new SingleFile {Destination = "../file"}}}, new[] {tempFile}, new SilentTaskHandler(), null),
                    "Should reject breakout path in SingleFile.Destination");
            }

            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe {Steps = {new RemoveStep {Path = "../file"}}}, new TemporaryFile[0], new SilentTaskHandler(), null),
                "Should reject breakout path in RemoveStep.Path");

            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe {Steps = {new RenameStep {Source = "../source", Destination = "destination"}}}, new TemporaryFile[0], new SilentTaskHandler(), null),
                "Should reject breakout path in RenameStep.Source");
            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe {Steps = {new RenameStep {Source = "source", Destination = "../destination"}}}, new TemporaryFile[0], new SilentTaskHandler(), null),
                "Should reject breakout path in RenameStep.Destination");
        }
    }
}
