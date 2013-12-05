﻿/*
 * Copyright 2010-2013 Bastian Eicher, Roland Leopold Walkling
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
using Common.Collections;
using Common.Storage;
using Common.Streams;
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
        /// <param name="downloadedFiles">Files downloaded for the the <paramref name="recipe"/>. Must be in same order as <see cref="DownloadRetrievalMethod"/> elements in <paramref name="recipe"/>.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; may be <see langword="null"/>.</param>
        /// <returns>A <see cref="TemporaryDirectory"/> with the resulting directory content.</returns>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="recipe"/> contains unknown step types.</exception>
        public static TemporaryDirectory Apply(this Recipe recipe, IEnumerable<TemporaryFile> downloadedFiles, ITaskHandler handler, object tag = null)
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
                new PerTypeDispatcher<IRecipeStep>(false)
                {
                    (Model.Archive step) =>
                    {
                        downloadedEnum.MoveNext();
                        step.Apply(downloadedEnum.Current, workingDir, handler, tag);
                    },
                    (SingleFile step) =>
                    {
                        downloadedEnum.MoveNext();
                        step.Apply(downloadedEnum.Current, workingDir, handler, tag);
                    },
                    (RemoveStep step) => step.Apply(workingDir),
                    (RenameStep step) => step.Apply(workingDir)
                }.Dispatch(recipe.Steps);
                // ReSharper restore AccessToDisposedClosure
                return workingDir;
            }
                #region Error handling
            catch (Exception)
            {
                workingDir.Dispose();
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Applies a <see cref="Model.Archive"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Model.Archive"/> to apply.</param>
        /// <param name="localPath">The local path of the archive.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; may be <see langword="null"/>.</param>
        /// <exception cref="IOException">Thrown if a path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply(this Model.Archive step, string localPath, TemporaryDirectory workingDir, ITaskHandler handler, object tag = null)
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

            using (var stream = new OffsetStream(File.OpenRead(localPath), step.StartOffset))
            {
                var extractor = Extractor.CreateExtractor(stream, step.MimeType, workingDir);
                extractor.SubDir = step.Extract;
                extractor.Destination = FileUtils.UnifySlashes(step.Destination);
                handler.RunTask(extractor, tag); // Defer task to handler
            }
        }

        /// <summary>
        /// Applies a <see cref="SingleFile"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Model.Archive"/> to apply.</param>
        /// <param name="localPath">The local path of the file.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; may be <see langword="null"/>.</param>
        /// <exception cref="IOException">Thrown if a path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply(this SingleFile step, string localPath, TemporaryDirectory workingDir, ITaskHandler handler, object tag = null)
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
        /// <param name="step">The <see cref="Model.Archive"/> to apply.</param>
        /// <param name="downloadedFile">The file downloaded from <see cref="DownloadRetrievalMethod.Href"/>.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <param name="tag">The <see cref="ITaskHandler"/> tag used by <paramref name="handler"/>; may be <see langword="null"/>.</param>
        /// <exception cref="IOException">Thrown if a path specified in <paramref name="step"/> is illegal.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "tag", Justification = "Number of method parameters must match overloaded method to ensure proper type-based compiler selection.")]
        public static void Apply(this SingleFile step, TemporaryFile downloadedFile, TemporaryDirectory workingDir, ITaskHandler handler, object tag = null)
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

            // Update in flag files aswell
            string xbitFile = Path.Combine(workingDir, ".xbit");
            if (FlagUtils.GetExternalFlags(".xbit", workingDir).Contains(destinationPath))
                FlagUtils.RemoveExternalFlag(xbitFile, destination);
        }

        /// <summary>
        /// Applies a <see cref="RemoveStep"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Model.Archive"/> to apply.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <exception cref="IOException">Thrown if a path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply(this RemoveStep step, TemporaryDirectory workingDir)
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

            // Delete the element
            string absolutePath = Path.Combine(workingDir, path);
            if (File.Exists(absolutePath)) File.Delete(absolutePath);
            else if (Directory.Exists(absolutePath)) Directory.Delete(absolutePath, recursive: true);

            // Remove from flag files aswell
            FlagUtils.RemoveExternalFlag(Path.Combine(workingDir, ".xbit"), path);
            FlagUtils.RemoveExternalFlag(Path.Combine(workingDir, ".symlink"), path);
        }

        /// <summary>
        /// Applies a <see cref="RenameStep"/> to a <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <param name="step">The <see cref="Model.Archive"/> to apply.</param>
        /// <param name="workingDir">The <see cref="TemporaryDirectory"/> to apply the changes to.</param>
        /// <exception cref="IOException">Thrown if a path specified in <paramref name="step"/> is illegal.</exception>
        public static void Apply(this RenameStep step, TemporaryDirectory workingDir)
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

            // Rename the element
            string sourcePath = Path.Combine(workingDir, source);
            string destinationPath = Path.Combine(workingDir, destination);
            string parentDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);
            File.Move(sourcePath, destinationPath);

            // Update in flag files aswell
            string xbitFile = Path.Combine(workingDir, ".xbit");
            if (FlagUtils.GetExternalFlags(".xbit", workingDir).Contains(sourcePath))
            {
                FlagUtils.RemoveExternalFlag(xbitFile, source);
                FlagUtils.SetExternalFlag(xbitFile, destination);
            }
            string symlinkFile = Path.Combine(workingDir, ".symlink");
            if (FlagUtils.GetExternalFlags(".symlink", workingDir).Contains(sourcePath))
            {
                FlagUtils.RemoveExternalFlag(symlinkFile, source);
                FlagUtils.SetExternalFlag(symlinkFile, destination);
            }
        }
    }
}
