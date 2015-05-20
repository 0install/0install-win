/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.Capture;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    /// <summary>
    /// Holds state shared between Wizard pages when capturing an installer.
    /// </summary>
    internal sealed class InstallerCapture : IDisposable
    {
        /// <summary>
        /// Holds the <see cref="CaptureSession"/> created at the start of the process.
        /// </summary>
        public CaptureSession CaptureSession { get; set; }

        [CanBeNull]
        private Uri _url;

        [CanBeNull]
        private string _localPath;

        /// <summary>
        /// Sets the installer source to a pre-existing local file.
        /// </summary>
        /// <param name="url">The URL the file was originally downloaded from.</param>
        /// <param name="path">The local path of the file.</param>
        /// <remarks>Use either this or <see cref="Download"/>.</remarks>
        public void SetLocal([NotNull] Uri url, [NotNull] string path)
        {
            _url = url;
            _localPath = path;
        }

        [CanBeNull]
        private TemporaryDirectory _tempDir;

        /// <summary>
        /// Downloads the installer from the web to a temporary file.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">A downloaded file could not be written to the disk.</exception>
        /// <exception cref="UnauthorizedAccessException">An operation failed due to insufficient rights.</exception>
        /// <remarks>Use either this or <see cref="SetLocal"/>.</remarks>
        public void Download([NotNull] Uri url, [NotNull] ITaskHandler handler)
        {
            _url = url;

            if (_tempDir != null) _tempDir.Dispose();
            _tempDir = new TemporaryDirectory("0publish");

            try
            {
                _localPath = Path.Combine(_tempDir, url.GetLocalFileName());
                handler.RunTask(new DownloadFile(url, _localPath));
            }
                #region Error handling
            catch (Exception)
            {
                _tempDir.Dispose();
                _tempDir = null;
                _url = null;
                _localPath = null;
                throw;
            }
            #endregion
        }

        /// <summary>
        /// Disposes any temporary files created by <see cref="Download"/>.
        /// </summary>
        public void Dispose()
        {
            if (_tempDir != null) _tempDir.Dispose();
        }

        /// <summary>
        /// Runs the installer and waits for it to exit.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        /// <exception cref="IOException">There is a problem access a temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a temporary file is not permitted.</exception>
        public void RunInstaller([NotNull] ITaskHandler handler)
        {
            if (string.IsNullOrEmpty(_localPath)) throw new InvalidOperationException();

            var process = ProcessUtils.Start(_localPath);
            if (process == null) return;
            handler.RunTask(new SimpleTask(Resources.WaitingForInstaller, () => process.WaitForExit()));
        }

        /// <summary>
        /// Tries extracting the installer as an <see cref="Archive"/>.
        /// </summary>
        /// <param name="feedBuilder">All collected data is stored into this builder.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about progress.</param>
        /// <exception cref="OperationCanceledException">The user canceled the operation.</exception>
        /// <exception cref="IOException">The installer could not be extracted as an archive.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to a temporary file is not permitted.</exception>
        public void ExtractInstallerAsArchive([NotNull] FeedBuilder feedBuilder, [NotNull] ITaskHandler handler)
        {
            if (string.IsNullOrEmpty(_localPath) || _url == null) throw new InvalidOperationException();

            var archive = new Archive
            {
                Href = _url,
                MimeType = _localPath.EndsWith(@".msi")
                    ? Archive.MimeTypeMsi
                    // 7zip's extraction logic can handle a number of self-extracting formats
                    : Archive.MimeType7Z
            };
            feedBuilder.RetrievalMethod = archive;
            feedBuilder.TemporaryDirectory = archive.LocalApply(_localPath, handler);
        }
    }
}
