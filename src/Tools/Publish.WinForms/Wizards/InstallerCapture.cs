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
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
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
        [CanBeNull]
        private Uri _url;

        [CanBeNull]
        private string _localPath;

        public void SetLocal([NotNull] Uri url, [NotNull] string path)
        {
            _url = url;
            _localPath = path;
        }

        [CanBeNull]
        private TemporaryDirectory _tempDir;

        public void Download([NotNull] Uri url, [NotNull] ITaskHandler handler)
        {
            _url = url;

            if (_tempDir != null) _tempDir.Dispose();
            _tempDir = new TemporaryDirectory("0publish");

            try
            {
                _localPath = Path.Combine(_tempDir, Path.GetFileName(url.LocalPath));
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

        public void Dispose()
        {
            if (_tempDir != null) _tempDir.Dispose();
        }

        public CaptureSession CaptureSession { get; set; }

        public void RunInstaller([NotNull] ITaskHandler handler)
        {
            if (string.IsNullOrEmpty(_localPath)) throw new InvalidOperationException();

            var process = Process.Start(_localPath);
            if (process == null) return;
            handler.RunTask(new SimpleTask(Resources.WaitingForInstaller, () => process.WaitForExit()));
        }

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
