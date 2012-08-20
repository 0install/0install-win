/*
 * Copyright 2010-2012 Bastian Eicher, Roland Leopold Walkling
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Common.Collections;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Provides helper methods for dealing with <see cref="Recipe"/>s.
    /// </summary>
    public static class RecipeUtils
    {
        /// <summary>
        /// Applies a <see cref="Recipe"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="recipe">The <see cref="Recipe"/> to apply.</param>
        /// <param name="archiveInfos">Archives downloaded for the the <paramref name="recipe"/>. Must be in same order as <see cref="Archive"/> elements in <paramref name="recipe"/>.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>.</param>
        /// <returns>A <see cref="TemporaryDirectory"/> with the resulting directory content.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="recipe"/> contains unknown <see cref="IRecipeStep"/>s.</exception>
        public static TemporaryDirectory ApplyRecipe(Recipe recipe, IEnumerable<ArchiveFileInfo> archiveInfos, ITaskHandler handler, object tag)
        {
            #region Sanity checks
            if (recipe == null) throw new ArgumentNullException("recipe");
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var targetDir = new TemporaryDirectory("0install-recipe");

            try
            {
                IEnumerator<ArchiveFileInfo> archivesEnum = archiveInfos.GetEnumerator();
                // ReSharper disable AccessToDisposedClosure
                new PerTypeDispatcher<IRecipeStep>(false)
                {
                    (Model.Archive step) =>
                    {
                        archivesEnum.MoveNext();
                        ApplyArchive(archivesEnum.Current, targetDir.Path, handler, tag);
                    },
                    (AddToplevelStep step) => ApplyAddToplevel(step, targetDir.Path),
                    (AddDirectoryStep step) => ApplyAddDirectory(step, targetDir.Path),
                    (RemoveStep step) => ApplyRemove(step, targetDir.Path),
                    (RenameStep step) => ApplyRename(step, targetDir.Path)
                }.Dispatch(recipe.Steps);
                // ReSharper restore AccessToDisposedClosure
                return targetDir;
            }
                #region Error handling
            catch (Exception)
            {
                targetDir.Dispose();
                throw;
            }
            #endregion
        }

        private static void ApplyArchive(ArchiveFileInfo archive, string targetDir, ITaskHandler handler, object tag)
        {
            using (Extractor extractor = Extractor.CreateExtractor(archive.MimeType, archive.Path, archive.StartOffset, targetDir))
            {
                extractor.SubDir = archive.SubDir;
                handler.RunTask(extractor, tag); // Defer task to handler
            }
        }

        private static void ApplyAddToplevel(AddToplevelStep step, string targetDir)
        {
            if (FileUtils.UnifySlashes(step.Directory).Contains(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture))) throw new IOException(string.Format(Resources.RecipeInvalidPath, step.Directory));

            // Move all files to temp dir and then move temp dir to destination
            var tempDir = new TemporaryDirectory("0install-recipe");
            foreach (string entry in Directory.GetFileSystemEntries(targetDir))
            {
                string entryName = Path.GetFileName(entry);
                if (entryName == null || entryName == ".xbit" || entryName == ".symlink") continue;

                if (File.Exists(entry)) File.Move(entry, Path.Combine(tempDir.Path, entryName));
                else if (Directory.Exists(entry)) Directory.Move(entry, Path.Combine(tempDir.Path, entryName));
            }
            Directory.Move(tempDir.Path, Path.Combine(targetDir, step.Directory));

            // Update flag files
            FlagUtils.PrefixExternalFlags(Path.Combine(targetDir, ".xbit"), step.Directory);
            FlagUtils.PrefixExternalFlags(Path.Combine(targetDir, ".symlink"), step.Directory);
        }

        private static void ApplyAddDirectory(AddDirectoryStep step, string targetDir)
        {
            string path = FileUtils.UnifySlashes(step.Path);
            if (FileUtils.IsBreakoutPath(path)) throw new IOException(string.Format(Resources.RecipeInvalidPath, path));

            // Create the directory
            Directory.CreateDirectory(Path.Combine(targetDir, path));
        }

        private static void ApplyRemove(RemoveStep step, string targetDir)
        {
            string path = FileUtils.UnifySlashes(step.Path);
            if (FileUtils.IsBreakoutPath(path)) throw new IOException(string.Format(Resources.RecipeInvalidPath, path));

            // Delete the element
            string absolutePath = Path.Combine(targetDir, path);
            if (File.Exists(absolutePath)) File.Delete(absolutePath);
            else if (Directory.Exists(absolutePath)) Directory.Delete(absolutePath, true);

            // Remove from flag files aswell
            FlagUtils.RemoveExternalFlag(Path.Combine(targetDir, ".xbit"), path);
            FlagUtils.RemoveExternalFlag(Path.Combine(targetDir, ".symlink"), path);
        }

        private static void ApplyRename(RenameStep step, string targetDir)
        {
            string source = FileUtils.UnifySlashes(step.Source);
            string destination = FileUtils.UnifySlashes(step.Destination);
            if (FileUtils.IsBreakoutPath(source)) throw new IOException(string.Format(Resources.RecipeInvalidPath, source));
            if (FileUtils.IsBreakoutPath(destination)) throw new IOException(string.Format(Resources.RecipeInvalidPath, destination));

            // Rename the element
            string sourcePath = Path.Combine(targetDir, source);
            string destinationPath = Path.Combine(targetDir, destination);
            File.Move(sourcePath, destinationPath);

            // Update in flag files aswell
            string xbitFile = Path.Combine(targetDir, ".xbit");
            if (FlagUtils.GetExternalFlags(".xbit", targetDir).Contains(sourcePath))
            {
                FlagUtils.RemoveExternalFlag(xbitFile, source);
                FlagUtils.SetExternalFlag(xbitFile, destination);
            }
            string symlinkFile = Path.Combine(targetDir, ".symlink");
            if (FlagUtils.GetExternalFlags(".symlink", targetDir).Contains(sourcePath))
            {
                FlagUtils.RemoveExternalFlag(symlinkFile, source);
                FlagUtils.SetExternalFlag(symlinkFile, destination);
            }
        }
    }
}
