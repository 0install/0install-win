using System;
using System.Collections.Generic;
using System.Text;
using Common.Helpers;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;
using System.IO;

namespace ZeroInstall.DownloadBroker
{
    class HierarchyToManifest : HierarchyVisitor
    {
        readonly StreamWriter writer;

        public HierarchyToManifest(Stream target)
        {
            writer = new StreamWriter(target) { NewLine = "\n" };
        }

        public override void VisitRoot(RootEntry entry)
        {
            visitChildren(entry);
            writer.Flush();
        }
        public override void VisitFolder(FolderEntry entry)
        {
            ManifestNode node = new ManifestDirectory(FileHelper.UnixTime(entry.LastWriteTime), "/" + entry.RelativePath.Replace("\\", "/"));
            writer.WriteLine(ManifestFormat.Sha256.GenerateEntryForNode(node));
            visitChildren(entry);
        }
        public override void VisitFile(FileEntry entry)
        {
            ManifestNode node = null;
            string hash;
            long size;
            using (var entryData = new MemoryStream(entry.Content))
            {
                size = entryData.Length;
                hash = FileHelper.ComputeHash(entryData, ManifestFormat.Sha256.HashAlgorithm);
            }
            if (entry.IsExecutable()) node = new ManifestExecutableFile(hash, FileHelper.UnixTime(entry.LastWriteTime), size, entry.Name);
            else node = new ManifestFile(hash, FileHelper.UnixTime(entry.LastWriteTime), size, entry.Name);
            writer.WriteLine(ManifestFormat.Sha256.GenerateEntryForNode(node));
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
                return new ManifestDigest(ManifestFormat.Sha256.Prefix + FileHelper.ComputeHash(dotFile, ManifestFormat.Sha256.HashAlgorithm));
            }
        }

        private static void WriteHierarchyManifestToStream(PackageBuilder package, Stream dotFile)
        {
            var manifestCollector = new HierarchyToManifest(dotFile);
            package.Hierarchy.AcceptVisitor(manifestCollector);
        }
    }
}
