/*
 * Copyright 2010-2016 Bastian Eicher, Roland Leopold Walkling
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
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
        /// <param name="downloadedFiles">Files downloaded for the the <paramref name="recipe"/>. Must be in same order as <see cref="DownloadRetrievalMethod"/> elements in <paramref name="recipe"/>.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; can be <c>null</c>.</param>
        /// <returns>A <see cref="TemporaryDirectory"/> with the resulting directory content.</returns>
        /// <exception cref="ArgumentException">The <see cref="Archive"/>s in <paramref name="recipe"/> and the files in <paramref name="downloadedFiles"/> do not match up.</exception>
        /// <exception cref="NotSupportedException"><paramref name="recipe"/> contains unknown step types.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "False positivie due to usage inside lamda")]
        public static TemporaryDirectory Apply([NotNull] this Recipe recipe, [NotNull, ItemNotNull] IEnumerable<TemporaryFile> downloadedFiles, [NotNull] ITaskHandler handler, [CanBeNull] object tag = null)
        {
            #region Sanity checks
            if (recipe == null) throw new ArgumentNullException("recipe");
            if (downloadedFiles == null) throw new ArgumentNullException("downloadedFiles");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            if (recipe.UnknownElements != null && recipe.UnknownElements.Length != 0)
                throw new NotSupportedException(string.Format(Resources.UnknownRecipeStepType, recipe.UnknownElements[0].Name));

            var workingDir = new TemporaryDirectory("0install-recipe");

            try
            {
                IEnumerator<TemporaryFile> downloadedEnum = downloadedFiles.GetEnumerator();
                // ReSharper disable AccessToDisposedClosure
                new PerTypeDispatcher<IRecipeStep>(ignoreMissing: false)
                {
                    (Archive step) =>
                    {
                        downloadedEnum.MoveNext();
                        if (downloadedEnum.Current == null) throw new ArgumentException(Resources.RecipeFileNotDownloaded, "downloadedFiles");
                        step.Apply(downloadedEnum.Current, workingDir, handler, tag);
                    },
                    (SingleFile step) =>
                    {
                        downloadedEnum.MoveNext();
                        if (downloadedEnum.Current == null) throw new ArgumentException(Resources.RecipeFileNotDownloaded, "downloadedFiles");
                        step.Apply(downloadedEnum.Current, workingDir, handler, tag);
                    },
                    (RemoveStep step) => step.Apply(workingDir),
                    (RenameStep step) => step.Apply(workingDir)
                }.Dispatch(recipe.Steps);
                // ReSharper restore AccessToDisposedClosure
                return workingDir;
            }
                #region Error handling
            catch
            {
                workingDir.Dispose();
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Applies a <see cref="Store.Model.Archive"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Store.Model.Archive"/> to apply.</param>
        /// <param name="localPath">The local path of the archive.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; can be <c>null</c>.</param>
        /// <exception cref="IOException">A path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply([NotNull] this Archive step, [NotNull] string localPath, [NotNull] TemporaryDirectory workingDir, [NotNull] ITaskHandler handler, [CanBeNull] object tag = null)
        {
            #region Sanity checks
            if (step == null) throw new ArgumentNullException("step");
            if (string.IsNullOrEmpty(localPath)) throw new ArgumentNullException("localPath");
            if (workingDir == null) throw new ArgumentNullException("workingDir");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            #region Path validation
            if (!string.IsNullOrEmpty(step.Destination))
            {
                string destination = FileUtils.UnifySlashes(step.Destination);
                if (FileUtils.IsBreakoutPath(destination)) throw new IOException(string.Format(Resources.RecipeInvalidPath, destination));
            }
            #endregion

            if (string.IsNullOrEmpty(step.MimeType)) throw new IOException(Resources.UnknownArchiveType);

            using (var extractor = Extractor.Create(localPath, workingDir, step.MimeType))
            {
                extractor.SubDir = step.Extract;
                extractor.Destination = FileUtils.UnifySlashes(step.Destination);
                extractor.Tag = tag;
                handler.RunTask(extractor);
            }
        }

        /// <summary>
        /// Applies a <see cref="SingleFile"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Store.Model.Archive"/> to apply.</param>
        /// <param name="localPath">The local path of the file.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; can be <c>null</c>.</param>
        /// <exception cref="IOException">A path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply([NotNull] this SingleFile step, [NotNull] string localPath, [NotNull] TemporaryDirectory workingDir, [NotNull] ITaskHandler handler, [CanBeNull] object tag = null)
        {
            #region Sanity checks
            if (step == null) throw new ArgumentNullException("step");
            if (string.IsNullOrEmpty(localPath)) throw new ArgumentNullException("localPath");
            if (workingDir == null) throw new ArgumentNullException("workingDir");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Use a copy of the original file because the source file is moved
            using (var tempFile = new TemporaryFile("0install"))
            {
                // ReSharper disable once AccessToDisposedClosure
                handler.RunTask(new SimpleTask(Resources.CopyFiles, () => File.Copy(localPath, tempFile, overwrite: true)));
                step.Apply(tempFile, workingDir, handler, tag);
            }
        }

        /// <summary>
        /// Applies a <see cref="SingleFile"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Store.Model.Archive"/> to apply.</param>
        /// <param name="downloadedFile">The file downloaded from <see cref="DownloadRetrievalMethod.Href"/>.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; can be <c>null</c>.</param>
        /// <exception cref="IOException">A path specified in <paramref name="step"/> is illegal.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tag", Justification = "Number of method parameters must match overloaded method to ensure proper type-based compiler selection")]
        public static void Apply([NotNull] this SingleFile step, [NotNull] TemporaryFile downloadedFile, [NotNull] TemporaryDirectory workingDir, [NotNull] ITaskHandler handler, [CanBeNull] object tag = null)
        {
            #region Sanity checks
            if (step == null) throw new ArgumentNullException("step");
            if (downloadedFile == null) throw new ArgumentNullException("downloadedFile");
            if (workingDir == null) throw new ArgumentNullException("workingDir");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            #region Path validation
            if (string.IsNullOrEmpty(step.Destination)) throw new IOException(Resources.FileMissingDest);
            string destination = FileUtils.UnifySlashes(step.Destination);
            if (FileUtils.IsBreakoutPath(destination)) throw new IOException(string.Format(Resources.RecipeInvalidPath, destination));
            #endregion

            string destinationPath = Path.Combine(workingDir, destination);
            string parentDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);
            FileUtils.Replace(downloadedFile, destinationPath);
            File.SetLastWriteTimeUtc(destinationPath, FileUtils.FromUnixTime(0));

            // Update in flag files as well
            FlagUtils.Remove(Path.Combine(workingDir, FlagUtils.XbitFile), destination);
            FlagUtils.Remove(Path.Combine(workingDir, FlagUtils.SymlinkFile), destination);
        }

        /// <summary>
        /// Applies a <see cref="RemoveStep"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Store.Model.Archive"/> to apply.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <exception cref="IOException">A path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply([NotNull] this RemoveStep step, [NotNull] TemporaryDirectory workingDir)
        {
            #region Sanity checks
            if (step == null) throw new ArgumentNullException("step");
            if (workingDir == null) throw new ArgumentNullException("workingDir");
            #endregion

            #region Path validation
            if (string.IsNullOrEmpty(step.Path)) throw new IOException(string.Format(Resources.RecipeInvalidPath, "(empty)"));
            string path = FileUtils.UnifySlashes(step.Path);
            if (FileUtils.IsBreakoutPath(path)) throw new IOException(string.Format(Resources.RecipeInvalidPath, path));
            #endregion

            string absolutePath = Path.Combine(workingDir, path);
            if (Directory.Exists(absolutePath)) Directory.Delete(absolutePath, recursive: true);
            else File.Delete(absolutePath);

            // Update in flag files as well
            FlagUtils.Remove(Path.Combine(workingDir, FlagUtils.XbitFile), path);
            FlagUtils.Remove(Path.Combine(workingDir, FlagUtils.SymlinkFile), path);
        }

        /// <summary>
        /// Applies a <see cref="RenameStep"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Store.Model.Archive"/> to apply.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <exception cref="IOException">A path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply([NotNull] this RenameStep step, [NotNull] TemporaryDirectory workingDir)
        {
            #region Sanity checks
            if (step == null) throw new ArgumentNullException("step");
            if (workingDir == null) throw new ArgumentNullException("workingDir");
            #endregion

            #region Path validation
            if (string.IsNullOrEmpty(step.Source)) throw new IOException(string.Format(Resources.RecipeInvalidPath, "(empty)"));
            if (string.IsNullOrEmpty(step.Destination)) throw new IOException(string.Format(Resources.RecipeInvalidPath, "(empty)"));
            string source = FileUtils.UnifySlashes(step.Source);
            string destination = FileUtils.UnifySlashes(step.Destination);
            if (FileUtils.IsBreakoutPath(source)) throw new IOException(string.Format(Resources.RecipeInvalidPath, source));
            if (FileUtils.IsBreakoutPath(destination)) throw new IOException(string.Format(Resources.RecipeInvalidPath, destination));
            #endregion

            string sourcePath = Path.Combine(workingDir, source);
            string destinationPath = Path.Combine(workingDir, destination);
            string parentDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);

            if (Directory.Exists(sourcePath)) Directory.Move(sourcePath, destinationPath);
            else File.Move(sourcePath, destinationPath);

            // Update in flag files as well
            FlagUtils.Rename(Path.Combine(workingDir, FlagUtils.XbitFile), source, destination);
            FlagUtils.Rename(Path.Combine(workingDir, FlagUtils.SymlinkFile), source, destination);
        }
    }
}
