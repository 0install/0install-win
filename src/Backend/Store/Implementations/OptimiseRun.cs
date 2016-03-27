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
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Manages state during a single <see cref="IStore.Optimise"/> run.
    /// </summary>
    internal sealed class OptimiseRun : IDisposable
    {
        private struct DedupKey
        {
            public readonly long Size;
            public readonly DateTime LastModified;
            public readonly ManifestFormat Format;
            public readonly string Digest;

            public DedupKey(long size, DateTime lastModified, ManifestFormat format, string digest)
            {
                Size = size;
                LastModified = lastModified;
                Format = format;
                Digest = digest;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is DedupKey)) return false;
                var other = (DedupKey)obj;
                return Size == other.Size && LastModified == other.LastModified && Format.Equals(other.Format) && string.Equals(Digest, other.Digest);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = Size.GetHashCode();
                    hashCode = (hashCode * 397) ^ LastModified.GetHashCode();
                    hashCode = (hashCode * 397) ^ Format.GetHashCode();
                    hashCode = (hashCode * 397) ^ Digest.GetHashCode();
                    return hashCode;
                }
            }
        }

        private struct StoreFile
        {
            public readonly string ImplementationPath;
            public readonly string RelativePath;

            public StoreFile(string implementationPath, string relativePath)
            {
                ImplementationPath = implementationPath;
                RelativePath = relativePath;
            }

            public static implicit operator string(StoreFile file)
            {
                return Path.Combine(file.ImplementationPath, file.RelativePath);
            }
        }

        private readonly Dictionary<DedupKey, StoreFile> _fileHashes = new Dictionary<DedupKey, StoreFile>();
        private readonly HashSet<string> _unsealedImplementations = new HashSet<string>();
        private readonly string _storePath;

        /// <summary>
        /// The number of bytes saved by deduplication.
        /// </summary>
        public long SavedBytes;

        /// <summary>
        /// Creates a new optimise run.
        /// </summary>
        /// <param name="storePath">The <see cref="IStore.DirectoryPath"/>.</param>
        public OptimiseRun(string storePath)
        {
            _storePath = storePath;
        }

        /// <summary>
        /// Executes the work-step for a single implementation.
        /// </summary>
        public void Work(ManifestDigest manifestDigest)
        {
            string digestString = manifestDigest.Best;
            if (digestString == null) return;
            string implementationPath = Path.Combine(_storePath, digestString);
            var manifest = Manifest.Load(Path.Combine(implementationPath, Manifest.ManifestFile), ManifestFormat.FromPrefix(digestString));

            string currentDirectory = "";
            new AggregateDispatcher<ManifestNode>
            {
                (ManifestDirectory x) => { currentDirectory = FileUtils.UnifySlashes(x.FullPath.TrimStart('/')); },
                (ManifestFileBase x) =>
                {
                    if (x.Size == 0) return;

                    var key = new DedupKey(x.Size, x.ModifiedTime, manifest.Format, x.Digest);
                    var file = new StoreFile(implementationPath, Path.Combine(currentDirectory, x.Name));

                    StoreFile existingFile;
                    if (_fileHashes.TryGetValue(key, out existingFile))
                    {
                        if (!FileUtils.AreHardlinked(file, existingFile))
                        {
                            if (JoinWithHardlink(file, existingFile))
                                SavedBytes += x.Size;
                        }
                    }
                    else _fileHashes.Add(key, file);
                }
            }.Dispatch(manifest);
        }

        private bool JoinWithHardlink(StoreFile file1, StoreFile file2)
        {
            if (FileUtils.AreHardlinked(file1, file2)) return false;

            if (_unsealedImplementations.Add(file1.ImplementationPath))
                FileUtils.DisableWriteProtection(file1.ImplementationPath);
            if (_unsealedImplementations.Add(file2.ImplementationPath))
                FileUtils.DisableWriteProtection(file2.ImplementationPath);

            string tempFile = Path.Combine(_storePath, Path.GetRandomFileName());
            try
            {
                Log.Info("Hard link: " + file1 + " <=> " + file2);
                FileUtils.CreateHardlink(tempFile, file2);
                FileUtils.Replace(tempFile, file1);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
            return true;
        }

        public void Dispose()
        {
            foreach (string path in _unsealedImplementations)
                FileUtils.EnableWriteProtection(path);
        }
    }
}
