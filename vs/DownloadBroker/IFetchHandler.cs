/*
 * Copyright 2010 Bastian Eicher
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

using Common.Download;

namespace ZeroInstall.DownloadBroker
{
    /// <summary>
    /// Callback methods to be used when the the user is to be informed about download and extraction progress.
    /// </summary>
    /// <remarks>
    /// All callbacks are called from the original thread.
    /// Thread-safety messures are needed only if the process was started on a background thread and is intended to update a UI.
    /// </remarks>
    public interface IFetchHandler
    {
        /// <summary>
        /// Called when a new download is about to be started.
        /// </summary>
        /// <param name="download">The acutal download. Can be used for tracking the progress.</param>
        void StartingDownload(DownloadFile download);

        /// <summary>
        /// Called to inform the user of the user of the progress of extracting files from an archive.
        /// </summary>
        /// <param name="progress">The progress of the operation as a value between 0 and 1; -1 when unknown.</param>
        /// <param name="file">The name of the file currently being processed.</param>
        void ReportExtractionProgress(float progress, string file);

        /// <summary>
        /// Called to inform the user of the user of the progress of generating a manifest (hashing files).
        /// </summary>
        /// <param name="progress">The progress of the operation as a value between 0 and 1; -1 when unknown.</param>
        /// <param name="file">The name of the file currently being processed.</param>
        void ReportManifestProgress(float progress, string file);
    }
}
