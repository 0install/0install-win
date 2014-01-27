﻿/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;
using System.Linq;
using Common.Collections;
using Common.Dispatch;
using Common.Storage;
using Common.Tasks;
using Common.Undo;
using Common.Utils;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Helper methods for manipulating <see cref="RetrievalMethod"/>s.
    /// </summary>
    public static class RetrievalMethodUtils
    {
        #region Download
        /// <summary>
        /// Downloads and applies a <see cref="RetrievalMethod"/> and adds missing properties.
        /// </summary>
        /// <param name="retrievalMethod">The <see cref="RetrievalMethod"/> to be downloaded.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <returns>A temporary directory containing the extracted content.</returns>
        public static TemporaryDirectory DownloadAndApply(this RetrievalMethod retrievalMethod, ITaskHandler handler, ICommandExecutor executor)
        {
            var download = retrievalMethod as DownloadRetrievalMethod;
            if (download != null) return download.DownloadAndApply(handler, executor);

            var recipe = retrievalMethod as Recipe;
            if (recipe != null) return recipe.DownloadAndApply(handler, executor);

            throw new NotSupportedException(Resources.UnknownRetrievalMethodType);
        }

        /// <summary>
        /// Downloads and applies a <see cref="DownloadRetrievalMethod"/> and adds missing properties.
        /// </summary>
        /// <param name="retrievalMethod">The <see cref="DownloadRetrievalMethod"/> to be downloaded.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <returns>A temporary directory containing the extracted content.</returns>
        public static TemporaryDirectory DownloadAndApply(this DownloadRetrievalMethod retrievalMethod, ITaskHandler handler, ICommandExecutor executor)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException("retrievalMethod");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            using (var downloadedFile = retrievalMethod.Download(handler, executor))
            {
                var extractionDir = new TemporaryDirectory("0publish");
                try
                {
                    new PerTypeDispatcher<DownloadRetrievalMethod>(true)
                    {
                        // ReSharper disable AccessToDisposedClosure
                        (Archive archive) => archive.Apply(downloadedFile, extractionDir, handler),
                        (SingleFile file) => file.Apply(downloadedFile, extractionDir, handler)
                        // ReSharper restore AccessToDisposedClosure
                    }.Dispatch(retrievalMethod);
                }
                    #region Error handling
                catch
                {
                    extractionDir.Dispose();
                    throw;
                }
                #endregion

                return extractionDir;
            }
        }

        /// <summary>
        /// Downloads and applies a <see cref="Recipe"/> and adds missing properties.
        /// </summary>
        /// <param name="recipe">The <see cref="Recipe"/> to be applied.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <returns>A temporary directory containing the result of the recipe.</returns>
        public static TemporaryDirectory DownloadAndApply(this Recipe recipe, ITaskHandler handler, ICommandExecutor executor)
        {
            #region Sanity checks
            if (recipe == null) throw new ArgumentNullException("recipe");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var downloadedFiles = new List<TemporaryFile>();
            try
            {
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var step in recipe.Steps.OfType<DownloadRetrievalMethod>())
                    downloadedFiles.Add(step.Download(handler, executor));
                // ReSharper restore LoopCanBeConvertedToQuery

                // Apply the recipe
                return recipe.Apply(downloadedFiles, handler);
            }
            finally
            {
                // Clean up temporary archive files
                foreach (var downloadedFile in downloadedFiles) downloadedFile.Dispose();
            }
        }

        /// <summary>
        /// Downloads a <see cref="DownloadRetrievalMethod"/> and adds missing properties.
        /// </summary>
        /// <param name="retrievalMethod">The <see cref="DownloadRetrievalMethod"/> to be downloaded.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <returns>A downloaded file.</returns>
        public static TemporaryFile Download(this DownloadRetrievalMethod retrievalMethod, ITaskHandler handler, ICommandExecutor executor)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException("retrievalMethod");
            if (handler == null) throw new ArgumentNullException("handler");
            if (executor == null) throw new ArgumentNullException("executor");
            #endregion

            if (retrievalMethod.Href == null) throw new ArgumentException(Resources.HrefMissing, "retrievalMethod");
            new PerTypeDispatcher<DownloadRetrievalMethod>(false)
            {
                // ReSharper disable AccessToDisposedClosure
                (Archive archive) =>
                {
                    // Guess MIME types now because the file ending is not known later
                    if (string.IsNullOrEmpty(archive.MimeType))
                    {
                        string mimeType = Archive.GuessMimeType(archive.Href.ToString());
                        executor.Execute(new SetValueCommand<string>(() => archive.MimeType, value => archive.MimeType = value, mimeType));
                    }
                },
                (SingleFile file) =>
                {
                    // Guess file name based on URL
                    if (string.IsNullOrEmpty(file.Destination))
                    {
                        string destination = file.Href.ToString().GetRightPartAtLastOccurrence('/').StripCharacters(Path.GetInvalidFileNameChars());
                        executor.Execute(new SetValueCommand<string>(() => file.Destination, value => file.Destination = value, destination));
                    }
                }
                // ReSharper restore AccessToDisposedClosure
            }.Dispatch(retrievalMethod);

            // Download the file
            var href = FeedElementUtils.GetAbsoluteHref(retrievalMethod.Href, executor.Path);
            var downloadedFile = new TemporaryFile("0publish");
            handler.RunTask(new DownloadFile(href, downloadedFile)); // Defer task to handler

            // Set downloaded file size
            long newSize = new FileInfo(downloadedFile).Length;
            if (retrievalMethod.Size != newSize)
                executor.Execute(new SetValueCommand<long>(() => retrievalMethod.Size, value => retrievalMethod.Size = value, newSize));

            return downloadedFile;
        }
        #endregion

        #region Local
        /// <summary>
        /// Applies a locally stored <see cref="DownloadRetrievalMethod"/> and adds missing properties.
        /// </summary>
        /// <param name="retrievalMethod">The <see cref="DownloadRetrievalMethod"/> to be applied.</param>
        /// <param name="localPath">The local file path where the <paramref name="retrievalMethod"/> is located.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <param name="executor">Used to apply properties in an undoable fashion.</param>
        /// <returns>A temporary directory containing the extracted content.</returns>
        public static TemporaryDirectory LocalApply(this DownloadRetrievalMethod retrievalMethod, string localPath, ITaskHandler handler, ICommandExecutor executor)
        {
            #region Sanity checks
            if (retrievalMethod == null) throw new ArgumentNullException("retrievalMethod");
            if (string.IsNullOrEmpty(localPath)) throw new ArgumentNullException("localPath");
            if (executor == null) throw new ArgumentNullException("executor");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Set local file size
            long newSize = new FileInfo(localPath).Length;
            if (retrievalMethod.Size != newSize)
                executor.Execute(new SetValueCommand<long>(() => retrievalMethod.Size, value => retrievalMethod.Size = value, newSize));

            var extractionDir = new TemporaryDirectory("0publish");
            try
            {
                new PerTypeDispatcher<DownloadRetrievalMethod>(true)
                {
                    // ReSharper disable AccessToDisposedClosure
                    (Archive archive) =>
                    {
                        // Guess MIME types now because the file ending is not known later
                        if (string.IsNullOrEmpty(archive.MimeType))
                        {
                            string mimeType = Archive.GuessMimeType(localPath);
                            executor.Execute(new SetValueCommand<string>(() => archive.MimeType, value => archive.MimeType = value, mimeType));
                        }

                        archive.Apply(localPath, extractionDir, handler);
                    },
                    (SingleFile file) =>
                    {
                        // Guess file name based on local path
                        if (string.IsNullOrEmpty(file.Destination))
                        {
                            string destination = Path.GetFileName(localPath);
                            executor.Execute(new SetValueCommand<string>(() => file.Destination, value => file.Destination = value, destination));
                        }

                        file.Apply(localPath, extractionDir, handler);
                    }
                    // ReSharper restore AccessToDisposedClosure
                }.Dispatch(retrievalMethod);
            }
                #region Error handling
            catch (Exception)
            {
                extractionDir.Dispose();
                throw;
            }
            #endregion

            return extractionDir;
        }
        #endregion
    }
}
