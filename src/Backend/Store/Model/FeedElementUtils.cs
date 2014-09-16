/*
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
using System.IO;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Helper methods for handling <see cref="FeedElement"/>s.
    /// </summary>
    public static class FeedElementUtils
    {
        /// <summary>
        /// Turns a relative path into an absolute one, using the file containing the reference as the base.
        /// </summary>
        /// <param name="path">The potentially relative path; will remain untouched if absolute.</param>
        /// <param name="sourcePath">The file containing the reference.</param>
        /// <returns>An absolute path.</returns>
        /// <exception cref="IOException"><paramref name="sourcePath"/> is not a local file path.</exception>
        public static string GetAbsolutePath(string path, string sourcePath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (Path.IsPathRooted(path)) return path;

            if (!Path.IsPathRooted(sourcePath)) throw new IOException(string.Format(Resources.RelativePathInNonRemoteFeed, path));
            string sourceDir = Path.GetDirectoryName(sourcePath);
            if (string.IsNullOrEmpty(sourceDir)) throw new IOException(string.Format(Resources.RelativePathInNonRemoteFeed, path));

            return Path.Combine(sourceDir, FileUtils.UnifySlashes(path));
        }

        /// <summary>
        /// Turns a relative HREF into an absolute one, using the file containing the reference as the base.
        /// </summary>
        /// <param name="href">The potentially relative HREF; will remain untouched if absolute.</param>
        /// <param name="sourcePath">The file containing the reference.</param>
        /// <returns>An absolute HREF.</returns>
        /// <exception cref="IOException"><paramref name="sourcePath"/> is not a local file path.</exception>
        public static Uri GetAbsoluteHref(Uri href, string sourcePath)
        {
            #region Sanity checks
            if (href == null) throw new ArgumentNullException("href");
            #endregion

            if (href.IsAbsoluteUri) return href;

            if (!Path.IsPathRooted(sourcePath)) throw new IOException(string.Format(Resources.RelativeUriInRemoteFeed, href));

            return new Uri(new Uri(sourcePath), href);
        }
    }
}
