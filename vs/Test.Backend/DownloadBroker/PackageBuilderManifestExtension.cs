/*
 * Copyright 2010 Roland Leopold Walkling
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

using Common.Utils;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;
using System.IO;

namespace ZeroInstall.DownloadBroker
{
    class HierarchyToManifest : HierarchyVisitor
    {
        readonly StreamWriter _writer;

        public HierarchyToManifest(Stream target)
        {
            _writer = new StreamWriter(target) { NewLine = "\n" };
        }

        public override void VisitRoot(RootEntry entry)
        {
            visitChildren(entry);
            _writer.Flush();
        }
        public override void VisitFolder(FolderEntry entry)
        {
            ManifestNode node = new ManifestDirectory(FileUtils.UnixTime(entry.LastWriteTime), "/" + entry.RelativePath.Replace("\\", "/"));
            _writer.WriteLine(ManifestFormat.Sha256.GenerateEntryForNode(node));
            visitChildren(entry);
        }
        public override void VisitFile(FileEntry entry)
        {
            ManifestNode node;
            string hash;
            long size;
            using (var entryData = new MemoryStream(entry.Content))
            {
                size = entryData.Length;
                hash = FileUtils.ComputeHash(entryData, ManifestFormat.Sha256.HashAlgorithm);
            }
            if (entry.IsExecutable) node = new ManifestExecutableFile(hash, FileUtils.UnixTime(entry.LastWriteTime), size, entry.Name);
            else node = new ManifestNormalFile(hash, FileUtils.UnixTime(entry.LastWriteTime), size, entry.Name);
            _writer.WriteLine(ManifestFormat.Sha256.GenerateEntryForNode(node));
        }
    }

    public static class PackageBuilderManifestExtension
    {
        public static ManifestDigest ComputePackageDigest(PackageBuilder package)
        {
            using (var dotFile = new MemoryStream())
            {
                WriteHierarchyManifestToStream(package, dotFile);
                dotFile.Seek(0, SeekOrigin.Begin);
                return new ManifestDigest(ManifestFormat.Sha256.Prefix + FileUtils.ComputeHash(dotFile, ManifestFormat.Sha256.HashAlgorithm));
            }
        }

        private static void WriteHierarchyManifestToStream(PackageBuilder package, Stream dotFile)
        {
            var manifestCollector = new HierarchyToManifest(dotFile);
            package.Hierarchy.AcceptVisitor(manifestCollector);
        }
    }
}
