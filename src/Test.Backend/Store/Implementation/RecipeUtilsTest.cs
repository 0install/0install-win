/*
 * Copyright 2010-2012 Bastian Eicher
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

using System.Collections.Generic;
using System.IO;
using Common.Storage;
using Common.Streams;
using Common.Utils;
using NUnit.Framework;
using ZeroInstall.Injector;
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
                    StreamUtils.Copy(TestData.GetTestZipArchiveStream(), stream);

                var recipe = new Recipe
                {
                    Steps =
                        {
                            new Model.Archive(),
                            new AddToplevelStep {Directory = "toplevel"},
                            new AddDirectoryStep {Path = "sub/dir"},
                            new Model.Archive(),
                            new RemoveStep {Path = "toplevel/regular"},
                            new RenameStep {Source = "executable", Destination = "executable2"}
                        }
                };
                var archives = new[]
                {
                    new ArchiveFileInfo {Path = archiveFile.Path, MimeType = "application/zip"},
                    new ArchiveFileInfo {Path = archiveFile.Path, MimeType = "application/zip"}
                };
                using (TemporaryDirectory recipeDir = RecipeUtils.ApplyRecipe(recipe, archives, new SilentHandler(), null))
                {
                    ICollection<string> xbits = FlagUtils.GetExternalFlags(".xbit", recipeDir.Path);
                    //var symlinks = FlagUtils.GetExternalFlags(".symlink", recipeDir.Path);

                    // toplevel/executable [X]
                    string path = FileUtils.PathCombine(recipeDir.Path, "toplevel", "executable");
                    Assert.IsTrue(File.Exists(path), "Missing file: toplevel/executable");
                    if (MonoUtils.IsUnix) Assert.IsTrue(FileUtils.IsExecutable(path), "Not executable: toplevel/executable");
                    else CollectionAssert.Contains(xbits, path, "Not executable: toplevel/executable");

                    // sub/dir [D]
                    Assert.IsTrue(Directory.Exists(FileUtils.PathCombine(recipeDir.Path, "sub", "dir")), "Missing directory: sub/dir");

                    // regular
                    Assert.IsTrue(File.Exists(FileUtils.PathCombine(recipeDir.Path, "regular")), "Missing file: regular");

                    // executable2 [X]
                    path = FileUtils.PathCombine(recipeDir.Path, "executable2");
                    Assert.IsTrue(File.Exists(path), "Missing file: executable2");
                    if (MonoUtils.IsUnix) Assert.IsTrue(FileUtils.IsExecutable(path), "Not executable: executable2");
                    else CollectionAssert.Contains(xbits, path, "Not executable: executable2");
                }
            }
        }

        [Test]
        public void TestApplyRecipeExceptions()
        {
            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe
            {Steps = {new AddToplevelStep {Directory = "top/level"}}}, new ArchiveFileInfo[0], new SilentHandler(), null));

            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe
            {Steps = {new AddDirectoryStep {Path = "../dir"}}}, new ArchiveFileInfo[0], new SilentHandler(), null));

            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe
            {Steps = {new RemoveStep {Path = "../file"}}}, new ArchiveFileInfo[0], new SilentHandler(), null));

            Assert.Throws<IOException>(() => RecipeUtils.ApplyRecipe(new Recipe
            {Steps = {new RenameStep {Source = "source", Destination = "../destination"}}}, new ArchiveFileInfo[0], new SilentHandler(), null));
        }
    }
}
