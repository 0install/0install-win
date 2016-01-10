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
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Deployment
{
    /// <summary>
    /// Deletes files listed in a <see cref="Manifest"/> file from a directory.
    /// </summary>
    public class ClearDirectory : DirectoryOperation
    {
        private readonly Stack<string> _pendingDirectoryDeletes = new Stack<string>();

        /// <summary>
        /// Creates a new directory clear task.
        /// </summary>
        /// <param name="path">The path of the directory to clear.</param>
        /// <param name="manifest">The contents of a <see cref="Store.Implementations.Manifest"/> file describing the directory.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about IO tasks.</param>
        public ClearDirectory([NotNull] string path, [NotNull] Manifest manifest, [NotNull] ITaskHandler handler)
            : base(path, manifest, handler)
        {
        }

        private readonly Stack<KeyValuePair<string, string>> _pendingFilesDeletes = new Stack<KeyValuePair<string, string>>();

        /// <inheritdoc/>
        protected override void OnStage()
        {
            _pendingDirectoryDeletes.Push(Path);

            var filesToDelete = new List<string>();

            string manifestPath = System.IO.Path.Combine(Path, Manifest.ManifestFile);
            if (File.Exists(manifestPath))
                filesToDelete.Add(manifestPath);

            foreach (var pair in ElementPaths)
            {
                string elementPath = System.IO.Path.Combine(Path, pair.Key);

                if (pair.Value is ManifestDirectory)
                {
                    if (Directory.Exists(elementPath))
                        _pendingDirectoryDeletes.Push(elementPath);
                }
                else
                {
                    if (File.Exists(elementPath))
                        filesToDelete.Add(elementPath);
                }
            }

            if (filesToDelete.Count != 0)
            {
                UnlockFiles(filesToDelete);

                Handler.RunTask(ForEachTask.Create(Resources.DeletingObsoleteFiles, filesToDelete, path =>
                {
                    string tempPath = Randomize(path);
                    File.Move(path, tempPath);
                    _pendingFilesDeletes.Push(new KeyValuePair<string, string>(path, tempPath));
                }));
            }
        }

        /// <inheritdoc/>
        protected override void OnCommit()
        {
            Handler.RunTask(new SimpleTask(Resources.DeletingObsoleteFiles, () =>
            {
                _pendingFilesDeletes.PopEach(x => File.Delete(x.Value));
                _pendingDirectoryDeletes.PopEach(path =>
                {
                    if (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length == 0)
                        Directory.Delete(path);
                });
            }));
        }

        /// <inheritdoc/>
        protected override void OnRollback()
        {
            _pendingFilesDeletes.PopEach(x =>
            {
                try
                {
                    File.Move(x.Value, x.Key);
                }
                    #region Error handling
                catch (Exception ex)
                {
                    Log.Error(ex);
                    throw;
                }
                #endregion
            });
        }
    }
}
