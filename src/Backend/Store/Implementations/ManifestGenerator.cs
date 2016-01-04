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
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Generates a <see cref="Implementations.Manifest"/> for a directory.
    /// </summary>
    public class ManifestGenerator : DirectoryWalkTask
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return string.Format(Resources.GeneratingManifest, Format); } }

        /// <summary>
        /// The format of the manifest to generate.
        /// </summary>
        [NotNull]
        public ManifestFormat Format { get; private set; }

        private readonly List<ManifestNode> _nodes = new List<ManifestNode>();

        /// <summary>
        /// If <see cref="TaskBase.State"/> is <see cref="TaskState.Complete"/> this property contains the generated <see cref="Implementations.Manifest"/>; otherwise it's <c>null</c>.
        /// </summary>
        public Manifest Manifest { get { return new Manifest(Format, _nodes);} }
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to generate a manifest for a directory in the filesystem.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to analyze.</param>
        /// <param name="format">The format of the manifest to generate.</param>
        public ManifestGenerator([NotNull] string sourceDirectory, [NotNull] ManifestFormat format) : base(sourceDirectory)
        {
            #region Sanity checks
            if (format == null) throw new ArgumentNullException("format");
            #endregion

            Format = format;
        }
        #endregion

        /// <inheritdoc/>
        protected override void HandleFile(FileInfo file, bool executable = false)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            using (var stream = file.OpenRead())
            {
                if (executable) _nodes.Add(new ManifestExecutableFile(Format.DigestContent(stream), file.LastWriteTimeUtc.ToUnixTime(), file.Length, file.Name));
                else _nodes.Add(new ManifestNormalFile(Format.DigestContent(stream), file.LastWriteTimeUtc.ToUnixTime(), file.Length, file.Name));
            }
        }

        /// <inheritdoc/>
        protected override void HandleSymlink(FileSystemInfo symlink, byte[] data)
        {
            #region Sanity checks
            if (symlink == null) throw new ArgumentNullException("symlink");
            #endregion

            _nodes.Add(new ManifestSymlink(Format.DigestContent(data), data.Length, symlink.Name));
        }

        /// <inheritdoc/>
        protected override void HandleDirectory(DirectoryInfo directory)
        {
            #region Sanity checks
            if (directory == null) throw new ArgumentNullException("directory");
            #endregion

            _nodes.Add(new ManifestDirectory("/" + directory.RelativeTo(SourceDirectory)));
        }
    }
}
