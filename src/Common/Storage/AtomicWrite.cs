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

using System;
using System.IO;
using Common.Utils;

namespace Common.Storage
{
    /// <summary>
    /// Provides a temporary path to write to and atomically inserts it at the target location on disposal.
    /// </summary>
    public sealed class AtomicWrite : IDisposable
    {
        #region Properties
        /// <summary>
        /// The file path of the final target.
        /// </summary>
        public string TargetPath { get; private set; }

        /// <summary>
        /// The temporary file path to write to.
        /// </summary>
        public string WritePath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares a atomic write operation.
        /// </summary>
        /// <param name="path">The file path of the final target.</param>
        public AtomicWrite(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            TargetPath = path;

            // Make sure the containing directory exists
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Prepend random string for temp file name
            WritePath = directory + Path.DirectorySeparatorChar + "temp." + Path.GetRandomFileName() + "." + Path.GetFileName(path);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Replaces <see cref="TargetPath"/> with the contents of <see cref="WritePath"/>.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (File.Exists(WritePath)) FileUtils.Replace(WritePath, TargetPath);
            }
            finally
            {
                if (File.Exists(WritePath)) File.Delete(WritePath);
            }
        }
        #endregion
    }
}
