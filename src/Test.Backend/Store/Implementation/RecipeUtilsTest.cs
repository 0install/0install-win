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
        public void TestApplyRecipe()
        {
            using (var archiveFile = new TemporaryFile("0install-unit-tests"))
            {
                using (FileStream stream = File.Create(archiveFile.Path))
                    TestData.GetTestZipArchiveStream().CopyTo(stream);

                var recipe = new Recipe
                {
                    Steps =
                    {
                        new Model.Archive(),
                        new AddDirectoryStep {Path = "toplevel/subdir3"},
                        new Model.Archive(),
                        new RemoveStep {Path = "toplevel/subdir2"},
                        new RenameStep {Source = "subdir2/executable", Destination = "subdir2/executable2"}
                    }
                };
                var archives = new[]
                {
                    new ArchiveFileInfo {Path = archiveFile.Path, MimeType = "application/zip"},
                    new ArchiveFileInfo {Path = archiveFile.Path, MimeType = "application/zip"}
                };
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, archives, new SilentTaskHandler(), null))
                {
                    if (!MonoUtils.IsUnix)
                    {
                        CollectionAssert.AreEquivalent(new[]
                        {
                            new [] {recipeDir.Path, "subdir2", "executable2"}.Aggregate(Path.Combine)
                        }, FlagUtils.GetExternalFlags(".xbit", recipeDir.Path));
                        CollectionAssert.AreEquivalent(new[]
                        {
                            new [] {recipeDir.Path, "symlink"}.Aggregate(Path.Combine),
                            new [] {recipeDir.Path, "toplevel", "symlink"}.Aggregate(Path.Combine)
                        }, FlagUtils.GetExternalFlags(".symlink", recipeDir.Path));
                    }

                    // /symlink [S]
                    string path = Path.Combine(recipeDir.Path, "symlink");
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);
                    if (MonoUtils.IsUnix) Assert.IsTrue(FileUtils.IsSymlink(path), "Not symlink: " + path);

                    // /subdir1/regular
                    path = new [] {recipeDir.Path, "subdir1", "regular"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);

                    // /subdir2/executable [deleted]
                    path = new [] {recipeDir.Path, "subdir2", "executable"}.Aggregate(Path.Combine);
                    Assert.IsFalse(File.Exists(path), "File should not exist: " + path);

                    // /subdir2/executable2 [X]
                    path = new [] {recipeDir.Path, "subdir2", "executable2"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);
                    if (MonoUtils.IsUnix) Assert.IsTrue(FileUtils.IsExecutable(path), "Not executable: " + path);

                    // /toplevel/symlink [S]
                    path = new [] {recipeDir.Path, "toplevel", "symlink"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);
                    if (MonoUtils.IsUnix) Assert.IsTrue(FileUtils.IsSymlink(path), "Not symlink: " + path);

                    // /toplevel/subdir1/regular
                    path = new [] {recipeDir.Path, "toplevel", "subdir1", "regular"}.Aggregate(Path.Combine);
                    Assert.IsTrue(File.Exists(path), "Missing file: " + path);

                    // /toplevel/subdir2 [deleted]
                    path = new [] {recipeDir.Path, "toplevel", "subdir2"}.Aggregate(Path.Combine);
                    Assert.IsFalse(Directory.Exists(path), "Directory should not exist: " + path);

                    // /toplevel/subdir3 [D]
                    path = new[] { recipeDir.Path, "toplevel", "subdir3" }.Aggregate(Path.Combine);
                    Assert.IsTrue(Directory.Exists(path), "Missing directory: " + path);
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
