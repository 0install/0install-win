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
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Contains test methods for <see cref="RecipeUtils"/>.
    /// </summary>
    [TestFixture]
    public class RecipeUtilsTest
    {
        [Test]
        public void TestApplyRecipeDiretory()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                using (FileStream stream = File.Create(archiveFile))
                    TestData.GetTestZipArchiveStream().CopyTo(stream);
                var archives = new[] {new ArchiveFileInfo {Path = archiveFile, MimeType = "application/zip"}};

                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Model.Archive(),
                        new AddDirectoryStep {Path = "subdir3"},
                    }
                };
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, archives, new SilentTaskHandler(), null))
                {
                    // /subdir3 [D]
                    string path = Path.Combine(recipeDir, "subdir3");
                    Assert.IsTrue(Directory.Exists(path), "Missing directory: " + path);
                }
            }
        }

        [Test]
        public void TestApplyRecipeRemove()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                using (FileStream stream = File.Create(archiveFile))
                    TestData.GetTestZipArchiveStream().CopyTo(stream);
                var archives = new[] {new ArchiveFileInfo {Path = archiveFile, MimeType = "application/zip"}};

                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Model.Archive(),
                        new RemoveStep {Path = "symlink"},
                        new RemoveStep {Path = "subdir2"}
                    }
                };
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, archives, new SilentTaskHandler(), null))
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
                    TestData.GetTestZipArchiveStream().CopyTo(stream);
                var archives = new[] {new ArchiveFileInfo {Path = archiveFile, MimeType = "application/zip"}};

                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Model.Archive(),
                        new RenameStep {Source = "symlink", Destination = "symlink2"},
                        new RenameStep {Source = "subdir2/executable", Destination = "subdir2/executable2"}
                    }
                };
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, archives, new SilentTaskHandler(), null))
                {
                    if (!MonoUtils.IsUnix)
                    {
                        CollectionAssert.AreEquivalent(
                            new[] {new[] {recipeDir, "subdir2", "executable2"}.Aggregate(Path.Combine)},
                            FlagUtils.GetExternalFlags(".xbit", recipeDir));
                        CollectionAssert.AreEquivalent(
                            new[] {new[] {recipeDir, "symlink2"}.Aggregate(Path.Combine)},
                            FlagUtils.GetExternalFlags(".symlink", recipeDir));
                    }

                    // /symlink [deleted]
                    string path = Path.Combine(recipeDir, "symlink");
                    Assert.IsFalse(File.Exists(path), "File should not exist: " + path);

                    // /symlink2 [S]
                    path = Path.Combine(recipeDir, "symlink2");
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
            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe
            {Steps = {new AddDirectoryStep {Path = "../dir"}}}, new ArchiveFileInfo[0], new SilentTaskHandler(), null));

            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe
            {Steps = {new RemoveStep {Path = "../file"}}}, new ArchiveFileInfo[0], new SilentTaskHandler(), null));

            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe
            {Steps = {new RenameStep {Source = "source", Destination = "../destination"}}}, new ArchiveFileInfo[0], new SilentTaskHandler(), null));
        }
    }
}
