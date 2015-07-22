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
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Publish.Properties;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Creates a ZIP archive from a local directory.
    /// </summary>
    public sealed class CreateZip : TaskBase
    {
        /// <inheritdoc/>
        public override string Name { get { return string.Format(Resources.CreatingZip, DestinationFile); } }

        /// <inheritdoc/>
        public override bool CanCancel { get { return true; } }

        /// <inheritdoc/>
        protected override bool UnitsByte { get { return false; } }

        /// <summary>
        /// The path of the directory to capture/store in the ZIP archive.
        /// </summary>
        [NotNull]
        public string SourceDirectory { get; private set; }

        /// <summary>
        /// The path of the ZIP file to create.
        /// </summary>
        [NotNull]
        public string DestinationFile { get; private set; }

        /// <summary>
        /// Creates a new ZIP task.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to capture/store in the ZIP archive.</param>
        /// <param name="destinationFile">The path of the ZIP file to create.</param>
        public CreateZip([NotNull] string sourceDirectory, [CanBeNull] string destinationFile)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourceDirectory)) throw new ArgumentNullException("sourceDirectory");
            if (string.IsNullOrEmpty(destinationFile)) throw new ArgumentNullException("destinationFile");
            #endregion

            SourceDirectory = sourceDirectory;
            DestinationFile = destinationFile;
        }

        /// <inheritdoc/>
        protected override void Execute()
        {
            UnitsTotal = 100;
            State = TaskState.Data;

            var fastZip = new FastZip(new FastZipEvents
            {
                Progress = (sender, args) =>
                {
                    UnitsProcessed = (long)args.PercentComplete;
                    args.ContinueRunning = !CancellationToken.IsCancellationRequested;
                }
            });
            fastZip.CreateZip(DestinationFile, SourceDirectory, recurse: true, fileFilter: null);

            State = TaskState.Complete;
        }
    }
}
